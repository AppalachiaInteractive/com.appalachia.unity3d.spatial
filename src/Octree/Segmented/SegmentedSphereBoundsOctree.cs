#region

using Appalachia.Core.Math.Geometry;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree.Segmented
{
    public sealed class SegmentedSphereBoundsOctree<TTK, T> : SegmentedOctree<TTK,
        SegmentedSphereBoundsOctree<TTK, T>, SphereBoundsOctree<T>, SphereBounds, T>
    {
        public SegmentedSphereBoundsOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = SphereBoundsOctree<T>._MAX_DEPTH,
            int initialCapacity = SphereBoundsOctree<T>._INITIAL_CAPACITY,
            int depth = 0) : base(style, position, size, maxDepth, initialCapacity, depth)
        {
        }

        public SegmentedSphereBoundsOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = SphereBoundsOctree<T>._MAX_DEPTH,
            int initialCapacity = SphereBoundsOctree<T>._INITIAL_CAPACITY,
            int depth = 0) : base(style, bounds, maxDepth, initialCapacity, depth)
        {
        }

        /// <inheritdoc />
        protected override SphereBoundsOctree<T> CreateFromVectors(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int depth)
        {
            return new(
                style,
                position,
                size,
                _maxDepth,
                _initialCapacity,
                _capacityIncreaseMultiplier,
                depth
            );
        }
    }
}
