
#region

using System;
using Appalachia.Core.Objects.Root;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Appalachia.Spatial.MeshBurial
{
    [Serializable]
    public class MeshBurialOptimizationState : AppalachiaSimpleBase
    {
        [TitleGroup("Reference")]
        public double initialError;

        [TitleGroup("Reference")]
        public Matrix4x4 initialMatrix;

        [TitleGroup("Results")] public double bestError;

        [FormerlySerializedAs("bestShift")]
        [TitleGroup("Results")]
        public Matrix4x4 bestMatrix;

        [TitleGroup("Results")] public double executionTime;

        [TitleGroup("Results")] public int permissiveness = 1;
    }
}
