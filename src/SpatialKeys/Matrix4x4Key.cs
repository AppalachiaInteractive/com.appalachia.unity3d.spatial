#region

using System;
using Appalachia.CI.Constants;
using Appalachia.Core.Extensions;
using Appalachia.Utility.Extensions;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.SpatialKeys
{
    [Serializable]
    public struct Matrix4x4Key : IEquatable<Matrix4x4Key>, ISerializationCallbackReceiver
    {
        [SerializeField] private int _groupingScale;
        [SerializeField] private int _m00;
        [SerializeField] private int _m01;
        [SerializeField] private int _m02;
        [SerializeField] private int _m03;
        [SerializeField] private int _m10;
        [SerializeField] private int _m11;
        [SerializeField] private int _m12;
        [SerializeField] private int _m13;
        [SerializeField] private int _m20;
        [SerializeField] private int _m21;
        [SerializeField] private int _m22;
        [SerializeField] private int _m23;
        [SerializeField] private int _m30;
        [SerializeField] private int _m31;
        [SerializeField] private int _m32;
        [SerializeField] private int _m33;
        [SerializeField] private float4x4 original;
        [SerializeField] private float3 translation;
        [SerializeField] private quaternion rotation;
        [SerializeField] private float3 scale;

        public Matrix4x4Key(Matrix4x4 matrix, int groupingScale)
        {
            original = matrix;
            translation = matrix.GetPositionFromMatrix();
            rotation = matrix.GetRotationFromMatrix();
            scale = matrix.GetScaleFromMatrix();
            _groupingScale = groupingScale;
            _m00 = GetRounded(matrix.m00, groupingScale);
            _m01 = GetRounded(matrix.m01, groupingScale);
            _m02 = GetRounded(matrix.m02, groupingScale);
            _m03 = GetRounded(matrix.m03, groupingScale);
            _m10 = GetRounded(matrix.m10, groupingScale);
            _m11 = GetRounded(matrix.m11, groupingScale);
            _m12 = GetRounded(matrix.m12, groupingScale);
            _m13 = GetRounded(matrix.m13, groupingScale);
            _m20 = GetRounded(matrix.m20, groupingScale);
            _m21 = GetRounded(matrix.m21, groupingScale);
            _m22 = GetRounded(matrix.m22, groupingScale);
            _m23 = GetRounded(matrix.m23, groupingScale);
            _m30 = GetRounded(matrix.m30, groupingScale);
            _m31 = GetRounded(matrix.m31, groupingScale);
            _m32 = GetRounded(matrix.m32, groupingScale);
            _m33 = GetRounded(matrix.m33, groupingScale);
        }

        public Matrix4x4Key(float4x4 matrix, int groupingScale)
        {
            original = matrix;
            translation = matrix.GetPositionFromMatrix();
            rotation = matrix.GetRotationFromMatrix();
            scale = matrix.GetScaleFromMatrix();
            _groupingScale = groupingScale;
            _m00 = GetRounded(matrix.c0.x, groupingScale);
            _m01 = GetRounded(matrix.c1.x, groupingScale);
            _m02 = GetRounded(matrix.c2.x, groupingScale);
            _m03 = GetRounded(matrix.c3.x, groupingScale);
            _m10 = GetRounded(matrix.c0.y, groupingScale);
            _m11 = GetRounded(matrix.c1.y, groupingScale);
            _m12 = GetRounded(matrix.c2.y, groupingScale);
            _m13 = GetRounded(matrix.c3.y, groupingScale);
            _m20 = GetRounded(matrix.c0.z, groupingScale);
            _m21 = GetRounded(matrix.c1.z, groupingScale);
            _m22 = GetRounded(matrix.c2.z, groupingScale);
            _m23 = GetRounded(matrix.c3.z, groupingScale);
            _m30 = GetRounded(matrix.c0.w, groupingScale);
            _m31 = GetRounded(matrix.c1.w, groupingScale);
            _m32 = GetRounded(matrix.c2.w, groupingScale);
            _m33 = GetRounded(matrix.c3.w, groupingScale);
        }

        public bool Equals(Matrix4x4Key other)
        {
            return (_groupingScale == other._groupingScale) &&
                   (_m00 == other._m00) &&
                   (_m01 == other._m01) &&
                   (_m02 == other._m02) &&
                   (_m03 == other._m03) &&
                   (_m10 == other._m10) &&
                   (_m11 == other._m11) &&
                   (_m12 == other._m12) &&
                   (_m13 == other._m13) &&
                   (_m20 == other._m20) &&
                   (_m21 == other._m21) &&
                   (_m22 == other._m22) &&
                   (_m23 == other._m23) &&
                   (_m30 == other._m30) &&
                   (_m31 == other._m31) &&
                   (_m32 == other._m32) &&
                   (_m33 == other._m33);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if ((_groupingScale == 0) && !original.Equals(default))
            {
                _groupingScale = CONSTANTS.MatrixKeyGrouping;
                _m00 = GetRounded(original.c0.x, _groupingScale);
                _m01 = GetRounded(original.c1.x, _groupingScale);
                _m02 = GetRounded(original.c2.x, _groupingScale);
                _m03 = GetRounded(original.c3.x, _groupingScale);
                _m10 = GetRounded(original.c0.y, _groupingScale);
                _m11 = GetRounded(original.c1.y, _groupingScale);
                _m12 = GetRounded(original.c2.y, _groupingScale);
                _m13 = GetRounded(original.c3.y, _groupingScale);
                _m20 = GetRounded(original.c0.z, _groupingScale);
                _m21 = GetRounded(original.c1.z, _groupingScale);
                _m22 = GetRounded(original.c2.z, _groupingScale);
                _m23 = GetRounded(original.c3.z, _groupingScale);
                _m30 = GetRounded(original.c0.w, _groupingScale);
                _m31 = GetRounded(original.c1.w, _groupingScale);
                _m32 = GetRounded(original.c2.w, _groupingScale);
                _m33 = GetRounded(original.c3.w, _groupingScale);
            }
        }

        private static int GetRounded(float value, int scale)
        {
            return (int) math.round(value * scale);
        }

        public Matrix4x4 ToMatrix4x4()
        {
            var fScale = (float) _groupingScale;
            return new Matrix4x4
            {
                m00 = _m00 / fScale,
                m01 = _m01 / fScale,
                m02 = _m02 / fScale,
                m03 = _m03 / fScale,
                m10 = _m10 / fScale,
                m11 = _m11 / fScale,
                m12 = _m12 / fScale,
                m13 = _m13 / fScale,
                m20 = _m20 / fScale,
                m21 = _m21 / fScale,
                m22 = _m22 / fScale,
                m23 = _m23 / fScale,
                m30 = _m30 / fScale,
                m31 = _m31 / fScale,
                m32 = _m32 / fScale,
                m33 = _m33 / fScale
            };
        }

        public float4x4 Tofloat4x4()
        {
            var fScale = (float) _groupingScale;
            return new float4x4(
                _m00 / fScale,
                _m01 / fScale,
                _m02 / fScale,
                _m03 / fScale,
                _m10 / fScale,
                _m11 / fScale,
                _m12 / fScale,
                _m13 / fScale,
                _m20 / fScale,
                _m21 / fScale,
                _m22 / fScale,
                _m23 / fScale,
                _m30 / fScale,
                _m31 / fScale,
                _m32 / fScale,
                _m33 / fScale
            );
        }

        public override bool Equals(object obj)
        {
            return obj is Matrix4x4Key other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _groupingScale;
                hashCode = (hashCode * 397) ^ _m00;
                hashCode = (hashCode * 397) ^ _m01;
                hashCode = (hashCode * 397) ^ _m02;
                hashCode = (hashCode * 397) ^ _m03;
                hashCode = (hashCode * 397) ^ _m10;
                hashCode = (hashCode * 397) ^ _m11;
                hashCode = (hashCode * 397) ^ _m12;
                hashCode = (hashCode * 397) ^ _m13;
                hashCode = (hashCode * 397) ^ _m20;
                hashCode = (hashCode * 397) ^ _m21;
                hashCode = (hashCode * 397) ^ _m22;
                hashCode = (hashCode * 397) ^ _m23;
                hashCode = (hashCode * 397) ^ _m30;
                hashCode = (hashCode * 397) ^ _m31;
                hashCode = (hashCode * 397) ^ _m32;
                hashCode = (hashCode * 397) ^ _m33;
                return hashCode;
            }
        }

        public static bool operator ==(Matrix4x4Key left, Matrix4x4Key right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix4x4Key left, Matrix4x4Key right)
        {
            return !left.Equals(right);
        }

        [BurstDiscard]
        public override string ToString()
        {
            const string float3format = "{0:F2}, {1:F2}, {2:F2}";
            const string float4format = float3format + ", {3:F2}";

            var t = string.Format(float3format, translation.x, translation.y, translation.z);
            var r = string.Format(
                float4format,
                rotation.value.x,
                rotation.value.y,
                rotation.value.z,
                rotation.value.w
            );
            var s = string.Format(float3format, scale.x, scale.y, scale.z);

            return $"T:[{t}] R:[{r}] S:[{s}]";
        }
    }
}
