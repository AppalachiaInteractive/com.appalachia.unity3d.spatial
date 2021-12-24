#if UNITY_EDITOR
using Appalachia.Core.Objects.Scriptables;

namespace Appalachia.Spatial.Voxels.Gizmos
{
    public class MainVoxelDataGizmoSettingsCollection : SingletonAppalachiaObjectLookupCollection<
        VoxelDataGizmoStyle, VoxelDataGizmoSettings, VoxelDataGizmoStyleList, VoxelDataGizmoSettingsList,
        VoxelDataGizmoSettingsLookup, VoxelDataGizmoSettingsCollection, MainVoxelDataGizmoSettingsCollection>
    {
    }
}

#endif
