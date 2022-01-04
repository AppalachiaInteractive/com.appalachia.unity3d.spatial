#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Appalachia.Core.Assets;
using Appalachia.Core.Attributes;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Preferences;
using Appalachia.Editing.Core.Behaviours;
using Appalachia.Jobs.MeshData;
using Appalachia.Jobs.Optimization.Options;
using Appalachia.Jobs.Optimization.Utilities;
using Appalachia.Jobs.Transfers;
using Appalachia.Spatial.MeshBurial.Processing.QueueItems;
using Appalachia.Spatial.MeshBurial.State;
using Appalachia.Utility.Async;
using Appalachia.Utility.Strings;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.MeshBurial.Processing
{
    [CallStaticConstructorInEditor]
    public partial class
        MeshBurialExecutionManager : SingletonEditorOnlyAppalachiaBehaviour<MeshBurialExecutionManager>
    {
        static MeshBurialExecutionManager()
        {
            MeshBurialManagementQueue.InstanceAvailable += i => _meshBurialManagementQueue = i;
            MeshBurialAdjustmentCollection.InstanceAvailable += i => _meshBurialAdjustmentCollection = i;
        }

        #region Preferences

        [NonSerialized] private PREF<bool> _bury;

        internal PREF<bool> IsBuryingEnabled
        {
            get
            {
                if (_bury == null)
                {
                    _bury = PREFS.REG(PKG.Prefs.Group, "Enabled", true);
                }

                return _bury;
            }
        }

        #endregion

        #region Static Fields and Autoproperties

        private static MeshBurialAdjustmentCollection _meshBurialAdjustmentCollection;

        private static MeshBurialManagementQueue _meshBurialManagementQueue;

        #endregion

        #region Fields and Autoproperties

        public Bounds bounds;
        private Action finalizeAction;
        private Action<NativeArray<float4x4>> matrixAssignment;
        private bool _pending0Log;
        private bool resultDataFinalized;
        private DateTime _itemStart;
        private DateTime _iterationStart;
        private double _trackingError;
        private float _degreeAdjustment;
        private int _appliedAdjustments;
        private int _lastLogAt;
        private int _processed;

        private int cvci;
        private int cvii;
        private int cvpii;

        private JobHandle pendingHandle;
        private JobRandoms randoms;
        private List<Action> _queueActions = new();
        private MeshBurialAdjustmentState adjustmentState;
        private MeshBurialInstanceData resultData;
        private MeshBurialOptions burialOptions;
        private MeshBurialQueueItem lastItem;
        private MeshBurialSharedState sharedState;
        private MeshBurialSummaryData _tracking;
        private NativeList<float4x4> matrices;
        private NativeList<JobHandle> dependencyList;
        private OptimizationOptions optimizationOptions;
        private RandomSearchOptions randomSearchOptions;
        private VegetationSystemPro _vegetationSystem;

        #endregion

        #region Event Functions

        private void Update()
        {
            using (_PRF_Update.Auto())
            {
                if (!DependenciesAreReady || !FullyInitialized)
                {
                    return;
                }
                
                _iterationStart = DateTime.Now;

                CheckAndLogQueueDepth();

                if (ShouldEscape())
                {
                    return;
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
                            _queueActions[i]();

                            if (ShouldEscape())
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Context.Log.Error(ex);

                    resultDataFinalized = true;

                    IsBuryingEnabled.Value = false;
                }
                finally
                {
                    CheckAndLogQueueDepth();

                    _meshBurialAdjustmentCollection.MarkAsModified();
                    _meshBurialManagementQueue.MarkAsModified();
                }
            }
        }

        #endregion

        public void EnsureCompleted()
        {
            using (_PRF_EnsureCompleted.Auto())
            {
                pendingHandle.Complete();
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);

                MeshObjectManager.instance.RegisterDisposalDependency(EnsureCompleted);

                if (IsBuryingEnabled.Value)
                {
                    IsBuryingEnabled.Value = false;
                    UnityEditor.EditorApplication.delayCall += ToggleEnableMeshBurials;
                }

                if (!randoms.IsCreated)
                {
                    randoms = new JobRandoms(Allocator.Persistent);
                }

                PopulateQueueingActions();
            }
        }

        protected override async AppaTask WhenDestroyed()
        {
            using (_PRF_OnDestroy.Auto())
            {
                await base.WhenDestroyed();

                NativeDisposal();
            }
        }

        protected override async AppaTask WhenDisabled()

        {
            using (_PRF_OnDisable.Auto())
            {
                await base.WhenDisabled();

                pendingHandle.Complete();

                NativeDisposal();
            }
        }

        private void ApplyFinalizedResults()
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

        private void CheckAndLogQueueDepth()
        {
            using (_PRF_CheckAndLogQueueDepth.Auto())
            {
                if (_meshBurialManagementQueue == null)
                {
                    InitializeSynchronous();
                    return;
                }

                var queueDepth = _meshBurialManagementQueue.Count;

                if (queueDepth > 0)
                {
                    if ((queueDepth > _lastLogAt) ||
                        ((queueDepth != _lastLogAt) &&
                         (_LOG.Value > 0) &&
                         ((Time.frameCount % _LOG.Value) == 0)))
                    {
                        Context.Log.Info(
                            ZString.Format("Mesh Burial Queue Depth: {0}  [{1}]", queueDepth, Time.frameCount)
                        );
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
                            Context.Log.Info(
                                ZString.Format("Mesh Burial Queue Depth: 0  [{0}]", Time.frameCount)
                            );
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

        private void CheckFinalizedResult(out bool requeue)
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

                    adjustmentState.AddOrUpdate(
                        iteration.initial.matrix,
                        burialOptions.matchTerrainNormal,
                        iteration.proposed.matrix,
                        result.error
                    );

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

        private void FinalizeResultData()
        {
            using (_PRF_FinalizeResultData.Auto())
            {
                CheckFinalizedResult(out var requeue);

                if (requeue)
                {
                    new TransferJob_float4x4_NA_float4x4_NA
                    {
                        input = resultData.requeueableMatrices, output = matrices
                    }.Run(matrices.Length);

                    burialOptions.permissiveness += 1;
                    optimizationOptions.randomSearch.iterations =
                        (int)(optimizationOptions.randomSearch.iterations * 1.5);

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

        private void NativeDisposal()
        {
            using (_PRF_NativeDisposal.Auto())
            {
                randoms.Dispose();

                if (dependencyList.IsCreated)
                {
                    dependencyList.Dispose();
                }

                resultData?.Dispose();
            }
        }

        private void PopulateQueueingActions()
        {
            using (_PRF_PopulateQueueingActions.Auto())
            {
                if (_queueActions == null)
                {
                    _queueActions = new List<Action>();
                }

                if ((_vegetationSystem == null) && (VegetationStudioManager.Instance != null))
                {
                    _vegetationSystem = VegetationStudioManager.Instance.VegetationSystemList[0];
                }

                if (_queueActions.Count == 0)
                {
                    _queueActions.Add(() => ProcessGenericQueue(_meshBurialManagementQueue.array));
                    _queueActions.Add(() => ProcessGenericQueue(_meshBurialManagementQueue.gameObject));
                    _queueActions.Add(() => ProcessGenericQueue(_meshBurialManagementQueue.native));

                    //_queueActions.Add(() => ProcessGenericQueue(QUEUES.runtimePrefabRenderingSets));
                    _queueActions.Add(() => ProcessVegetationQueue(_meshBurialManagementQueue.vegetation));
                }
            }
        }

        private void ProcessGenericQueue<T>(
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
                                            (burialOptions.minimalRotation
                                                ? ops.xzDegreeAdjustmentMinimal
                                                : ops.xzDegreeAdjustment);

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
                                        Context.Log.Info(
                                            ZString.Format(
                                                "Items [{0}] took {1:F2} seconds to process.",
                                                item,
                                                duration
                                            )
                                        );
                                    }
                                }

                                queue.ResetCurrent();
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    Context.Log.Error(ZString.Format("Error while burying meshes: \r\n{0}", ex.Message));
                    Context.Log.Error(ex);
                    IsBuryingEnabled.Value = false;
                }
            }
        }

        private void ProcessVegetationQueue<T>(AppaTemporalQueue<T> queue)
            where T : MeshBurialQueueItem
        {
            using (_PRF_ProcessVegetationQueue.Auto())
            {
                ProcessGenericQueue(
                    _meshBurialManagementQueue.vegetation,
                    ProcessVegetationQueue_ShouldNotProcess,
                    ProcessVegetationQueue_PreAction,
                    ProcessVegetationQueue_PostAction
                );
            }
        }

        private void ProcessVegetationQueue_PostAction(AppaTemporalQueue<MeshBurialVegetationQueueItem> queue)
        {
            using (_PRF_ProcessVegetationQueue_PostAction.Auto())
            {
                if (!queue.HasCurrent)
                {
                    if (_meshBurialManagementQueue.pendingVegetationKeys.ContainsKey(cvci))
                    {
                        if (_meshBurialManagementQueue.pendingVegetationKeys[cvci].ContainsKey(cvii))
                        {
                            if (_meshBurialManagementQueue.pendingVegetationKeys[cvci][cvii].Contains(cvpii))
                            {
                                _meshBurialManagementQueue.pendingVegetationKeys[cvci][cvii].Remove(cvpii);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessVegetationQueue_PreAction(MeshBurialVegetationQueueItem current)
        {
            using (_PRF_ProcessVegetationQueue_PreAction.Auto())
            {
                cvci = current.cellIndex;
                cvpii = current.packageIndex;
                cvii = current.itemIndex;
            }
        }

        private bool ProcessVegetationQueue_ShouldNotProcess(MeshBurialVegetationQueueItem current)
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

        private bool ShouldEscape()
        {
            using (_PRF_ShouldEscape.Auto())
            {
                if ((_ITER.Value > 0) && (_processed > _ITER.Value))
                {
                    IsBuryingEnabled.Value = false;
                    return true;
                }

                if (!IsBuryingEnabled.Value)
                {
                    bounds = new Bounds();
                    return true;
                }

                if (!pendingHandle.IsCompleted)
                {
                    return true;
                }

                if ((_meshBurialManagementQueue.Count == 0) && resultDataFinalized)
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

        private void StatusLog(MeshBurialSummaryData summary, int? permissive = null, string name = null)
        {
            using (_PRF_StatusLog.Auto())
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
        }

        private void StatusLog(
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
                    Context.Log.Info(
                        ZString.Format(" [BATCH]  [TOT |{0,4}", total) +
                        ZString.Format("]  [AVG |{0:F3}",       average) +
                        ZString.Format("]  [+ |{0,4}",          good) +
                        ZString.Format("]  [- |{0,4}",          bad) +
                        ZString.Format("]  [x |{0,4}",          discard) +
                        ZString.Format("]  [zI |{0,4}",         zeroIn) +
                        ZString.Format("]  [zO |{0,4}",         zeroOut)
                    );
                }
                else
                {
                    Context.Log.Info(
                        ZString.Format(" [TOT |{0,4}",    total) +
                        ZString.Format("]  [AVG |{0:F3}", average) +
                        ZString.Format("]  [+ |{0,4}",    good) +
                        ZString.Format("]  [- |{0,4}",    bad) +
                        ZString.Format("]  [x |{0,4}",    discard) +
                        ZString.Format("]  [zI |{0,4}",   zeroIn) +
                        ZString.Format("]  [zO |{0,4}",   zeroOut) +
                        ZString.Format("]  [PER |{0}",    permissive) +
                        ZString.Format("]  [REQ |{0,5}",  requeue.Value) +
                        ZString.Format("]  [ID |{0}",     name)
                    );
                }
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(MeshBurialExecutionManager) + ".";

        private readonly ProfilerMarker _PRF_ApplyFinalizedResults =
            new(_PRF_PFX + nameof(ApplyFinalizedResults));

        private readonly ProfilerMarker _PRF_CheckAndLogQueueDepth =
            new(_PRF_PFX + nameof(CheckAndLogQueueDepth));

        private readonly ProfilerMarker _PRF_CheckFinalizedResult =
            new(_PRF_PFX + nameof(CheckFinalizedResult));

        private readonly ProfilerMarker _PRF_EnsureCompleted = new(_PRF_PFX + nameof(EnsureCompleted));

        private readonly ProfilerMarker _PRF_FinalizeResultData = new(_PRF_PFX + nameof(FinalizeResultData));

        private readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_OnDestroy =
            new ProfilerMarker(_PRF_PFX + nameof(OnDestroy));

        private static readonly ProfilerMarker _PRF_NativeDisposal =
            new ProfilerMarker(_PRF_PFX + nameof(NativeDisposal));

        private readonly ProfilerMarker _PRF_OnDisable = new ProfilerMarker(_PRF_PFX + nameof(OnDisable));

        private readonly ProfilerMarker _PRF_PopulateQueueingActions =
            new(_PRF_PFX + nameof(PopulateQueueingActions));

        private readonly ProfilerMarker _PRF_ProcessGenericQueue =
            new(_PRF_PFX + nameof(ProcessGenericQueue));

        private readonly ProfilerMarker _PRF_ProcessGenericQueue_FinalizeAction =
            new(_PRF_PFX + nameof(ProcessGenericQueue) + ".FinalizeAction");

        private readonly ProfilerMarker _PRF_ProcessVegetationQueue =
            new(_PRF_PFX + nameof(ProcessVegetationQueue));

        private readonly ProfilerMarker _PRF_ProcessVegetationQueue_PostAction =
            new(_PRF_PFX + nameof(ProcessVegetationQueue_PostAction));

        private readonly ProfilerMarker _PRF_ProcessVegetationQueue_PreAction =
            new(_PRF_PFX + nameof(ProcessVegetationQueue_PreAction));

        private readonly ProfilerMarker _PRF_ProcessVegetationQueue_ShouldNotProcess =
            new(_PRF_PFX + nameof(ProcessVegetationQueue_ShouldNotProcess));

        private readonly ProfilerMarker _PRF_ShouldEscape = new(_PRF_PFX + nameof(ShouldEscape));

        private readonly ProfilerMarker _PRF_StatusLog = new(_PRF_PFX + nameof(StatusLog));

        private static readonly ProfilerMarker _PRF_Update = new ProfilerMarker(_PRF_PFX + nameof(Update));

        #endregion
    }
}

#endif
