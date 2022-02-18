#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Collections;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Gizmos
{
    [Serializable]
    public class VoxelDataGizmoSettingsLookup : AppaLookup<VoxelDataGizmoStyle, VoxelDataGizmoSettings,
        VoxelDataGizmoStyleList, VoxelDataGizmoSettingsList>
    {
        /// <inheritdoc />
        protected override Color GetDisplayColor(VoxelDataGizmoStyle key, VoxelDataGizmoSettings value)
        {
            return Color.white;
        }

        /// <inheritdoc />
        protected override string GetDisplaySubtitle(VoxelDataGizmoStyle key, VoxelDataGizmoSettings value)
        {
            return value.name;
        }

        /// <inheritdoc />
        protected override string GetDisplayTitle(VoxelDataGizmoStyle key, VoxelDataGizmoSettings value)
        {
            return key.ToString();
        }
    }
}

#endif
