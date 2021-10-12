#region

using System;
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
        private const string _PRF_PFX = nameof(MeshBurialArrayQueueItem) + ".";

        private static readonly ProfilerMarker _PRF_GetAllMatrices =
            new(_PRF_PFX + nameof(GetAllMatrices));

        private static readonly ProfilerMarker _PRF_SetAllMatrices =
            new(_PRF_PFX + nameof(SetAllMatrices));

        [SerializeField] private float4x4[] matrices;

        public MeshBurialArrayQueueItem(
            GameObject model,
            float4x4[] matrices,
            bool adoptTerrainNormal = true) : base(
            $"Array: {model.name}",
            model,
            matrices.Length,
            adoptTerrainNormal
        )
        {
            this.matrices = matrices;
        }

        /*
        protected override bool TryGetMatrixInternal(int i, out float4x4 matrix)
        {
            matrix = matrices[i];
            return true;
        }

        protected override void SetMatrixInternal(int i, float4x4 m)
        {
            matrices[i] = m;
        }
        */

        protected override float GetDegreeAdjustmentStrengthInternal()
        {
            return 1.0f;
        }

        protected override void OnCompleteInternal()
        {
        }

        public override void GetAllMatrices(NativeList<float4x4> mats)
        {
            using (_PRF_GetAllMatrices.Auto())
            {
                mats.CopyFromNBC(matrices);
            }
        }

        public override void SetAllMatrices(NativeArray<float4x4> mats)
        {
            using (_PRF_SetAllMatrices.Auto())
            {
                mats.CopyTo(matrices);
            }
        }

        public override string ToString()
        {
            return name;
        }
    }
}
