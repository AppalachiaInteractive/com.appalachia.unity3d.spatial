#if UNITY_EDITOR

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
    public class MeshBurialSharedStateLookup : AppaLookup<int, MeshBurialSharedState, intList,
        AppaList_MeshBurialSharedState>
    {
        /// <inheritdoc />
        protected override Color GetDisplayColor(int key, MeshBurialSharedState value)
        {
            return Color.white;
        }

        /// <inheritdoc />
        protected override string GetDisplaySubtitle(int key, MeshBurialSharedState value)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        protected override string GetDisplayTitle(int key, MeshBurialSharedState value)
        {
            return value.meshObject.mesh.name;
        }
    }
}

#endif
