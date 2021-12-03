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
    public abstract class VoxelDataStoreLookup<TVoxelData, TRaycastHit, TThis, TLookup, TValue,
                                               TValueList> : AppalachiaObjectLookupCollection<
        string, TValue, AppaList_string, TValueList, TLookup, TThis>
        where TThis : AppalachiaObjectLookupCollection<string, TValue,
            AppaList_string, TValueList, TLookup, TThis>
        where TLookup : AppaLookup<string, TValue, AppaList_string, TValueList>, new()
        where TValue : VoxelPersistentDataStoreBase<TVoxelData, TValue, TRaycastHit>
        where TValueList : AppaList<TValue>, new()
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TValue, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        protected override string GetUniqueKeyFromValue(TValue value)
        {
            return value.identifier;
        }

        public override bool HasDefault => false;
    }
}
