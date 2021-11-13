#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Core.Attributes.Editing;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public struct MeshBurialAdjustmentEntry : IEquatable<MeshBurialAdjustmentEntry>
    {
        [HorizontalGroup("A")]
        [SmartLabel]
        [ToggleLeft]
        public bool adoptTerrainNormal;

        [HorizontalGroup("A")]
        [SmartLabel]
        public double error;

        [HorizontalGroup("B")]
        [SmartLabel]
        public Matrix4x4 input;

        [HorizontalGroup("B")]
        [SmartLabel]
        public Matrix4x4 adjustment;

        [DebuggerStepThrough] public bool Equals(MeshBurialAdjustmentEntry other)
        {
            return (adoptTerrainNormal == other.adoptTerrainNormal) &&
                   input.Equals(other.input) &&
                   adjustment.Equals(other.adjustment) &&
                   error.Equals(other.error);
        }

        [DebuggerStepThrough] public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((MeshBurialAdjustmentEntry) obj);
        }

        [DebuggerStepThrough] public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = adoptTerrainNormal.GetHashCode();
                hashCode = (hashCode * 397) ^ input.GetHashCode();
                hashCode = (hashCode * 397) ^ adjustment.GetHashCode();
                hashCode = (hashCode * 397) ^ error.GetHashCode();
                return hashCode;
            }
        }

        [DebuggerStepThrough] public static bool operator ==(
            MeshBurialAdjustmentEntry left,
            MeshBurialAdjustmentEntry right)
        {
            return Equals(left, right);
        }

        [DebuggerStepThrough] public static bool operator !=(
            MeshBurialAdjustmentEntry left,
            MeshBurialAdjustmentEntry right)
        {
            return !Equals(left, right);
        }
    }
}

#endif