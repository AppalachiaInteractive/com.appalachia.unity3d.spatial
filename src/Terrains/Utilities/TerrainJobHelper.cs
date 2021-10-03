#region

using Appalachia.Core.Collections.Native;
using Appalachia.Spatial;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Core.Terrains.Utilities
{
    public static class TerrainJobHelper
    {
        public static float3 GetTerrainScale(Terrain terrain)
        {
            var terrainData = terrain.terrainData;

            var scale = float3.zero;

            scale.x = terrainData.size.x / (terrainData.heightmapResolution - 1);
            scale.y = terrainData.size.y;
            scale.z = terrainData.size.z / (terrainData.heightmapResolution - 1);

            return scale;
        }

        public static NativeKeyArray2D<int, float> InitializeJobHeights(Terrain[] terrains)
        {
            var maxResolution = 0;

            for (var terrainIndex = 0; terrainIndex < terrains.Length; terrainIndex++)
            {
                var terrain = terrains[terrainIndex];
                var terrainData = terrain.terrainData;
                maxResolution = math.max(maxResolution, terrainData.heightmapResolution);
            }

            var array = new NativeKeyArray2D<int, float>(terrains.Length, maxResolution * maxResolution, Allocator.Persistent);

            LoadHeightData(terrains, array);

            return array;
        }

        public static void LoadHeightData(Terrain[] terrains, NativeKeyArray2D<int, float> array)
        {
            for (var terrainIndex = 0; terrainIndex < terrains.Length; terrainIndex++)
            {
                var terrain = terrains[terrainIndex];
                var terrainData = terrain.terrainData;
                var resolution = terrainData.heightmapResolution;

                var heights2D = terrainData.GetHeights(0, 0, resolution, resolution);

                var sizeY = heights2D.GetLength(0);
                var sizeX = heights2D.GetLength(1);

                //var resultArray1D = new NativeArray<T>(array2D.GetLength(0) * array2D.GetLength(1), allocator);

                array[terrainIndex] = terrain.GetHashCode();

                for (var y = 0; y < sizeY; y++)
                {
                    for (var x = 0; x < sizeX; x++)
                    {
                        array[terrainIndex, x + (y * sizeY)] = heights2D[y, x];
                    }
                }
            }
        }

        public static NativeArray<float> LoadHeightData(Terrain terrain, Allocator allocator)
        {
            if (terrain == null)
            {
                return default;
            }
            
            var terrainData = terrain.terrainData;
            var resolution = terrainData.heightmapResolution;
            var heights2D = terrainData.GetHeights(0, 0, resolution, resolution);

            return HeightmapJobHelper.LoadHeightData(heights2D, allocator);
        }

        public static float CalculateHeightDifference(
            float3 worldPosition,
            TerrainJobData terrainData,
            int terrainIndex,
            NativeKeyArray2D<int, float> heights)
        {
            var height = GetWorldSpaceHeight(
                worldPosition,
                terrainData.terrainPosition,
                terrainIndex,
                heights,
                terrainData.resolution,
                terrainData.scale
            );

            var diff = worldPosition.y - height;

            return diff;
        }

        public static float CalculateHeightDifference(float3 worldPosition, TerrainJobData terrainData, NativeArray<float> heights)
        {
            var height = HeightmapJobHelper.GetWorldSpaceHeight(worldPosition, terrainData.terrainPosition, heights, terrainData.resolution, terrainData.resolution, terrainData.scale);

            var diff = worldPosition.y - height;

            return diff;
        }
        
        public static float3 GetTerrainNormal(this TerrainMetadata terrain, Vector3 worldPosition)
        {
            return HeightmapJobHelper.GetHeightmapNormal(worldPosition, terrain.terrainPosition, terrain.heights, terrain.resolution, terrain.resolution, terrain.scale);
        }

        public static float3 GetTerrainNormal(float3 worldPosition, TerrainJobData jobData, int terrainIndex, NativeKeyArray2D<int, float> heights)
        {
            return GetTerrainNormal(worldPosition, jobData.terrainPosition, terrainIndex, heights, jobData.resolution, jobData.scale);
        }

        public static float3 GetTerrainNormal(float3 worldPosition, TerrainJobData jobData, NativeArray<float> heights)
        {
            return HeightmapJobHelper.GetHeightmapNormal(worldPosition, jobData.terrainPosition, heights, jobData.resolution, jobData.resolution, jobData.scale);
        }

        public static float3 GetTerrainNormal(
            float3 worldPosition,
            float3 terrainPosition,
            int terrainIndex,
            NativeKeyArray2D<int, float> heights,
            int heightmapResolution,
            float3 heightmapScale)
        {
            var terrainRelative = worldPosition - terrainPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= heightmapScale.x;
            y /= heightmapScale.z;

            x = math.clamp(x, 0, heightmapResolution - 1);
            y = math.clamp(y, 0, heightmapResolution - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = CalculateNormalSobel(terrainIndex, heights, xCoord + 0, yCoord + 0, heightmapResolution, heightmapScale);
            var value10 = CalculateNormalSobel(terrainIndex, heights, xCoord + 1, yCoord + 0, heightmapResolution, heightmapScale);
            var value01 = CalculateNormalSobel(terrainIndex, heights, xCoord + 0, yCoord + 1, heightmapResolution, heightmapScale);
            var value11 = CalculateNormalSobel(terrainIndex, heights, xCoord + 1, yCoord + 1, heightmapResolution, heightmapScale);

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return math.normalizesafe(value);
        }

        public static float GetWorldSpaceHeight(this TerrainMetadata terrain, Vector3 worldPosition)
        {
            return HeightmapJobHelper.GetWorldSpaceHeight(worldPosition, terrain.terrainPosition, terrain.heights, terrain.resolution, terrain.resolution, terrain.scale);
        }

        public static float GetWorldSpaceHeight(
            Vector3 worldPosition,
            Vector3 terrainPosition,
            int terrainIndex,
            NativeKeyArray2D<int, float> heights,
            int heightmapResolution,
            float3 heightmapScale)
        {
            var terrainRelative = worldPosition - terrainPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= heightmapScale.x;
            y /= heightmapScale.z;

            x = math.clamp(x, 0, heightmapResolution - 1);
            y = math.clamp(y, 0, heightmapResolution - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = SampleHeightmap(terrainIndex, heights, xCoord + 0, yCoord + 0, heightmapResolution, heightmapScale);
            var value10 = SampleHeightmap(terrainIndex, heights, xCoord + 1, yCoord + 0, heightmapResolution, heightmapScale);
            var value01 = SampleHeightmap(terrainIndex, heights, xCoord + 0, yCoord + 1, heightmapResolution, heightmapScale);
            var value11 = SampleHeightmap(terrainIndex, heights, xCoord + 1, yCoord + 1, heightmapResolution, heightmapScale);

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return value + terrainPosition.y;
        }
        
        private static float3 CalculateNormalSobel(
            int terrainIndex,
            NativeKeyArray2D<int, float> heights,
            int x,
            int y,
            int heightmapResolution,
            float3 heightmapScale)
        {
            float3 normal;
            var dX = SampleHeightmap(terrainIndex, heights, x - 1, y - 1, heightmapResolution, heightmapScale) * -1.0F;
            dX += SampleHeightmap(terrainIndex, heights, x - 1, y,     heightmapResolution, heightmapScale) * -2.0F;
            dX += SampleHeightmap(terrainIndex, heights, x - 1, y + 1, heightmapResolution, heightmapScale) * -1.0F;
            dX += SampleHeightmap(terrainIndex, heights, x + 1, y - 1, heightmapResolution, heightmapScale) * 1.0F;
            dX += SampleHeightmap(terrainIndex, heights, x + 1, y,     heightmapResolution, heightmapScale) * 2.0F;
            dX += SampleHeightmap(terrainIndex, heights, x + 1, y + 1, heightmapResolution, heightmapScale) * 1.0F;

            dX /= heightmapScale.x;

            var dY = SampleHeightmap(terrainIndex, heights, x - 1, y - 1, heightmapResolution, heightmapScale) * -1.0F;
            dY += SampleHeightmap(terrainIndex, heights, x,     y - 1, heightmapResolution, heightmapScale) * -2.0F;
            dY += SampleHeightmap(terrainIndex, heights, x + 1, y - 1, heightmapResolution, heightmapScale) * -1.0F;
            dY += SampleHeightmap(terrainIndex, heights, x - 1, y + 1, heightmapResolution, heightmapScale) * 1.0F;
            dY += SampleHeightmap(terrainIndex, heights, x,     y + 1, heightmapResolution, heightmapScale) * 2.0F;
            dY += SampleHeightmap(terrainIndex, heights, x + 1, y + 1, heightmapResolution, heightmapScale) * 1.0F;
            dY /= heightmapScale.z;

            normal.x = -dX;
            normal.y = 8;
            normal.z = -dY;
            return math.normalize(normal);
        }
        
        public static float SampleHeightmap(
            int terrainIndex,
            NativeKeyArray2D<int, float> heights,
            int x,
            int y,
            int heightmapResolution,
            float3 heightmapScale)
        {
            x = math.clamp(x, 0, heightmapResolution - 1);
            y = math.clamp(y, 0, heightmapResolution - 1);
            return heights[terrainIndex, (y * heightmapResolution) + x] * heightmapScale.y;
        }
    }
}
