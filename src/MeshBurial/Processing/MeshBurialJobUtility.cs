#region

using Appalachia.Core.Collections.Native;
using Appalachia.Core.Extensions;
using Appalachia.Core.MeshData;
using Appalachia.Core.Terrains;
using Appalachia.Core.Terrains.Utilities;
using Appalachia.Utility.Constants;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    [BurstCompile]
    public struct MeshBurialJobUtility
    {
        public static float4x4 ProposeMatrixAdjustment(
            MeshBurialOptions burialOptions,
            MeshObject meshObject,
            float4x4 ltw,
            TerrainJobData terrainData,
            int terrainIndex,
            NativeKeyArray2D<int, float> terrainHeights,
            NativeArray3D<double> parameterSets,
            int instanceIndex,
            int iterationIndex)
        {
            if (burialOptions.matchTerrainNormal)
            {
                ltw = math.mul(ltw, MatchTerrainNormal(ltw, terrainData, terrainIndex, terrainHeights));
            }

            if (burialOptions.accountForMeshNormal)
            {
                ltw = math.mul(ltw, AccountForMeshNormal(meshObject));
            }

            if (burialOptions.applyParameters)
            {
                ltw = math.mul(ltw, AdjustForParameters(parameterSets, instanceIndex, iterationIndex));
            }

            if (burialOptions.adjustHeight)
            {
                ltw = math.mul(ltw, AdjustForBurialMovement(ltw, meshObject, terrainData, terrainIndex, terrainHeights));
            }

            if (burialOptions.applyTestValue)
            {
                ltw = math.mul(ltw, GetTestAdjustment(burialOptions));
            }

            return ltw;
        }

        public static float4x4 AccountForMeshNormal(MeshObject meshObject)
        {
            var adjustment = meshObject.BorderNormal.fromToRotation(float3c.up, true);

            return new float4x4(adjustment, float3.zero);
        }

        public static float4x4 MatchTerrainNormal(
            float4x4 localToWorldMatrix,
            TerrainJobData terrainData,
            int terrainIndex,
            NativeKeyArray2D<int, float> heights)
        {
            var position_W = localToWorldMatrix.GetPositionFromMatrix();
            var terrainN_W = TerrainJobHelper.GetTerrainNormal(position_W, terrainData, terrainIndex, heights);
            var wtl = localToWorldMatrix.Inverse();
            var terrainN_L = wtl.MultiplyVector(terrainN_W);

            var adjustment = float3c.up.fromToRotation(terrainN_L, true);

            return new float4x4(adjustment, float3.zero);
        }

        public static float4x4 MatchTerrainNormal(float4x4 localToWorldMatrix, TerrainJobData terrainData, NativeArray<float> heights)
        {
            var position_W = localToWorldMatrix.GetPositionFromMatrix();
            var terrainN_W = TerrainJobHelper.GetTerrainNormal(position_W, terrainData, heights);
            var wtl = localToWorldMatrix.Inverse();
            var terrainN_L = wtl.MultiplyVector(terrainN_W);

            var adjustment = float3c.up.fromToRotation(terrainN_L, true);

            return new float4x4(adjustment, float3.zero);
        }

        public static float4x4 AdjustForParameters(NativeArray3D<double> parameterSets, int instanceIndex, int iterationIndex)
        {
            var x = (float) parameterSets[instanceIndex, iterationIndex, 0];
            var z = (float) parameterSets[instanceIndex, iterationIndex, 1];

            var adjustment = new float3(x, 0f, z);
            var adjustmentRads = math.radians(adjustment);
            var rotation = quaternion.EulerZXY(adjustmentRads);

            return new float4x4(rotation, float3.zero);
        }

        public static float4x4 AdjustForBurialMovement(
            float4x4 localToWorldMatrix,
            MeshObject meshObject,
            TerrainJobData terrainData,
            int terrainIndex,
            NativeKeyArray2D<int, float> terrainheights)
        {
            var maximumOffset = -1024.0f;

            for (var i = 0; i < meshObject.edges.Length; i++)
            {
                var edge = meshObject.edges[i];

                if (edge.triangleCount != 1)
                {
                    continue;
                }

                var vertexA = meshObject.vertices[edge.aIndex];

                var positionA_W = localToWorldMatrix.MultiplyPoint3x4(vertexA.position);

                var heightDifferenceA = TerrainJobHelper.CalculateHeightDifference(positionA_W, terrainData, terrainIndex, terrainheights);

                maximumOffset = math.max(heightDifferenceA, maximumOffset);
            }

            var movement_W = new float3(0, -maximumOffset - .005f, 0);

            var worldToLocalMatrix = localToWorldMatrix.Inverse();

            var localOffset = worldToLocalMatrix.MultiplyVector(movement_W);

            return float4x4.Translate(localOffset);
        }

        public static float4x4 AdjustForBurialMovement(
            float4x4 localToWorldMatrix,
            MeshObject meshObject,
            TerrainJobData terrainData,
            NativeArray<float> heights)
        {
            var maximumOffset = -1024.0f;

            for (var i = 0; i < meshObject.edges.Length; i++)
            {
                var edge = meshObject.edges[i];

                if (edge.triangleCount != 1)
                {
                    continue;
                }

                var vertexA = meshObject.vertices[edge.aIndex];

                var positionA_W = localToWorldMatrix.MultiplyPoint3x4(vertexA.position);

                var heightDifferenceA = TerrainJobHelper.CalculateHeightDifference(positionA_W, terrainData, heights);

                maximumOffset = math.max(heightDifferenceA, maximumOffset);
            }

            var movement_W = new float3(0, -maximumOffset - .005f, 0);

            var worldToLocalMatrix = localToWorldMatrix.Inverse();

            var localOffset = worldToLocalMatrix.MultiplyVector(movement_W);

            return float4x4.Translate(localOffset);
        }

        public static float4x4 GetTestAdjustment(MeshBurialOptions burialOptions)
        {
            var testValue = burialOptions.testValue;

            return testValue;
        }

        public static double CalculateError(
            MeshObject meshObject,
            float4x4 localToWorldMatrix,
            TerrainJobData terrainData,
            int terrainIndex,
            NativeKeyArray2D<int, float> heights)
        {
            if (localToWorldMatrix.Equals(float4x4.zero))
            {
                return 1.0;
            }

            var borderErrors = 0;
            var internalTests = 0;
            var internalErrors = 0;

            //var worldToLocalMatrix = math.inverse(localToWorldMatrix);

            for (var i = 0; i < meshObject.edges.Length; i++)
            {
                var edge = meshObject.edges[i];

                var vertexA = meshObject.vertices[edge.aIndex];

                var positionA_W = localToWorldMatrix.MultiplyPoint3x4(vertexA.position);
                var heightDifferenceA = TerrainJobHelper.CalculateHeightDifference(positionA_W, terrainData, terrainIndex, heights);

                if (edge.triangleCount == 1)
                {
                    if (heightDifferenceA > 0f)
                    {
                        borderErrors += 1;
                    }
                }
                else
                {
                    internalTests += 1;
                    if (heightDifferenceA < 0f)
                    {
                        internalErrors += 1;
                    }
                }
            }

            if (borderErrors > 0)
            {
                return 1.0;
            }

            if (internalErrors == internalTests)
            {
                return 1.0;
            }

            var error = math.clamp(internalErrors / (double) internalTests, 0.0, 1.0);
            return error;
        }
    }
}
