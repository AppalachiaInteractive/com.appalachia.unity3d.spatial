#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Objects.Scriptables;

#endregion

namespace Appalachia.Spatial.Voxels.Gizmos
{
    [Serializable]
    public class VoxelDataGizmoSettingsCollection : AppalachiaObjectLookupCollection<VoxelDataGizmoStyle,
        VoxelDataGizmoSettings, VoxelDataGizmoStyleList, VoxelDataGizmoSettingsList,
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
