#region

using System;
using Appalachia.Utility.Constants;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree
{
    public sealed class PointOctree<T> : Octree<PointOctree<T>, Vector3, T>
    {
        private const string _PRF_PFX = nameof(PointOctree<T>) + ".";

        private static readonly ProfilerMarker _PRF_CreateFromVectors = new ProfilerMarker(_PRF_PFX + nameof(CreateFromVectors));

        private static readonly ProfilerMarker _PRF_ContainedInTree = new ProfilerMarker(_PRF_PFX + nameof(ContainedInTree));

        private static readonly ProfilerMarker _PRF_GetAppropriateChildIndex = new ProfilerMarker(_PRF_PFX + nameof(GetAppropriateChildIndex));

        private static readonly ProfilerMarker _PRF_NodeIsEligible = new ProfilerMarker(_PRF_PFX + nameof(NodeIsEligible));

        private static readonly ProfilerMarker _PRF_Magnitude = new ProfilerMarker(_PRF_PFX + nameof(Magnitude));

        private static readonly ProfilerMarker _PRF_MagnitudeSquared = new ProfilerMarker(_PRF_PFX + nameof(MagnitudeSquared));

        public PointOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(style, position, size, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
        }

        public PointOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(style, bounds, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
        }

        protected override Color gizmoNodeColor => Color.blue;
        protected override Color gizmoBoundsColor => Color.yellow;

        protected override PointOctree<T> CreateFromVectors(OctreeStyle style, Vector3 position, Vector3 size, int depth)
        {
            using (_PRF_CreateFromVectors.Auto())
            {
                return new PointOctree<T>(style, position, size, _maxDepth, _initialCapacity, depth);
            }
        }

        protected override bool ContainedInTree(Bounds bounds, Vector3 key)
        {
            using (_PRF_ContainedInTree.Auto())
            {
                return bounds.Contains(key);
            }
        }

        protected override int GetAppropriateChildIndex(Vector3 key)
        {
            using (_PRF_GetAppropriateChildIndex.Auto())
            {
                return GetAppropriateChildIndexFromVector(key);
            }
        }

        protected override bool NodeIsEligible(Vector3 key, OctreeQueryMode mode, Bounds bounds)
        {
            using (_PRF_NodeIsEligible.Auto())
            {
                switch (mode)
                {
                    /*case OctreeQueryMode.Inside:
                    case OctreeQueryMode.Intersecting:*/
                    case OctreeQueryMode.InsideOrIntersecting:
                        return bounds.Contains(key);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
        }

        protected override float Magnitude(Vector3 position, Vector3 key)
        {
            using (_PRF_Magnitude.Auto())
            {
                return math.distance(position, key);
            }
        }

        protected override float MagnitudeSquared(Vector3 position, Vector3 key)
        {
            using (_PRF_MagnitudeSquared.Auto())
            {
                return math.distancesq(position, key);
            }
        }

        protected override void DrawNodeGizmo(Vector3 key, T value)
        {
            Gizmos.DrawWireCube(key, float3c.one * math.clamp(_bounds.size.magnitude / 512.0f, 0.1f, 1.0f));
        }
    }
}
