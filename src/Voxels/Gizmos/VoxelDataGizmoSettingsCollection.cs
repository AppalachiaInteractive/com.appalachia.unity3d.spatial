#if UNITY_EDITOR

#region

using Appalachia.Core.Scriptables;

#endregion

namespace Appalachia.Spatial.Voxels.Gizmos
{
    public class VoxelDataGizmoSettingsCollection : AppalachiaObjectLookupCollection<VoxelDataGizmoStyle,
        VoxelDataGizmoSettings, AppaList_VoxelDataGizmoStyle, AppaList_VoxelDataGizmoSettings,
        VoxelDataGizmoSettingsLookup, VoxelDataGizmoSettingsCollection>
    {
        public override bool HasDefault => false;

        protected override VoxelDataGizmoStyle GetUniqueKeyFromValue(VoxelDataGizmoSettings value)
        {
            return value.style;
        }
    }
}

#endif
