#if UNITY_EDITOR
namespace Appalachia.Spatial.ConvexDecomposition
{
    public enum ColliderBehavior
    {
        AlwaysEnabled = 0,
        EnabledInEditMode = 1,
        EnabledAtRuntime = 2,
        NeverEnabled = 3
    }
}

#endif