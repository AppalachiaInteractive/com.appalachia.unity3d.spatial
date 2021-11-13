#if UNITY_EDITOR

#region

using Appalachia.CI.Constants;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Extensions;
using Appalachia.Jobs.Burstable;
using Appalachia.Jobs.MeshData;
using Appalachia.Jobs.Optimization;
using Appalachia.Jobs.Optimization.Metadata;
using Appalachia.Jobs.Optimization.Options;
using Appalachia.Jobs.Optimization.Utilities;
using Appalachia.Spatial.MeshBurial.State;
using Appalachia.Spatial.SpatialKeys;
using Appalachia.Spatial.Terrains;
using Appalachia.Utility.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public static class MeshBurialJobManager
    {
        public const int INNER_LOOP = 8;

        private const string _PRF_PFX = nameof(MeshBurialJobManager) + ".";

        private static readonly ProfilerMarker _PRF_ScheduleMeshBurialJobs =
            new(_PRF_PFX + nameof(ScheduleMeshBurialJobs));

        private static readonly ProfilerMarker _PRF_ScheduleMeshBurialJobs_UpdateData =
            new(_PRF_PFX + nameof(ScheduleMeshBurialJobs) + ".UpdateData");

        private static readonly ProfilerMarker _PRF_ScheduleMeshBurialJobs_SetupJobs =
            new(_PRF_PFX + nameof(ScheduleMeshBurialJobs) + ".SetupJobs");

        private static readonly ProfilerMarker
            _PRF_ScheduleMeshBurialJobs_ScheduleOptimizationJobs = new(_PRF_PFX +
                                                                       nameof(ScheduleMeshBurialJobs) +
                                                                       ".ScheduleOptimizationJobs");

        private static readonly ProfilerMarker _PRF_ScheduleMeshBurialJobs_ScheduleJobs =
            new(_PRF_PFX + nameof(ScheduleMeshBurialJobs) + ".ScheduleJobs");

        public static JobHandle ScheduleMeshBurialJobs(
            MeshBurialInstanceData resultData,
            MeshObject meshObject,
            MeshBurialAdjustmentState adjustmentState,
            NativeArray<float4x4> matrices,
            MeshBurialOptimizationParameters ops,
            OptimizationOptions optimizationOptions,
            MeshBurialOptions burialOptions,
            float degreeAdjustment,
            JobRandoms randoms,
            NativeList<JobHandle> dependencyList,
            JobHandle dependencies = default)
        {
            using (_PRF_ScheduleMeshBurialJobs.Auto())
            {
                var instanceCount = matrices.Length;
                var iterationCount = optimizationOptions.randomSearch.iterations;

                SafeNative.SafeClear(ref dependencyList);

                using (_PRF_ScheduleMeshBurialJobs_UpdateData.Auto())
                {
                    resultData.Update(
                        degreeAdjustment,
                        instanceCount,
                        iterationCount,
                        adjustmentState
                    );
                }

                using (_PRF_ScheduleMeshBurialJobs_SetupJobs.Auto())
                {
                    var lookup = resultData.adjustmentLookup;

                    dependencies = new PopulateLookupsJob
                    {
                        lookup = lookup,
                        lookupError = resultData.lookupError,
                        lookupResults = resultData.lookupResults,
                        matrices = matrices,
                        meshObject = meshObject,
                        terrainHashCodes = resultData.terrainHashCodes,
                        terrainHeights = resultData.terrainHeights,
                        terrainLookup_Values = resultData.terrainLookup_Values
                    }.Schedule(instanceCount, INNER_LOOP, dependencies);

                    dependencies = new PrepareInstanceCalculationJob
                    {
                        burialOptions = burialOptions,
                        exclusions = resultData.exclusions,
                        instancesCollection = resultData.instances,
                        lookupError = resultData.lookupError.AsDeferredJobArray(),
                        lookupResults = resultData.lookupResults.AsDeferredJobArray(),
                        matrices = matrices,
                        meshObject = meshObject,
                        terrainHashCodes = resultData.terrainHashCodes.AsDeferredJobArray(),
                        terrainHeights = resultData.terrainHeights,
                        terrainLookup = resultData.terrainLookup
                    }.Schedule(instanceCount, INNER_LOOP, dependencies);
                }

                using (_PRF_ScheduleMeshBurialJobs_ScheduleOptimizationJobs.Auto())
                {
                    dependencies = Optimizer.ScheduleOptimizationJobs(
                        resultData.paramSpecs,
                        resultData.parameterSets,
                        resultData.results,
                        resultData.bestResults,
                        randoms,
                        dependencies,
                        (deps, parameterSets, results) => new MeshBurialJob
                        {
                            burialOptions = burialOptions,
                            exclusions = resultData.exclusions.AsDeferredJobArray(),
                            instancesCollection = resultData.instances,
                            meshObject = meshObject,
                            parameterSets = parameterSets,
                            resultsCollection = results,
                            terrainHashCodes = resultData.terrainHashCodes,
                            terrainHeights = resultData.terrainHeights,
                            terrainLookup = resultData.terrainLookup
                        }.Schedule(instanceCount, INNER_LOOP, deps)
                    );
                }

                dependencies = new ResultCleanupJob
                {
                    bestMatrices = resultData.bestMatrices,
                    bestResults = resultData.bestResults,
                    burialOptions = burialOptions,
                    instances = resultData.instances,
                    requeueResults = resultData.requeueableMatrices
                }.Schedule(instanceCount, INNER_LOOP, dependencies);

                dependencies = new ResultSummaryJob
                {
                    burialOptions = burialOptions,
                    instances = resultData.instances,
                    results = resultData.results,
                    bestResults = resultData.bestResults,
                    summaries = resultData.summaries
                }.Schedule(dependencies);

                using (_PRF_ScheduleMeshBurialJobs_ScheduleJobs.Auto())
                {
                    JobHandle.ScheduleBatchedJobs();
                }

                return dependencies;
            }
        }

        [BurstCompile]
        public struct PopulateLookupsJob : IJobParallelFor
        {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<TerrainJobData> terrainLookup_Values;

            [ReadOnly] public MeshObject meshObject;
            [ReadOnly] public NativeArray<float4x4> matrices;
            [ReadOnly] public NativeHashMap<Matrix4x4Key, MeshBurialAdjustment> lookup;
            [ReadOnly] public NativeKeyArray2D<int, float> terrainHeights;
            [WriteOnly] public NativeArray<int> terrainHashCodes;
            [WriteOnly] public NativeArray<double> lookupError;
            [WriteOnly] public NativeArray<float4x4> lookupResults;

            public void Execute(int index)
            {
                var matrix = matrices[index];

                if (matrix.Equals(float4x4.zero))
                {
                    return;
                }

                var position = matrix.GetPositionFromMatrix();

                TerrainJobData terrainData = default;

                for (var i = 0; i < terrainLookup_Values.Length; i++)
                {
                    terrainData = terrainLookup_Values[i];

                    if (terrainData.IsPositionValidForTerrain(position))
                    {
                        terrainHashCodes[index] = terrainData.hashCode;
                        break;
                    }
                }

                if (terrainData.Equals(default))
                {
                    terrainHashCodes[index] = int.MinValue;
                    lookupResults[index] = float4x4.zero;
                    lookupError[index] = 1.0;
                    return;
                }

                var spatialKey = new Matrix4x4Key(matrix, CONSTANTS.MatrixKeyGrouping);

                var terrainIndex = 0;
                for (var i = 0; i < terrainHeights.Length0; i++)
                {
                    var heightHash = terrainHeights[i];
                    if (heightHash == terrainData.hashCode)
                    {
                        terrainIndex = i;
                        break;
                    }
                }

                if (lookup.ContainsKey(spatialKey))
                {
                    var found = lookup[spatialKey];
                    var foundLTW = found.matrix;
                    var foundError = found.error;

                    if (foundError < .0000001)
                    {
                        foundError = MeshBurialJobUtility.CalculateError(
                            meshObject,
                            foundLTW,
                            terrainData,
                            terrainIndex,
                            terrainHeights
                        );
                    }

                    lookupResults[index] = foundLTW;
                    lookupError[index] = foundError;
                }
                else
                {
                    lookupResults[index] = float4x4.zero;
                    lookupError[index] = 1.0;
                }
            }
        }

        [BurstCompile]
        public struct PrepareInstanceCalculationJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<double> lookupError;
            [ReadOnly] public NativeArray<float4x4> lookupResults;

            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeKeyArray2D<int, float> terrainHeights;

            [ReadOnly] public NativeHashMap<int, TerrainJobData> terrainLookup;
            [ReadOnly] public NativeArray<int> terrainHashCodes;
            [ReadOnly] public MeshObject meshObject;
            [ReadOnly] public NativeArray<float4x4> matrices;
            [ReadOnly] public MeshBurialOptions burialOptions;

            [NativeDisableParallelForRestriction]
            public NativeArray2D<MeshBurialInstanceTracking> instancesCollection;

            [WriteOnly] public NativeArray<bool> exclusions;

            public void Execute(int instanceIndex)
            {
                var iterationCount = instancesCollection.Length1;

                var matrix = matrices[instanceIndex];
                var terrainHashCode = terrainHashCodes[instanceIndex];
                var lookupMatrix = lookupResults[instanceIndex];
                var lookupMatrixError = lookupError[instanceIndex];
                var useLookup = lookupMatrixError < burialOptions.threshold;

                if (matrix.Equals(float4x4.zero) ||
                    matrix.anyNaN() ||
                    (terrainHashCode == int.MinValue))
                {
                    exclusions[instanceIndex] = true;

                    for (var iterationIndex = 0; iterationIndex < iterationCount; iterationIndex++)
                    {
                        var iteration = instancesCollection[instanceIndex, iterationIndex];

                        iteration.initial.matrix = matrix;
                        iteration.initial.error = 1.0;
                        iteration.proposed.matrix = float4x4.zero;
                        iteration.proposed.error = 1.0;
                        iteration.excluded = true;

                        instancesCollection[instanceIndex, iterationIndex] = iteration;
                    }

                    return;
                }

                exclusions[instanceIndex] = useLookup;

                for (var iterationIndex = 0; iterationIndex < iterationCount; iterationIndex++)
                {
                    var iteration = instancesCollection[instanceIndex, iterationIndex];

                    iteration.initial.matrix = useLookup ? lookupMatrix : matrix;
                    iteration.initial.error = useLookup ? lookupMatrixError : 0.0;
                    iteration.proposed.matrix = useLookup ? lookupMatrix : float4x4.zero;
                    iteration.proposed.error = useLookup ? lookupMatrixError : 1.0;
                    iteration.excluded = useLookup;

                    instancesCollection[instanceIndex, iterationIndex] = iteration;
                }

                var mainInstance = instancesCollection[instanceIndex, 0];

                if (!useLookup && (mainInstance.initial.error < 0.000001))
                {
                    var terrainData = terrainLookup[terrainHashCode];

                    var terrainIndex = 0;
                    for (var i = 0; i < terrainHeights.Length0; i++)
                    {
                        var heightHash = terrainHeights[i];

                        if (heightHash == terrainData.hashCode)
                        {
                            terrainIndex = i;
                            break;
                        }
                    }

                    mainInstance.initial.error = MeshBurialJobUtility.CalculateError(
                        meshObject,
                        matrix,
                        terrainData,
                        terrainIndex,
                        terrainHeights
                    );

                    for (var iterationIndex = 1; iterationIndex < iterationCount; iterationIndex++)
                    {
                        var iteration = instancesCollection[instanceIndex, iterationIndex];

                        iteration.initial.error = mainInstance.initial.error;

                        instancesCollection[instanceIndex, iterationIndex] = iteration;
                    }
                }

                if (mainInstance.initial.error < burialOptions.threshold)
                {
                    exclusions[instanceIndex] = useLookup;

                    for (var iterationIndex = 1; iterationIndex < iterationCount; iterationIndex++)
                    {
                        var iteration = instancesCollection[instanceIndex, iterationIndex];

                        iteration.proposed = mainInstance.initial;
                        iteration.proposed.error = mainInstance.initial.error;
                        iteration.excluded = true;

                        instancesCollection[instanceIndex, iterationIndex] = iteration;
                    }
                }
            }
        }

        [BurstCompile]
        public struct MeshBurialJob : IJobParallelFor
        {
            [ReadOnly] public MeshBurialOptions burialOptions;
            [ReadOnly] public MeshObject meshObject;
            [ReadOnly] public NativeArray3D<double> parameterSets;
            [ReadOnly] public NativeArray<bool> exclusions;
            [ReadOnly] public NativeArray<int> terrainHashCodes;
            [ReadOnly] public NativeHashMap<int, TerrainJobData> terrainLookup;

            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeKeyArray2D<int, float> terrainHeights;

            [NativeDisableParallelForRestriction]
            public NativeArray2D<MeshBurialInstanceTracking> instancesCollection;

            [NativeDisableParallelForRestriction]
            public NativeArray2D<OptimizationResult> resultsCollection;

            public void Execute(int instanceIndex)
            {
                var fullExcluded = exclusions[instanceIndex];
                var iterationCount = instancesCollection.Length1;

                if (fullExcluded)
                {
                    for (var iterationIndex = 0; iterationIndex < iterationCount; iterationIndex++)
                    {
                        var iteration = instancesCollection[instanceIndex, iterationIndex];

                        iteration.excluded = true;

                        resultsCollection[instanceIndex, iterationIndex] = new OptimizationResult(
                            iteration.proposed.error,
                            iterationIndex
                        );

                        instancesCollection[instanceIndex, iterationIndex] = iteration;
                    }

                    return;
                }

                var terrainHashCode = terrainHashCodes[instanceIndex];
                var terrainData = terrainLookup[terrainHashCode];

                var terrainIndex = 0;
                for (var i = 0; i < terrainHeights.Length0; i++)
                {
                    var heightHash = terrainHeights[i];

                    if (heightHash == terrainData.hashCode)
                    {
                        terrainIndex = i;
                        break;
                    }
                }

                for (var iterationIndex = 0; iterationIndex < iterationCount; iterationIndex++)
                {
                    var iteration = instancesCollection[instanceIndex, iterationIndex];

                    if (iteration.excluded)
                    {
                        resultsCollection[instanceIndex, iterationIndex] = new OptimizationResult(
                            iteration.proposed.error,
                            iterationIndex
                        );
                        continue;
                    }

                    iteration.proposed.matrix = MeshBurialJobUtility.ProposeMatrixAdjustment(
                        burialOptions,
                        meshObject,
                        iteration.initial.matrix,
                        terrainData,
                        terrainIndex,
                        terrainHeights,
                        parameterSets,
                        instanceIndex,
                        iterationIndex
                    );

                    var error = MeshBurialJobUtility.CalculateError(
                        meshObject,
                        iteration.proposed.matrix,
                        terrainData,
                        terrainIndex,
                        terrainHeights
                    );

                    iteration.proposed.error = error;

                    resultsCollection[instanceIndex, iterationIndex] =
                        new OptimizationResult(error, iterationIndex);

                    instancesCollection[instanceIndex, iterationIndex] = iteration;
                }
            }
        }

        [BurstCompile]
        public struct ResultCleanupJob : IJobParallelFor
        {
            [ReadOnly] public MeshBurialOptions burialOptions;

            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray2D<MeshBurialInstanceTracking> instances;

            [ReadOnly] public NativeArray<OptimizationResult> bestResults;
            [WriteOnly] public NativeArray<float4x4> bestMatrices;
            [WriteOnly] public NativeArray<float4x4> requeueResults;

            public void Execute(int instanceIndex)
            {
                var bestIteration = bestResults[instanceIndex];

                var bestIterationIndex = bestIteration.iterationIndex;
                var bestError = bestIteration.error;

                var bestMatrix = instances[instanceIndex, bestIterationIndex];

                if (bestError < burialOptions.threshold)
                {
                    requeueResults[instanceIndex] = bestMatrix.proposed.matrix;
                }

                bestMatrices[instanceIndex] = bestMatrix.proposed.matrix;
            }
        }

        [BurstCompile]
        public struct ResultSummaryJob : IJob
        {
            [ReadOnly] public MeshBurialOptions burialOptions;
            [ReadOnly] public NativeArray2D<MeshBurialInstanceTracking> instances;
            [ReadOnly] public NativeArray2D<OptimizationResult> results;
            [ReadOnly] public NativeArray<OptimizationResult> bestResults;

            [WriteOnly] public NativeArray<MeshBurialSummaryData> summaries;

            public void Execute()
            {
                var summary = new MeshBurialSummaryData {total = bestResults.Length};

                for (var instanceIndex = 0; instanceIndex < results.Length0; instanceIndex++)
                {
                    var bestResult = bestResults[instanceIndex];
                    var bestInstance = instances[instanceIndex, bestResult.iterationIndex];

                    summary.average += bestResult.error;

                    if (bestResult.error >= 1.0)
                    {
                        summary.discard += 1;
                    }
                    else if (bestResult.error >= burialOptions.threshold)
                    {
                        summary.bad += 1;
                    }
                    else
                    {
                        summary.good += 1;
                    }

                    if (bestInstance.initial.matrix.Equals(float4x4.zero))
                    {
                        summary.zeroIn += 1;
                    }

                    if (bestInstance.proposed.matrix.Equals(float4x4.zero))
                    {
                        summary.zeroOut += 1;
                    }

                    if (summary.bounds == default(BoundsBurst))
                    {
                        summary.bounds.center = bestInstance.proposed.matrix.c3.xyz;
                    }
                    else
                    {
                        summary.bounds.Encapsulate(bestInstance.proposed.matrix.c3.xyz);
                    }

                    if ((burialOptions.permissiveness < 4) &&
                        (bestResult.error > burialOptions.threshold))
                    {
                        summary.requeue = true;
                    }
                }

                if (summary.requeue &&
                    ((summary.discard + summary.bad) == 1) &&
                    (summary.total > 10))
                {
                    summary.requeue = false;
                }

                summaries[0] = summary;
            }
        }
    }
}

#endif