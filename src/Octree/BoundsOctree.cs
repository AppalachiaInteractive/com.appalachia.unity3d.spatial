#region

using System;
using Appalachia.Core.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree
{
    public sealed class BoundsOctree<T> : Octree<BoundsOctree<T>, Bounds, T>
    {
        private const string _PRF_PFX = nameof(BoundsOctree<T>) + ".";

        private static readonly ProfilerMarker _PRF_GetRayHits = new(_PRF_PFX + nameof(GetRayHits));

        private static readonly ProfilerMarker _PRF_GetRayHitsWhere =
            new(_PRF_PFX + nameof(GetRayHitsWhere));

        private static readonly ProfilerMarker _PRF_CreateFromVectors =
            new(_PRF_PFX + nameof(CreateFromVectors));

        private static readonly ProfilerMarker _PRF_ContainedInTree =
            new(_PRF_PFX + nameof(ContainedInTree));

        private static readonly ProfilerMarker _PRF_GetAppropriateChildIndex =
            new(_PRF_PFX + nameof(GetAppropriateChildIndex));

        private static readonly ProfilerMarker _PRF_NodeIsEligible =
            new(_PRF_PFX + nameof(NodeIsEligible));

        private static readonly ProfilerMarker _PRF_Magnitude = new(_PRF_PFX + nameof(Magnitude));

        private static readonly ProfilerMarker _PRF_MagnitudeSquared =
            new(_PRF_PFX + nameof(MagnitudeSquared));

        public BoundsOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(
            style,
            position,
            size,
            maxDepth,
            initialCapacity,
            capacityIncreaseMultiplier,
            depth
        )
        {
        }

        public BoundsOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = _MAX_DEPTH,
            int initialCapacity = _INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = _CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : base(
            style,
            bounds,
            maxDepth,
            initialCapacity,
            capacityIncreaseMultiplier,
            depth
        )
        {
        }

        protected override Color gizmoNodeColor => Color.cyan;
        protected override Color gizmoBoundsColor => Color.red;

        public void GetRayHits(Ray ray, AppaList<Bounds> keys, AppaList<T> values)
        {
            using (_PRF_GetRayHits.Auto())
            {
                GetRayHitsWhere(ray, null, keys, values);
            }
        }

        public void GetRayHitsWhere(
            Ray ray,
            Predicate<T> predicate,
            AppaList<Bounds> keys,
            AppaList<T> values)
        {
            using (_PRF_GetRayHitsWhere.Auto())
            {
                if (!_bounds.IntersectRay(ray))
                {
                    return;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var index = 0; index < _keys.Count; index++)
                    {
                        var key = _keys[index];

                        if (!key.IntersectRay(ray))
                        {
                            continue;
                        }

                        var value = _values[index];

                        if ((predicate == null) || predicate(value))
                        {
                            keys.Add(key);
                            values.Add(value);
                        }
                    }
                }

                if (_childTrees == null)
                {
                    return;
                }

                for (var i = 0; i < _childTrees.Length; ++i)
                {
                    _childTrees[i].GetRayHitsWhere(ray, predicate, keys, values);
                }
            }
        }

        protected override BoundsOctree<T> CreateFromVectors(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int depth)
        {
            using (_PRF_CreateFromVectors.Auto())
            {
                return new BoundsOctree<T>(
                    style,
                    position,
                    size,
                    _maxDepth,
                    _initialCapacity,
                    depth
                );
            }
        }

        protected override bool ContainedInTree(Bounds bounds, Bounds key)
        {
            using (_PRF_ContainedInTree.Auto())
            {
                return bounds.Contains(key.center);
            }
        }

        protected override int GetAppropriateChildIndex(Bounds key)
        {
            using (_PRF_GetAppropriateChildIndex.Auto())
            {
                return GetAppropriateChildIndexFromVector(key.center);
            }
        }

        protected override bool NodeIsEligible(Bounds key, OctreeQueryMode mode, Bounds bounds)
        {
            using (_PRF_NodeIsEligible.Auto())
            {
                switch (mode)
                {
                    /*case OctreeQueryMode.Inside:
                        return bounds.Contains(key.center);
                    case OctreeQueryMode.Intersecting:
                        return bounds.Intersects(key);*/
                    case OctreeQueryMode.InsideOrIntersecting:
                        return /*bounds.Contains(key.center) || */bounds.Intersects(key);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
        }

        protected override float Magnitude(Vector3 position, Bounds key)
        {
            using (_PRF_Magnitude.Auto())
            {
                return math.distance(key.center, position);
            }
        }

        protected override float MagnitudeSquared(Vector3 position, Bounds key)
        {
            using (_PRF_MagnitudeSquared.Auto())
            {
                return math.distancesq(key.center, position);
            }
        }

        protected override void DrawNodeGizmo(Bounds key, T value)
        {
            Gizmos.DrawWireCube(key.center, key.size);
        }
    }
}
