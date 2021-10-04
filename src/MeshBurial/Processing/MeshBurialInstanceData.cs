#region

using System;
using Appalachia.Base.Behaviours;
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
    public class MeshBurialInstanceData : InternalBase<MeshBurialInstanceData>
    {
        private const string _PRF_PFX = nameof(MeshBurialInstanceData) + ".";
        
        private const int _BASE_CAPACITY = 8192;
        private const Allocator alloc = Allocator.Persistent;

        public NativeArray2D<MeshBurialInstanceTracking> instances;

        public NativeArray3D<double> parameterSets;
        public NativeArray<ParameterSpecification> paramSpecs;

        public NativeArray2D<OptimizationResult> results;
        public NativeList<OptimizationResult> bestResults;
        public NativeList<float4x4> bestMatrices;
        public NativeList<float4x4> requeueableMatrices;
        public NativeList<bool> exclusions;
        public NativeList<MeshBurialSummaryData> summaries;

        public NativeHashMap<Matrix4x4Key, MeshBurialAdjustment> adjustmentLookup;
        public NativeList<float4x4> lookupResults;
        public NativeList<double> lookupError;

        public NativeHashMap<int, TerrainJobData> terrainLookup;
        public NativeKeyArray2D<int, float> terrainHeights;
        public NativeArray<TerrainJobData> terrainLookup_Values;
        public NativeList<int> terrainHashCodes;

        public int instanceCount;
        public int iterationCount;

        private int inst => instanceCount;
        private int iter => iterationCount;

        private static readonly ProfilerMarker _PRF_Dispose = new ProfilerMarker(_PRF_PFX + nameof(Dispose));
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

        private static readonly ProfilerMarker _PRF_Update = new ProfilerMarker(_PRF_PFX + nameof(Update));
        public void Update(double degreeAdjustment, int instCount, int iterations, MeshBurialAdjustmentState adjustmentState)
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

        private static readonly ProfilerMarker _PRF_Update_Instances = new ProfilerMarker(_PRF_PFX + nameof(Update_Instances));
        private void Update_Instances()
        {
            using (_PRF_Update_Instances.Auto())
            {
                instances.EnsureCapacityAndLength(inst, iter);
            }
        }

        private static readonly ProfilerMarker _PRF_Update_Results = new ProfilerMarker(_PRF_PFX + nameof(Update_Results));
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

        private static readonly ProfilerMarker _PRF_Update_Parameters = new ProfilerMarker(_PRF_PFX + nameof(Update_Parameters));
        
        private void Update_Parameters(double degAdjust)
        {
            using (_PRF_Update_Parameters.Auto())
            {
                if (paramSpecs.ShouldAllocate())
                {
                    paramSpecs = new NativeArray<ParameterSpecification>(2, alloc);
                }

                paramSpecs[0] = new ParameterSpecification(TransformType.Linear, ParameterType.Continuous, -degAdjust, degAdjust);
                paramSpecs[1] = paramSpecs[0];

                parameterSets.EnsureCapacityAndLength(inst, iter, paramSpecs.Length);
            }
        }

        private static readonly ProfilerMarker _PRF_Update_Lookups = new ProfilerMarker(_PRF_PFX + nameof(Update_Lookups));
        private void Update_Lookups(MeshBurialAdjustmentState adjustmentState)
        {
            using (_PRF_Update_Lookups.Auto())
            {
                adjustmentLookup = adjustmentState.GetNative();

                lookupResults.EnsureCapacityAndLength(inst);
                lookupError.EnsureCapacityAndLength(inst);
                terrainHashCodes.EnsureCapacityAndLength(inst);

                terrainLookup = TerrainMetadataManager.GetNativeMetadata();

                if (terrainLookup_Values.ShouldAllocate())
                {
                    terrainLookup_Values.SafeDispose();
                    terrainLookup_Values = terrainLookup.GetValueArray(alloc);
                }

                terrainHeights = TerrainMetadataManager.GetNativeHeights();
            }
        }
    }
}
