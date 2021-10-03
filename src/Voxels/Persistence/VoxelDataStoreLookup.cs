#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Scriptables;
using Appalachia.Voxels.Casting;
using Appalachia.Voxels.VoxelTypes;

#endregion

namespace Appalachia.Voxels.Persistence
{
    [Serializable]
    public abstract class
        VoxelDataStoreLookup<TVoxelData, TRaycastHit, TLookup, TIndex, TDataStore, TDataStoreList> : ScriptableObjectLookupCollection<TLookup, TIndex, string, TDataStore,
            AppaList_string, TDataStoreList>
        where TLookup : ScriptableObjectLookupCollection<TLookup, TIndex, string, TDataStore, AppaList_string, TDataStoreList>
        where TIndex : AppaLookup<string, TDataStore, AppaList_string, TDataStoreList>, new()
        where TDataStoreList : AppaList<TDataStore>, new()
        where TDataStore : VoxelPersistentDataStoreBase<TVoxelData, TDataStore, TRaycastHit>
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TDataStore, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        protected override string GetUniqueKeyFromValue(TDataStore value)
        {
            return value.identifier;
        }
    }
}
