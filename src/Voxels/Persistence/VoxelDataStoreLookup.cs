#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;

#endregion

namespace Appalachia.Spatial.Voxels.Persistence
{
    [Serializable]
    public abstract class
        VoxelDataStoreLookup<TVoxelData, TRaycastHit, TThis, TLookup, TValue, TValueList> :
            AppalachiaObjectLookupCollection<string, TValue, stringList, TValueList, TLookup, TThis>
        where TLookup : AppaLookup<string, TValue, stringList, TValueList>, new()
        where TValue : VoxelPersistentDataStoreBase<TVoxelData, TValue, TRaycastHit>
        where TValueList : AppaList<TValue>, new()
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TValue, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
        where TThis : VoxelDataStoreLookup<TVoxelData, TRaycastHit, TThis, TLookup, TValue, TValueList>
    {
        public override bool HasDefault => false;

        protected override string GetUniqueKeyFromValue(TValue value)
        {
            return value.identifier;
        }
    }
}
