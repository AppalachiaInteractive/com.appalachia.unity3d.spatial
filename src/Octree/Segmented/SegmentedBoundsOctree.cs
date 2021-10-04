#region

using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree.Segmented
{
    public sealed class SegmentedBoundsOctree<TTK, T> : SegmentedOctree<TTK,
        SegmentedBoundsOctree<TTK, T>, BoundsOctree<T>, Bounds, T>
    {
        public SegmentedBoundsOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = BoundsOctree<T>._MAX_DEPTH,
            int initialCapacity = BoundsOctree<T>._INITIAL_CAPACITY,
            int depth = 0) : base(style, position, size, maxDepth, initialCapacity, depth)
        {
        }

        public SegmentedBoundsOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = BoundsOctree<T>._MAX_DEPTH,
            int initialCapacity = BoundsOctree<T>._INITIAL_CAPACITY,
            int depth = 0) : base(style, bounds, maxDepth, initialCapacity, depth)
        {
        }

        protected override BoundsOctree<T> CreateFromVectors(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int depth)
        {
            return new(style, position, size, _maxDepth, _initialCapacity,
                _capacityIncreaseMultiplier, depth);
        }
    }
}
