#region

using System;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;

#endregion

namespace Appalachia.Spatial.Voxels.Persistence
{
    [Serializable]
    public sealed class VoxelPersistentDataStore : VoxelPersistentDataStoreBase<PersistentVoxels,
        VoxelPersistentDataStore, VoxelRaycastHit>
    {
        protected override void RecordAdditional(PersistentVoxels data)
        {
        }

        protected override void RestoreAdditional(PersistentVoxels data)
        {
        }
    }
}
