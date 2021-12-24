#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Collections.Extensions;
using Appalachia.Core.Collections.Native;
using Appalachia.Jobs.Optimization.Metadata;
using Appalachia.Jobs.Optimization.Parameters;
using Appalachia.Spatial.MeshBurial.State;
using Appalachia.Spatial.SpatialKeys;
using Appalachia.Spatial.Terrains;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    [Serializable]
    public class MeshBurialInstanceData
    {
        private const string _PRF_PFX = nameof(MeshBurialInstanceData) + ".";

        private const int _BASE_CAPACITY = 8192;
        private const Allocator alloc = Allocator.Persistent;

        private static readonly ProfilerMarker _PRF_Dispose = new(_PRF_PFX + nameof(Dispose));

        private static readonly ProfilerMarker _PRF_Update = new(_PRF_PFX + nameof(Update));

        private static readonly ProfilerMarker _PRF_Update_Instances =
            new(_PRF_PFX + nameof(Update_Instances));

        private static readonly ProfilerMarker _PRF_Update_Results =
            new(_PRF_PFX + nameof(Update_Results));

        private static readonly ProfilerMarker _PRF_Update_Parameters =
            new(_PRF_PFX + nameof(Update_Parameters));

        private static readonly ProfilerMarker _PRF_Update_Lookups =
            new(_PRF_PFX + nameof(Update_Lookups));

        public int instanceCount;
        public int iterationCount;

        public NativeHashMap<Matrix4x4Key, MeshBurialAdjustment> adjustmentLookup;
        public NativeList<float4x4> bestMatrices;
        public NativeList<OptimizationResult> bestResults;
        public NativeList<bool> exclusions;

        public NativeArray2D<MeshBurialInstanceTracking> instances;
        public NativeList<double> lookupError;
        public NativeList<float4x4> lookupResults;

        public NativeArray3D<double> parameterSets;
        public NativeArray<ParameterSpecification> paramSpecs;
        public NativeList<float4x4> requeueableMatrices;

        public NativeArray2D<OptimizationResult> results;
        public NativeList<MeshBurialSummaryData> summaries;
        public NativeList<int> terrainHashCodes;
        public NativeKeyArray2D<int, float> terrainHeights;

        public NativeHashMap<int, TerrainJobData> terrainLookup;
        public NativeArray<TerrainJobData> terrainLookup_Values;

        private int inst => instanceCount;
        private int iter => iterationCount;

        public void Dispose()
        {
            using (_PRF_Dispose.Auto())
            {
                //if (instanceCollection != null)
                instances.SafeDispose();
                paramSpecs.SafeDispose();
                parameterSets.SafeDispose();
                results.SafeDispose();
                bestMatrices.SafeDispose();
                requeueableMatrices.SafeDispose();
                bestResults.SafeDispose();
                terrainLookup_Values.SafeDispose();
                terrainHashCodes.SafeDispose();
                lookupResults.SafeDispose();
                lookupError.SafeDispose();
                exclusions.SafeDispose();
                summaries.SafeDispose();
            }

            adjustmentLookup = default;
        }

        public void Update(
            double degreeAdjustment,
            int instCount,
            int iterations,
            MeshBurialAdjustmentState adjustmentState)
        {
            using (_PRF_Update.Auto())
            {
                instanceCount = instCount;
                iterationCount = iterations;

                Update_Instances();

                Update_Results();

                Update_Parameters(degreeAdjustment);

                Update_Lookups(adjustmentState);
            }
        }

        private void Update_Instances()
        {
            using (_PRF_Update_Instances.Auto())
            {
                instances.EnsureCapacityAndLength(inst, iter);
            }
        }

        private void Update_Results()
        {
            using (_PRF_Update_Results.Auto())
            {
                results.EnsureCapacityAndLength(inst, iter);
                bestResults.EnsureCapacityAndLength(inst);
                bestMatrices.EnsureCapacityAndLength(inst);
                requeueableMatrices.EnsureCapacityAndLength(inst);
                exclusions.EnsureCapacityAndLength(inst);
                summaries.EnsureCapacityAndLength(1);
            }
        }

        private void Update_Parameters(double degAdjust)
        {
            using (_PRF_Update_Parameters.Auto())
            {
                if (paramSpecs.ShouldAllocate())
                {
                    paramSpecs = new NativeArray<ParameterSpecification>(2, alloc);
                }

                paramSpecs[0] = new ParameterSpecification(
                    TransformType.Linear,
                    ParameterType.Continuous,
                    -degAdjust,
                    degAdjust
                );
                paramSpecs[1] = paramSpecs[0];

                parameterSets.EnsureCapacityAndLength(inst, iter, paramSpecs.Length);
            }
        }

        private void Update_Lookups(MeshBurialAdjustmentState adjustmentState)
        {
            using (_PRF_Update_Lookups.Auto())
            {
                adjustmentLookup = adjustmentState.GetNative();

                lookupResults.EnsureCapacityAndLength(inst);
                lookupError.EnsureCapacityAndLength(inst);
                terrainHashCodes.EnsureCapacityAndLength(inst);

                terrainLookup = TerrainMetadataManager.instance.GetNativeMetadata();

                if (terrainLookup_Values.ShouldAllocate())
                {
                    terrainLookup_Values.SafeDispose();
                    terrainLookup_Values = terrainLookup.GetValueArray(alloc);
                }

                terrainHeights = TerrainMetadataManager.instance.GetNativeHeights();
            }
        }
    }
}

#endif