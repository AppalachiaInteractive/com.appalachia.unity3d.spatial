#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Spatial.MeshBurial.State;
using Appalachia.Utility.Strings;
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
        /// <inheritdoc />
        protected override Color GetDisplayColor(GameObject key, MeshBurialAdjustmentState value)
        {
            return Color.white;
        }

        /// <inheritdoc />
        protected override string GetDisplaySubtitle(GameObject key, MeshBurialAdjustmentState value)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        protected override string GetDisplayTitle(GameObject key, MeshBurialAdjustmentState value)
        {
            return ZString.Format("Prefab: {0}", key.name);
        }
    }
}

#endif
