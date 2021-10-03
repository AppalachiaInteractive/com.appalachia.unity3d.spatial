#region

#endregion

using Appalachia.Core.Burstable;

namespace Appalachia.Spatial.MeshBurial.Processing
{
    public struct MeshBurialSummaryData
    {
        public double average;
        public int good;
        public int bad;
        public int discard;
        public int zeroIn;
        public int zeroOut;
        public int total;
        public BoundsBurst bounds;
        public bool requeue;
    }
}
