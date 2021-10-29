#if UNITY_EDITOR

#region

using Appalachia.CI.Constants;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Generation
{
    public partial class DecomposedCollider
    {
        private const string _PRF_PFX = nameof(DecomposedCollider) + ".";
        
               
        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "1x Volume Tolerance", priority = PKG.Priority + 0)]
        public static void Decompose_1(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1f);
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "1.1x Volume Tolerance", priority = PKG.Priority + 1)]
        public static void Decompose_2(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.1f);
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "1.2x Volume Tolerance", priority = PKG.Priority + 2)]
        public static void Decompose_3(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.2f);
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "1.3x Volume Tolerance", priority = PKG.Priority + 3)]
        public static void Decompose_4(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.3f);
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "1.4x Volume Tolerance", priority = PKG.Priority + 4)]
        public static void Decompose_5(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.4f);
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "1.5x Volume Tolerance", priority = PKG.Priority + 5)]
        public static void Decompose_75(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.5f);
        }

        [UnityEditor.MenuItem(PKG.Menu.Appalachia.Create.Base + "2.0x Volume Tolerance", priority = PKG.Priority + 6)]
        public static void Decompose_10(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 2f);
        }

        private static readonly ProfilerMarker _PRF_Decompose = new ProfilerMarker(_PRF_PFX + nameof(Decompose));
        public static void Decompose(MenuCommand menuCommand, float tolerance)
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
    }
}

#endif