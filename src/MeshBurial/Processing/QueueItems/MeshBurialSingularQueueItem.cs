#region

using System;
using Appalachia.Spatial.MeshBurial.State;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public abstract class MeshBurialSingularQueueItem : MeshBurialQueueItem
    {
        private MeshBurialAdjustmentState _adjustmentState;

        private bool _adoptTerrainNormal;
        private int _hashCode;

        //private int[] _terrainHashCode;
        private float4x4[] _matrices;
        protected GameObject _model;
        private MeshBurialSharedState _sharedState;

        protected MeshBurialSingularQueueItem(
            string name,
            GameObject model,
            float4x4 matrix,
            bool adoptTerrainNormal = true) : base(name, 1)
        {
            _model = model;
            _hashCode = _model.GetHashCode();
            _adjustmentState = MeshBurialAdjustmentCollection.instance.GetByPrefab(_model);
            _sharedState = MeshBurialSharedStateManager.Get(_model);
            _adoptTerrainNormal = adoptTerrainNormal;
            _matrices = new float4x4[0];
            _matrices[0] = matrix;
        }

        protected override void OnInitializeInternal()
        {
        }

        protected override int GetModelHashCodeInternal()
        {
            return _hashCode;
        }

        protected override bool GetAdoptTerrainNormalInternal()
        {
            return _adoptTerrainNormal;
        }

        protected override MeshBurialSharedState GetMeshBurialSharedStateInternal()
        {
            return _sharedState;
        }

        protected override MeshBurialAdjustmentState GetMeshBurialAdjustmentStateInternal()
        {
            return _adjustmentState;
        }
    }
}
