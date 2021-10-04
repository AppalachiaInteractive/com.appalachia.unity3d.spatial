#region

using System;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.SpatialKeys
{
    [Serializable]
    public struct QuaternionKey : IEquatable<QuaternionKey>
    {
        [SerializeField] private int _groupingScale;
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        [SerializeField] private int _z;
        [SerializeField] private int _w;

        public QuaternionKey(Quaternion vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
            _z = GetRounded(vector.z, groupingScale);
            _w = GetRounded(vector.w, groupingScale);
        }

        public QuaternionKey(quaternion vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.value.x, groupingScale);
            _y = GetRounded(vector.value.y, groupingScale);
            _z = GetRounded(vector.value.z, groupingScale);
            _w = GetRounded(vector.value.w, groupingScale);
        }

        public bool Equals(QuaternionKey other)
        {
            return (_groupingScale == other._groupingScale) &&
                   (_x == other._x) &&
                   (_y == other._y) &&
                   (_z == other._z) &&
                   (_w == other._w);
        }

        private static int GetRounded(float value, int scale)
        {
            return (int) math.round(value * scale);
        }

        public Quaternion ToQuaternion()
        {
            var fScale = (float) _groupingScale;
            return new Quaternion
            {
                x = _x / fScale,
                y = _y / fScale,
                z = _z / fScale,
                w = _w / fScale
            };
        }

        public quaternion Toquaternion()
        {
            var fScale = (float) _groupingScale;
            return new quaternion(_x / fScale, _y / fScale, _z / fScale, _w / fScale);
        }

        public override bool Equals(object obj)
        {
            return obj is QuaternionKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _groupingScale;
                hashCode = (hashCode * 397) ^ _x;
                hashCode = (hashCode * 397) ^ _y;
                hashCode = (hashCode * 397) ^ _z;
                hashCode = (hashCode * 397) ^ _w;
                return hashCode;
            }
        }

        public static bool operator ==(QuaternionKey left, QuaternionKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(QuaternionKey left, QuaternionKey right)
        {
            return !left.Equals(right);
        }
    }
}
