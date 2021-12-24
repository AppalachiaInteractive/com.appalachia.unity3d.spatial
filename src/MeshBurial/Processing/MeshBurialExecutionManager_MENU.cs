#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Preferences;
using Unity.Mathematics;

// ReSharper disable CompareOfFloatsByEqualityOperator

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public partial class MeshBurialExecutionManager
    {
        #region PKG.Menu.Appalachia.Enable

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Tools.Enable.Base, true)]
        private static bool ToggleEnableMeshBurialsValidate()
        {
            if (!instance.IsBuryingEnabled.IsAwake)
            {
                return false;
            }

            UnityEditor.Menu.SetChecked(
                PKG.Menu.Appalachia.Tools.Enable.Base,
                instance.IsBuryingEnabled.Value
            );
            return true;
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Tools.Enable.Base, priority = PKG.Priority + 1)]
        public static void ToggleEnableMeshBurials()
        {
            instance.EnableMeshBurials();
        }

        public static void InitializeEnableMeshBurials()
        {
            if (!instance.IsBuryingEnabled.IsAwake)
            {
                UnityEditor.EditorApplication.delayCall += InitializeEnableMeshBurials;
                return;
            }

            if (instance.IsBuryingEnabled.v)
            {
                instance.EnableMeshBurials(true);
            }
        }

        public void EnableMeshBurials(bool force = false)
        {
            if (!IsBuryingEnabled.IsAwake)
            {
                return;
            }

            IsBuryingEnabled.Value = force || !IsBuryingEnabled.Value;

            if (IsBuryingEnabled.Value)
            {
                _processed = 0;
                enabled = true;
            }
            else
            {
                enabled = false;

                pendingHandle.Complete();

                if ((resultData != null) && !resultDataFinalized)
                {
                    ApplyFinalizedResults();
                }
            }
        }

        #endregion

        #region MENU_DEBUGLOG_

        [NonSerialized]
        public static readonly PREF<bool> _DEBUGLOG = PREFS.REG(
            PKG.Prefs.Group,
            "Enable Debug Logging",
            false
        );

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Logging.Base, true)]
        private static bool MENU_DEBUGLOG_VALIDATE()
        {
            UnityEditor.Menu.SetChecked(PKG.Menu.Appalachia.Logging.Base, _DEBUGLOG.Value);
            return true;
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Logging.Base, priority = PKG.Priority)]
        public static void MENU_DEBUGLOG()
        {
            _DEBUGLOG.v = !_DEBUGLOG.v;
        }

        #endregion

        #region MENU_DEBUGZEROLOG_

        private const string MENU_DEBUGZEROLOG_ =
            PKG.Menu.Appalachia.Logging.Base + "Enable Zero Matrix Logging";

        [NonSerialized]
        public static readonly PREF<bool> _DEBUGZEROLOG = PREFS.REG(
            PKG.Prefs.Group,
            "Enable Zero Matrix Logging",
            false
        );

        [UnityEditor.MenuItem(MENU_DEBUGZEROLOG_, true)]
        private static bool MENU_DEBUGZEROLOG_VALIDATE()
        {
            UnityEditor.Menu.SetChecked(MENU_DEBUGZEROLOG_, _DEBUGZEROLOG.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_DEBUGZEROLOG_, priority = PKG.Menu.Appalachia.Logging.Priority)]
        public static void MENU_DEBUGZEROLOG()
        {
            _DEBUGZEROLOG.v = !_DEBUGZEROLOG.v;
        }

        #endregion

        #region MENU_JOBS_MESH_NORMALS_

        private const string MENU_JOBS_MESH_NORMALS_ =
            PKG.Menu.Appalachia.Jobs.Base + "Account for Mesh Normals";

        [NonSerialized]
        public static readonly PREF<bool> _MESH_NORMALS = PREFS.REG(
            PKG.Prefs.Group,
            "Account Mesh Normals",
            true
        );

        [UnityEditor.MenuItem(MENU_JOBS_MESH_NORMALS_, true)]
        private static bool MENU_JOBS_MESH_NORMALS_VALIDATE()
        {
            if (!_MESH_NORMALS.IsAwake)
            {
                return false;
            }

            UnityEditor.Menu.SetChecked(MENU_JOBS_MESH_NORMALS_, _MESH_NORMALS.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_JOBS_MESH_NORMALS_, priority = PKG.Menu.Appalachia.Jobs.Priority)]
        private static void MENU_JOBS_MESH_NORMALS()
        {
            if (!_MESH_NORMALS.IsAwake)
            {
                return;
            }

            _MESH_NORMALS.Value = !_MESH_NORMALS.Value;
        }

        #endregion

        #region MENU_JOBS_TERRAIN_NORMALS_

        private const string MENU_JOBS_TERRAIN_NORMALS_ =
            PKG.Menu.Appalachia.Jobs.Base + "Compensate for Terrain Normals";

        [NonSerialized]
        public static readonly PREF<bool> _TERRAIN_NORMALS = PREFS.REG(
            PKG.Prefs.Group,
            "Terrain Normals",
            true
        );

        [UnityEditor.MenuItem(MENU_JOBS_TERRAIN_NORMALS_, true)]
        private static bool MENU_JOBS_TERRAIN_NORMALS_VALIDATE()
        {
            if (!_TERRAIN_NORMALS.IsAwake)
            {
                return false;
            }

            UnityEditor.Menu.SetChecked(MENU_JOBS_TERRAIN_NORMALS_, _TERRAIN_NORMALS.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_JOBS_TERRAIN_NORMALS_, priority = PKG.Menu.Appalachia.Jobs.Priority)]
        private static void MENU_JOBS_TERRAIN_NORMALS()
        {
            if (!_TERRAIN_NORMALS.IsAwake)
            {
                return;
            }

            _TERRAIN_NORMALS.Value = !_TERRAIN_NORMALS.Value;
        }

        #endregion

        #region MENU_JOBS_HEIGHTS_

        private const string MENU_JOBS_HEIGHTS_ = PKG.Menu.Appalachia.Jobs.Base + "Adjust Height To Terrain";

        [NonSerialized]
        public static readonly PREF<bool> _HEIGHT = PREFS.REG(PKG.Prefs.Group, "Adjust Height", true);

        [UnityEditor.MenuItem(MENU_JOBS_HEIGHTS_, true)]
        private static bool MENU_JOBS_HEIGHTS_VALIDATE()
        {
            if (!_HEIGHT.IsAwake)
            {
                return false;
            }

            UnityEditor.Menu.SetChecked(MENU_JOBS_HEIGHTS_, _HEIGHT.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_JOBS_HEIGHTS_, priority = PKG.Menu.Appalachia.Jobs.Priority)]
        private static void MENU_JOBS_HEIGHTS()
        {
            if (!_HEIGHT.IsAwake)
            {
                return;
            }

            _HEIGHT.Value = !_HEIGHT.Value;
        }

        #endregion

        #region MENU_JOBS_PARAMS_

        private const string MENU_JOBS_PARAMS_ =
            PKG.Menu.Appalachia.Jobs.Base + "Apply Parameterized Rotation";

        [NonSerialized]
        public static readonly PREF<bool> _PARAMS = PREFS.REG(PKG.Prefs.Group, "Apply Params", true);

        [UnityEditor.MenuItem(MENU_JOBS_PARAMS_, true)]
        private static bool MENU_JOBS_PARAMS_VALIDATE()
        {
            if (!_PARAMS.IsAwake)
            {
                return false;
            }

            UnityEditor.Menu.SetChecked(MENU_JOBS_PARAMS_, _PARAMS.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_JOBS_PARAMS_, priority = PKG.Menu.Appalachia.Jobs.Priority)]
        private static void MENU_JOBS_PARAMS()
        {
            if (!_PARAMS.IsAwake)
            {
                return;
            }

            _PARAMS.Value = !_PARAMS.Value;
        }

        #endregion

        #region MENU_JOBS_TESTS_

        private const string MENU_JOBS_TESTS_ = PKG.Menu.Appalachia.Jobs.Base + "Apply Fixed Test Value";

        [NonSerialized]
        public static readonly PREF<bool> _TEST = PREFS.REG(PKG.Prefs.Group, "Apply Fixed Test Value", false);

        [UnityEditor.MenuItem(MENU_JOBS_TESTS_, true)]
        private static bool MENU_JOBS_TESTS_validate()
        {
            if (!_TEST.IsAwake)
            {
                return false;
            }

            UnityEditor.Menu.SetChecked(MENU_JOBS_TESTS_, _TEST.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_JOBS_TESTS_, priority = PKG.Menu.Appalachia.Jobs.Priority)]
        private static void MENU_JOBS_TESTS()
        {
            if (!_TEST.IsAwake)
            {
                return;
            }

            _TEST.Value = !_TEST.Value;
        }

        #endregion

        #region Fixed Test Value

        [NonSerialized]
        public static readonly PREF<float3> _TEST_VALUE = PREFS.REG(
            PKG.Prefs.Group,
            "Fixed Test Value",
            float3.zero
        );

        private const string _TEST_M = PKG.Menu.Appalachia.Jobs.Base + "Test Value/";

        private static readonly float3 _TS_z0_z0_z0v = new(0, 0, 0);
        private static readonly float3 _TS_z0_z0_p1v = new(0, 0, +1);
        private static readonly float3 _TS_z0_z0_n1v = new(0, 0, -1);
        private static readonly float3 _TS_z0_p1_z0v = new(0, +1, 0);
        private static readonly float3 _TS_z0_p1_p1v = new(0, +1, +1);
        private static readonly float3 _TS_z0_p1_n1v = new(0, +1, -1);
        private static readonly float3 _TS_z0_n1_z0v = new(0, -1, 0);
        private static readonly float3 _TS_z0_n1_p1v = new(0, -1, +1);
        private static readonly float3 _TS_z0_n1_n1v = new(0, -1, -1);
        private static readonly float3 _TS_p1_z0_z0v = new(+1, 0, 0);
        private static readonly float3 _TS_p1_z0_p1v = new(+1, 0, +1);
        private static readonly float3 _TS_p1_z0_n1v = new(+1, 0, -1);
        private static readonly float3 _TS_p1_p1_z0v = new(+1, +1, 0);
        private static readonly float3 _TS_p1_p1_p1v = new(+1, +1, +1);
        private static readonly float3 _TS_p1_p1_n1v = new(+1, +1, -1);
        private static readonly float3 _TS_p1_n1_z0v = new(+1, -1, 0);
        private static readonly float3 _TS_p1_n1_p1v = new(+1, -1, +1);
        private static readonly float3 _TS_p1_n1_n1v = new(+1, -1, -1);
        private static readonly float3 _TS_n1_z0_z0v = new(-1, 0, 0);
        private static readonly float3 _TS_n1_z0_p1v = new(-1, 0, +1);
        private static readonly float3 _TS_n1_z0_n1v = new(-1, 0, -1);
        private static readonly float3 _TS_n1_p1_z0v = new(-1, +1, 0);
        private static readonly float3 _TS_n1_p1_p1v = new(-1, +1, +1);
        private static readonly float3 _TS_n1_p1_n1v = new(-1, +1, -1);
        private static readonly float3 _TS_n1_n1_z0v = new(-1, -1, 0);
        private static readonly float3 _TS_n1_n1_p1v = new(-1, -1, +1);

        private static bool _TS_z0_z0_z0 => _TEST_VALUE.Value.Equals(_TS_z0_z0_z0v);
        private static bool _TS_z0_z0_p1 => _TEST_VALUE.Value.Equals(_TS_z0_z0_p1v);
        private static bool _TS_z0_z0_n1 => _TEST_VALUE.Value.Equals(_TS_z0_z0_n1v);
        private static bool _TS_z0_p1_z0 => _TEST_VALUE.Value.Equals(_TS_z0_p1_z0v);
        private static bool _TS_z0_p1_p1 => _TEST_VALUE.Value.Equals(_TS_z0_p1_p1v);
        private static bool _TS_z0_p1_n1 => _TEST_VALUE.Value.Equals(_TS_z0_p1_n1v);
        private static bool _TS_z0_n1_z0 => _TEST_VALUE.Value.Equals(_TS_z0_n1_z0v);
        private static bool _TS_z0_n1_p1 => _TEST_VALUE.Value.Equals(_TS_z0_n1_p1v);
        private static bool _TS_z0_n1_n1 => _TEST_VALUE.Value.Equals(_TS_z0_n1_n1v);
        private static bool _TS_p1_z0_z0 => _TEST_VALUE.Value.Equals(_TS_p1_z0_z0v);
        private static bool _TS_p1_z0_p1 => _TEST_VALUE.Value.Equals(_TS_p1_z0_p1v);
        private static bool _TS_p1_z0_n1 => _TEST_VALUE.Value.Equals(_TS_p1_z0_n1v);
        private static bool _TS_p1_p1_z0 => _TEST_VALUE.Value.Equals(_TS_p1_p1_z0v);
        private static bool _TS_p1_p1_p1 => _TEST_VALUE.Value.Equals(_TS_p1_p1_p1v);
        private static bool _TS_p1_p1_n1 => _TEST_VALUE.Value.Equals(_TS_p1_p1_n1v);
        private static bool _TS_p1_n1_z0 => _TEST_VALUE.Value.Equals(_TS_p1_n1_z0v);
        private static bool _TS_p1_n1_p1 => _TEST_VALUE.Value.Equals(_TS_p1_n1_p1v);
        private static bool _TS_p1_n1_n1 => _TEST_VALUE.Value.Equals(_TS_p1_n1_n1v);
        private static bool _TS_n1_z0_z0 => _TEST_VALUE.Value.Equals(_TS_n1_z0_z0v);
        private static bool _TS_n1_z0_p1 => _TEST_VALUE.Value.Equals(_TS_n1_z0_p1v);
        private static bool _TS_n1_z0_n1 => _TEST_VALUE.Value.Equals(_TS_n1_z0_n1v);
        private static bool _TS_n1_p1_z0 => _TEST_VALUE.Value.Equals(_TS_n1_p1_z0v);
        private static bool _TS_n1_p1_p1 => _TEST_VALUE.Value.Equals(_TS_n1_p1_p1v);
        private static bool _TS_n1_p1_n1 => _TEST_VALUE.Value.Equals(_TS_n1_p1_n1v);
        private static bool _TS_n1_n1_z0 => _TEST_VALUE.Value.Equals(_TS_n1_n1_z0v);
        private static bool _TS_n1_n1_p1 => _TEST_VALUE.Value.Equals(_TS_n1_n1_p1v);

        private const string _TS_z0_z0_z0s = _TEST_M + "(  0,  0,  0 )";
        private const string _TS_z0_z0_p1s = _TEST_M + "(  0,  0, +1 )";
        private const string _TS_z0_z0_n1s = _TEST_M + "(  0,  0, -1 )";
        private const string _TS_z0_p1_z0s = _TEST_M + "(  0, +1,  0 )";
        private const string _TS_z0_p1_p1s = _TEST_M + "(  0, +1, +1 )";
        private const string _TS_z0_p1_n1s = _TEST_M + "(  0, +1, -1 )";
        private const string _TS_z0_n1_z0s = _TEST_M + "(  0, -1,  0 )";
        private const string _TS_z0_n1_p1s = _TEST_M + "(  0, -1, +1 )";
        private const string _TS_z0_n1_n1s = _TEST_M + "(  0, -1, -1 )";
        private const string _TS_p1_z0_z0s = _TEST_M + "( +1,  0,  0 )";
        private const string _TS_p1_z0_p1s = _TEST_M + "( +1,  0, +1 )";
        private const string _TS_p1_z0_n1s = _TEST_M + "( +1,  0, -1 )";
        private const string _TS_p1_p1_z0s = _TEST_M + "( +1, +1,  0 )";
        private const string _TS_p1_p1_p1s = _TEST_M + "( +1, +1, +1 )";
        private const string _TS_p1_p1_n1s = _TEST_M + "( +1, +1, -1 )";
        private const string _TS_p1_n1_z0s = _TEST_M + "( +1, -1,  0 )";
        private const string _TS_p1_n1_p1s = _TEST_M + "( +1, -1, +1 )";
        private const string _TS_p1_n1_n1s = _TEST_M + "( +1, -1, -1 )";
        private const string _TS_n1_z0_z0s = _TEST_M + "( -1,  0,  0 )";
        private const string _TS_n1_z0_p1s = _TEST_M + "( -1,  0, +1 )";
        private const string _TS_n1_z0_n1s = _TEST_M + "( -1,  0, -1 )";
        private const string _TS_n1_p1_z0s = _TEST_M + "( -1, +1,  0 )";
        private const string _TS_n1_p1_p1s = _TEST_M + "( -1, +1, +1 )";
        private const string _TS_n1_p1_n1s = _TEST_M + "( -1, +1, -1 )";
        private const string _TS_n1_n1_z0s = _TEST_M + "( -1, -1,  0 )";
        private const string _TS_n1_n1_p1s = _TEST_M + "( -1, -1, +1 )";

        [UnityEditor.MenuItem(_TS_z0_z0_z0s, true)]
        private static bool _TS_z0_z0_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_z0_z0s, _TS_z0_z0_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_z0_z0s)]
        private static void _TS_z0_z0_z0_()
        {
            _TEST_VALUE.Value = _TS_z0_z0_z0v;
        }

        [UnityEditor.MenuItem(_TS_z0_z0_p1s, true)]
        private static bool _TS_z0_z0_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_z0_p1s, _TS_z0_z0_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_z0_p1s)]
        private static void _TS_z0_z0_p1_()
        {
            _TEST_VALUE.Value = _TS_z0_z0_p1v;
        }

        [UnityEditor.MenuItem(_TS_z0_z0_n1s, true)]
        private static bool _TS_z0_z0_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_z0_n1s, _TS_z0_z0_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_z0_n1s)]
        private static void _TS_z0_z0_n1_()
        {
            _TEST_VALUE.Value = _TS_z0_z0_n1v;
        }

        [UnityEditor.MenuItem(_TS_z0_p1_z0s, true)]
        private static bool _TS_z0_p1_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_p1_z0s, _TS_z0_p1_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_p1_z0s)]
        private static void _TS_z0_p1_z0_()
        {
            _TEST_VALUE.Value = _TS_z0_p1_z0v;
        }

        [UnityEditor.MenuItem(_TS_z0_p1_p1s, true)]
        private static bool _TS_z0_p1_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_p1_p1s, _TS_z0_p1_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_p1_p1s)]
        private static void _TS_z0_p1_p1_()
        {
            _TEST_VALUE.Value = _TS_z0_p1_p1v;
        }

        [UnityEditor.MenuItem(_TS_z0_p1_n1s, true)]
        private static bool _TS_z0_p1_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_p1_n1s, _TS_z0_p1_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_p1_n1s)]
        private static void _TS_z0_p1_n1_()
        {
            _TEST_VALUE.Value = _TS_z0_p1_n1v;
        }

        [UnityEditor.MenuItem(_TS_z0_n1_z0s, true)]
        private static bool _TS_z0_n1_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_n1_z0s, _TS_z0_n1_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_n1_z0s)]
        private static void _TS_z0_n1_z0_()
        {
            _TEST_VALUE.Value = _TS_z0_n1_z0v;
        }

        [UnityEditor.MenuItem(_TS_z0_n1_p1s, true)]
        private static bool _TS_z0_n1_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_n1_p1s, _TS_z0_n1_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_n1_p1s)]
        private static void _TS_z0_n1_p1_()
        {
            _TEST_VALUE.Value = _TS_z0_n1_p1v;
        }

        [UnityEditor.MenuItem(_TS_z0_n1_n1s, true)]
        private static bool _TS_z0_n1_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_z0_n1_n1s, _TS_z0_n1_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_z0_n1_n1s)]
        private static void _TS_z0_n1_n1_()
        {
            _TEST_VALUE.Value = _TS_z0_n1_n1v;
        }

        [UnityEditor.MenuItem(_TS_p1_z0_z0s, true)]
        private static bool _TS_p1_z0_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_z0_z0s, _TS_p1_z0_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_z0_z0s)]
        private static void _TS_p1_z0_z0_()
        {
            _TEST_VALUE.Value = _TS_p1_z0_z0v;
        }

        [UnityEditor.MenuItem(_TS_p1_z0_p1s, true)]
        private static bool _TS_p1_z0_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_z0_p1s, _TS_p1_z0_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_z0_p1s)]
        private static void _TS_p1_z0_p1_()
        {
            _TEST_VALUE.Value = _TS_p1_z0_p1v;
        }

        [UnityEditor.MenuItem(_TS_p1_z0_n1s, true)]
        private static bool _TS_p1_z0_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_z0_n1s, _TS_p1_z0_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_z0_n1s)]
        private static void _TS_p1_z0_n1_()
        {
            _TEST_VALUE.Value = _TS_p1_z0_n1v;
        }

        [UnityEditor.MenuItem(_TS_p1_p1_z0s, true)]
        private static bool _TS_p1_p1_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_p1_z0s, _TS_p1_p1_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_p1_z0s)]
        private static void _TS_p1_p1_z0_()
        {
            _TEST_VALUE.Value = _TS_p1_p1_z0v;
        }

        [UnityEditor.MenuItem(_TS_p1_p1_p1s, true)]
        private static bool _TS_p1_p1_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_p1_p1s, _TS_p1_p1_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_p1_p1s)]
        private static void _TS_p1_p1_p1_()
        {
            _TEST_VALUE.Value = _TS_p1_p1_p1v;
        }

        [UnityEditor.MenuItem(_TS_p1_p1_n1s, true)]
        private static bool _TS_p1_p1_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_p1_n1s, _TS_p1_p1_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_p1_n1s)]
        private static void _TS_p1_p1_n1_()
        {
            _TEST_VALUE.Value = _TS_p1_p1_n1v;
        }

        [UnityEditor.MenuItem(_TS_p1_n1_z0s, true)]
        private static bool _TS_p1_n1_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_n1_z0s, _TS_p1_n1_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_n1_z0s)]
        private static void _TS_p1_n1_z0_()
        {
            _TEST_VALUE.Value = _TS_p1_n1_z0v;
        }

        [UnityEditor.MenuItem(_TS_p1_n1_p1s, true)]
        private static bool _TS_p1_n1_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_n1_p1s, _TS_p1_n1_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_n1_p1s)]
        private static void _TS_p1_n1_p1_()
        {
            _TEST_VALUE.Value = _TS_p1_n1_p1v;
        }

        [UnityEditor.MenuItem(_TS_p1_n1_n1s, true)]
        private static bool _TS_p1_n1_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_p1_n1_n1s, _TS_p1_n1_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_p1_n1_n1s)]
        private static void _TS_p1_n1_n1_()
        {
            _TEST_VALUE.Value = _TS_p1_n1_n1v;
        }

        [UnityEditor.MenuItem(_TS_n1_z0_z0s, true)]
        private static bool _TS_n1_z0_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_z0_z0s, _TS_n1_z0_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_z0_z0s)]
        private static void _TS_n1_z0_z0_()
        {
            _TEST_VALUE.Value = _TS_n1_z0_z0v;
        }

        [UnityEditor.MenuItem(_TS_n1_z0_p1s, true)]
        private static bool _TS_n1_z0_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_z0_p1s, _TS_n1_z0_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_z0_p1s)]
        private static void _TS_n1_z0_p1_()
        {
            _TEST_VALUE.Value = _TS_n1_z0_p1v;
        }

        [UnityEditor.MenuItem(_TS_n1_z0_n1s, true)]
        private static bool _TS_n1_z0_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_z0_n1s, _TS_n1_z0_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_z0_n1s)]
        private static void _TS_n1_z0_n1_()
        {
            _TEST_VALUE.Value = _TS_n1_z0_n1v;
        }

        [UnityEditor.MenuItem(_TS_n1_p1_z0s, true)]
        private static bool _TS_n1_p1_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_p1_z0s, _TS_n1_p1_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_p1_z0s)]
        private static void _TS_n1_p1_z0_()
        {
            _TEST_VALUE.Value = _TS_n1_p1_z0v;
        }

        [UnityEditor.MenuItem(_TS_n1_p1_p1s, true)]
        private static bool _TS_n1_p1_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_p1_p1s, _TS_n1_p1_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_p1_p1s)]
        private static void _TS_n1_p1_p1_()
        {
            _TEST_VALUE.Value = _TS_n1_p1_p1v;
        }

        [UnityEditor.MenuItem(_TS_n1_p1_n1s, true)]
        private static bool _TS_n1_p1_n1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_p1_n1s, _TS_n1_p1_n1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_p1_n1s)]
        private static void _TS_n1_p1_n1_()
        {
            _TEST_VALUE.Value = _TS_n1_p1_n1v;
        }

        [UnityEditor.MenuItem(_TS_n1_n1_z0s, true)]
        private static bool _TS_n1_n1_z0_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_n1_z0s, _TS_n1_n1_z0);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_n1_z0s)]
        private static void _TS_n1_n1_z0_()
        {
            _TEST_VALUE.Value = _TS_n1_n1_z0v;
        }

        [UnityEditor.MenuItem(_TS_n1_n1_p1s, true)]
        private static bool _TS_n1_n1_p1_V()
        {
            UnityEditor.Menu.SetChecked(_TS_n1_n1_p1s, _TS_n1_n1_p1);
            return true;
        }

        [UnityEditor.MenuItem(_TS_n1_n1_p1s)]
        private static void _TS_n1_n1_p1_()
        {
            _TEST_VALUE.Value = _TS_n1_n1_p1v;
        }

        #endregion

        #region Timing (Milliseconds)

        [NonSerialized]
        public static readonly PREF<int> _TIME = PREFS.REG(PKG.Prefs.Group, "Frame Time", _T_030v);

        private const int _T_010v = 010;
        private const int _T_020v = 020;
        private const int _T_030v = 030;
        private const int _T_040v = 040;
        private const int _T_050v = 050;
        private const int _T_100v = 100;
        private const int _T_150v = 150;
        private const int _T_200v = 200;
        private const int _T_250v = 250;

        private static bool _T_010 => _TIME.Value == _T_010v;
        private static bool _T_020 => _TIME.Value == _T_020v;
        private static bool _T_030 => _TIME.Value == _T_030v;
        private static bool _T_040 => _TIME.Value == _T_040v;
        private static bool _T_050 => _TIME.Value == _T_050v;
        private static bool _T_100 => _TIME.Value == _T_100v;
        private static bool _T_150 => _TIME.Value == _T_150v;
        private static bool _T_200 => _TIME.Value == _T_200v;
        private static bool _T_250 => _TIME.Value == _T_250v;

        private const string TIME = PKG.Menu.Appalachia.Timing.Base;

        [UnityEditor.MenuItem(TIME + "10", true)]
        private static bool TIME_010_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "10", _T_010);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "20", true)]
        private static bool TIME_020_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "20", _T_020);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "30", true)]
        private static bool TIME_030_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "30", _T_030);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "40", true)]
        private static bool TIME_040_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "40", _T_040);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "50", true)]
        private static bool TIME_050_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "50", _T_050);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "100", true)]
        private static bool TIME_100_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "100", _T_100);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "150", true)]
        private static bool TIME_150_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "150", _T_150);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "200", true)]
        private static bool TIME_200_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "200", _T_200);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "250", true)]
        private static bool TIME_250_v()
        {
            UnityEditor.Menu.SetChecked(TIME + "250", _T_250);
            return true;
        }

        [UnityEditor.MenuItem(TIME + "10")]
        private static void TIME_010()
        {
            _TIME.Value = _T_010v;
        }

        [UnityEditor.MenuItem(TIME + "20")]
        private static void TIME_020()
        {
            _TIME.Value = _T_020v;
        }

        [UnityEditor.MenuItem(TIME + "30")]
        private static void TIME_030()
        {
            _TIME.Value = _T_030v;
        }

        [UnityEditor.MenuItem(TIME + "40")]
        private static void TIME_040()
        {
            _TIME.Value = _T_040v;
        }

        [UnityEditor.MenuItem(TIME + "50")]
        private static void TIME_050()
        {
            _TIME.Value = _T_050v;
        }

        [UnityEditor.MenuItem(TIME + "100")]
        private static void TIME_100()
        {
            _TIME.Value = _T_100v;
        }

        [UnityEditor.MenuItem(TIME + "150")]
        private static void TIME_150()
        {
            _TIME.Value = _T_150v;
        }

        [UnityEditor.MenuItem(TIME + "200")]
        private static void TIME_200()
        {
            _TIME.Value = _T_200v;
        }

        [UnityEditor.MenuItem(TIME + "250")]
        private static void TIME_250()
        {
            _TIME.Value = _T_250v;
        }

        #endregion

        #region Long Item Log Timing (Milliseconds)

        private const string MENU_TIMELOG_ = PKG.Menu.Appalachia.Logging.Base + "Enable Long Item Logging";

        [NonSerialized]
        public static readonly PREF<bool> _TIMELOG = PREFS.REG(
            PKG.Prefs.Group,
            "Enable Long Item Logging",
            true
        );

        [UnityEditor.MenuItem(MENU_TIMELOG_, true)]
        private static bool MENU_TIMELOG_VALIDATE()
        {
            UnityEditor.Menu.SetChecked(MENU_TIMELOG_, _TIMELOG.Value);
            return true;
        }

        [UnityEditor.MenuItem(MENU_TIMELOG_, priority = PKG.Menu.Appalachia.Logging.Priority)]
        public static void MENU_TIMELOG()
        {
            _TIMELOG.v = !_TIMELOG.v;
        }

        [NonSerialized]
        public static readonly PREF<int> _TIMELOGTIME = PREFS.REG(PKG.Prefs.Group, "Frame Time", _TLT_030v);

        private const string TIMELOGTIME =
            PKG.Menu.Appalachia.Logging.Base + "Item Timing Log Threshold (Seconds)/";

        private const int _TLT_001v = 001;
        private const int _TLT_002v = 002;
        private const int _TLT_003v = 003;
        private const int _TLT_004v = 004;
        private const int _TLT_005v = 005;
        private const int _TLT_010v = 010;
        private const int _TLT_015v = 015;
        private const int _TLT_020v = 020;
        private const int _TLT_025v = 025;
        private const int _TLT_030v = 030;
        private const int _TLT_045v = 045;
        private const int _TLT_060v = 060;
        private const int _TLT_090v = 090;
        private const int _TLT_120v = 120;
        private const int _TLT_150v = 150;
        private const int _TLT_180v = 180;
        private const int _TLT_240v = 240;
        private const int _TLT_300v = 300;
        private const int _TLT_360v = 360;
        private const int _TLT_420v = 420;

        private static bool _TLT_001 => _TIMELOGTIME.Value == _TLT_001v;
        private static bool _TLT_002 => _TIMELOGTIME.Value == _TLT_002v;
        private static bool _TLT_003 => _TIMELOGTIME.Value == _TLT_003v;
        private static bool _TLT_004 => _TIMELOGTIME.Value == _TLT_004v;
        private static bool _TLT_005 => _TIMELOGTIME.Value == _TLT_005v;
        private static bool _TLT_010 => _TIMELOGTIME.Value == _TLT_010v;
        private static bool _TLT_015 => _TIMELOGTIME.Value == _TLT_015v;
        private static bool _TLT_020 => _TIMELOGTIME.Value == _TLT_020v;
        private static bool _TLT_025 => _TIMELOGTIME.Value == _TLT_025v;
        private static bool _TLT_030 => _TIMELOGTIME.Value == _TLT_030v;
        private static bool _TLT_045 => _TIMELOGTIME.Value == _TLT_045v;
        private static bool _TLT_060 => _TIMELOGTIME.Value == _TLT_060v;
        private static bool _TLT_090 => _TIMELOGTIME.Value == _TLT_090v;
        private static bool _TLT_120 => _TIMELOGTIME.Value == _TLT_120v;
        private static bool _TLT_150 => _TIMELOGTIME.Value == _TLT_150v;
        private static bool _TLT_180 => _TIMELOGTIME.Value == _TLT_180v;
        private static bool _TLT_240 => _TIMELOGTIME.Value == _TLT_240v;
        private static bool _TLT_300 => _TIMELOGTIME.Value == _TLT_300v;
        private static bool _TLT_360 => _TIMELOGTIME.Value == _TLT_360v;
        private static bool _TLT_420 => _TIMELOGTIME.Value == _TLT_420v;

        [UnityEditor.MenuItem(TIMELOGTIME + "1", true)]
        private static bool _TLT_001_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "1", _TLT_001);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "2", true)]
        private static bool _TLT_002_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "2", _TLT_002);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "3", true)]
        private static bool _TLT_003_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "3", _TLT_003);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "4", true)]
        private static bool _TLT_004_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "4", _TLT_004);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "5", true)]
        private static bool _TLT_005_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "5", _TLT_005);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "10", true)]
        private static bool _TLT_010_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "10", _TLT_010);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "15", true)]
        private static bool _TLT_015_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "15", _TLT_015);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "20", true)]
        private static bool _TLT_020_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "20", _TLT_020);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "25", true)]
        private static bool _TLT_025_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "25", _TLT_025);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "30", true)]
        private static bool _TLT_030_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "30", _TLT_030);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "45", true)]
        private static bool _TLT_045_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "45", _TLT_045);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "60", true)]
        private static bool _TLT_060_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "60", _TLT_060);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "90", true)]
        private static bool _TLT_090_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "90", _TLT_090);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "120", true)]
        private static bool _TLT_120_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "120", _TLT_120);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "150", true)]
        private static bool _TLT_150_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "150", _TLT_150);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "180", true)]
        private static bool _TLT_180_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "180", _TLT_180);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "240", true)]
        private static bool _TLT_240_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "240", _TLT_240);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "300", true)]
        private static bool _TLT_300_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "300", _TLT_300);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "360", true)]
        private static bool _TLT_360_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "360", _TLT_360);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "420", true)]
        private static bool _TLT_420_v()
        {
            UnityEditor.Menu.SetChecked(TIMELOGTIME + "420", _TLT_420);
            return true;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "1")]
        private static void TLT_001()
        {
            _TIMELOGTIME.Value = _TLT_001v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "2")]
        private static void TLT_002()
        {
            _TIMELOGTIME.Value = _TLT_002v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "3")]
        private static void TLT_003()
        {
            _TIMELOGTIME.Value = _TLT_003v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "4")]
        private static void TLT_004()
        {
            _TIMELOGTIME.Value = _TLT_004v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "5")]
        private static void TLT_005()
        {
            _TIMELOGTIME.Value = _TLT_005v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "10")]
        private static void TLT_010()
        {
            _TIMELOGTIME.Value = _TLT_010v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "15")]
        private static void TLT_015()
        {
            _TIMELOGTIME.Value = _TLT_015v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "20")]
        private static void TLT_020()
        {
            _TIMELOGTIME.Value = _TLT_020v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "25")]
        private static void TLT_025()
        {
            _TIMELOGTIME.Value = _TLT_025v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "30")]
        private static void TLT_030()
        {
            _TIMELOGTIME.Value = _TLT_030v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "45")]
        private static void TLT_045()
        {
            _TIMELOGTIME.Value = _TLT_045v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "60")]
        private static void TLT_060()
        {
            _TIMELOGTIME.Value = _TLT_060v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "90")]
        private static void TLT_090()
        {
            _TIMELOGTIME.Value = _TLT_090v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "120")]
        private static void TLT_120()
        {
            _TIMELOGTIME.Value = _TLT_120v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "150")]
        private static void TLT_150()
        {
            _TIMELOGTIME.Value = _TLT_150v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "180")]
        private static void TLT_180()
        {
            _TIMELOGTIME.Value = _TLT_180v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "240")]
        private static void TLT_240()
        {
            _TIMELOGTIME.Value = _TLT_240v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "300")]
        private static void TLT_300()
        {
            _TIMELOGTIME.Value = _TLT_300v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "360")]
        private static void TLT_360()
        {
            _TIMELOGTIME.Value = _TLT_360v;
        }

        [UnityEditor.MenuItem(TIMELOGTIME + "420")]
        private static void TLT_420()
        {
            _TIMELOGTIME.Value = _TLT_420v;
        }

        #endregion

        #region Logging (Frames)

        [NonSerialized]
        public static readonly PREF<int> _LOG = PREFS.REG(PKG.Prefs.Group, "Log Threshold", 300);

        private const string LOG = PKG.Menu.Appalachia.Tools.Base + "Logging (Frames)/";

        private const int _L_OFFv = 000;
        private const int _L_030v = 030;
        private const int _L_060v = 060;
        private const int _L_090v = 090;
        private const int _L_120v = 120;
        private const int _L_150v = 150;
        private const int _L_300v = 300;
        private const int _L_600v = 600;
        private const int _L_900v = 900;

        private static bool _L_OFF => _LOG.Value == _L_OFFv;
        private static bool _L_030 => _LOG.Value == _L_030v;
        private static bool _L_060 => _LOG.Value == _L_060v;
        private static bool _L_090 => _LOG.Value == _L_090v;
        private static bool _L_120 => _LOG.Value == _L_120v;
        private static bool _L_150 => _LOG.Value == _L_150v;
        private static bool _L_300 => _LOG.Value == _L_300v;
        private static bool _L_600 => _LOG.Value == _L_600v;
        private static bool _L_900 => _LOG.Value == _L_900v;

        [UnityEditor.MenuItem(LOG + "Off", true)]
        private static bool LOG_OFF_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "Off", _L_OFF);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "30", true)]
        private static bool LOG_030_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "30", _L_030);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "60", true)]
        private static bool LOG_060_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "60", _L_060);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "90", true)]
        private static bool LOG_090_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "90", _L_090);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "120", true)]
        private static bool LOG_120_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "120", _L_120);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "150", true)]
        private static bool LOG_150_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "150", _L_150);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "300", true)]
        private static bool LOG_300_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "300", _L_300);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "600", true)]
        private static bool LOG_600_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "600", _L_600);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "900", true)]
        private static bool LOG_900_v()
        {
            UnityEditor.Menu.SetChecked(LOG + "900", _L_900);
            return true;
        }

        [UnityEditor.MenuItem(LOG + "Off")]
        private static void LOG_OFF()
        {
            _LOG.Value = _L_OFFv;
        }

        [UnityEditor.MenuItem(LOG + "30")]
        private static void LOG_030()
        {
            _LOG.Value = _L_030v;
        }

        [UnityEditor.MenuItem(LOG + "60")]
        private static void LOG_060()
        {
            _LOG.Value = _L_060v;
        }

        [UnityEditor.MenuItem(LOG + "90")]
        private static void LOG_090()
        {
            _LOG.Value = _L_090v;
        }

        [UnityEditor.MenuItem(LOG + "120")]
        private static void LOG_120()
        {
            _LOG.Value = _L_120v;
        }

        [UnityEditor.MenuItem(LOG + "150")]
        private static void LOG_150()
        {
            _LOG.Value = _L_150v;
        }

        [UnityEditor.MenuItem(LOG + "300")]
        private static void LOG_300()
        {
            _LOG.Value = _L_300v;
        }

        [UnityEditor.MenuItem(LOG + "600")]
        private static void LOG_600()
        {
            _LOG.Value = _L_600v;
        }

        [UnityEditor.MenuItem(LOG + "900")]
        private static void LOG_900()
        {
            _LOG.Value = _L_900v;
        }

        #endregion

        #region Allowed Error

        [NonSerialized]
        public static readonly PREF<double> _ERROR = PREFS.REG(PKG.Prefs.Group, "Allowed Error", _E_030v);

        private const string ERROR = PKG.Menu.Appalachia.Tools.Base + "Allowed Error/";

        private const double _E_100v = 1.0;
        private const double _E_099v = 0.99;
        private const double _E_095v = 0.95;
        private const double _E_090v = 0.9;
        private const double _E_085v = 0.85;
        private const double _E_080v = 0.8;
        private const double _E_070v = 0.7;
        private const double _E_060v = 0.6;
        private const double _E_050v = 0.5;
        private const double _E_040v = 0.4;
        private const double _E_030v = 0.3;
        private const double _E_020v = 0.2;
        private const double _E_010v = 0.1;

        private static bool _E_100 => _ERROR.Value == _E_100v;
        private static bool _E_099 => _ERROR.Value == _E_099v;
        private static bool _E_095 => _ERROR.Value == _E_095v;
        private static bool _E_090 => _ERROR.Value == _E_090v;
        private static bool _E_085 => _ERROR.Value == _E_085v;
        private static bool _E_080 => _ERROR.Value == _E_080v;
        private static bool _E_070 => _ERROR.Value == _E_070v;
        private static bool _E_060 => _ERROR.Value == _E_060v;
        private static bool _E_050 => _ERROR.Value == _E_050v;
        private static bool _E_040 => _ERROR.Value == _E_040v;
        private static bool _E_030 => _ERROR.Value == _E_030v;
        private static bool _E_020 => _ERROR.Value == _E_020v;
        private static bool _E_010 => _ERROR.Value == _E_010v;

        [UnityEditor.MenuItem(ERROR + "1.00", true)]
        private static bool ERROR_100_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "1.00", _E_100);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.99", true)]
        private static bool ERROR_099_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.99", _E_099);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.95", true)]
        private static bool ERROR_095_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.95", _E_095);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.90", true)]
        private static bool ERROR_090_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.90", _E_090);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.85", true)]
        private static bool ERROR_085_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.85", _E_085);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.80", true)]
        private static bool ERROR_080_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.80", _E_080);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.70", true)]
        private static bool ERROR_070_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.70", _E_070);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.60", true)]
        private static bool ERROR_060_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.60", _E_060);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.50", true)]
        private static bool ERROR_050_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.50", _E_050);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.40", true)]
        private static bool ERROR_040_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.40", _E_040);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.30", true)]
        private static bool ERROR_030_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.30", _E_030);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.20", true)]
        private static bool ERROR_020_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.20", _E_020);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "0.10", true)]
        private static bool ERROR_0100_v()
        {
            UnityEditor.Menu.SetChecked(ERROR + "0.10", _E_010);
            return true;
        }

        [UnityEditor.MenuItem(ERROR + "1.00")]
        private static void ERROR_100()
        {
            _ERROR.Value = _E_100v;
        }

        [UnityEditor.MenuItem(ERROR + "0.99")]
        private static void ERROR_099()
        {
            _ERROR.Value = _E_099v;
        }

        [UnityEditor.MenuItem(ERROR + "0.95")]
        private static void ERROR_095()
        {
            _ERROR.Value = _E_095v;
        }

        [UnityEditor.MenuItem(ERROR + "0.90")]
        private static void ERROR_090()
        {
            _ERROR.Value = _E_090v;
        }

        [UnityEditor.MenuItem(ERROR + "0.85")]
        private static void ERROR_085()
        {
            _ERROR.Value = _E_085v;
        }

        [UnityEditor.MenuItem(ERROR + "0.80")]
        private static void ERROR_080()
        {
            _ERROR.Value = _E_080v;
        }

        [UnityEditor.MenuItem(ERROR + "0.70")]
        private static void ERROR_070()
        {
            _ERROR.Value = _E_070v;
        }

        [UnityEditor.MenuItem(ERROR + "0.60")]
        private static void ERROR_060()
        {
            _ERROR.Value = _E_060v;
        }

        [UnityEditor.MenuItem(ERROR + "0.50")]
        private static void ERROR_050()
        {
            _ERROR.Value = _E_050v;
        }

        [UnityEditor.MenuItem(ERROR + "0.40")]
        private static void ERROR_040()
        {
            _ERROR.Value = _E_040v;
        }

        [UnityEditor.MenuItem(ERROR + "0.30")]
        private static void ERROR_030()
        {
            _ERROR.Value = _E_030v;
        }

        [UnityEditor.MenuItem(ERROR + "0.20")]
        private static void ERROR_020()
        {
            _ERROR.Value = _E_020v;
        }

        [UnityEditor.MenuItem(ERROR + "0.10")]
        private static void ERROR_010()
        {
            _ERROR.Value = _E_010v;
        }

        #endregion

        #region Allowed Iterations

        [NonSerialized]
        public static readonly PREF<int> _ITER = PREFS.REG(PKG.Prefs.Group, "Allowed Iterations", _I_0010v);

        private const string _IT_ = PKG.Menu.Appalachia.Tools.Base + "Allowed Iterations/";

        private const int _I_0000v = 0000;
        private const int _I_0010v = 0010;
        private const int _I_0020v = 0020;
        private const int _I_0030v = 0030;
        private const int _I_0040v = 0040;
        private const int _I_0050v = 0050;
        private const int _I_0125v = 0120;
        private const int _I_0250v = 0250;
        private const int _I_0500v = 0500;
        private const int _I_1000v = 1000;

        private static bool _I_0000 => _ITER.Value == _I_0000v;
        private static bool _I_0010 => _ITER.Value == _I_0010v;
        private static bool _I_0020 => _ITER.Value == _I_0020v;
        private static bool _I_0030 => _ITER.Value == _I_0030v;
        private static bool _I_0040 => _ITER.Value == _I_0040v;
        private static bool _I_0050 => _ITER.Value == _I_0050v;
        private static bool _I_0125 => _ITER.Value == _I_0125v;
        private static bool _I_0250 => _ITER.Value == _I_0250v;
        private static bool _I_0500 => _ITER.Value == _I_0500v;
        private static bool _I_1000 => _ITER.Value == _I_1000v;

        [UnityEditor.MenuItem(_IT_ + "Unlimited", true)]
        private static bool IT_0000_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "Unlimited", _I_0000);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "10", true)]
        private static bool IT_0010_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "10", _I_0010);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "20", true)]
        private static bool IT_0020_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "20", _I_0020);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "30", true)]
        private static bool IT_0030_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "30", _I_0030);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "40", true)]
        private static bool IT_0040_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "40", _I_0040);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "50", true)]
        private static bool IT_0050_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "50", _I_0050);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "125", true)]
        private static bool IT_0125_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "125", _I_0125);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "250", true)]
        private static bool IT_0250_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "250", _I_0250);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "500", true)]
        private static bool IT_0500_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "500", _I_0500);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "1000", true)]
        private static bool IT_1000_v()
        {
            UnityEditor.Menu.SetChecked(_IT_ + "1000", _I_1000);
            return true;
        }

        [UnityEditor.MenuItem(_IT_ + "Unlimited")]
        private static void IT_0000()
        {
            _ITER.Value = _I_0000v;
        }

        [UnityEditor.MenuItem(_IT_ + "10")]
        private static void IT_0010()
        {
            _ITER.Value = _I_0010v;
        }

        [UnityEditor.MenuItem(_IT_ + "20")]
        private static void IT_0020()
        {
            _ITER.Value = _I_0020v;
        }

        [UnityEditor.MenuItem(_IT_ + "30")]
        private static void IT_0030()
        {
            _ITER.Value = _I_0030v;
        }

        [UnityEditor.MenuItem(_IT_ + "40")]
        private static void IT_0040()
        {
            _ITER.Value = _I_0040v;
        }

        [UnityEditor.MenuItem(_IT_ + "50")]
        private static void IT_0050()
        {
            _ITER.Value = _I_0050v;
        }

        [UnityEditor.MenuItem(_IT_ + "125")]
        private static void IT_0125()
        {
            _ITER.Value = _I_0125v;
        }

        [UnityEditor.MenuItem(_IT_ + "250")]
        private static void IT_0250()
        {
            _ITER.Value = _I_0250v;
        }

        [UnityEditor.MenuItem(_IT_ + "500")]
        private static void IT_0500()
        {
            _ITER.Value = _I_0500v;
        }

        [UnityEditor.MenuItem(_IT_ + "1000")]
        private static void IT_1000()
        {
            _ITER.Value = _I_1000v;
        }

        #endregion

        #region Instance Iterations

        [NonSerialized]
        public static readonly PREF<int> _INST_ITER = PREFS.REG(
            PKG.Prefs.Group,
            "Instance Iterations",
            _II_01024v
        );

        private const string _II = PKG.Menu.Appalachia.Tools.Base + "Instance Iterations/";

        private const int _II_00128v = 00128;
        private const int _II_00192v = 00192;
        private const int _II_00256v = 00256;
        private const int _II_00384v = 00384;
        private const int _II_00512v = 00512;
        private const int _II_00768v = 00768;
        private const int _II_01024v = 01024;
        private const int _II_01536v = 01536;
        private const int _II_02048v = 02048;
        private const int _II_03072v = 03072;
        private const int _II_04096v = 04096;
        private const int _II_06144v = 06144;
        private const int _II_08192v = 08192;
        private const int _II_12228v = 12228;
        private const int _II_16384v = 16384;

        private static bool _II_00128 => _INST_ITER.Value == _II_00128v;
        private static bool _II_00192 => _INST_ITER.Value == _II_00192v;
        private static bool _II_00256 => _INST_ITER.Value == _II_00256v;
        private static bool _II_00384 => _INST_ITER.Value == _II_00384v;
        private static bool _II_00512 => _INST_ITER.Value == _II_00512v;
        private static bool _II_00768 => _INST_ITER.Value == _II_00768v;
        private static bool _II_01024 => _INST_ITER.Value == _II_01024v;
        private static bool _II_01536 => _INST_ITER.Value == _II_01536v;
        private static bool _II_02048 => _INST_ITER.Value == _II_02048v;
        private static bool _II_03072 => _INST_ITER.Value == _II_03072v;
        private static bool _II_04096 => _INST_ITER.Value == _II_04096v;
        private static bool _II_06144 => _INST_ITER.Value == _II_06144v;
        private static bool _II_08192 => _INST_ITER.Value == _II_08192v;
        private static bool _II_12228 => _INST_ITER.Value == _II_12228v;
        private static bool _II_16384 => _INST_ITER.Value == _II_16384v;

        [UnityEditor.MenuItem(_II + "128", true)]
        private static bool II_00128v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "128", _II_00128);
            return true;
        }

        [UnityEditor.MenuItem(_II + "192", true)]
        private static bool II_00192v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "192", _II_00192);
            return true;
        }

        [UnityEditor.MenuItem(_II + "256", true)]
        private static bool II_00256v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "256", _II_00256);
            return true;
        }

        [UnityEditor.MenuItem(_II + "384", true)]
        private static bool II_00384v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "384", _II_00384);
            return true;
        }

        [UnityEditor.MenuItem(_II + "512", true)]
        private static bool II_00512v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "512", _II_00512);
            return true;
        }

        [UnityEditor.MenuItem(_II + "768", true)]
        private static bool II_00768v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "768", _II_00768);
            return true;
        }

        [UnityEditor.MenuItem(_II + "1024", true)]
        private static bool II_01024v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "1024", _II_01024);
            return true;
        }

        [UnityEditor.MenuItem(_II + "1536", true)]
        private static bool II_01536v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "1536", _II_01536);
            return true;
        }

        [UnityEditor.MenuItem(_II + "2048", true)]
        private static bool II_02048v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "2048", _II_02048);
            return true;
        }

        [UnityEditor.MenuItem(_II + "3072", true)]
        private static bool II_03072v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "3072", _II_03072);
            return true;
        }

        [UnityEditor.MenuItem(_II + "4096", true)]
        private static bool II_04096v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "4096", _II_04096);
            return true;
        }

        [UnityEditor.MenuItem(_II + "6144", true)]
        private static bool II_06144v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "6144", _II_06144);
            return true;
        }

        [UnityEditor.MenuItem(_II + "8192", true)]
        private static bool II_08192v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "8192", _II_08192);
            return true;
        }

        [UnityEditor.MenuItem(_II + "12228", true)]
        private static bool II_12228v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "12228", _II_12228);
            return true;
        }

        [UnityEditor.MenuItem(_II + "16384", true)]
        private static bool II_16384v_v()
        {
            UnityEditor.Menu.SetChecked(_II + "16384", _II_16384);
            return true;
        }

        [UnityEditor.MenuItem(_II + "128")]
        private static void II_00128()
        {
            _INST_ITER.Value = _II_00128v;
        }

        [UnityEditor.MenuItem(_II + "192")]
        private static void II_00192()
        {
            _INST_ITER.Value = _II_00192v;
        }

        [UnityEditor.MenuItem(_II + "256")]
        private static void II_00256()
        {
            _INST_ITER.Value = _II_00256v;
        }

        [UnityEditor.MenuItem(_II + "384")]
        private static void II_00384()
        {
            _INST_ITER.Value = _II_00384v;
        }

        [UnityEditor.MenuItem(_II + "512")]
        private static void II_00512()
        {
            _INST_ITER.Value = _II_00512v;
        }

        [UnityEditor.MenuItem(_II + "768")]
        private static void II_00768()
        {
            _INST_ITER.Value = _II_00768v;
        }

        [UnityEditor.MenuItem(_II + "1024")]
        private static void II_01024()
        {
            _INST_ITER.Value = _II_01024v;
        }

        [UnityEditor.MenuItem(_II + "1536")]
        private static void II_01536()
        {
            _INST_ITER.Value = _II_01536v;
        }

        [UnityEditor.MenuItem(_II + "2048")]
        private static void II_02048()
        {
            _INST_ITER.Value = _II_02048v;
        }

        [UnityEditor.MenuItem(_II + "3072")]
        private static void II_03072()
        {
            _INST_ITER.Value = _II_03072v;
        }

        [UnityEditor.MenuItem(_II + "4096")]
        private static void II_04096()
        {
            _INST_ITER.Value = _II_04096v;
        }

        [UnityEditor.MenuItem(_II + "6144")]
        private static void II_06144()
        {
            _INST_ITER.Value = _II_06144v;
        }

        [UnityEditor.MenuItem(_II + "8192")]
        private static void II_08192()
        {
            _INST_ITER.Value = _II_08192v;
        }

        [UnityEditor.MenuItem(_II + "12228")]
        private static void II_12228()
        {
            _INST_ITER.Value = _II_12228v;
        }

        [UnityEditor.MenuItem(_II + "16384")]
        private static void II_16384()
        {
            _INST_ITER.Value = _II_16384v;
        }

        #endregion
    }
}

#endif
