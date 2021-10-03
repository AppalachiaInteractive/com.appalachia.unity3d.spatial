#region

using System;
using Appalachia.Spatial.MeshBurial.State;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public abstract class MeshBurialManySameQueueItem : MeshBurialManyQueueItem
    {
        private const string _PRF_PFX = nameof(MeshBurialManySameQueueItem) + ".";
        
        [SerializeField] private GameObject _model;
        [SerializeField] private int _modelHashCode;
        [SerializeField] private bool _adoptTerrainNormal;
        private MeshBurialSharedState _sharedState;
        private MeshBurialAdjustmentState _adjustmentState;

        protected MeshBurialManySameQueueItem(string name, GameObject model, int length, bool adoptTerrainNormal = true) : base(name, length)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            _model = model;
            _modelHashCode = _model.GetHashCode();
            _adoptTerrainNormal = adoptTerrainNormal;
        }

        private static readonly ProfilerMarker _PRF_OnInitializeInternal = new ProfilerMarker(_PRF_PFX + nameof(OnInitializeInternal));
        
        protected override void OnInitializeInternal()
        {
            using (_PRF_OnInitializeInternal.Auto())
            {
                base.OnInitializeInternal();

                _sharedState = MeshBurialSharedStateManager.GetByPrefab(_model);
                _adjustmentState = MeshBurialAdjustmentCollection.instance.GetByPrefab(_model);
            }
        }

        protected override int GetModelHashCodeInternal()
        {
            return _modelHashCode;
        }

        protected override bool GetAdoptTerrainNormalInternal()
        {
            return _adoptTerrainNormal;
        }

        protected override MeshBurialSharedState GetMeshBurialSharedStateInternal()
        {
            if (_sharedState == null)
            {
                _sharedState = MeshBurialSharedStateManager.GetByPrefab(_model);
            }

            return _sharedState;
        }

        protected override MeshBurialAdjustmentState GetMeshBurialAdjustmentStateInternal()
        {
            if (_adjustmentState == null)
            {
                _adjustmentState = MeshBurialAdjustmentCollection.instance.GetByPrefab(_model);
            }

            return _adjustmentState;
        }
    }
}
