#if UNITY_EDITOR

#region

using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Generation
{
    public partial class DecomposedCollider
    {
        public static void Decompose(UnityEditor.MenuCommand menuCommand, float tolerance)
        {
            using (_PRF_Decompose.Auto())
            {
                var go = menuCommand.context as GameObject;
                if (go == null)
                {
                    return;
                }

                var decomposed = go.GetComponent<DecomposedCollider>();

                if (decomposed == null)
                {
                    decomposed = go.AddComponent<DecomposedCollider>();
                }

                decomposed.data.settings.maxConvexHulls = 64;
                decomposed.data.successThreshold = tolerance;

                decomposed.ExecuteDecomposition(ExecutionStyle.Normal);
            }
        }

        #region Menu Items

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "1x Volume Tolerance",
            priority = PKG.Priority + 0
        )]
        public static void Decompose_1(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1f);
        }

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "2.0x Volume Tolerance",
            priority = PKG.Priority + 6
        )]
        public static void Decompose_10(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 2f);
        }

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "1.1x Volume Tolerance",
            priority = PKG.Priority + 1
        )]
        public static void Decompose_2(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.1f);
        }

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "1.2x Volume Tolerance",
            priority = PKG.Priority + 2
        )]
        public static void Decompose_3(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.2f);
        }

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "1.3x Volume Tolerance",
            priority = PKG.Priority + 3
        )]
        public static void Decompose_4(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.3f);
        }

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "1.4x Volume Tolerance",
            priority = PKG.Priority + 4
        )]
        public static void Decompose_5(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.4f);
        }

        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Create.Base + "1.5x Volume Tolerance",
            priority = PKG.Priority + 5
        )]
        public static void Decompose_75(UnityEditor.MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.5f);
        }

        #endregion

        #region Profiling

        private const string _PRF_PFX = nameof(DecomposedCollider) + ".";

        private static readonly ProfilerMarker _PRF_Decompose =
            new ProfilerMarker(_PRF_PFX + nameof(Decompose));

        #endregion
    }
}

#endif
