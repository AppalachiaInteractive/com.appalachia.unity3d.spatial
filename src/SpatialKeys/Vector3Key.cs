#region

using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.SpatialKeys
{
    [Serializable]
    public struct Vector3Key : IEquatable<Vector3Key>
    {
        [SerializeField] private int _groupingScale;
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        [SerializeField] private int _z;
        [SerializeField] private float _descaledX;
        [SerializeField] private float _descaledY;
        [SerializeField] private float _descaledZ;

        public Vector3Key(Vector3 vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
            _z = GetRounded(vector.z, groupingScale);
            _descaledX = _x / (float) groupingScale;
            _descaledY = _y / (float) groupingScale;
            _descaledZ = _z / (float) groupingScale;
        }

        public Vector3Key(float3 vector, int groupingScale)
        {
            _groupingScale = groupingScale;
            _x = GetRounded(vector.x, groupingScale);
            _y = GetRounded(vector.y, groupingScale);
            _z = GetRounded(vector.z, groupingScale);
            _descaledX = _x / (float) groupingScale;
            _descaledY = _y / (float) groupingScale;
            _descaledZ = _z / (float) groupingScale;
        }

        public bool Equals(Vector3Key other)
        {
            return (_groupingScale == other._groupingScale) &&
                   (_x == other._x) &&
                   (_y == other._y) &&
                   (_z == other._z);
        }

        private static int GetRounded(float value, int scale)
        {
            return (int) math.round(value * scale);
        }

        public Vector3 ToVector3()
        {
            var fScale = (float) _groupingScale;
            return new Vector3 {x = _x / fScale, y = _y / fScale, z = _z / fScale};
        }

        public float3 Tofloat3()
        {
            var fScale = (float) _groupingScale;
            return new float3(_x / fScale, _y / fScale, _z / fScale);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3Key other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _groupingScale;
                hashCode = (hashCode * 397) ^ _x;
                hashCode = (hashCode * 397) ^ _y;
                hashCode = (hashCode * 397) ^ _z;
                return hashCode;
            }
        }

        public static bool operator ==(Vector3Key left, Vector3Key right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3Key left, Vector3Key right)
        {
            return !left.Equals(right);
        }

        [BurstDiscard]
        public override string ToString()
        {
            const string float3format = "{0:F2}, {1:F2}, {2:F2}";
            return string.Format(float3format, _descaledX, _descaledY, _descaledZ);
        }
    }
}
