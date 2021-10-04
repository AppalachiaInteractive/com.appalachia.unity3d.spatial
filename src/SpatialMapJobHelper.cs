#region

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial
{
    public static class SpatialMapJobHelper
    {
        public static float3 GetMapScale(Bounds bounds, int width, int height)
        {
            var scale = float3.zero;

            scale.x = bounds.size.x / (width - 1);
            scale.y = bounds.size.y;
            scale.z = bounds.size.z / (height - 1);

            return scale;
        }

        public static NativeArray<float4> LoadMapData(Texture2D mapTexture, Allocator allocator)
        {
            var samples = new float4[mapTexture.width, mapTexture.height];

            for (var y = 0; y < mapTexture.height; y++)
            {
                for (var x = 0; x < mapTexture.width; x++)
                {
                    var color = mapTexture.GetPixel(x, y);

                    samples[x, y] = new float4(color.r, color.g, color.b, color.a);
                }
            }

            return LoadMapData(samples, allocator);
        }

        public static NativeArray<float4> LoadMapData(float4[,] mapData, Allocator allocator)
        {
            var values = MirrorAndFlatten(mapData, allocator);

            return values;
        }

        public static NativeArray<T> MirrorAndFlatten<T>(T[,] array2D, Allocator allocator)
            where T : struct
        {
            var resultArray1D = new NativeArray<T>(
                array2D.GetLength(0) * array2D.GetLength(1),
                allocator
            );

            for (var y = 0; y < array2D.GetLength(0); y++)
            {
                for (var x = 0; x < array2D.GetLength(1); x++)
                {
                    resultArray1D[x + (y * array2D.GetLength(0))] = array2D[y, x];
                }
            }

            return resultArray1D;
        }

        public static float4 CalculateDifference(
            float3 worldPosition,
            float3 mapPosition,
            NativeArray<float4> values,
            int mapWidth,
            int mapHeight,
            float3 mapScale)
        {
            var height = GetWorldSpaceValue(
                worldPosition,
                mapPosition,
                values,
                mapWidth,
                mapHeight,
                mapScale
            );

            var diff = worldPosition.y - height;

            return diff;
        }

        public static float4 CalculateDifference(
            float3 worldPosition,
            float3 mapPosition,
            float4[] values,
            int mapWidth,
            int mapHeight,
            float3 mapScale)
        {
            var height = GetWorldSpaceValue(
                worldPosition,
                mapPosition,
                values,
                mapWidth,
                mapHeight,
                mapScale
            );

            var diff = worldPosition.y - height;

            return diff;
        }

        public static float4 GetWorldSpaceValue(
            float3 worldPosition,
            float3 mapPosition,
            NativeArray<float4> values,
            int mapWidth,
            int mapHeight,
            float3 mapScale)
        {
            var terrainRelative = worldPosition - mapPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= mapScale.x;
            y /= mapScale.z;

            x = math.clamp(x, 0, mapWidth - 1);
            y = math.clamp(y, 0, mapHeight - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = SampleMap(
                values,
                xCoord + 0,
                yCoord + 0,
                mapWidth,
                mapHeight,
                mapScale.y
            );
            var value10 = SampleMap(
                values,
                xCoord + 1,
                yCoord + 0,
                mapWidth,
                mapHeight,
                mapScale.y
            );
            var value01 = SampleMap(
                values,
                xCoord + 0,
                yCoord + 1,
                mapWidth,
                mapHeight,
                mapScale.y
            );
            var value11 = SampleMap(
                values,
                xCoord + 1,
                yCoord + 1,
                mapWidth,
                mapHeight,
                mapScale.y
            );

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return value + mapPosition.y;
        }

        public static float4 GetWorldSpaceValue(
            float3 worldPosition,
            float3 mapPosition,
            float4[] values,
            int mapWidth,
            int mapHeight,
            float3 mapScale)
        {
            var terrainRelative = worldPosition - mapPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= mapScale.x;
            y /= mapScale.z;

            x = math.clamp(x, 0, mapWidth - 1);
            y = math.clamp(y, 0, mapHeight - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = SampleMap(
                values,
                xCoord + 0,
                yCoord + 0,
                mapWidth,
                mapHeight,
                mapScale.y
            );
            var value10 = SampleMap(
                values,
                xCoord + 1,
                yCoord + 0,
                mapWidth,
                mapHeight,
                mapScale.y
            );
            var value01 = SampleMap(
                values,
                xCoord + 0,
                yCoord + 1,
                mapWidth,
                mapHeight,
                mapScale.y
            );
            var value11 = SampleMap(
                values,
                xCoord + 1,
                yCoord + 1,
                mapWidth,
                mapHeight,
                mapScale.y
            );

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return value + mapPosition.y;
        }

        public static float4 SampleMap(
            NativeArray<float4> values,
            int x,
            int y,
            int mapWidth,
            int mapHeight,
            float mapYScale)
        {
            x = math.clamp(x, 0, mapWidth - 1);
            y = math.clamp(y, 0, mapHeight - 1);
            return values[(y * mapWidth) + x] * mapYScale;
        }

        public static float4 SampleMap(
            float4[] values,
            int x,
            int y,
            int mapWidth,
            int mapHeight,
            float mapYScale)
        {
            x = math.clamp(x, 0, mapWidth - 1);
            y = math.clamp(y, 0, mapHeight - 1);
            return values[(y * mapWidth) + x] * mapYScale;
        }
    }
}
