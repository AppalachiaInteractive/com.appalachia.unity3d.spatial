#if UNITY_EDITOR

#region

using Appalachia.CI.Constants;
using Appalachia.CI.Integration.Assets;
using Appalachia.Spatial.MeshBurial.State;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public static partial class MeshBurialManagementProcessor
    {
        public const string MENU_BASE = "Tools/Mesh Burial/";
        public const string MENU_VSP = MENU_BASE + "VSP/";

        private const string MENU_VSP_ENABLE = MENU_VSP + "Enable VSP Mesh Burial";
        private const string MENU_VSP_DISABLE = MENU_VSP + "Disable VSP Items";

        private const string MENU_RESET = MENU_BASE + "Manage/";

        private static bool _vspMeshBurialEnabled;

        private static readonly VegetationSystemPro.MultiOnVegetationCellSpawnedDelegate
            _enqueueCell = c => EnqueueCell(c);

        private static readonly VegetationSystemPro.MultiOnVegetationStudioRefreshDelegate
            _refreshSystem = RequeueAllCells;

        [MenuItem(MENU_VSP_ENABLE, true)]
        public static bool ToggleEnableVSPMeshBurialsValidate()
        {
            Menu.SetChecked(MENU_VSP_ENABLE, _vspMeshBurialEnabled);
            return true;
        }

        [MenuItem(MENU_VSP_ENABLE, false, APPA_MENU.TOOLS.MESH_BURY.ENABLE_VSP)]
        public static void ToggleEnableVSPMeshBurials()
        {
            _vspMeshBurialEnabled = !_vspMeshBurialEnabled;

            if (_vegetationSystem == null)
            {
                _vegetationSystem = Object.FindObjectOfType<VegetationSystemPro>();
            }

            if (_vegetationSystem != null)
            {
                if (_vspMeshBurialEnabled)
                {
                    _vegetationSystem.OnVegetationCellLoadCompleted += _enqueueCell;
                    _vegetationSystem.OnRefreshVegetationSystemDelegate += _refreshSystem;
                }
                else
                {
                    _vegetationSystem.OnVegetationCellLoadCompleted -= _enqueueCell;
                    _vegetationSystem.OnRefreshVegetationSystemDelegate -= _refreshSystem;
                }
            }
        }

        [MenuItem(MENU_VSP_DISABLE, false, APPA_MENU.TOOLS.MESH_BURY.ENABLE_VSP)]
        public static void DisableVSPItems()
        {
            for (var i = 0; i < _vegetationSystem.VegetationPackageProList.Count; i++)
            {
                var package = _vegetationSystem.VegetationPackageProList[i];

                for (var j = 0; j < package.VegetationInfoList.Count; j++)
                {
                    var info = package.VegetationInfoList[j];

                    info.EnableMeshBurying = false;
                }
            }
        }

        [MenuItem(MENU_RESET + "Refresh", false, APPA_MENU.TOOLS.MESH_BURY.RESET)]
        public static void Refresh()
        {
            QUEUES.pendingVegetationKeys.Clear();
            RequeueAllCells(_vegetationSystem);

            //RefreshPrefabRenderingSets();
        }

        [MenuItem(MENU_RESET + "Refresh and Start", false, APPA_MENU.TOOLS.MESH_BURY.RESET)]
        public static void RefreshAndStart()
        {
            QUEUES.pendingVegetationKeys.Clear();
            RequeueAllCells(_vegetationSystem);

            //RefreshPrefabRenderingSets();

            if (!MeshBurialExecutionManager._BURY.v)
            {
                MeshBurialExecutionManager.EnableMeshBurials();
            }
        }

        [MenuItem(MENU_RESET + "Execute Full Reset", false, APPA_MENU.TOOLS.MESH_BURY.RESET)]
        public static void Reset()
        {
            MeshBurialExecutionManager.EnsureCompleted();
            MeshBurialAdjustmentCollection.instance.Reset();
        }

        [MenuItem(MENU_RESET + "Reset and Refresh", false, APPA_MENU.TOOLS.MESH_BURY.RESET)]
        public static void ResetRefresh()
        {
            Reset();
            _vegetationSystem.ClearCache();
            MeshBurialExecutionManager.EnableMeshBurials(true);
            Refresh();
        }

        [MenuItem(MENU_BASE + "Force Save", false, APPA_MENU.TOOLS.MESH_BURY.FORCE_SAVE)]
        public static void ForceSave()
        {
            var collection = MeshBurialAdjustmentCollection.instance;

            for (var i = 0; i < collection.State.Count; i++)
            {
                collection.State.at[i].SetDirty();
            }

            MeshBurialAdjustmentCollection.instance.SetDirty();
            AssetDatabaseManager.SaveAssets();
        }
    }
}

#endif
