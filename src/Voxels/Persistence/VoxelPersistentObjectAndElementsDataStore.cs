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
        VoxelPersistentObjectAndElementsDataStore<TVoxelData, TDataStore, TObject, TElement> : VoxelPersistentDataStoreBase<TVoxelData, TDataStore,
            VoxelRaycastHit<TElement>>
        where TVoxelData : PersistentVoxelsObjectAndElementsBase<TVoxelData, TDataStore, TObject, TElement>
        where TDataStore : VoxelPersistentObjectAndElementsDataStore<TVoxelData, TDataStore, TObject, TElement>
        where TObject : IVoxelsInit, new()
        where TElement : struct
    {
        public TObject objectData;
        public TElement[] elementDatas;

        protected override void RecordAdditional(TVoxelData data)
        {
            objectData = data.objectData;
            elementDatas = data.elementDatas.ToArray();
        }

        protected override void RestoreAdditional(TVoxelData data)
        {
            data.objectData = objectData;
            data.elementDatas = new NativeArray<TElement>(elementDatas, Allocator.Persistent);
        }
    }
}
