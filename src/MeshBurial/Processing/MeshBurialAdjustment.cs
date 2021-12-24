#if UNITY_EDITOR

#region

using Unity.Mathematics;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public struct MeshBurialAdjustment
    {
        public float4x4 matrix;

        public double error;
    }
}

#endif