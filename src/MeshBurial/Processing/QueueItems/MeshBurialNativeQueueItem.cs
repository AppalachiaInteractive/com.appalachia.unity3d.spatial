#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Utility.Strings;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public class MeshBurialNativeQueueItem : MeshBurialManySameQueueItem
    {
        private NativeList<float4x4> _matrices;

        public MeshBurialNativeQueueItem(
            GameObject model,
            NativeList<float4x4> matrices,
            bool adoptTerrainNormal = true) : base(
            ZString.Format("NativeList: {0}", model.name),
            model,
            matrices.Length,
            adoptTerrainNormal
        )
        {
            _matrices = matrices;
        }

        /*
        protected override bool TryGetMatrixInternal(int i, out float4x4 matrix)
        {
            if (i >= _matrices.Length)
            {
                matrix = default;
                return false;
            }

            matrix = _matrices[i];
            return true;
        }

        protected override void SetMatrixInternal(int i, float4x4 m)
        {
            _matrices[i] = m;
        }*/

        protected override float GetDegreeAdjustmentStrengthInternal()
        {
            return 1.0f;
        }

        protected override void OnCompleteInternal()
        {
        }

        public override void GetAllMatrices(NativeList<float4x4> matrices)
        {
            matrices.AddRange(_matrices);
        }

        public override void SetAllMatrices(NativeArray<float4x4> matrices)
        {
            _matrices.Clear();
            _matrices.AddRange(matrices);
        }

        [DebuggerStepThrough] public override string ToString()
        {
            return name;
        }
    }
}

#endif