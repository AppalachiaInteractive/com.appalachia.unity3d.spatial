#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Spatial.Collections;
using Appalachia.Spatial.MeshBurial.State;
using Appalachia.Spatial.SpatialKeys;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Collections
{
    [Serializable]
    [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 5),
     HideReferenceObjectPicker]
    public class MeshBurialAdjustmentEntryWrapperLookup : AppaLookup<Matrix4x4Key, MeshBurialAdjustmentEntryWrapper, AppaList_Matrix4x4Key,
        AppaList_MeshBurialAdjustmentEntryWrapper>
    {
        protected override bool ShouldDisplayTitle => true;

        protected override string GetDisplayTitle(Matrix4x4Key key, MeshBurialAdjustmentEntryWrapper value)
        {
            return key.ToString();
        }

        protected override string GetDisplaySubtitle(Matrix4x4Key key, MeshBurialAdjustmentEntryWrapper value)
        {
            return value.ToString();
        }

        protected override Color GetDisplayColor(Matrix4x4Key key, MeshBurialAdjustmentEntryWrapper value)
        {
            return Color.white;
        }
    }
}
