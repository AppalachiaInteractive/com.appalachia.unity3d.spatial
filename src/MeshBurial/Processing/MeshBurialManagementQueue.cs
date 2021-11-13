#if UNITY_EDITOR

#region

using Appalachia.CI.Constants;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Sets;
using Appalachia.Core.Collections.NonSerialized;
using Appalachia.Core.Scriptables;
using Appalachia.Spatial.MeshBurial.Processing.QueueItems;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public class
        MeshBurialManagementQueue : SingletonAppalachiaObject<MeshBurialManagementQueue>
    {
        private const string _PRF_PFX = nameof(MeshBurialManagementQueue) + ".";


        //prefabSpawnPointStates.Count +
        //runtimePrefabRenderingElements.Count +
        //runtimePrefabRenderingSets.Count;

        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_ClearQueues =
            new(_PRF_PFX + nameof(ClearQueues));

        public AppaTemporalQueue<MeshBurialArrayQueueItem> array;

        public AppaTemporalQueue<MeshBurialVegetationQueueItem> vegetation;

        public AppaTemporalQueue<MeshBurialNativeQueueItem> native;

        public AppaTemporalQueue<MeshBurialGameObjectQueueItem> gameObject;

        public NonSerializedAppaLookup2<int, int, AppaSet_int> pendingVegetationKeys;

        //public AppaTemporalQueue<PrefabSpawnPointStateQueueItem> prefabSpawnPointStates;

        //public AppaTemporalQueue<MeshBurialRuntimePrefabRenderingElementQueueItem> runtimePrefabRenderingElements;

        //public AppaTemporalQueue<MeshBurialRuntimePrefabRenderingSetQueueItem> runtimePrefabRenderingSets;

        public int Count => vegetation.Count + native.Count + array.Count + gameObject.Count; // +

        public void Initialize()
        {
            using (_PRF_Initialize.Auto())
            {
                if (pendingVegetationKeys == null)
                {
                    pendingVegetationKeys = new NonSerializedAppaLookup2<int, int, AppaSet_int>();
                }

                if (vegetation == null)
                {
                    vegetation = new AppaTemporalQueue<MeshBurialVegetationQueueItem>();
                }

                if (native == null)
                {
                    native = new AppaTemporalQueue<MeshBurialNativeQueueItem>();
                }

                if (array == null)
                {
                    array = new AppaTemporalQueue<MeshBurialArrayQueueItem>();
                }

                if (gameObject == null)
                {
                    gameObject = new AppaTemporalQueue<MeshBurialGameObjectQueueItem>();
                }

                //if (prefabSpawnPointStates == null) { prefabSpawnPointStates = new AppaTemporalQueue<PrefabSpawnPointStateQueueItem>(); }

                //if (runtimePrefabRenderingElements == null) { runtimePrefabRenderingElements = new AppaTemporalQueue<MeshBurialRuntimePrefabRenderingElementQueueItem>(); }

                //if (runtimePrefabRenderingSets == null) { runtimePrefabRenderingSets = new AppaTemporalQueue<MeshBurialRuntimePrefabRenderingSetQueueItem>(); }

                SetDirty();
            }
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Tools.Base + "Clear Queues", priority = PKG.Menu.Appalachia.Tools.Priority)]
        public static void ClearQueues()
        {
            using (_PRF_ClearQueues.Auto())
            {
                instance.array.Clear();
                instance.gameObject.Clear();
                instance.native.Clear();

                //instance.prefabSpawnPointStates.Clear();
                //instance.runtimePrefabRenderingElements.Clear();
                //instance.runtimePrefabRenderingSets.Clear();
                instance.vegetation.Clear();

                instance.array.ResetCurrent();
                instance.gameObject.ResetCurrent();
                instance.native.ResetCurrent();

                //instance.prefabSpawnPointStates.ResetCurrent();
                //instance.runtimePrefabRenderingElements.ResetCurrent();
                //instance.runtimePrefabRenderingSets.ResetCurrent();
                instance.vegetation.ResetCurrent();

                instance.SetDirty();
            }
        }
    }
}

#endif