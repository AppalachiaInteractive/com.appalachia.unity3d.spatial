#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Spatial.MeshBurial.State;

#endregion

namespace Appalachia.Spatial.MeshBurial.Collections
{
    [Serializable]
    public sealed class
        AppaList_MeshBurialAdjustmentEntryWrapper : AppaList<MeshBurialAdjustmentEntryWrapper>
    {
        public AppaList_MeshBurialAdjustmentEntryWrapper()
        {
        }

        public AppaList_MeshBurialAdjustmentEntryWrapper(
            int capacity,
            float capacityIncreaseMultiplier = 2,
            bool noTracking = false) : base(capacity, capacityIncreaseMultiplier, noTracking)
        {
        }

        public AppaList_MeshBurialAdjustmentEntryWrapper(
            AppaList<MeshBurialAdjustmentEntryWrapper> list) : base(list)
        {
        }

        public AppaList_MeshBurialAdjustmentEntryWrapper(MeshBurialAdjustmentEntryWrapper[] values)
            : base(values)
        {
        }
    }
}
