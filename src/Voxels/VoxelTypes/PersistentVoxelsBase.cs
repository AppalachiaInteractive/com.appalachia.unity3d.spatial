#region

using System;
using Appalachia.CI.Integration.Core;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.Persistence;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.VoxelTypes
{
    [Serializable]
    public abstract class
        PersistentVoxelsBase<TVoxelData, TDataStore, TRaycastHit> : VoxelsBase<TVoxelData, TRaycastHit>
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TDataStore, TRaycastHit>
        where TDataStore : VoxelPersistentDataStoreBase<TVoxelData, TDataStore, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        protected PersistentVoxelsBase(string identifier)
        {
            this.identifier = identifier;
        }

        #region Fields and Autoproperties

        [SerializeField] public string identifier;
        [SerializeField] public TDataStore dataStore;

        #endregion

        public override bool IsPersistent => true;

        public override void InitializeDataStore()
        {
            dataStore.Record((TVoxelData) this);
        }

        public override void OnInitialize()
        {
#if UNITY_EDITOR
            if (dataStore == null)
            {
                dataStore = AppalachiaObjectFactory.LoadExistingOrCreateNewAsset<TDataStore>(identifier);
            }
#endif
        }

        public void RestoreFromDataStore(
            Transform transform,
            Collider[] colliders,
            MeshRenderer[] renderers,
            float activeRatio = 1.0f)
        {
            dataStore.Restore((TVoxelData) this);
            _transform = transform;
            this.colliders = colliders;
            this.renderers = renderers;
            CalculateVoxelActiveRatio();

            if (Math.Abs(this.activeRatio - activeRatio) > float.Epsilon)
            {
                UpdateVoxelActiveRatio(activeRatio);
            }

            Initialize(transform);
        }
    }
}
