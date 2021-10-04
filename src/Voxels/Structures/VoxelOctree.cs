#region

using Appalachia.Editing.Preferences.Globals;
using Appalachia.Spatial.Octree;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;
using Appalachia.Utility.Constants;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Structures
{
    public sealed class VoxelOctree<TVoxelData, TVoxelRaycastHit, TValue> : Octree<VoxelOctree<TVoxelData, TVoxelRaycastHit, TValue>, int, TValue>
        where TValue : struct, IOctreeNodeGizmoDrawer
        where TVoxelData : VoxelsBase<TVoxelData, TVoxelRaycastHit>
        where TVoxelRaycastHit : struct, IVoxelRaycastHit
    {
        private readonly TVoxelData _voxelData;

        public VoxelOctree(
            TVoxelData voxelData,
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(style, position, size, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
            _voxelData = voxelData;
        }

        public VoxelOctree(
            TVoxelData voxelData,
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(style, bounds, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
            _voxelData = voxelData;
        }

        protected override Color gizmoNodeColor => ColorPrefs.Instance.Octree_Voxel_NodeColor.v;
        protected override Color gizmoBoundsColor => ColorPrefs.Instance.Octree_Voxel_BoundsColor.v;

        private float gizmoNodeScale => ColorPrefs.Instance.Octree_Voxel_NodeScale.v;

        protected override VoxelOctree<TVoxelData, TVoxelRaycastHit, TValue> CreateFromVectors(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int depth)
        {
            using (_PRF_CreateFromVectors.Auto())
            {
                return new VoxelOctree<TVoxelData, TVoxelRaycastHit, TValue>(_voxelData, style, position, size, _maxDepth, _initialCapacity, depth);
            }
        }

        protected override bool ContainedInTree(Bounds bounds, int key)
        {
            using (_PRF_ContainedInTree.Auto())
            {
                var voxel = _voxelData.voxels[key];
                return bounds.Contains(voxel.position);
            }
        }

        protected override int GetAppropriateChildIndex(int key)
        {
            using (_PRF_GetAppropriateChildIndex.Auto())
            {
                var voxel = _voxelData.voxels[key];
                return GetAppropriateChildIndexFromVector(voxel.position);
            }
        }

        protected override bool NodeIsEligible(int key, OctreeQueryMode mode, Bounds bounds)
        {
            using (_PRF_NodeIsEligible.Auto())
            {
                var voxel = _voxelData.voxels[key];
                return bounds.Contains(voxel.position);
            }
        }

        protected override float Magnitude(Vector3 position, int key)
        {
            using (_PRF_Magnitude.Auto())
            {
                var voxel = _voxelData.voxels[key];
                return math.distance(position, voxel.position);
            }
        }

        protected override float MagnitudeSquared(Vector3 position, int key)
        {
            using (_PRF_MagnitudeSquared.Auto())
            {
                var voxel = _voxelData.voxels[key];
                return math.distancesq(position, voxel.position);
            }
        }

        protected override void DrawNodeGizmo(int key, TValue value)
        {
            using (_PRF_DrawNodeGizmo.Auto())
            {
                var voxel = _voxelData.voxels[key];
                var scale = _voxelData.resolution * gizmoNodeScale;
                UnityEngine.Gizmos.DrawWireCube(voxel.position, float3c.one * scale);
                value.DrawGizmo(voxel.position, scale);
            }
        }

#region Profiling

        private const string _PRF_PFX = nameof(VoxelOctree<TVoxelData, TVoxelRaycastHit, TValue>) + ".";
        private static readonly ProfilerMarker _PRF_CreateFromVectors = new ProfilerMarker(_PRF_PFX + nameof(CreateFromVectors));
        private static readonly ProfilerMarker _PRF_ContainedInTree = new ProfilerMarker(_PRF_PFX + nameof(ContainedInTree));
        private static readonly ProfilerMarker _PRF_GetAppropriateChildIndex = new ProfilerMarker(_PRF_PFX + nameof(GetAppropriateChildIndex));
        private static readonly ProfilerMarker _PRF_NodeIsEligible = new ProfilerMarker(_PRF_PFX + nameof(NodeIsEligible));
        private static readonly ProfilerMarker _PRF_Magnitude = new ProfilerMarker(_PRF_PFX + nameof(Magnitude));
        private static readonly ProfilerMarker _PRF_MagnitudeSquared = new ProfilerMarker(_PRF_PFX + nameof(MagnitudeSquared));
        private static readonly ProfilerMarker _PRF_DrawNodeGizmo = new ProfilerMarker(_PRF_PFX + nameof(DrawNodeGizmo));

#endregion
    }
}
