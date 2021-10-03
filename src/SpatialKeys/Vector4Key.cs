#region

using System;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.SpatialKeys
{
    [Serializable]
    public struct Vector4Key : IEquatable<Vector4Key>
    {
        [SerializeField] private int _groupingScale;
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        [SerializeField] private int _z;
        [SerializeField] private int _w;

        public Vector4Key(Vector4 vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
            _z = GetRounded(vector.z, groupingScale);
            _w = GetRounded(vector.w, groupingScale);
        }

        public Vector4Key(float4 vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
            _z = GetRounded(vector.z, groupingScale);
            _w = GetRounded(vector.w, groupingScale);
        }

        private static int GetRounded(float value, int scale)
        {
            return (int) math.round(value * scale);
        }

        public Vector4 ToVector4()
        {
            var fScale = (float) _groupingScale;
            return new Vector4
            {
                x = _x / fScale,
                y = _y / fScale,
                z = _z / fScale,
                w = _w / fScale
            };
        }

        public float4 Tofloat4()
        {
            var fScale = (float) _groupingScale;
            return new float4(_x / fScale, _y / fScale, _z / fScale, _w / fScale);
        }

        public bool Equals(Vector4Key other)
        {
            return _groupingScale == other._groupingScale && _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector4Key other && Equals(other);
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

        public static bool operator ==(Vector4Key left, Vector4Key right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4Key left, Vector4Key right)
        {
            return !left.Equals(right);
        }
    }
}
