#region

using System;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;
using Unity.Collections;

#endregion

namespace Appalachia.Spatial.Voxels.Persistence
{
    [Serializable]
    public abstract class
        VoxelPersistentElementsDataStore<TVoxelData, TDataStore, TElement> : VoxelPersistentDataStoreBase<TVoxelData, TDataStore,
            VoxelRaycastHit<TElement>>
        where TVoxelData : PersistentVoxelsElementsBase<TVoxelData, TDataStore, TElement>
        where TDataStore : VoxelPersistentElementsDataStore<TVoxelData, TDataStore, TElement>
        where TElement : struct
    {
        public TElement[] elementDatas;

        protected override void RecordAdditional(TVoxelData data)
        {
            elementDatas = data.elementDatas.ToArray();
        }

        protected override void RestoreAdditional(TVoxelData data)
        {
            data.elementDatas = new NativeArray<TElement>(elementDatas, Allocator.Persistent);
        }
    }
}
