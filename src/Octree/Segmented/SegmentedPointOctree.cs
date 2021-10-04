#region

using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree.Segmented
{
    public sealed class SegmentedPointOctree<TTK, T> : SegmentedOctree<TTK,
        SegmentedPointOctree<TTK, T>, PointOctree<T>, Vector3, T>
    {
        public SegmentedPointOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = PointOctree<T>._MAX_DEPTH,
            int initialCapacity = PointOctree<T>._INITIAL_CAPACITY,
            int depth = 0) : base(style, position, size, maxDepth, initialCapacity, depth)
        {
        }

        public SegmentedPointOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = PointOctree<T>._MAX_DEPTH,
            int initialCapacity = PointOctree<T>._INITIAL_CAPACITY,
            int depth = 0) : base(style, bounds, maxDepth, initialCapacity, depth)
        {
        }

        protected override PointOctree<T> CreateFromVectors(
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
