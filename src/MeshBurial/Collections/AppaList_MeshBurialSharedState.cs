#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Spatial.MeshBurial.State;

#endregion

namespace Appalachia.Spatial.MeshBurial.Collections
{
    [Serializable]
    public sealed class AppaList_MeshBurialSharedState : AppaList<MeshBurialSharedState>
    {
        public AppaList_MeshBurialSharedState()
        {
        }

        public AppaList_MeshBurialSharedState(
            int capacity,
            float capacityIncreaseMultiplier = 2,
            bool noTracking = false) : base(capacity, capacityIncreaseMultiplier, noTracking)
        {
        }

        public AppaList_MeshBurialSharedState(AppaList<MeshBurialSharedState> list) : base(list)
        {
        }

        public AppaList_MeshBurialSharedState(MeshBurialSharedState[] values) : base(values)
        {
        }
    }
}

#endif