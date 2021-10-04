#region

using Appalachia.Core.Collections;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Gizmos
{
    public class VoxelDataGizmoSettingsIndex : AppaLookup<VoxelDataGizmoStyle, VoxelDataGizmoSettings, AppaList_VoxelDataGizmoStyle,
        AppaList_VoxelDataGizmoSettings>
    {
        protected override string GetDisplayTitle(VoxelDataGizmoStyle key, VoxelDataGizmoSettings value)
        {
            return key.ToString();
        }

        protected override string GetDisplaySubtitle(VoxelDataGizmoStyle key, VoxelDataGizmoSettings value)
        {
            return value.name;
        }

        protected override Color GetDisplayColor(VoxelDataGizmoStyle key, VoxelDataGizmoSettings value)
        {
            return Color.white;
        }
    }
}
