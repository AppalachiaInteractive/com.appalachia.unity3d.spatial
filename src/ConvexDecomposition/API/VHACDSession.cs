#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;

namespace Appalachia.Spatial.ConvexDecomposition.API
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VHACDSession
    {
        // Maximum number of convex hulls to produce (default = 64, range = 1 - 1024)
        public int maxConvexHulls;
        // Maximum number of voxels generated during the voxelization stage
        // (default = 100,000, range = 10,000 - 64,000,000)
        public int resolution;
        // Maximum allowed concavity (default = 0.0025, range = 0.0 - 1.0)
        public double concavity;
        // Controls the granularity of the search for the "best" clipping plane
        // (default = 4, range = 1 - 16)
        public int planeDownsampling;
        // Controls the precision of the convex - hull generation process during
        // the clipping plane selection stage(default = 4, range = 1 - 16)
        public int convexhullDownsampling;
        // Controls the bias toward clipping along symmetry planes
        // (default = 0.05, range = 0.0 - 1.0)
        public double alpha;
        // Controls the bias toward clipping along revolution axes
        // (default = 0.05, range = 0.0 - 1.0)
        public double beta;
        // Enable / disable normalizing the mesh before applying the convex
        // decomposition (default = false)
        public int pca;
        // 0: voxel - based approximate convex decomposition, 1 : tetrahedron - based
        // approximate convex decomposition (default = 0, range = { 0,1 })
        public int mode;
        // Controls the maximum number of triangles per convex hull
        // (default = 64, range = 4 - 1024)
        public int maxNumVerticesPerCH;
        // Controls the adaptive sampling of the generated convex hulls
        // (default = 0.0001, range = 0.0 - 0.01)
        public double minVolumePerCH;
        // Enable / disable approximation when computing convex hulls
        // (default = true)
        public int convexHullApproximation;
        // Project the output convex hull vertices onto the original source mesh to
        // increase the floating point accuracy of the results (default = true)
        public int projectHullVertices;
        // Enable / disable OpenCL acceleration (default = false)
        public int oclAcceleration;
        // OpenCL platform id (default = 0, range = 0 - # OCL platforms)
        public int oclPlatformID;
        // OpenCL device id (default = 0, range = 0 - # OCL devices)
        public int oclDeviceID;

        public IntPtr outPoints;
        public IntPtr outTriangles;
        public IntPtr outPointOffsets;
        public IntPtr outTriangleOffsets;
        public int outNumPoints;
        public int outNumTriangles;
        public int outNumMeshes;

        public static VHACDSession Create()
        {
            VHACDSession session = new VHACDSession();
            session.maxConvexHulls = 64;
            session.resolution = 100000;
            session.concavity = 0.0025;
            session.planeDownsampling = 4;
            session.convexhullDownsampling = 4;
            session.alpha = 0.05;
            session.beta = 0.05;
            session.pca = 0;
            session.mode = 0;
            session.maxNumVerticesPerCH = 64;
            session.minVolumePerCH = 0.0001;
            session.convexHullApproximation = 1;
            session.projectHullVertices = 1;
            session.oclAcceleration = 0;
            session.oclPlatformID = 0;
            session.oclDeviceID = 0;
            return session;
        }
    }
}

#endif