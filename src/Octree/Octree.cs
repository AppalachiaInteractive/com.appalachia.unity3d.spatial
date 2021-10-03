#region

using System;
using System.Collections.Generic;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.NonSerialized;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Octree
{
    public abstract class Octree<TTree, TKey, TValue>
        where TTree : Octree<TTree, TKey, TValue>
    {
        private const string _PRF_PFX = nameof(Octree<TTree, TKey, TValue>) + ".";
        public const int _INITIAL_CAPACITY = 128;
        public const float _CAPACITY_INCREASE_MULTIPLIER = 3.0f;
        public const int _MAX_DEPTH = 3;

        protected Bounds _bounds;
        protected Vector3[] _boundsExtents;
        protected NonSerializedList<TKey> _keys;
        protected NonSerializedList<TValue> _values;
        protected TTree[] _childTrees;
        protected int _nodeCount;

        protected readonly int _depth;
        protected readonly int _maxDepth;
        protected readonly int _initialCapacity;
        protected readonly float _capacityIncreaseMultiplier;

        protected OctreeStyle _style;

        public int NodeCount => _nodeCount;

#region Constructors

        protected abstract TTree CreateFromVectors(OctreeStyle style, Vector3 position, Vector3 size, int depth);

        protected Octree(
            OctreeStyle style,
            Vector3 position,
            Vector3 size,
            int maxDepth,
            int initialCapacity,
            float capacityIncreaseMultiplier,
            int depth)
        {
            _bounds.center = position;
            _bounds.size = size;

            _maxDepth = maxDepth;
            _initialCapacity = initialCapacity;
            _capacityIncreaseMultiplier = capacityIncreaseMultiplier;
            _depth = depth;
            _style = style;

            switch (style)
            {
                case OctreeStyle.FourDivisions:
                {
                    _boundsExtents = new Vector3[4];

                    var miniBoxSize = _bounds.extents;
                    miniBoxSize.y = _bounds.size.y;

                    var firstBoxMidpoint = _bounds.min + (miniBoxSize * 0.5f);

                    for (var i = 0; i < _boundsExtents.Length; i++)
                    {
                        _boundsExtents[i] = firstBoxMidpoint;
                    }

                    _boundsExtents[1].x += miniBoxSize.x;

                    _boundsExtents[2].z += miniBoxSize.z;

                    _boundsExtents[3].x += miniBoxSize.x;
                    _boundsExtents[3].z += miniBoxSize.z;

                    break;
                }
                case OctreeStyle.EightDivisions:
                {
                    _boundsExtents = new Vector3[8];

                    var miniBoxSize = _bounds.extents;

                    var firstBoxMidpoint = _bounds.min + (miniBoxSize * 0.5f);

                    for (var i = 0; i < _boundsExtents.Length; i++)
                    {
                        _boundsExtents[i] = firstBoxMidpoint;
                    }

                    _boundsExtents[1].x += miniBoxSize.x;

                    _boundsExtents[2].z += miniBoxSize.z;

                    _boundsExtents[3].x += miniBoxSize.x;
                    _boundsExtents[3].z += miniBoxSize.z;

                    _boundsExtents[4].y += miniBoxSize.y;

                    _boundsExtents[5].x += miniBoxSize.x;
                    _boundsExtents[5].y += miniBoxSize.y;

                    _boundsExtents[6].y += miniBoxSize.y;
                    _boundsExtents[6].z += miniBoxSize.z;

                    _boundsExtents[7] += miniBoxSize;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected Octree(
            OctreeStyle style,
            Bounds bounds,
            int maxDepth,
            int initialCapacity,
            float capacityIncreaseMultiplier,
            int depth) : this(style, bounds.center, bounds.size, maxDepth, initialCapacity, capacityIncreaseMultiplier, depth)
        {
        }

#endregion

#region Children

        private static readonly ProfilerMarker _PRF_CreateChildren = new ProfilerMarker(_PRF_PFX + nameof(CreateChildren));

        protected TTree[] CreateChildren()
        {
            using (_PRF_CreateChildren.Auto())
            {
                var ext = _bounds.extents;
                var newDepth = _depth + 1;

                return _style == OctreeStyle.EightDivisions
                    ? new TTree[8]
                    {
                        CreateFromVectors(_style, _boundsExtents[0], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[1], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[2], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[3], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[4], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[5], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[6], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[7], ext, newDepth)
                    }
                    : new TTree[4]
                    {
                        CreateFromVectors(_style, _boundsExtents[0], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[1], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[2], ext, newDepth),
                        CreateFromVectors(_style, _boundsExtents[3], ext, newDepth)
                    };
            }
        }

        private static readonly ProfilerMarker _PRF_GetAppropriateChildIndexFromVector =
            new ProfilerMarker(_PRF_PFX + nameof(GetAppropriateChildIndexFromVector));

        protected int GetAppropriateChildIndexFromVector(Vector3 position)
        {
            using (_PRF_GetAppropriateChildIndexFromVector.Auto())
            {
                var center = _bounds.center;

                switch (_style)
                {
                    case OctreeStyle.FourDivisions:
                    {
                        //  x < midpoint:  0, 2
                        if (position.x < center.x)
                        {
                            // z < midpoint:  0, 1
                            if (position.z < center.z)
                            {
                                return 0;
                            }

                            // z < midpoint:  2, 3
                            return 2;
                        }

                        // x >= midpoint:  1, 3
                        // z < midpoint:  0, 1 
                        if (position.z < center.z)
                        {
                            return 1;
                        }

                        // z < midpoint:  2, 3
                        return 3;
                    }

                    case OctreeStyle.EightDivisions:
                    {
                        //  x < midpoint:  0, 2, 4, 6
                        if (position.x < center.x)
                        {
                            // y < midpoint:  0, 1, 2, 3
                            if (position.y < center.y)
                            {
                                // z < midpoint:  0, 1, 4, 5   
                                if (position.z < center.z)
                                {
                                    return 0;
                                }

                                // z < midpoint:  2, 3, 6, 7  

                                return 2;
                            }

                            // y >= midpoint:  4, 5, 6, 7

                            // z < midpoint:  0, 1, 4, 5   
                            if (position.z < center.z)
                            {
                                return 4;
                            }

                            // z < midpoint:  2, 3, 6, 7  

                            return 6;
                        }

                        // x >= midpoint:  1, 3, 5, 7

                        // y < midpoint:  0, 1, 2, 3
                        if (position.y < center.y)
                        {
                            // z < midpoint:  0, 1, 4, 5   
                            if (position.z < center.z)
                            {
                                return 1;
                            }

                            // z < midpoint:  2, 3, 6, 7  

                            return 3;
                        }

                        // y >= midpoint >>    4, 5, 6, 7

                        // z < midpoint:  0, 1, 4, 5   
                        if (position.z < center.z)
                        {
                            return 5;
                        }

                        // z < midpoint:  2, 3, 6, 7  

                        return 7;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

#endregion

#region Key Checks

        protected abstract bool ContainedInTree(Bounds bounds, TKey key);

        protected abstract int GetAppropriateChildIndex(TKey key);

        protected abstract bool NodeIsEligible(TKey key, OctreeQueryMode mode, Bounds bounds);

        protected abstract float Magnitude(Vector3 position, TKey key);

        protected abstract float MagnitudeSquared(Vector3 position, TKey key);

#endregion

#region Add

        public bool Add(TKey key, TValue value)
        {
            using (_PRF_Add.Auto())
            {
                return Add(key, value, 0);
            }
        }

        private static readonly ProfilerMarker _PRF_Add = new ProfilerMarker(_PRF_PFX + nameof(Add));
        private static readonly ProfilerMarker _PRF_Add_ContainsCheck = new ProfilerMarker(_PRF_PFX + nameof(Add) + ".ContainsCheck");
        private static readonly ProfilerMarker _PRF_Add_AddChildren = new ProfilerMarker(_PRF_PFX + nameof(Add) + ".AddChildren");
        private static readonly ProfilerMarker _PRF_Add_AddNew = new ProfilerMarker(_PRF_PFX + nameof(Add) + ".AddNew");
        private static readonly ProfilerMarker _PRF_Add_AddKey = new ProfilerMarker(_PRF_PFX + nameof(Add) + ".AddKey");
        private static readonly ProfilerMarker _PRF_Add_AddValue = new ProfilerMarker(_PRF_PFX + nameof(Add) + ".AddValue");

        private bool Add(TKey key, TValue value, int depth)
        {
            using (_PRF_Add.Auto())
            {
                if (depth == 0)
                {
                    using (_PRF_Add_ContainsCheck.Auto())
                    {
                        if (!ContainedInTree(_bounds, key))
                        {
                            return false;
                        }
                    }
                }

                if (depth < _maxDepth)
                {
                    using (_PRF_Add_AddChildren.Auto())
                    {
                        if (_childTrees == null)
                        {
                            _childTrees = CreateChildren();
                        }

                        var childIndex = GetAppropriateChildIndex(key);
                        var childTree = _childTrees[childIndex];

                        childTree.Add(key, value, depth + 1);
                        _nodeCount += 1;
                        return true;

                        /*for (var i = 0; i < _childTrees.Length; i++)
                        {

                            _nodeCount += 1;
                            return true;
                        }*/
                    }
                }

                EnsureListsInitialized();

                using (_PRF_Add_AddNew.Auto())
                {
                    using (_PRF_Add_AddKey.Auto())
                    {
                        _keys.Add(key);
                    }

                    using (_PRF_Add_AddValue.Auto())
                    {
                        _values.Add(value);
                    }

                    _nodeCount += 1;
                    return true;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_EnsureListsInitialized = new ProfilerMarker(_PRF_PFX + nameof(EnsureListsInitialized));

        private void EnsureListsInitialized()
        {
            if ((_keys == null) || (_values == null))
            {
                using (_PRF_EnsureListsInitialized.Auto())
                {
                    if (_keys == null)
                    {
                        _keys = new NonSerializedList<TKey>(_initialCapacity, _capacityIncreaseMultiplier);
                    }

                    if (_values == null)
                    {
                        _values = new NonSerializedList<TValue>(_initialCapacity, _capacityIncreaseMultiplier);
                    }

                    /*if (_nodes == null)
                    {
                        _nodes = new List<OctreeNode<TK, TV>>(_initialCapacity);
                    }*/
                }
            }
        }

#endregion

#region Remove

        private static readonly ProfilerMarker _PRF_Remove = new ProfilerMarker(_PRF_PFX + nameof(Remove));

        public bool Remove(TKey key, out TValue value)
        {
            using (_PRF_Remove.Auto())
            {
                if (!ContainedInTree(_bounds, key))
                {
                    value = default;
                    return false;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var i = 0; i < _keys.Count; i++)
                    {
                        if (!Equals(_keys[i], key))
                        {
                            continue;
                        }

                        value = _values[i];

                        _values.RemoveAt(i);
                        _keys.RemoveAt(i);

                        _nodeCount--;

                        return true;
                    }
                }

                if (_childTrees == null)
                {
                    value = default;
                    return false;
                }

                for (var i = 0; i < _childTrees.Length; i++)
                {
                    if (_childTrees[i].Remove(key, out value))
                    {
                        _nodeCount--;
                        return true;
                    }
                }

                value = default;
                return false;
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            using (_PRF_Remove.Auto())
            {
                if (!ContainedInTree(_bounds, key))
                {
                    return false;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var i = 0; i < _keys.Count; i++)
                    {
                        if (!Equals(_keys[i], key) && !Equals(_values[i], value))
                        {
                            continue;
                        }

                        _keys.RemoveAt(i);
                        _values.RemoveAt(i);
                        _nodeCount--;

                        return true;
                    }
                }

                if (_childTrees == null)
                {
                    return false;
                }

                for (var i = 0; i < _childTrees.Length; i++)
                {
                    if (_childTrees[i].Remove(key, value))
                    {
                        _nodeCount--;
                        return true;
                    }
                }

                return false;
            }
        }

        private static readonly ProfilerMarker _PRF_RemoveIntersecting = new ProfilerMarker(_PRF_PFX + nameof(RemoveIntersecting));

        public int RemoveIntersecting(OctreeQueryMode mode, Bounds bounds, List<TKey> removedKeys, List<TValue> removedValues)
        {
            using (_PRF_RemoveIntersecting.Auto())
            {
                var removals = 0;

                if (!_bounds.Intersects(bounds))
                {
                    return removals;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var i = _keys.Count - 1; i >= 0; i--)
                    {
                        var key = _keys[i];

                        if (!NodeIsEligible(key, mode, bounds))
                        {
                            continue;
                        }

                        var value = _values[i];

                        removedKeys.Add(key);
                        removedValues.Add(value);
                        _keys.RemoveAt(i);
                        _values.RemoveAt(i);
                        removals++;
                    }
                }

                if (_childTrees != null)
                {
                    for (var i = 0; i < _childTrees.Length; i++)
                    {
                        removals += _childTrees[i].RemoveIntersecting(mode, bounds, removedKeys, removedValues);
                    }
                }

                _nodeCount -= removals;
                return removals;
            }
        }

#endregion

#region Maintenance

        private static readonly ProfilerMarker _PRF_Clear = new ProfilerMarker(_PRF_PFX + nameof(Clear));

        public void Clear()
        {
            using (_PRF_Clear.Auto())
            {
                _keys?.Clear();
                _values?.Clear();

                _nodeCount = 0;

                if (_childTrees != null)
                {
                    for (var index = 0; index < _childTrees.Length; index++)
                    {
                        var child = _childTrees[index];
                        child.Clear();
                    }
                }
            }
        }

        private static readonly ProfilerMarker _PRF_Reposition = new ProfilerMarker(_PRF_PFX + nameof(Reposition));

        public void Reposition(TKey oldKey, TKey newKey)
        {
            using (_PRF_Reposition.Auto())
            {
                if (Remove(oldKey, out var value))
                {
                    Add(newKey, value);
                }
                else
                {
                    throw new NotSupportedException($"Could not reposition key from {oldKey} to {newKey}.");
                }
            }
        }

        /*public int Reorganize(
            Bounds bounds,
            Func<TK, TV, TK> keyCheck,
            FastList<TK> keysToReorganize,
            FastList<TV> valuesToReorganize)
        {
            using (PROFILING.OCTREE.Reorganize.Auto())
            {
                var removals = 0;

                if (!_bounds.Intersects(bounds))
                {
                    return removals;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var i = _keys.Count - 1; i >= 0; i--)
                    {
                        var key = _keys[i];
                        var value = _values[i];

                        var newKey = keyCheck(key, value);

                        if (!Equals(newKey, key))
                        {
                            keysToReorganize.Add(key);
                            valuesToReorganize.Add(value);

                            _keys.RemoveAt(i);
                            _values.RemoveAt(i);

                            removals++;
                        }
                    }
                }

                if (_childTrees != null)
                {
                    for (var i = 0; i < _childTrees.Length; i++)
                    {
                        removals += _childTrees[i].Reorganize(bounds, keyCheck, keysToReorganize, valuesToReorganize);
                    }
                }

                _nodeCount -= removals;

                if (_depth == 0)
                {
                    for (var i = 0; i < keysToReorganize.Count; i++)
                    {
                        Add(keysToReorganize[i], valuesToReorganize[i], 0);
                    }
                }

                return removals;
            }
        }*/

#endregion

#region Query

        private static readonly ProfilerMarker _PRF_GetByKey = new ProfilerMarker(_PRF_PFX + nameof(GetByKey));

        public TValue GetByKey(TKey key)
        {
            using (_PRF_GetByKey.Auto())
            {
                if (!ContainedInTree(_bounds, key))
                {
                    return default;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var index = 0; index < _keys.Count; index++)
                    {
                        var k = _keys[index];

                        if (k.Equals(key))
                        {
                            return _values[index];
                        }
                    }
                }

                if (_childTrees == null)
                {
                    return default;
                }

                for (var i = 0; i < _childTrees.Length; i++)
                {
                    var result = _childTrees[i].GetByKey(key);

                    if (result != null)
                    {
                        return result;
                    }
                }

                return default;
            }
        }

        private static readonly ProfilerMarker _PRF_HasAny = new ProfilerMarker(_PRF_PFX + nameof(HasAny));

        public bool HasAny(OctreeQueryMode queryMode, Bounds bounds)
        {
            using (_PRF_HasAny.Auto())
            {
                if (!_bounds.Intersects(bounds))
                {
                    return false;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var index = 0; index < _keys.Count; index++)
                    {
                        var i = _keys[index];
                        if (NodeIsEligible(i, queryMode, bounds))
                        {
                            return true;
                        }
                    }
                }

                if (_childTrees == null)
                {
                    return false;
                }

                for (var i = 0; i < _childTrees.Length; i++)
                {
                    if (_childTrees[i].HasAny(queryMode, bounds))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static readonly ProfilerMarker _PRF_GetAll = new ProfilerMarker(_PRF_PFX + nameof(GetAll));

        public void GetAll(OctreeQueryMode queryMode, Bounds bounds, AppaList<TKey> keys, AppaList<TValue> values)
        {
            using (_PRF_GetAll.Auto())
            {
                GetAllWhere(queryMode, bounds, null, keys, values);
            }
        }

        private static readonly ProfilerMarker _PRF_GetAllWhere = new ProfilerMarker(_PRF_PFX + nameof(GetAllWhere));

        public void GetAllWhere(OctreeQueryMode queryMode, Bounds bounds, Predicate<TValue> predicate, AppaList<TKey> keys, AppaList<TValue> values)
        {
            using (_PRF_GetAllWhere.Auto())
            {
                if (!_bounds.Intersects(bounds))
                {
                    return;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var index = 0; index < _keys.Count; index++)
                    {
                        var key = _keys[index];

                        if (!NodeIsEligible(key, queryMode, bounds))
                        {
                            continue;
                        }

                        var value = _values[index];

                        if ((predicate == null) || predicate(value))
                        {
                            keys?.Add(key);
                            values?.Add(value);
                        }
                    }
                }

                if (_childTrees == null)
                {
                    return;
                }

                for (var i = 0; i < _childTrees.Length; i++)
                {
                    _childTrees[i].GetAllWhere(queryMode, bounds, predicate, keys, values);
                }
            }
        }

        private MagnitudeSquaredComparer GetNearestWhere_magnitudeSort;

        private class MagnitudeSquaredComparer : Comparer<TKey>
        {
            public Octree<TTree, TKey, TValue> tree;
            public Vector3 boundsCenter;

            public override int Compare(TKey x, TKey y)
            {
                var xm = tree.MagnitudeSquared(boundsCenter, x);
                var ym = tree.MagnitudeSquared(boundsCenter, y);

                return xm.CompareTo(ym);
            }
        }

        private static readonly ProfilerMarker _PRF_GetNearestWhere = new ProfilerMarker(_PRF_PFX + nameof(GetNearestWhere));

        public bool GetNearestWhere(OctreeQueryMode queryMode, Bounds bounds, Predicate<TValue> predicate, out TKey key, out TValue value)
        {
            using (_PRF_GetNearestWhere.Auto())
            {
                var keyResults = new NonSerializedList<TKey>(32);
                var valueResults = new NonSerializedList<TValue>(32);

                GetAllWhere(queryMode, bounds, null, keyResults, valueResults);

                if (keyResults.Count == 0)
                {
                    key = default;
                    value = default;
                    return false;
                }

                var keyArray = keyResults.ToArray();
                var valueArray = valueResults.ToArray();

                if (GetNearestWhere_magnitudeSort == null)
                {
                    GetNearestWhere_magnitudeSort = new MagnitudeSquaredComparer {tree = this};
                }

                GetNearestWhere_magnitudeSort.boundsCenter = bounds.center;

                Array.Sort(keyArray, valueArray, GetNearestWhere_magnitudeSort);

                for (var i = 0; i < keyArray.Length; i++)
                {
                    if ((predicate != null) && !predicate(valueArray[i]))
                    {
                        continue;
                    }

                    key = keyArray[i];
                    value = valueArray[i];
                    return true;
                }

                key = default;
                value = default;
                return false;
            }
        }

        private static readonly ProfilerMarker _PRF_YieldAll = new ProfilerMarker(_PRF_PFX + nameof(YieldAll));

        public IEnumerable<TValue> YieldAll(OctreeQueryMode queryMode, Bounds bounds)
        {
            using (_PRF_YieldAll.Auto())
            {
                return YieldAllWhere(queryMode, bounds, null);
            }
        }

        private static readonly ProfilerMarker _PRF_YieldAllWhere = new ProfilerMarker(_PRF_PFX + nameof(YieldAllWhere));

        public IEnumerable<TValue> YieldAllWhere(OctreeQueryMode queryMode, Bounds bounds, Predicate<TValue> predicate)
        {
            using (_PRF_YieldAllWhere.Auto())
            {
                if (!_bounds.Intersects(bounds))
                {
                    yield break;
                }

                if ((_keys != null) && (_values != null))
                {
                    for (var index = 0; index < _keys.Count; index++)
                    {
                        var key = _keys[index];
                        if (!NodeIsEligible(key, queryMode, bounds))
                        {
                            continue;
                        }

                        var value = _values[index];

                        if ((predicate == null) || predicate(value))
                        {
                            yield return value;
                        }
                    }
                }

                if (_childTrees == null)
                {
                    yield break;
                }

                for (var i = 0; i < _childTrees.Length; i++)
                {
                    var children = _childTrees[i].YieldAllWhere(queryMode, bounds, predicate);

                    foreach (var child in children)
                    {
                        yield return child;
                    }
                }
            }
        }

#endregion

#region Gizmos

        private static readonly ProfilerMarker _PRF_DrawGizmos = new ProfilerMarker(_PRF_PFX + nameof(DrawGizmos));
        private static readonly ProfilerMarker _PRF_InitializeGizmoColors = new ProfilerMarker(_PRF_PFX + nameof(InitializeGizmoColors));
        private static readonly ProfilerMarker _PRF_DrawGizmosBoundsWithData = new ProfilerMarker(_PRF_PFX + nameof(DrawGizmosBoundsWithData));
        private static readonly ProfilerMarker _PRF_DrawGizmosBoundsWithoutData = new ProfilerMarker(_PRF_PFX + nameof(DrawGizmosBoundsWithoutData));
        private static readonly ProfilerMarker _PRF_DrawNodeGizmos = new ProfilerMarker(_PRF_PFX + nameof(DrawNodeGizmos));
        
        private Color _gizmoNodeColor;
        private Color _gizmoBoundsWithDataColor;
        private Color _gizmoBoundsWithoutDataColor;

        protected abstract Color gizmoNodeColor { get; }
        protected abstract Color gizmoBoundsColor { get; }
        
        public void DrawGizmos(int drawDepth = -1)
        {
            using (_PRF_DrawGizmos.Auto())
            {
                DrawGizmos(drawDepth, 0);
            }
        }

        private void InitializeGizmoColors()
        {
            using (_PRF_InitializeGizmoColors.Auto())
            {
                if (_gizmoNodeColor == default)
                {
                    _gizmoNodeColor = gizmoNodeColor;
                }

                if (_gizmoBoundsWithDataColor == default)
                {
                    _gizmoBoundsWithDataColor = gizmoBoundsColor;
                }

                if (_gizmoBoundsWithoutDataColor == default)
                {
                    _gizmoBoundsWithoutDataColor = _gizmoBoundsWithDataColor;
                    _gizmoBoundsWithoutDataColor *= .15f;
                }
            }
        }

        /*private void DrawGizmos(int drawDepth, int depth)
        {
            using (_PRF_DrawGizmos.Auto())
            {
                InitializeGizmoColors();

                if ((drawDepth < 0) || (drawDepth == depth))
                {
                    if ((_keys != null) && (_keys.Count > 0))
                    {
                        Gizmos.color = _gizmoBoundsWithDataColor;
                    }
                    else
                    {
                        Gizmos.color = _gizmoBoundsWithoutDataColor;
                    }

                    Gizmos.DrawWireCube(_bounds.center, _bounds.size);
                    if ((_keys != null) && (_values != null))
                    {
                        Gizmos.color = _gizmoNodeColor;

                        for (var index = 0; index < _keys.Count; index++)
                        {
                            DrawNodeGizmo(_keys[index], _values[index]);
                        }
                    }
                }

                if (_childTrees != null)
                {
                    for (var i = 0; i < _childTrees.Length; i++)
                    {
                        _childTrees[i].DrawGizmos(drawDepth, depth + 1);
                    }
                }
            }
        }
        */

        private void DrawGizmos(int drawDepth, int depth)
        {
            using (_PRF_DrawGizmos.Auto())
            {
                InitializeGizmoColors();

                DrawGizmosBoundsWithData(drawDepth, depth);
                DrawGizmosBoundsWithoutData(drawDepth, depth);
                DrawNodeGizmos(drawDepth, depth);
            }
        }

        private void DrawGizmosBoundsWithData(int drawDepth, int depth)
        {
            using (_PRF_DrawGizmosBoundsWithData.Auto())
            {
                if ((drawDepth < 0) || (drawDepth == depth))
                {
                    if ((_keys != null) && (_keys.Count > 0))
                    {
                        if (Gizmos.color != _gizmoBoundsWithDataColor)
                        {
                            Gizmos.color = _gizmoBoundsWithDataColor;
                        }
                        
                        Gizmos.DrawWireCube(_bounds.center, _bounds.size);
                    }
                }

                if (_childTrees != null)
                {
                    for (var i = 0; i < _childTrees.Length; i++)
                    {
                        _childTrees[i].DrawGizmosBoundsWithData(drawDepth, depth + 1);
                    }
                }
            }
        }

        private void DrawGizmosBoundsWithoutData(int drawDepth, int depth)
        {
            using (_PRF_DrawGizmosBoundsWithoutData.Auto())
            {
                if ((drawDepth < 0) || (drawDepth == depth))
                {
                    if ((_keys == null) || (_keys.Count <= 0))
                    {
                        if (Gizmos.color != _gizmoBoundsWithoutDataColor)
                        {
                            Gizmos.color = _gizmoBoundsWithoutDataColor;
                        }
                        
                        Gizmos.DrawWireCube(_bounds.center, _bounds.size);
                    }
                }

                if (_childTrees != null)
                {
                    for (var i = 0; i < _childTrees.Length; i++)
                    {
                        _childTrees[i].DrawGizmosBoundsWithoutData(drawDepth, depth + 1);
                    }
                }
            }
        }

        private void DrawNodeGizmos(int drawDepth, int depth)
        {
            using (_PRF_DrawNodeGizmos.Auto())
            {
                if ((drawDepth < 0) || (drawDepth == depth))
                {
                    if ((_keys != null) && (_values != null))
                    {
                        if (Gizmos.color != _gizmoNodeColor)
                        {
                            Gizmos.color = _gizmoNodeColor;                            
                        }

                        for (var index = 0; index < _keys.Count; index++)
                        {
                            DrawNodeGizmo(_keys[index], _values[index]);
                        }
                    }
                }

                if (_childTrees != null)
                {
                    for (var i = 0; i < _childTrees.Length; i++)
                    {
                        _childTrees[i].DrawNodeGizmos(drawDepth, depth + 1);
                    }
                }
            }
        }
        
        protected abstract void DrawNodeGizmo(TKey key, TValue value);

#endregion
    }
}
