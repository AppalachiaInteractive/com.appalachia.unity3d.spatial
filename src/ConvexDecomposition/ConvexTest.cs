using Appalachia.Core.Objects.Root;

namespace Appalachia.Spatial.ConvexDecomposition
{
    public class ConvexTest : SingletonAppalachiaObject<ConvexTest>
    {
        #region Fields and Autoproperties

        public ConvexMeshSettings settings;

        #endregion

        #region Menu Items

        [UnityEditor.MenuItem(PKG.Menu.Assets.Base + "Create/Internal/ConvexDecomposition/Test")]
        public static void Create()
        {
            StaticContext.Log.Info(instance.name);
        }

        #endregion
    }
}
