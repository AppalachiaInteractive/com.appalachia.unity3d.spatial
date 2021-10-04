#region

using System;
using Appalachia.Base.Scriptables;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.Persistence;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.VoxelTypes
{
    [Serializable]
    public abstract class
        PersistentVoxelsBase<TVoxelData, TDataStore, TRaycastHit> : VoxelsBase<TVoxelData,
            TRaycastHit>
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TDataStore, TRaycastHit>
        where TDataStore : VoxelPersistentDataStoreBase<TVoxelData, TDataStore, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        [SerializeField] public string identifier;
        [SerializeField] public TDataStore dataStore;

        protected PersistentVoxelsBase(string identifier)
        {
            this.identifier = identifier;
        }

        public override bool IsPersistent => true;

        public override void OnInitialize()
        {
            if (dataStore == null)
            {
                dataStore = SelfSavingScriptableObject<TDataStore>.LoadOrCreateNew(identifier);
            }
        }

        public override void InitializeDataStore()
        {
            dataStore.Record((TVoxelData) this);
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

            if (this.activeRatio != activeRatio)
            {
                UpdateVoxelActiveRatio(activeRatio);
            }

            Initialize(transform);
        }
    }
}
