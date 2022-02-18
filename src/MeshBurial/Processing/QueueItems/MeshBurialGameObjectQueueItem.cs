#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Strings;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public class MeshBurialGameObjectQueueItem : MeshBurialSingularQueueItem
    {
        public MeshBurialGameObjectQueueItem(GameObject model, bool adoptTerrainNormal = true) : base(
            ZString.Format("GameObject: {0}", model.name),
            model,
            model.transform.localToWorldMatrix,
            model,
            adoptTerrainNormal
        )
        {
        }

        /// <inheritdoc />
        public override void GetAllMatrices(NativeList<float4x4> matrices)
        {
            matrices.Add(_model.transform.localToWorldMatrix);
        }

        /// <inheritdoc />
        public override void SetAllMatrices(NativeArray<float4x4> matrices)
        {
            _model.transform.Matrix4x4ToTransform(matrices[0]);
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public override string ToString()
        {
            return name;
        }

        /*
        /// <inheritdoc />
protected override void SetMatrixInternal(int i, float4x4 m)
        {
            _model.transform.Matrix4x4ToTransform(m);
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
    }
}

#endif
