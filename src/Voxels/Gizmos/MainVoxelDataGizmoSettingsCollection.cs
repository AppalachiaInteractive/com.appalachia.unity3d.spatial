#if UNITY_EDITOR
using Appalachia.Core.Objects.Scriptables;

namespace Appalachia.Spatial.Voxels.Gizmos
{
    public class MainVoxelDataGizmoSettingsCollection : SingletonAppalachiaObjectLookupCollection<
        VoxelDataGizmoStyle, VoxelDataGizmoSettings, VoxelDataGizmoStyleList, VoxelDataGizmoSettingsList,
        VoxelDataGizmoSettingsLookup, VoxelDataGizmoSettingsCollection, MainVoxelDataGizmoSettingsCollection>
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem(
            PKG.Menu.Assets.Base + nameof(MainVoxelDataGizmoSettingsCollection),
            priority = PKG.Menu.Assets.Priority
        )]
        public static void CreateAsset()
        {
            CreateNew<MainVoxelDataGizmoSettingsCollection>();
        }
#endif
    }
}

#endif
