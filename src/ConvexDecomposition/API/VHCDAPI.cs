#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Appalachia.Core.Attributes;
using Appalachia.Core.Objects.Root;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.API
{
    [NonSerializable]
    public class VHCDAPI : AppalachiaSimpleBase
    {
        private const string _PRF_PFX = nameof(VHCDAPI) + ".";

#if UNITY_EDITOR_WIN
        [DllImport("libvhacdapi.dll")]
        private static extern bool VHACDGetOpenCLPlatforms(IntPtr outPlatforms);

        [DllImport("libvhacdapi.dll")]
        private static extern bool VHACDGetOpenCLDevices(uint platformIndex, IntPtr outDevices);

        [DllImport("libvhacdapi.dll")]
        private static extern bool VHACDConvexDecomposition(
            [In] double[] inPoints,
            int inNumPoints,
            [In] int[] inTriangles,
            int inNumTriangles,
            [In] [Out] ref VHACDSession session);

        [DllImport("libvhacdapi.dll")]
        private static extern void VHACDShutdown([In] [Out] ref VHACDSession session);
#else
        [DllImport("libvhacdapi")]
        static extern bool VHACDGetOpenCLPlatforms(IntPtr outPlatforms);

        [DllImport("libvhacdapi")]
        static extern bool VHACDGetOpenCLDevices(uint platformIndex, IntPtr outDevices);

        [DllImport("libvhacdapi")]
        static extern bool VHACDConvexDecomposition(
            [In] double[] inPoints, int inNumPoints,
            [In] int[] inTriangles, int inNumTriangles,
            [In][Out] ref VHACDSession session);

        [DllImport("libvhacdapi")]
        static extern void VHACDShutdown([In][Out] ref VHACDSession session);
#endif

        public static List<string> GetPlatforms()
        {
            var names = new List<string>();

            var namesPtr = Marshal.AllocHGlobal(8 * 64);
            if (!VHACDGetOpenCLPlatforms(namesPtr))
            {
                Marshal.FreeHGlobal(namesPtr);
                return names;
            }

            var namesData = new byte[8 * 64];
            Marshal.Copy(namesPtr, namesData, 0, 8 * 64);
            Marshal.FreeHGlobal(namesPtr);

            for (var i = 0; i < 8; i++)
            {
                var name = Encoding.UTF8.GetString(namesData, i * 64, 64).TrimEnd('\0');
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        public static List<string> GetDevices(uint platformIndex)
        {
            var names = new List<string>();

            var namesPtr = Marshal.AllocHGlobal(8 * 64);
            if (!VHACDGetOpenCLDevices(platformIndex, namesPtr))
            {
                Marshal.FreeHGlobal(namesPtr);
                return names;
            }

            var namesData = new byte[8 * 64];
            Marshal.Copy(namesPtr, namesData, 0, 8 * 64);
            Marshal.FreeHGlobal(namesPtr);

            for (var i = 0; i < 8; i++)
            {
                var name = Encoding.UTF8.GetString(namesData, i * 64, 64).TrimEnd('\0');
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        public static List<Mesh> ConvexDecomposition(Vector3[] vertices, int[] indices, VHACDSession session)
        {
            using (_PRF_ConvexDecomposition.Auto())
            {
                var points = new double[vertices.Length * 3];

                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex = vertices[i];
                    var offset = i * 3;
                    points[offset + 0] = vertex.x;
                    points[offset + 1] = vertex.y;
                    points[offset + 2] = vertex.z;
                }

                return ConvexDecomposition(points, indices, session);
            }
        }

        private static readonly ProfilerMarker _PRF_ConvexDecomposition_Marshall =
            new ProfilerMarker(_PRF_PFX + nameof(ConvexDecomposition) + ".Marshall");

        private static readonly ProfilerMarker _PRF_ConvexDecomposition =
            new ProfilerMarker(_PRF_PFX + nameof(ConvexDecomposition));

        private static readonly ProfilerMarker _PRF_VHACDConvexDecomposition =
            new ProfilerMarker(_PRF_PFX + nameof(VHACDConvexDecomposition));

        private static readonly ProfilerMarker _PRF_VHACDShutdown =
            new ProfilerMarker(_PRF_PFX + nameof(VHACDShutdown));

        private static readonly ProfilerMarker _PRF_ConvexDecomposition_BuildMeshes =
            new ProfilerMarker(_PRF_PFX + nameof(ConvexDecomposition) + ".BuildMeshes");

        public static List<Mesh> ConvexDecomposition(double[] points, int[] indices, VHACDSession session)
        {
            using (_PRF_ConvexDecomposition.Auto())
            {
                using (_PRF_VHACDConvexDecomposition.Auto())
                {
                    var res = VHACDConvexDecomposition(
                        points,
                        points.Length / 3,
                        indices,
                        indices.Length / 3,
                        ref session
                    );

                    if (!res)
                    {
                        return new List<Mesh>();
                    }
                }

                var outPoints = new double[session.outNumPoints * 3];
                var outTriangles = new int[session.outNumTriangles * 3];
                var outPointOffsets = new int[session.outNumMeshes];
                var outTriangleOffsets = new int[session.outNumMeshes];

                using (_PRF_ConvexDecomposition_Marshall.Auto())
                {
                    Marshal.Copy(session.outPoints,       outPoints,       0, outPoints.Length);
                    Marshal.Copy(session.outTriangles,    outTriangles,    0, outTriangles.Length);
                    Marshal.Copy(session.outPointOffsets, outPointOffsets, 0, outPointOffsets.Length);
                    Marshal.Copy(
                        session.outTriangleOffsets,
                        outTriangleOffsets,
                        0,
                        outTriangleOffsets.Length
                    );
                }

                var meshes = new List<Mesh>(session.outNumMeshes);

                using (_PRF_ConvexDecomposition_BuildMeshes.Auto())
                {
                    for (var i = 0; i < session.outNumMeshes; i++)
                    {
                        var pointOffset = outPointOffsets[i];
                        var triangleOffset = outTriangleOffsets[i];
                        int vertexDataCount;
                        int indexCount;
                        if (i < (session.outNumMeshes - 1))
                        {
                            vertexDataCount = outPointOffsets[i + 1] - pointOffset;
                            indexCount = outTriangleOffsets[i + 1] - triangleOffset;
                        }
                        else
                        {
                            vertexDataCount = outPoints.Length - pointOffset;
                            indexCount = outTriangles.Length - triangleOffset;
                        }

                        var curVertices = new Vector3[vertexDataCount / 3];
                        for (var j = 0; j < curVertices.Length; j++)
                        {
                            curVertices[j].x = (float)outPoints[pointOffset + 0];
                            curVertices[j].y = (float)outPoints[pointOffset + 1];
                            curVertices[j].z = (float)outPoints[pointOffset + 2];
                            pointOffset += 3;
                        }

                        var curIndices = new int[indexCount];
                        Array.Copy(outTriangles, triangleOffset, curIndices, 0, indexCount);

                        var mesh = new Mesh();
                        mesh.Clear();
                        mesh.vertices = curVertices;
                        mesh.triangles = curIndices;
                        mesh.Optimize();
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }

                using (_PRF_VHACDShutdown.Auto())
                {
                    VHACDShutdown(ref session);
                }

                return meshes;
            }
        }

        private static uint IntToUInt(int value)
        {
            return (uint)value;
        }
    }
}
#endif
