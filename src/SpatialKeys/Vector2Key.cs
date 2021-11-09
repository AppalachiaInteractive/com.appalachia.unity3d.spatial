#region

using System;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.SpatialKeys
{
    [Serializable]
    public struct Vector2Key : IEquatable<Vector2Key>
    {
        [SerializeField] private int _groupingScale;
        [SerializeField] private int _x;
        [SerializeField] private int _y;

        public Vector2Key(Vector2 vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
        }

        public Vector2Key(float2 vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
        }

        [DebuggerStepThrough] public bool Equals(Vector2Key other)
        {
            return (_groupingScale == other._groupingScale) && (_x == other._x) && (_y == other._y);
        }

        private static int GetRounded(float value, int scale)
        {
            return (int) math.round(value * scale);
        }

        public Vector2 ToVector2()
        {
            var fScale = (float) _groupingScale;
            return new Vector2 {x = _x / fScale, y = _y / fScale};
        }

        public float2 Tofloat2()
        {
            var fScale = (float) _groupingScale;
            return new float2(_x / fScale, _y / fScale);
        }

        [DebuggerStepThrough] public override bool Equals(object obj)
        {
            return obj is Vector2Key other && Equals(other);
        }

        [DebuggerStepThrough] public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _groupingScale;
                hashCode = (hashCode * 397) ^ _x;
                hashCode = (hashCode * 397) ^ _y;
                return hashCode;
            }
        }

        [DebuggerStepThrough] public static bool operator ==(Vector2Key left, Vector2Key right)
        {
            return left.Equals(right);
        }

        [DebuggerStepThrough] public static bool operator !=(Vector2Key left, Vector2Key right)
        {
            return !left.Equals(right);
        }
    }
}
