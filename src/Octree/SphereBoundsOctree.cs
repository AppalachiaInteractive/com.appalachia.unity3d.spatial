#region

using System;
using Appalachia.Core.Geometry;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree
{
    public class SphereBoundsOctree<T> : Octree<SphereBoundsOctree<T>, SphereBounds, T>
    {
        private const string _PRF_PFX = nameof(SphereBoundsOctree<T>) + ".";

        private static readonly ProfilerMarker _PRF_CreateFromVectors = new ProfilerMarker(_PRF_PFX + nameof(CreateFromVectors));

        private static readonly ProfilerMarker _PRF_ContainedInTree = new ProfilerMarker(_PRF_PFX + nameof(ContainedInTree));

        private static readonly ProfilerMarker _PRF_GetAppropriateChildIndex = new ProfilerMarker(_PRF_PFX + nameof(GetAppropriateChildIndex));

        private static readonly ProfilerMarker _PRF_NodeIsEligible = new ProfilerMarker(_PRF_PFX + nameof(NodeIsEligible));

        private static readonly ProfilerMarker _PRF_Magnitude = new ProfilerMarker(_PRF_PFX + nameof(Magnitude));

        private static readonly ProfilerMarker _PRF_MagnitudeSquared = new ProfilerMarker(_PRF_PFX + nameof(MagnitudeSquared));

        public SphereBoundsOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(style, position, size, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
        }

        public SphereBoundsOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(style, bounds, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
        }

        protected override Color gizmoNodeColor => Color.green;
        protected override Color gizmoBoundsColor => Color.magenta;

        protected override SphereBoundsOctree<T> CreateFromVectors(OctreeStyle style, Vector3 position, Vector3 size, int depth)
        {
            using (_PRF_CreateFromVectors.Auto())
            {
                return new SphereBoundsOctree<T>(style, position, size, _maxDepth, _initialCapacity, depth);
            }
        }

        protected override bool ContainedInTree(Bounds bounds, SphereBounds key)
        {
            using (_PRF_ContainedInTree.Auto())
            {
                return bounds.Contains(key.center);
            }
        }

        protected override int GetAppropriateChildIndex(SphereBounds key)
        {
            using (_PRF_GetAppropriateChildIndex.Auto())
            {
                return GetAppropriateChildIndexFromVector(key.center);
            }
        }

        protected override bool NodeIsEligible(SphereBounds key, OctreeQueryMode mode, Bounds bounds)
        {
            using (_PRF_NodeIsEligible.Auto())
            {
                switch (mode)
                {
                    /*case OctreeQueryMode.Inside:
                        return key.ContainedBy(bounds);
                    case OctreeQueryMode.Intersecting:
                        return key.Intersects(bounds);*/
                    case OctreeQueryMode.InsideOrIntersecting:
                        return !key.IsOutsideOf(bounds);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
        }

        protected override float Magnitude(Vector3 position, SphereBounds key)
        {
            using (_PRF_Magnitude.Auto())
            {
                return math.distance(key.center, position);
            }
        }

        protected override float MagnitudeSquared(Vector3 position, SphereBounds key)
        {
            using (_PRF_MagnitudeSquared.Auto())
            {
                return math.distancesq(key.center, position);
            }
        }

        protected override void DrawNodeGizmo(SphereBounds key, T valuee)

            //protected override void DrawNodeGizmos(OctreeNode<SphereBounds, T> node)
        {
            Gizmos.DrawLine(key.center, _bounds.center);
            Gizmos.DrawSphere(key.center, key.radius);
            Gizmos.DrawWireSphere(key.center, key.radius);
            /*Gizmos.DrawSphere(node.key.center, node.key.radius);
            Gizmos.DrawWireSphere(node.key.center, node.key.radius);*/
        }
    }
}
