using Appalachia.Core.Scriptables;
using Appalachia.Utility.Logging;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition
{
    public class ConvexTest : SingletonAppalachiaObject<ConvexTest>
    {
        [UnityEditor.MenuItem(PKG.Menu.Assets.Base + "Create/Internal/ConvexDecomposition/Test")]
        public static void Create()
        {
            AppaLog.Info(instance.name);
        }
        public ConvexMeshSettings settings;
    }
}
