using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Appalachia.Spatial
{
    public static class HeightmapJobHelper
    {
        public static float3 GetHeightmapScale(Bounds bounds, int width, int height)
        {
            var scale = float3.zero;

            scale.x = bounds.size.x / (width - 1);
            scale.y = bounds.size.y;
            scale.z = bounds.size.z / (height - 1);

            return scale;
        }

        public static NativeArray<float> LoadHeightData(
            Texture2D heightmapTexture,
            Allocator allocator)
        {
            var heightmapSamples = new float[heightmapTexture.width, heightmapTexture.height];

            for (var y = 0; y < heightmapTexture.height; y++)
            {
                for (var x = 0; x < heightmapTexture.width; x++)
                {
                    heightmapSamples[x, y] = heightmapTexture.GetPixel(x, y).grayscale;
                }
            }

            return LoadHeightData(heightmapSamples, allocator);
        }

        public static NativeArray<float> LoadHeightData(float[,] heights2D, Allocator allocator)
        {
            var heights = MirrorAndFlatten(heights2D, allocator);

            return heights;
        }

        public static NativeArray<T> MirrorAndFlatten<T>(T[,] array2D, Allocator allocator)
            where T : struct
        {
            var resultArray1D = new NativeArray<T>(
                array2D.GetLength(0) * array2D.GetLength(1),
                allocator
            );

            var height = array2D.GetLength(0);
            var width = array2D.GetLength(1);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    resultArray1D[(y * height) + x] = array2D[y, x];
                }
            }

            return resultArray1D;
        }

        public static float CalculateHeightDifference(
            float3 worldPosition,
            float3 terrainPosition,
            NativeArray<float> heights,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            var height = GetWorldSpaceHeight(
                worldPosition,
                terrainPosition,
                heights,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );

            var diff = worldPosition.y - height;

            return diff;
        }

        public static float CalculateHeightDifference(
            float3 worldPosition,
            float3 terrainPosition,
            float[] heights,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            var height = GetWorldSpaceHeight(
                worldPosition,
                terrainPosition,
                heights,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );

            var diff = worldPosition.y - height;

            return diff;
        }

        public static float3 GetHeightmapNormal(
            float3 worldPosition,
            float3 terrainPosition,
            NativeArray<float> heights,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            var terrainRelative = worldPosition - terrainPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= heightmapScale.x;
            y /= heightmapScale.z;

            x = math.clamp(x, 0, heightmapWidth - 1);
            y = math.clamp(y, 0, heightmapHeight - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = CalculateNormalSobel(
                heights,
                xCoord + 0,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );
            var value10 = CalculateNormalSobel(
                heights,
                xCoord + 1,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );
            var value01 = CalculateNormalSobel(
                heights,
                xCoord + 0,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );
            var value11 = CalculateNormalSobel(
                heights,
                xCoord + 1,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return math.normalizesafe(value);
        }

        public static float3 GetHeightmapNormal(
            float3 worldPosition,
            float3 terrainPosition,
            float[] heights,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            var terrainRelative = worldPosition - terrainPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= heightmapScale.x;
            y /= heightmapScale.z;

            x = math.clamp(x, 0, heightmapWidth - 1);
            y = math.clamp(y, 0, heightmapHeight - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = CalculateNormalSobel(
                heights,
                xCoord + 0,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );
            var value10 = CalculateNormalSobel(
                heights,
                xCoord + 1,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );
            var value01 = CalculateNormalSobel(
                heights,
                xCoord + 0,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );
            var value11 = CalculateNormalSobel(
                heights,
                xCoord + 1,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale
            );

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return math.normalizesafe(value);
        }

        public static float GetWorldSpaceHeight(
            float3 worldPosition,
            float3 terrainPosition,
            NativeArray<float> heights,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            var terrainRelative = worldPosition - terrainPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= heightmapScale.x;
            y /= heightmapScale.z;

            x = math.clamp(x, 0, heightmapWidth - 1);
            y = math.clamp(y, 0, heightmapHeight - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = SampleHeightmap(
                heights,
                xCoord + 0,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );
            var value10 = SampleHeightmap(
                heights,
                xCoord + 1,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );
            var value01 = SampleHeightmap(
                heights,
                xCoord + 0,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );
            var value11 = SampleHeightmap(
                heights,
                xCoord + 1,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return value + terrainPosition.y;
        }

        public static float GetWorldSpaceHeight(
            float3 worldPosition,
            float3 terrainPosition,
            float[] heights,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            var terrainRelative = worldPosition - terrainPosition;
            var x = terrainRelative.x;
            var y = terrainRelative.z;

            x /= heightmapScale.x;
            y /= heightmapScale.z;

            x = math.clamp(x, 0, heightmapWidth - 1);
            y = math.clamp(y, 0, heightmapHeight - 1);

            var xCoord = (int) x;
            var yCoord = (int) y;

            var value00 = SampleHeightmap(
                heights,
                xCoord + 0,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );
            var value10 = SampleHeightmap(
                heights,
                xCoord + 1,
                yCoord + 0,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );
            var value01 = SampleHeightmap(
                heights,
                xCoord + 0,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );
            var value11 = SampleHeightmap(
                heights,
                xCoord + 1,
                yCoord + 1,
                heightmapWidth,
                heightmapHeight,
                heightmapScale.y
            );

            var xBlend = x - xCoord;
            var yBlend = y - yCoord;

            var blendA = math.lerp(value00, value10, xBlend);
            var blendB = math.lerp(value01, value11, xBlend);

            var value = math.lerp(blendA, blendB, yBlend);

            return value + terrainPosition.y;
        }

        private static float3 CalculateNormalSobel(
            NativeArray<float> heights,
            int x,
            int y,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            float3 normal;
            var dX = SampleHeightmap(
                         heights,
                         x - 1,
                         y - 1,
                         heightmapWidth,
                         heightmapHeight,
                         heightmapScale.y
                     ) *
                     -1.0F;
            dX += SampleHeightmap(
                      heights,
                      x - 1,
                      y,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -2.0F;
            dX += SampleHeightmap(
                      heights,
                      x - 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -1.0F;
            dX += SampleHeightmap(
                      heights,
                      x + 1,
                      y - 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;
            dX += SampleHeightmap(
                      heights,
                      x + 1,
                      y,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  2.0F;
            dX += SampleHeightmap(
                      heights,
                      x + 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;

            dX /= heightmapScale.x;

            var dY = SampleHeightmap(
                         heights,
                         x - 1,
                         y - 1,
                         heightmapWidth,
                         heightmapHeight,
                         heightmapScale.y
                     ) *
                     -1.0F;
            dY += SampleHeightmap(
                      heights,
                      x,
                      y - 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -2.0F;
            dY += SampleHeightmap(
                      heights,
                      x + 1,
                      y - 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -1.0F;
            dY += SampleHeightmap(
                      heights,
                      x - 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;
            dY += SampleHeightmap(
                      heights,
                      x,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  2.0F;
            dY += SampleHeightmap(
                      heights,
                      x + 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;
            dY /= heightmapScale.z;

            normal.x = -dX;
            normal.y = 8;
            normal.z = -dY;
            return math.normalize(normal);
        }

        private static float3 CalculateNormalSobel(
            float[] heights,
            int x,
            int y,
            int heightmapWidth,
            int heightmapHeight,
            float3 heightmapScale)
        {
            float3 normal;
            var dX = SampleHeightmap(
                         heights,
                         x - 1,
                         y - 1,
                         heightmapWidth,
                         heightmapHeight,
                         heightmapScale.y
                     ) *
                     -1.0F;
            dX += SampleHeightmap(
                      heights,
                      x - 1,
                      y,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -2.0F;
            dX += SampleHeightmap(
                      heights,
                      x - 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -1.0F;
            dX += SampleHeightmap(
                      heights,
                      x + 1,
                      y - 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;
            dX += SampleHeightmap(
                      heights,
                      x + 1,
                      y,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  2.0F;
            dX += SampleHeightmap(
                      heights,
                      x + 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;

            dX /= heightmapScale.x;

            var dY = SampleHeightmap(
                         heights,
                         x - 1,
                         y - 1,
                         heightmapWidth,
                         heightmapHeight,
                         heightmapScale.y
                     ) *
                     -1.0F;
            dY += SampleHeightmap(
                      heights,
                      x,
                      y - 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -2.0F;
            dY += SampleHeightmap(
                      heights,
                      x + 1,
                      y - 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  -1.0F;
            dY += SampleHeightmap(
                      heights,
                      x - 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;
            dY += SampleHeightmap(
                      heights,
                      x,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  2.0F;
            dY += SampleHeightmap(
                      heights,
                      x + 1,
                      y + 1,
                      heightmapWidth,
                      heightmapHeight,
                      heightmapScale.y
                  ) *
                  1.0F;
            dY /= heightmapScale.z;

            normal.x = -dX;
            normal.y = 8;
            normal.z = -dY;
            return math.normalize(normal);
        }

        public static float SampleHeightmap(
            NativeArray<float> heights,
            int x,
            int y,
            int heightmapWidth,
            int heightmapHeight,
            float heightmapYScale)
        {
            x = math.clamp(x, 0, heightmapWidth - 1);
            y = math.clamp(y, 0, heightmapHeight - 1);
            return heights[(y * heightmapHeight) + x] * heightmapYScale;
        }

        public static float SampleHeightmap(
            float[] heights,
            int x,
            int y,
            int heightmapWidth,
            int heightmapHeight,
            float heightmapYScale)
        {
            x = math.clamp(x, 0, heightmapWidth - 1);
            y = math.clamp(y, 0, heightmapHeight - 1);
            return heights[(y * heightmapHeight) + x] * heightmapYScale;
        }
    }
}
