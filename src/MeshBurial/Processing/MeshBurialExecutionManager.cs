#region

using System;
using System.Collections.Generic;
using Appalachia.Core.Attributes;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Editing.AssetDB;
using Appalachia.Core.Jobs.Transfers;
using Appalachia.Core.MeshData;
using Appalachia.Optimization.Options;
using Appalachia.Optimization.Utilities;
using Appalachia.Spatial.MeshBurial.Processing.QueueItems;
using Appalachia.Spatial.MeshBurial.State;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    [InitializeOnLoad]
    public static partial class MeshBurialExecutionManager
    {
        private const string _PRF_PFX = nameof(MeshBurialExecutionManager) + ".";

        private static readonly EditorApplication.CallbackFunction _processFrame = MeshBurialExecutionManager_ProcessFrame;

        public static Bounds bounds;

        private static int _processed;

        private static JobRandoms randoms;
        private static JobHandle pendingHandle;
        private static MeshBurialQueueItem lastItem;
        private static MeshBurialInstanceData resultData;
        private static NativeList<JobHandle> dependencyList;

        private static bool resultDataFinalized;
        private static Action finalizeAction;
        private static Action<NativeArray<float4x4>> matrixAssignment;

        private static NativeList<float4x4> matrices;

        //private static float4x4[] matrices;
        //private static int[] terrainHashCodes;
        private static MeshBurialSharedState sharedState;
        private static MeshBurialAdjustmentState adjustmentState;
        private static RandomSearchOptions randomSearchOptions;
        private static OptimizationOptions optimizationOptions;
        private static MeshBurialOptions burialOptions;
        private static float _degreeAdjustment;

        private static int _appliedAdjustments;
        private static DateTime _iterationStart;
        private static DateTime _itemStart;

        private static List<Action> _queueActions = new List<Action>();

        private static bool _pending0Log;
        private static int _lastLogAt;

        private static MeshBurialSummaryData _tracking;
        private static double _trackingError;

        private static VegetationSystemPro _vegetationSystem;

        private static readonly ProfilerMarker _PRF_MeshBurialExecutionManager = new ProfilerMarker(_PRF_PFX + nameof(MeshBurialExecutionManager));
        
        static MeshBurialExecutionManager()
        {
            using (_PRF_MeshBurialExecutionManager.Auto())
            {
                MeshObjectManager.RegisterDisposalDependency(
                    () => MeshBurialExecutionManager.EnsureCompleted()
                );
                
                if (_BURY.Value)
                {
                    _BURY.Value = false;
                    EditorApplication.delayCall += ToggleEnableMeshBurials;
                }
            }
        }


        private static MeshBurialManagementQueue QUEUES => MeshBurialManagementQueue.instance;

        private static readonly ProfilerMarker _PRF_EnsureCompleted = new ProfilerMarker(_PRF_PFX + nameof(EnsureCompleted));
        public static void EnsureCompleted()
        {
            using (_PRF_EnsureCompleted.Auto())
            {
                pendingHandle.Complete();
            }
        }

        private static readonly ProfilerMarker _PRF_MeshBurialExecutionManager_ProcessFrame = new ProfilerMarker(_PRF_PFX + nameof(MeshBurialExecutionManager_ProcessFrame));
        private static readonly ProfilerMarker _PRF_MeshBurialExecutionManager_ProcessFrame_RecordIterationTime = new ProfilerMarker(_PRF_PFX + nameof(MeshBurialExecutionManager_ProcessFrame) + ".RecordIterationTime");

        private static readonly ProfilerMarker _PRF_MeshBurialExecutionManager_ProcessFrame_InitializeProcessing = new ProfilerMarker(_PRF_PFX + nameof(MeshBurialExecutionManager_ProcessFrame) + ".InitializeProcessing");
        private static readonly ProfilerMarker _PRF_MeshBurialExecutionManager_ProcessFrame_IterateQueueingActions = new ProfilerMarker(_PRF_PFX + nameof(MeshBurialExecutionManager_ProcessFrame) + ".IterateQueueingActions");
        private static readonly ProfilerMarker _PRF_MeshBurialExecutionManager_ProcessFrame_Finally = new ProfilerMarker(_PRF_PFX + nameof(MeshBurialExecutionManager_ProcessFrame) + ".Finally");
        private static void MeshBurialExecutionManager_ProcessFrame()
        {
            using (_PRF_MeshBurialExecutionManager_ProcessFrame.Auto())
            {
                using (_PRF_MeshBurialExecutionManager_ProcessFrame_RecordIterationTime.Auto())
                {

                    _iterationStart = DateTime.Now;

                }
                using (_PRF_MeshBurialExecutionManager_ProcessFrame_InitializeProcessing.Auto())
                {
                    Initialize();

                    CheckAndLogQueueDepth();

                    if (ShouldEscape())
                    {
                        return;
                    }
                }

                try
                {
                    if (ShouldEscape())
                    {
                        return;
                    }

                    if ((resultData != null) && !resultDataFinalized)
                    {
                        FinalizeResultData();
                    }
                    else
                    {
                        _processed += 1;

                        for (var i = 0; i < _queueActions.Count; i++)
                        {
                            using (_PRF_MeshBurialExecutionManager_ProcessFrame_IterateQueueingActions.Auto())
                            {
                                _queueActions[i]();

                                if (ShouldEscape())
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);

                    resultDataFinalized = true;

                    _BURY.Value = false;
                }
                finally
                {
                    using (_PRF_MeshBurialExecutionManager_ProcessFrame_Finally.Auto())
                    {
                        CheckAndLogQueueDepth();

                        MeshBurialAdjustmentCollection.instance.SetDirty();
                        QUEUES.SetDirty();
                    }
                }
            }
        }

        private static readonly ProfilerMarker _PRF_Initialize = new ProfilerMarker(_PRF_PFX + nameof(Initialize));
        private static void Initialize()
        {
            using (_PRF_Initialize.Auto())
            {
                if (!randoms.IsCreated)
                {
                    randoms = new JobRandoms(Allocator.Persistent);
                }

                PopulateQueueingActions();
            }
        }

        private static readonly ProfilerMarker _PRF_FinalizeResultData = new ProfilerMarker(_PRF_PFX + nameof(FinalizeResultData));
        private static void FinalizeResultData()
        {
            using (_PRF_FinalizeResultData.Auto())
            {
                CheckFinalizedResult(out var requeue);

                if (requeue)
                {
                    new TransferJob_float4x4_NA_float4x4_NA {input = resultData.requeueableMatrices, output = matrices}.Run(matrices.Length);

                    burialOptions.permissiveness += 1;
                    optimizationOptions.randomSearch.iterations = (int) (optimizationOptions.randomSearch.iterations * 1.5);

                    pendingHandle = MeshBurialJobManager.ScheduleMeshBurialJobs(
                        resultData,
                        sharedState.meshObject.data,
                        adjustmentState,
                        matrices,
                        sharedState.optimizationParams,
                        optimizationOptions,
                        burialOptions,
                        _degreeAdjustment,
                        randoms,
                        dependencyList
                    );
                }
                else
                {
                    ApplyFinalizedResults();
                }
            }
        }

        private static readonly ProfilerMarker _PRF_CheckFinalizedResult = new ProfilerMarker(_PRF_PFX + nameof(CheckFinalizedResult));
        private static void CheckFinalizedResult(out bool requeue)
        {
            using (_PRF_CheckFinalizedResult.Auto())
            {
                pendingHandle.Complete();

                var summary = resultData.summaries[0];

                if (_DEBUGLOG.v)
                {
                    StatusLog(summary, burialOptions.permissiveness, lastItem.name);
                }

                for (var instanceIndex = 0; instanceIndex < resultData.instanceCount; instanceIndex++)
                {
                    var result = resultData.bestResults[instanceIndex];
                    var iteration = resultData.instances[instanceIndex, result.iterationIndex];

                    var error = iteration.proposed.error;
                    var matrix = iteration.proposed.matrix;

                    if (matrix.Equals(float4x4.zero))
                    {
                        continue;
                    }

                    if (error > burialOptions.threshold)
                    {
                        continue;
                    }

                    adjustmentState.AddOrUpdate(iteration.initial.matrix, burialOptions.matchTerrainNormal, iteration.proposed.matrix, result.error);

                    if (!iteration.excluded)
                    {
                        adjustmentState.AddOrUpdate(
                            iteration.proposed.matrix,
                            burialOptions.matchTerrainNormal,
                            iteration.proposed.matrix,
                            result.error
                        );
                    }
                }

                bounds = new Bounds(summary.bounds.center, summary.bounds.size);

                if (!summary.requeue)
                {
                    _tracking.total += summary.total;
                    _tracking.good += summary.good;
                    _tracking.bad += summary.bad;
                    _tracking.discard += summary.discard;
                    _tracking.zeroIn += summary.zeroIn;
                    _tracking.zeroOut += summary.zeroOut;
                    _trackingError = summary.average * summary.total;
                    _tracking.average = _trackingError / _tracking.total;
                }

                requeue = summary.requeue;
            }
        }

        private static readonly ProfilerMarker _PRF_ApplyFinalizedResults = new ProfilerMarker(_PRF_PFX + nameof(ApplyFinalizedResults));
        private static void ApplyFinalizedResults()
        {
            using (_PRF_ApplyFinalizedResults.Auto())
            {
                if ((resultData == null) || (matrixAssignment == null) || (finalizeAction == null))
                {
                    resultDataFinalized = true;
                    return;
                }

                matrixAssignment(resultData.bestMatrices.AsArray());

                finalizeAction();

                resultDataFinalized = true;
            }
        }

        private static readonly ProfilerMarker _PRF_ProcessGenericQueue = new ProfilerMarker(_PRF_PFX + nameof(ProcessGenericQueue));
        private static readonly ProfilerMarker _PRF_ProcessGenericQueue_FinalizeAction = new ProfilerMarker(_PRF_PFX + nameof(ProcessGenericQueue) + ".FinalizeAction");
        private static void ProcessGenericQueue<T>(
            AppaTemporalQueue<T> queue,
            Func<T, bool> shouldNotProcess = null,
            Action<T> preAction = null,
            Action<AppaTemporalQueue<T>> postAction = null)
            where T : MeshBurialQueueItem
        {
            using (_PRF_ProcessGenericQueue.Auto())
            {
                try
                {
                    var item = queue.CurrentOrNext();

                    if (item != null)
                    {
                        _itemStart = DateTime.Now;
                        
                        if ((shouldNotProcess != null) && shouldNotProcess(item))
                        {
                            queue.ResetCurrent();
                            return;
                        }

                        lastItem = item;

                        preAction?.Invoke(item);

                        if (matrices.ShouldAllocate())
                        {
                            matrices.SafeDispose();
                            matrices = new NativeList<float4x4>(item.length * 2, Allocator.Persistent);
                        }
                        else
                        {
                            matrices.Clear();
                        }

                        item.GetAllMatrices(matrices);

                        sharedState = item.GetMeshBurialSharedState();
                        adjustmentState = item.GetMeshBurialAdjustmentState();

                        var iterations = _INST_ITER.Value;

                        if (iterations == 0)
                        {
                            iterations = 1024;
                        }

                        randomSearchOptions = new RandomSearchOptions(iterations);

                        optimizationOptions = new OptimizationOptions(randomSearchOptions);

                        burialOptions = new MeshBurialOptions
                        {
                            threshold = _ERROR.Value,
                            minimalRotation = false,
                            accountForMeshNormal = _MESH_NORMALS.Value,
                            matchTerrainNormal = item.GetAdoptTerrainNormal() && _TERRAIN_NORMALS.Value,
                            permissiveness = 1,
                            adjustHeight = _HEIGHT.Value,
                            applyParameters = _PARAMS.Value,
                            applyTestValue = _TEST.Value,
                            testValue = float4x4.Translate(_TEST_VALUE.Value)
                        };

                        resultDataFinalized = false;

                        if (!dependencyList.IsCreated)
                        {
                            dependencyList = new NativeList<JobHandle>(512, Allocator.Persistent);
                        }

                        if (resultData == null)
                        {
                            resultData = new MeshBurialInstanceData();
                        }

                        var ops = sharedState.optimizationParams;

                        _degreeAdjustment = burialOptions.permissiveness *
                                               (burialOptions.minimalRotation ? ops.xzDegreeAdjustmentMinimal : ops.xzDegreeAdjustment);

                        _degreeAdjustment *= item.GetDegreeAdjustmentStrength();
                        
                        pendingHandle = MeshBurialJobManager.ScheduleMeshBurialJobs(
                            resultData,
                            sharedState.meshObject.data,
                            adjustmentState,
                            matrices,
                            sharedState.optimizationParams,
                            optimizationOptions,
                            burialOptions,
                            _degreeAdjustment,
                            randoms,
                            dependencyList
                        );

                        matrixAssignment = f4x4 => item.SetAllMatrices(f4x4);

                        finalizeAction = () =>
                        {
                            using (_PRF_ProcessGenericQueue_FinalizeAction.Auto())
                            {
                                postAction?.Invoke(queue);

                                item.Complete();

                                if (_TIMELOG.Value)
                                {
                                    var duration = (DateTime.Now - _itemStart).TotalSeconds;
                                    if (duration > _TIMELOGTIME.Value)
                                    {
                                        Debug.Log($"Items [{item}] took {duration:F2} seconds to process.");
                                    }
                                }

                                queue.ResetCurrent();
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while burying meshes: \r\n{ex.Message}");
                    Debug.LogException(ex);
                    _BURY.Value = false;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_ProcessVegetationQueue = new ProfilerMarker(_PRF_PFX + nameof(ProcessVegetationQueue));
        private static void ProcessVegetationQueue<T>(AppaTemporalQueue<T> queue)
            where T : MeshBurialQueueItem
        {
            using (_PRF_ProcessVegetationQueue.Auto())
            {
                ProcessGenericQueue(
                    QUEUES.vegetation,
                    ProcessVegetationQueue_ShouldNotProcess,
                    ProcessVegetationQueue_PreAction,
                    ProcessVegetationQueue_PostAction
                );
            }
        }

        private static int cvci = 0;
        private static int cvpii = 0;
        private static int cvii = 0;

        private static readonly ProfilerMarker _PRF_ProcessVegetationQueue_ShouldNotProcess = new ProfilerMarker(_PRF_PFX + nameof(ProcessVegetationQueue_ShouldNotProcess));
        private static bool ProcessVegetationQueue_ShouldNotProcess(MeshBurialVegetationQueueItem current)
        {
            using (_PRF_ProcessVegetationQueue_ShouldNotProcess.Auto())
            {
                var packages = _vegetationSystem.VegetationPackageProList;

                if (current.packageIndex >= packages.Count)
                {
                    return true;
                }

                var package = packages[current.packageIndex];

                var cells = _vegetationSystem.VegetationCellList;

                if (current.cellIndex >= cells.Count)
                {
                    return true;
                }

                var cell = cells[current.cellIndex];

                var items = package.VegetationInfoList;

                if (current.itemIndex >= items.Count)
                {
                    return true;
                }

                var item = items[current.itemIndex];

                if (!item.EnableRuntimeSpawn || !item.EnableMeshBurying)
                {
                    return true;
                }

                var packageInstancesList = cell.VegetationPackageInstancesList;

                if (current.packageIndex >= packageInstancesList.Count)
                {
                    return true;
                }

                var packageInstances = packageInstancesList[current.packageIndex];

                var itemInstances = packageInstances.VegetationItemMatrixList;

                if (current.itemIndex >= itemInstances.Count)
                {
                    return true;
                }

                return false;
            }
        }

        private static readonly ProfilerMarker _PRF_ProcessVegetationQueue_PreAction = new ProfilerMarker(_PRF_PFX + nameof(ProcessVegetationQueue_PreAction));
        private static void ProcessVegetationQueue_PreAction(MeshBurialVegetationQueueItem current)
        {
            using (_PRF_ProcessVegetationQueue_PreAction.Auto())
            {
                cvci = current.cellIndex;
                cvpii = current.packageIndex;
                cvii = current.itemIndex;
            }
        }

        private static readonly ProfilerMarker _PRF_ProcessVegetationQueue_PostAction = new ProfilerMarker(_PRF_PFX + nameof(ProcessVegetationQueue_PostAction));
        private static void ProcessVegetationQueue_PostAction(AppaTemporalQueue<MeshBurialVegetationQueueItem> queue)
        {
            using (_PRF_ProcessVegetationQueue_PostAction.Auto())
            {
                if (!queue.HasCurrent)
                {
                    if (QUEUES.pendingVegetationKeys.ContainsKey(cvci))
                    {
                        if (QUEUES.pendingVegetationKeys[cvci].ContainsKey(cvii))
                        {
                            if (QUEUES.pendingVegetationKeys[cvci][cvii].Contains(cvpii))
                            {
                                QUEUES.pendingVegetationKeys[cvci][cvii].Remove(cvpii);
                            }
                        }
                    }
                }
            }
        }

        private static readonly ProfilerMarker _PRF_ShouldEscape = new ProfilerMarker(_PRF_PFX + nameof(ShouldEscape));
        private static bool ShouldEscape()
        {
            using (_PRF_ShouldEscape.Auto())
            {
                if ((_ITER.Value > 0) && (_processed > _ITER.Value))
                {
                    _BURY.Value = false;
                    return true;
                }

                if (!_BURY.Value)
                {
                    bounds = new Bounds();
                    return true;
                }

                if (!pendingHandle.IsCompleted)
                {
                    return true;
                }

                if ((QUEUES.Count == 0) && resultDataFinalized)
                {
                    return true;
                }

                if ((DateTime.Now - _iterationStart).TotalMilliseconds > _TIME.Value)
                {
                    return true;
                }

                return false;
            }
        }

        private static readonly ProfilerMarker _PRF_CheckAndLogQueueDepth = new ProfilerMarker(_PRF_PFX + nameof(CheckAndLogQueueDepth));
        private static void CheckAndLogQueueDepth()
        {
            using (_PRF_CheckAndLogQueueDepth.Auto())
            {
                var queueDepth = QUEUES.Count;

                if (queueDepth > 0)
                {
                    if ((queueDepth > _lastLogAt) || ((queueDepth != _lastLogAt) && (_LOG.Value > 0) && ((Time.frameCount % _LOG.Value) == 0)))
                    {
                        Debug.Log($"Mesh Burial Queue Depth: {queueDepth}  [{Time.frameCount}]");
                        _lastLogAt = queueDepth;
                    }

                    _pending0Log = true;
                }
                else if (queueDepth == 0)
                {
                    if (_pending0Log)
                    {
                        if (_appliedAdjustments > 0)
                        {
                            Debug.Log($"Mesh Burial Queue Depth: 0  [{Time.frameCount}]");
                        }

                        StatusLog(
                            _tracking.total,
                            _tracking.average,
                            _tracking.good,
                            _tracking.bad,
                            _tracking.discard,
                            _tracking.zeroIn,
                            _tracking.zeroOut
                        );

                        _pending0Log = false;
                        bounds = new Bounds();

                        if (_appliedAdjustments > 0)
                        {
                            AssetDatabaseSaveManager.SaveAssetsSoon();
                            _appliedAdjustments = 0;
                        }

                        _tracking = new MeshBurialSummaryData();
                    }

                    _lastLogAt = queueDepth;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_PopulateQueueingActions = new ProfilerMarker(_PRF_PFX + nameof(PopulateQueueingActions));
        private static readonly ProfilerMarker _PRF_PopulateQueueingActions_VegetationQueue = new ProfilerMarker(_PRF_PFX + nameof(PopulateQueueingActions) + ".VegetationQueue");
        private static void PopulateQueueingActions()
        {
            using (_PRF_PopulateQueueingActions.Auto())
            {
                if (_queueActions == null)
                {
                    _queueActions = new List<Action>();
                }

                if (_vegetationSystem == null)
                {
                    _vegetationSystem = VegetationStudioManager.Instance.VegetationSystemList[0];
                }

                if (_queueActions.Count == 0)
                {
                    _queueActions.Add(() => ProcessGenericQueue(QUEUES.array));
                    _queueActions.Add(() => ProcessGenericQueue(QUEUES.gameObject));
                    _queueActions.Add(() => ProcessGenericQueue(QUEUES.native));
                    //_queueActions.Add(() => ProcessGenericQueue(QUEUES.runtimePrefabRenderingSets));
                    _queueActions.Add(() => ProcessVegetationQueue(QUEUES.vegetation));
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DisposeNativeCollections = new ProfilerMarker(_PRF_PFX + nameof(DisposeNativeCollections));
        [ExecuteOnDisable]
        public static void DisposeNativeCollections()
        {
            using (_PRF_DisposeNativeCollections.Auto())
            {
                pendingHandle.Complete();

                //if (!manual) Debug.Log("Disposing native collections.");

                if (randoms.IsCreated)
                {
                    randoms.Dispose();
                }

                if (dependencyList.IsCreated)
                {
                    dependencyList.Dispose();
                }

                resultData?.Dispose();
            }
        }

        private static void StatusLog(MeshBurialSummaryData summary, int? permissive = null, string name = null)
        {
            StatusLog(
                summary.total,
                summary.average,
                summary.good,
                summary.bad,
                summary.discard,
                summary.zeroIn,
                summary.zeroOut,
                permissive,
                summary.requeue,
                name
            );
        }

        private static readonly ProfilerMarker _PRF_StatusLog = new ProfilerMarker(_PRF_PFX + nameof(StatusLog));
        private static void StatusLog(
            int total,
            double average,
            int good,
            int bad,
            int discard,
            int zeroIn,
            int zeroOut,
            int? permissive = null,
            bool? requeue = null,
            string name = null)
        {
            using (_PRF_StatusLog.Auto())
            {
                if ((requeue == null) || !requeue.Value)
                {
                    Debug.Log(
                        $" [BATCH]  [TOT |{total,4}" +
                        $"]  [AVG |{average:F3}" +
                        $"]  [+ |{good,4}" +
                        $"]  [- |{bad,4}" +
                        $"]  [x |{discard,4}" +
                        $"]  [zI |{zeroIn,4}" +
                        $"]  [zO |{zeroOut,4}"
                    );
                }
                else
                {
                    Debug.Log(
                        $" [TOT |{total,4}" +
                        $"]  [AVG |{average:F3}" +
                        $"]  [+ |{good,4}" +
                        $"]  [- |{bad,4}" +
                        $"]  [x |{discard,4}" +
                        $"]  [zI |{zeroIn,4}" +
                        $"]  [zO |{zeroOut,4}" +
                        $"]  [PER |{permissive}" +
                        $"]  [REQ |{requeue.Value,5}" +
                        $"]  [ID |{name}"
                    );
                }
            }
        }
    }
}
