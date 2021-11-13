#if UNITY_EDITOR

#region

using Appalachia.Core.Scriptables;

#endregion

namespace Appalachia.Spatial.Voxels.Gizmos
{
    public class VoxelDataGizmoSettingsLookup : AppalachiaObjectLookupCollection<
        VoxelDataGizmoSettingsLookup, VoxelDataGizmoSettingsIndex, VoxelDataGizmoStyle,
        VoxelDataGizmoSettings, AppaList_VoxelDataGizmoStyle, AppaList_VoxelDataGizmoSettings>
    {
        protected override VoxelDataGizmoStyle GetUniqueKeyFromValue(VoxelDataGizmoSettings value)
        {
            return value.style;
        }
    }
}

#endif