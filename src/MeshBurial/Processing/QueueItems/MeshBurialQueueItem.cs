#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Core.Behaviours;
using Appalachia.Spatial.MeshBurial.State;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public abstract class MeshBurialQueueItem : AppalachiaBase
    {
        private const string _PRF_PFX = nameof(MeshBurialQueueItem) + ".";

        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_GetMeshBurialSharedState =
            new(_PRF_PFX + nameof(GetMeshBurialSharedState));

        private static readonly ProfilerMarker _PRF_GetMeshBurialAdjustmentState =
            new(_PRF_PFX + nameof(GetMeshBurialAdjustmentState));

        private static readonly ProfilerMarker _PRF_Complete = new(_PRF_PFX + nameof(Complete));
        private bool _completed;

        private bool _initialized;

        //private int _current;

        protected MeshBurialQueueItem(string name, int length)
        {
            //_current = 0;
            this.name = name;
            this.length = length;
        }

        /*
        public int current
        {
            get => _current;
            set => _current = value;
        }
        */

        public int length { get; }

        public string name { get; }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            using (_PRF_Initialize.Auto())
            {
                _initialized = true;

                OnInitializeInternal();
            }
        }

        public int GetModelHashCode()
        {
            Initialize();
            return GetModelHashCodeInternal();
        }

        public bool GetAdoptTerrainNormal()
        {
            Initialize();
            return GetAdoptTerrainNormalInternal();
        }

        public float GetDegreeAdjustmentStrength()
        {
            Initialize();
            return GetDegreeAdjustmentStrengthInternal();
        }

        public MeshBurialSharedState GetMeshBurialSharedState()
        {
            using (_PRF_GetMeshBurialSharedState.Auto())
            {
                Initialize();

                var state = GetMeshBurialSharedStateInternal();

                return state;
            }
        }

        public MeshBurialAdjustmentState GetMeshBurialAdjustmentState()
        {
            using (_PRF_GetMeshBurialAdjustmentState.Auto())
            {
                Initialize();

                var state = GetMeshBurialAdjustmentStateInternal();

                return state;
            }
        }

        public void Complete()
        {
            using (_PRF_Complete.Auto())
            {
                Initialize();

                if (_completed)
                {
                    return;
                }

                _completed = true;

                OnCompleteInternal();
            }
        }

        protected abstract void OnInitializeInternal();

        //protected abstract bool TryGetMatrixInternal(int i, out float4x4 matrix);

        //protected abstract void SetMatrixInternal(int i, float4x4 m);

        protected abstract int GetModelHashCodeInternal();

        protected abstract bool GetAdoptTerrainNormalInternal();

        protected abstract float GetDegreeAdjustmentStrengthInternal();

        protected abstract MeshBurialSharedState GetMeshBurialSharedStateInternal();

        protected abstract MeshBurialAdjustmentState GetMeshBurialAdjustmentStateInternal();

        //protected abstract int GetTerrainHashCodeInternal(int i);

        protected abstract void OnCompleteInternal();

        public abstract void GetAllMatrices(NativeList<float4x4> matrices);

        public abstract void SetAllMatrices(NativeArray<float4x4> matrices);

        //public abstract int[] GetAllTerrainHashCodes();

        [DebuggerStepThrough] public abstract override string ToString();
    }
}

#endif