#region

using Unity.Mathematics;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public struct MeshBurialOptions
    {
        public double threshold;
        public bool accountForMeshNormal;
        public bool matchTerrainNormal;
        public bool adjustHeight;
        public bool applyParameters;
        public bool minimalRotation;
        public bool applyTestValue;
        public float4x4 testValue;
        public int permissiveness;
    }
}
