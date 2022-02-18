#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Core.Objects.Root;
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
        //private int _current;

        protected MeshBurialQueueItem(string name, int length, UnityEngine.Object owner) : base(owner)
        {
            //_current = 0;
            this.name = name;
            this.length = length;
        }

        #region Fields and Autoproperties

        private bool _completed;

        private bool _initialized;

        /*
        public int current
        {
            get => _current;
            set => _current = value;
        }
        */

        public int length { get; }

        public string name { get; }

        #endregion

        //public abstract int[] GetAllTerrainHashCodes();

        /// <inheritdoc />
        [DebuggerStepThrough]
        public abstract override string ToString();

        public abstract void GetAllMatrices(NativeList<float4x4> matrices);

        public abstract void SetAllMatrices(NativeArray<float4x4> matrices);

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

        public MeshBurialAdjustmentState GetMeshBurialAdjustmentState()
        {
            using (_PRF_GetMeshBurialAdjustmentState.Auto())
            {
                Initialize();

                var state = GetMeshBurialAdjustmentStateInternal();

                return state;
            }
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

        public int GetModelHashCode()
        {
            Initialize();
            return GetModelHashCodeInternal();
        }

        protected abstract bool GetAdoptTerrainNormalInternal();

        protected abstract float GetDegreeAdjustmentStrengthInternal();

        protected abstract MeshBurialAdjustmentState GetMeshBurialAdjustmentStateInternal();

        protected abstract MeshBurialSharedState GetMeshBurialSharedStateInternal();

        //protected abstract bool TryGetMatrixInternal(int i, out float4x4 matrix);

        //protected abstract void SetMatrixInternal(int i, float4x4 m);

        protected abstract int GetModelHashCodeInternal();

        //protected abstract int GetTerrainHashCodeInternal(int i);

        protected abstract void OnCompleteInternal();

        protected abstract void OnInitializeInternal();

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

        #region Profiling

        private const string _PRF_PFX = nameof(MeshBurialQueueItem) + ".";

        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_GetMeshBurialSharedState =
            new(_PRF_PFX + nameof(GetMeshBurialSharedState));

        private static readonly ProfilerMarker _PRF_GetMeshBurialAdjustmentState =
            new(_PRF_PFX + nameof(GetMeshBurialAdjustmentState));

        private static readonly ProfilerMarker _PRF_Complete = new(_PRF_PFX + nameof(Complete));

        #endregion
    }
}

#endif
