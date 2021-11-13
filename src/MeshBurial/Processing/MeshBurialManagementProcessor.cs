#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Attributes;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Sets;
using Appalachia.Core.Collections.NonSerialized;
using Appalachia.Spatial.MeshBurial.Processing.QueueItems;
using Appalachia.Spatial.MeshBurial.State;
using AwesomeTechnologies.VegetationSystem;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    [AlwaysInitializeOnLoad]
    public static partial class MeshBurialManagementProcessor
    {
        #region Profiling And Tracing Markers

        private const string _PRF_PFX = nameof(MeshBurialManagementProcessor) + ".";

        /*private static readonly ProfilerMarker _PRF_RefreshPrefabRenderingSets = new ProfilerMarker(_PRF_PFX + nameof(RefreshPrefabRenderingSets));
        public static void RefreshPrefabRenderingSets()
        {
            using (_PRF_RefreshPrefabRenderingSets.Auto())
            {
                var renderingSets = PrefabRenderingManager.instance.renderingSets;

                for (var i = 0; i < renderingSets.Sets.Count; i++)
                {
                    var renderingSet = renderingSets.Sets.at[i];

                    EnqueuePrefabRenderingSet(renderingSet);
                }
            }
        }*/

        private static VegetationSystemPro _vegetationSystem;
        private static readonly ProfilerMarker _PRF_Reset = new(_PRF_PFX + nameof(Reset));

        private static readonly ProfilerMarker _PRF_InitializeVSP = new(_PRF_PFX + nameof(InitializeVSP));

        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_RequeueAllCells = new(_PRF_PFX + nameof(RequeueAllCells));

        private static readonly ProfilerMarker _PRF_EnqueueCell = new(_PRF_PFX + nameof(EnqueueCell));

        private static readonly ProfilerMarker _PRF_ShouldAdoptTerrainNormal =
            new(_PRF_PFX + nameof(ShouldAdoptTerrainNormal));

        private static readonly ProfilerMarker _PRF_PrepareAndEnqueue =
            new(_PRF_PFX + nameof(PrepareAndEnqueue));

        /*
        private static readonly ProfilerMarker _PRF_EnqueuePrefabRenderingSet = new ProfilerMarker(_PRF_PFX + nameof(EnqueuePrefabRenderingSet));
        public static void EnqueuePrefabRenderingSet(PrefabRenderingSet renderingSet)
        {
            using (_PRF_EnqueuePrefabRenderingSet.Auto())
            {
                renderingSet.SyncExternalParameters(PrefabRenderingManager.instance.prefabSource);

                if (renderingSet.instanceManager.nextState != RuntimeStateCode.Enabled)
                {
                    return;
                }

                var physical = renderingSet.modelOptions.burialOptions;

                if (!physical.buryMesh)
                {
                    return;
                }

                renderingSet.instanceManager.transferOriginalToCurrent = false;

                var queueItem = new MeshBurialRuntimePrefabRenderingSetQueueItem(renderingSet);

                PrepareAndEnqueue(queueItem, QUEUES.runtimePrefabRenderingSets);
            }
        }
        */

        #endregion

        private static MeshBurialManagementQueue QUEUES => MeshBurialManagementQueue.instance;

        public static void EnqueueCell(VegetationCell cell, int packageIndex = -1, int itemIndex = -1)
        {
            using (_PRF_EnqueueCell.Auto())
            {
                InitializeVSP();

                var keys = QUEUES.pendingVegetationKeys;
                var queue = QUEUES.vegetation;

                if (keys.Count == 0)
                {
                    keys.Clear();
                }

                var alreadyQueued = keys.ContainsKey(cell.Index) &&
                                    keys[cell.Index].ContainsKey(packageIndex) &&
                                    keys[cell.Index][packageIndex].Contains(itemIndex);

                if (alreadyQueued)
                {
                    return;
                }

                if (!keys.ContainsKey(cell.Index))
                {
                    keys.Add(cell.Index, new NonSerializedAppaLookup<int, AppaSet_int>());
                }

                if (!keys[cell.Index].ContainsKey(packageIndex))
                {
                    keys[cell.Index].Add(packageIndex, new AppaSet_int {NoTracking = true});
                }

                if (!keys[cell.Index][packageIndex].Contains(itemIndex))
                {
                    keys[cell.Index][packageIndex].Add(itemIndex);
                }

                for (var i = 0; i < cell.VegetationPackageInstancesList.Count; i++)
                {
                    if ((packageIndex != -1) && (i != packageIndex))
                    {
                        continue;
                    }

                    var packageInstances = cell.VegetationPackageInstancesList[i];
                    var package = _vegetationSystem.VegetationPackageProList[i];

                    for (var j = 0; j < packageInstances.VegetationItemMatrixList.Count; j++)
                    {
                        if ((itemIndex != -1) && (i != itemIndex))
                        {
                            continue;
                        }

                        var itemInstances = packageInstances.VegetationItemMatrixList[j];

                        if (itemInstances.Length == 0)
                        {
                            continue;
                        }

                        var item = package.VegetationInfoList[j];

                        if (!item.EnableRuntimeSpawn ||
                            item.EnableExternalRendering ||
                            !item.EnableMeshBurying)
                        {
                            continue;
                        }

                        var adoptTerrainNormal = ShouldAdoptTerrainNormal(item);

                        var burial = new MeshBurialVegetationQueueItem(
                            cell.Index,
                            i,
                            j,
                            item.VegetationPrefab,
                            itemInstances.Length,
                            adoptTerrainNormal
                        );

                        try
                        {
                            PrepareAndEnqueue(burial, queue);
                        }
                        catch (NotSupportedException)
                        {
                            item.EnableMeshBurying = false;
                        }
                    }
                }
            }
        }

        public static void RequeueAllCells(VegetationSystemPro vsp)
        {
            using (_PRF_RequeueAllCells.Auto())
            {
                InitializeVSP();

                for (var h = 0; h < vsp.VegetationCellList.Count; h++)
                {
                    var cell = vsp.VegetationCellList[h];

                    _vegetationSystem.SpawnVegetationCell(cell);

                    EnqueueCell(cell);
                }
            }
        }

        public static void Reset(GameObject go)
        {
            using (_PRF_Reset.Auto())
            {
                var lookup = MeshBurialAdjustmentCollection.instance.GetByPrefab(go);

                if (lookup == null)
                {
                    return;
                }

                lookup.Reset();
            }
        }

        public static bool ShouldAdoptTerrainNormal(this VegetationItemInfoPro veggie)
        {
            using (_PRF_ShouldAdoptTerrainNormal.Auto())
            {
                var doNotAdoptTerrainRotation = (veggie.Rotation == VegetationRotationType.NoRotation) ||
                                                (veggie.Rotation == VegetationRotationType.RotateY) ||
                                                (veggie.Rotation == VegetationRotationType.RotateXYZ);

                return !doNotAdoptTerrainRotation;
            }
        }

        [ExecuteOnEnable]
        private static void Initialize()
        {
            using (_PRF_Initialize.Auto())
            {
                QUEUES.Initialize();

                _vspMeshBurialEnabled = false;
                UnityEditor.EditorApplication.delayCall += ToggleEnableVSPMeshBurials;
                MeshBurialExecutionManager.InitializeEnableMeshBurials();
            }
        }

        private static void InitializeVSP()
        {
            using (_PRF_InitializeVSP.Auto())
            {
                if (_vegetationSystem == null)
                {
                    _vegetationSystem = Object.FindObjectOfType<VegetationSystemPro>();
                }
            }
        }

        private static void PrepareAndEnqueue<T>(T item, AppaTemporalQueue<T> queue)
            where T : MeshBurialQueueItem
        {
            using (_PRF_PrepareAndEnqueue.Auto())
            {
                queue.Enqueue(item);

                QUEUES.SetDirty();
            }
        }
    }
}

#endif