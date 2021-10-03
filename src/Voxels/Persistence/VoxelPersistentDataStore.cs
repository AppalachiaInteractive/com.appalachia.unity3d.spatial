#region

using System;
using Appalachia.Voxels.Casting;
using Appalachia.Voxels.VoxelTypes;

#endregion

namespace Appalachia.Voxels.Persistence
{
    [Serializable]
    public sealed class VoxelPersistentDataStore : VoxelPersistentDataStoreBase<PersistentVoxels, VoxelPersistentDataStore, VoxelRaycastHit>
    {
        protected override void RecordAdditional(PersistentVoxels data)
        {
        }

        protected override void RestoreAdditional(PersistentVoxels data)
        {
        }
    }
}
