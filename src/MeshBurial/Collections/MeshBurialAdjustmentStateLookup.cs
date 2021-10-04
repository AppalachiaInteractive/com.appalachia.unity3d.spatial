#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Spatial.MeshBurial.State;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Collections
{
    [Serializable]
    [ListDrawerSettings(
        Expanded = true,
        DraggableItems = false,
        HideAddButton = true,
        HideRemoveButton = true,
        NumberOfItemsPerPage = 5
    )]
    public class MeshBurialAdjustmentStateLookup : AppaLookup<GameObject, MeshBurialAdjustmentState,
        AppaList_GameObject, AppaList_MeshBurialAdjustmentState>
    {
        protected override string GetDisplayTitle(GameObject key, MeshBurialAdjustmentState value)
        {
            return $"Prefab: {key.name}";
        }

        protected override string GetDisplaySubtitle(
            GameObject key,
            MeshBurialAdjustmentState value)
        {
            return string.Empty;
        }

        protected override Color GetDisplayColor(GameObject key, MeshBurialAdjustmentState value)
        {
            return Color.white;
        }
    }
}
