#region

using System;
using Appalachia.Spatial.MeshBurial.State;

#endregion

namespace Appalachia.Core.Collections.Implementations.Lists
{
    [Serializable]
    public sealed class AppaList_MeshBurialAdjustmentState : AppaList<MeshBurialAdjustmentState>
    {
        public AppaList_MeshBurialAdjustmentState()
        {
        }

        public AppaList_MeshBurialAdjustmentState(int capacity, float capacityIncreaseMultiplier = 2, bool noTracking = false) : base(
            capacity,
            capacityIncreaseMultiplier,
            noTracking
        )
        {
        }

        public AppaList_MeshBurialAdjustmentState(AppaList<MeshBurialAdjustmentState> list) : base(list)
        {
        }

        public AppaList_MeshBurialAdjustmentState(MeshBurialAdjustmentState[] values) : base(values)
        {
        }
    }
}
