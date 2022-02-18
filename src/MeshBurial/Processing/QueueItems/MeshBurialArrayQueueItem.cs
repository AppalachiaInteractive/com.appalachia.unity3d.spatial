#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Utility.Strings;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public class MeshBurialArrayQueueItem : MeshBurialManySameQueueItem
    {
        public MeshBurialArrayQueueItem(GameObject model, float4x4[] matrices, bool adoptTerrainNormal = true)
            : base(ZString.Format("Array: {0}", model.name), model, matrices.Length, adoptTerrainNormal)
        {
            this.matrices = matrices;
        }

        #region Fields and Autoproperties

        [SerializeField] private float4x4[] matrices;

        #endregion

        /// <inheritdoc />
        public override void GetAllMatrices(NativeList<float4x4> mats)
        {
            using (_PRF_GetAllMatrices.Auto())
            {
                mats.CopyFromNBC(matrices);
            }
        }

        /// <inheritdoc />
        public override void SetAllMatrices(NativeArray<float4x4> mats)
        {
            using (_PRF_SetAllMatrices.Auto())
            {
                mats.CopyTo(matrices);
            }
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return name;
        }

        /*
        /// <inheritdoc />
protected override bool TryGetMatrixInternal(int i, out float4x4 matrix)
        {
            matrix = matrices[i];
            return true;
        }

        /// <inheritdoc />
protected override void SetMatrixInternal(int i, float4x4 m)
        {
            matrices[i] = m;
        }
        */

        /// <inheritdoc />
        protected override float GetDegreeAdjustmentStrengthInternal()
        {
            return 1.0f;
        }

        /// <inheritdoc />
        protected override void OnCompleteInternal()
        {
        }

        #region Profiling

        private const string _PRF_PFX = nameof(MeshBurialArrayQueueItem) + ".";

        private static readonly ProfilerMarker _PRF_GetAllMatrices = new(_PRF_PFX + nameof(GetAllMatrices));

        private static readonly ProfilerMarker _PRF_SetAllMatrices = new(_PRF_PFX + nameof(SetAllMatrices));

        #endregion
    }
}

#endif
