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
        private static bool _vspMeshBurialEnabled;

        private static readonly VegetationSystemPro.MultiOnVegetationCellSpawnedDelegate
            _enqueueCell = c => EnqueueCell(c);

        private static readonly VegetationSystemPro.MultiOnVegetationStudioRefreshDelegate
            _refreshSystem = RequeueAllCells;

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.External.Base + "Enable", true)]
        public static bool ToggleEnableVSPMeshBurialsValidate()
        {
            Menu.SetChecked(PKG.Menu.Appalachia.External.Base + "Enable", _vspMeshBurialEnabled);
            return true;
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.External.Base + "Enable", priority = PKG.Menu.Appalachia.External.Priority)]
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

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.External.Base + "Disable", priority = PKG.Menu.Appalachia.External.Priority + 1)]
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

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Manage.Base + "Refresh", priority = PKG.Menu.Appalachia.Manage.Priority + 2)]
        public static void Refresh()
        {
            QUEUES.pendingVegetationKeys.Clear();
            RequeueAllCells(_vegetationSystem);

            //RefreshPrefabRenderingSets();
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Manage.Base + "Refresh and Start", priority = PKG.Menu.Appalachia.Manage.Priority + 4)]
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

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Manage.Base + "Execute Full Reset", priority = PKG.Menu.Appalachia.Manage.Priority + 6)]
        public static void Reset()
        {
            MeshBurialExecutionManager.EnsureCompleted();
            MeshBurialAdjustmentCollection.instance.Reset();
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Manage.Base + "Reset and Refresh", priority = PKG.Menu.Appalachia.Manage.Priority + 8)]
        public static void ResetRefresh()
        {
            Reset();
            _vegetationSystem.ClearCache();
            MeshBurialExecutionManager.EnableMeshBurials(true);
            Refresh();
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Manage.Base + "Force Save", priority = PKG.Menu.Appalachia.Manage.Priority + 10)]
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
