#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Scriptables;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;

#endregion

namespace Appalachia.Spatial.Voxels.Persistence
{
    [Serializable]
    public abstract class VoxelDataStoreLookup<TVoxelData, TRaycastHit, TLookup, TIndex, TDataStore,
                                               TDataStoreList> : AppalachiaObjectLookupCollection<
        TLookup, TIndex, string, TDataStore, AppaList_string, TDataStoreList>
        where TLookup : AppalachiaObjectLookupCollection<TLookup, TIndex, string, TDataStore,
            AppaList_string, TDataStoreList>
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
