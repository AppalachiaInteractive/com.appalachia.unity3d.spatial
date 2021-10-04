#region

using System;
using Appalachia.Core.Extensions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public class MeshBurialGameObjectQueueItem : MeshBurialSingularQueueItem
    {
        public MeshBurialGameObjectQueueItem(GameObject model, bool adoptTerrainNormal = true) :
            base(
                $"GameObject: {model.name}",
                model,
                model.transform.localToWorldMatrix,
                adoptTerrainNormal
            )
        {
        }

        /*
        protected override void SetMatrixInternal(int i, float4x4 m)
        {
            _model.transform.Matrix4x4ToTransform(m);
        }
        */

        protected override float GetDegreeAdjustmentStrengthInternal()
        {
            return 1.0f;
        }

        protected override void OnCompleteInternal()
        {
        }

        public override void GetAllMatrices(NativeList<float4x4> matrices)
        {
            matrices.Add(_model.transform.localToWorldMatrix);
        }

        public override void SetAllMatrices(NativeArray<float4x4> matrices)
        {
            _model.transform.Matrix4x4ToTransform(matrices[0]);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
