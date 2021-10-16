#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree.Segmented
{
    public abstract class SegmentedOctree<TTK, T, TT, TK, TV>
        where T : SegmentedOctree<TTK, T, TT, TK, TV>
        where TT : Octree<TT, TK, TV>
    {
        protected readonly float _capacityIncreaseMultiplier;
        protected readonly int _depth;
        protected readonly int _initialCapacity;
        protected readonly int _maxDepth;

        protected readonly OctreeStyle _style;
        protected Bounds _bounds;
        protected Vector3 _center;
        protected Vector3 _size;

        protected Dictionary<TTK, TT> _treeLookup;

        protected SegmentedOctree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth = Octree<TT, TK, TV>._MAX_DEPTH,
            int initialCapacity = Octree<TT, TK, TV>._INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = Octree<TT, TK, TV>._CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0) : this(
            style,
            new Bounds(position, size),
            maxDepth,
            initialCapacity,
            capacityIncreaseMultiplier,
            depth
        )
        {
        }

        protected SegmentedOctree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth = Octree<TT, TK, TV>._MAX_DEPTH,
            int initialCapacity = Octree<TT, TK, TV>._INITIAL_CAPACITY,
            float capacityIncreaseMultiplier = Octree<TT, TK, TV>._CAPACITY_INCREASE_MULTIPLIER,
            int depth = 0)
        {
            _style = style;
            _bounds = bounds;
            _center = _bounds.center;
            _size = _bounds.size;
            _maxDepth = maxDepth;
            _initialCapacity = initialCapacity;
            _capacityIncreaseMultiplier = capacityIncreaseMultiplier;
            _depth = depth;
        }

        public TT this[TTK key] => _treeLookup[key];

        public bool HasTree(TTK key)
        {
            if (_treeLookup == null)
            {
                _treeLookup = new Dictionary<TTK, TT>();
            }

            return _treeLookup.ContainsKey(key);
        }

        public bool AddTreeIfNecessary(TTK key)
        {
            if (_treeLookup == null)
            {
                _treeLookup = new Dictionary<TTK, TT>();
            }

            if (!_treeLookup.ContainsKey(key))
            {
                _treeLookup.Add(key, CreateFromVectors(_style, _center, _size, 0));

                return true;
            }

            return false;
        }

        public void RemoveTree(TTK key)
        {
            if (_treeLookup == null)
            {
                return;
            }

            if (_treeLookup.ContainsKey(key))
            {
                _treeLookup.Remove(key);
            }
        }

        public void TearDown(IEnumerable<TTK> keys)
        {
            if (_treeLookup == null)
            {
                return;
            }

            foreach (var key in keys)
            {
                if (_treeLookup.ContainsKey(key))
                {
                    var tree = _treeLookup[key];

                    tree.Clear();

                    _treeLookup.Remove(key);
                }
            }
        }

        public void TearDown()
        {
            if (_treeLookup == null)
            {
                return;
            }

            _treeLookup.Clear();
        }

        protected abstract TT CreateFromVectors(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int depth);
    }
}
