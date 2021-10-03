#region

using System;
using Appalachia.Voxels.Casting;
using Appalachia.Voxels.VoxelTypes;
using Unity.Collections;

#endregion

namespace Appalachia.Voxels.Persistence
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
