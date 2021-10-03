using Unity.Mathematics;

namespace Appalachia.Spatial.Octree
{
    public interface IOctreeNodeGizmoDrawer
    {
        void DrawGizmo(float3 position, float3 scale);
    }
}
