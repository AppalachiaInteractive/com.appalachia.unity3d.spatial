#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.Jobs.MeshData;
using Appalachia.Spatial.ConvexDecomposition.API;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition
{
    public static class ConvexMeshColliderGenerator
    {
        public static List<Mesh> GenerateCollisionMesh(
            double[] points,
            int[] triangles,
            int meshes,
            int resolutionPerMesh = 50000)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                if (points.Length < 30)
                {
                    throw new NotSupportedException("Bad vertex count.");
                }

                if (triangles.Length < 10)
                {
                    throw new NotSupportedException("Bad triangle count.");
                }

                if (triangles.Any(i => (i >= (points.Length/3)) || (i < 0)))
                {
                    throw new NotSupportedException("Bad triangle index.");
                }

                var session = VHACDSession.Create();

                session.maxConvexHulls = meshes;
                session.resolution = meshes * resolutionPerMesh;

                //session.convexHullApproximation = 0;

                var decompositionMeshes = VHCDAPI.ConvexDecomposition(points, triangles, session).ToList();

                return decompositionMeshes;
            }
        }
        
        public static List<Mesh> GenerateCollisionMesh(
            Vector3[] vertices,
            int[] triangles,
            int meshes,
            int resolutionPerMesh = 50000)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                if (vertices.Length < 10)
                {
                    throw new NotSupportedException("Bad vertex count.");
                }

                if (triangles.Length < 10)
                {
                    throw new NotSupportedException("Bad triangle count.");
                }

                if (triangles.Any(i => (i >= vertices.Length) || (i < 0)))
                {
                    throw new NotSupportedException("Bad triangle index.");
                }

                var session = VHACDSession.Create();

                session.maxConvexHulls = meshes;
                session.resolution = meshes * resolutionPerMesh;

                //session.convexHullApproximation = 0;

                var decompositionMeshes = VHCDAPI.ConvexDecomposition(vertices, triangles, session).ToList();

                return decompositionMeshes;
            }
        }

        
        public static List<Mesh> GenerateCollisionMesh(Mesh mesh, int meshes, int resolutionPerMesh = 50000)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                var v = mesh.vertices;
                var t = mesh.triangles;

                return GenerateCollisionMesh(v, t, meshes, resolutionPerMesh);
            }
        }

        private static readonly ProfilerMarker _PRF_GenerateCollisionMesh = new ProfilerMarker(_PRF_PFX + nameof(GenerateCollisionMesh));
        public static List<Mesh> GenerateCollisionMesh(
            MeshObject meshObject,
            int meshes,
            bool closeHoles = true,
            int resolutionPerMesh = 50000)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                var nativeTris = (closeHoles ? meshObject.solidTriangleIndices : meshObject.triangleIndices);

                var tris =nativeTris.ToArray();
                var points = meshObject.vertexPoints.ToArray();
            
                return GenerateCollisionMesh(points, tris, meshes, resolutionPerMesh);
            }
        }

        private const string _PRF_PFX = nameof(ConvexMeshColliderGenerator) + ".";
        private static readonly ProfilerMarker _PRF_GenerateCollisionMeshInBounds = new ProfilerMarker(_PRF_PFX + nameof(GenerateCollisionMeshInBounds));
        
        public static List<Mesh> GenerateCollisionMeshInBounds(Mesh mesh, int meshes, Bounds bounds, int resolutionPerMesh = 50000)
        {
            using (_PRF_GenerateCollisionMeshInBounds.Auto())
            {
                var points = mesh.vertices;
                var pointsf3 = new float3[points.Length];
                Array.Copy(points, pointsf3, points.Length);
                
                var tris = mesh.triangles;
                var normals = mesh.normals;
                
                var bounded = MeshObject.Bounded(pointsf3, normals, tris, bounds, false);

                return GenerateCollisionMesh(bounded, meshes, true, resolutionPerMesh);
            }
        }
        
        public static List<Mesh> GenerateCollisionMeshInBounds(
            MeshObject meshObject,
            int meshes,
            Bounds bounds, 
            bool closeHoles = true,
            int resolutionPerMesh = 50000)
        {
            using (_PRF_GenerateCollisionMeshInBounds.Auto())
            {
                var nativeTris = (closeHoles ? meshObject.solidTriangleIndices : meshObject.triangleIndices);

                var tris = nativeTris.ToArray();
                var points = meshObject.vertexPositions.ToArray();
                
                var normals = new Vector3[points.Length];
                for (var i = 0; i < normals.Length; i++)
                {
                    normals[i] = Vector3.forward;
                }

                var bounded = MeshObject.Bounded(points, normals, tris, bounds, true);

                return GenerateCollisionMesh(bounded, meshes, closeHoles, resolutionPerMesh);
            }
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        public static List<Mesh> GenerateCollisionMesh(
            double[] points,
            int[] triangles,
            ConvexMeshSettings settings)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                if (points.Length < 30)
                {
                    throw new NotSupportedException("Bad vertex count.");
                }

                if (triangles.Length < 10)
                {
                    throw new NotSupportedException("Bad triangle count.");
                }

                if (triangles.Any(i => (i >= (points.Length/3)) || (i < 0)))
                {
                    throw new NotSupportedException("Bad triangle index.");
                }
                
                var session = settings.ToSession();

                var decompositionMeshes = VHCDAPI.ConvexDecomposition(points, triangles, session).ToList();

                return decompositionMeshes;
            }
        }
        
        public static List<Mesh> GenerateCollisionMesh(
            Vector3[] vertices,
            int[] triangles,
            ConvexMeshSettings settings)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                if (vertices.Length < 10)
                {
                    throw new NotSupportedException("Bad vertex count.");
                }

                if (triangles.Length < 10)
                {
                    throw new NotSupportedException("Bad triangle count.");
                }

                if (triangles.Any(i => (i >= vertices.Length) || (i < 0)))
                {
                    throw new NotSupportedException("Bad triangle index.");
                }

                var session = settings.ToSession();

                var decompositionMeshes = VHCDAPI.ConvexDecomposition(vertices, triangles, session).ToList();

                return decompositionMeshes;
            }
        }

        
        public static List<Mesh> GenerateCollisionMesh(Mesh mesh, ConvexMeshSettings settings)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                var v = mesh.vertices;
                var t = mesh.triangles;

                return GenerateCollisionMesh(v, t, settings);
            }
        }
        
        public static List<Mesh> GenerateCollisionMesh(
            MeshObject meshObject,
            ConvexMeshSettings settings,
            bool closeHoles = true)
        {
            using (_PRF_GenerateCollisionMesh.Auto())
            {
                var nativeTris = (closeHoles ? meshObject.solidTriangleIndices : meshObject.triangleIndices);

                var tris =nativeTris.ToArray();
                var points = meshObject.vertexPoints.ToArray();
            
                return GenerateCollisionMesh(points, tris, settings);
            }
        }
        
        public static List<Mesh> GenerateCollisionMeshInBounds(Mesh mesh, Bounds bounds, 
                                                               ConvexMeshSettings settings)
        {
            using (_PRF_GenerateCollisionMeshInBounds.Auto())
            {
                var points = mesh.vertices;
                var pointsf3 = new float3[points.Length];
                Array.Copy(points, pointsf3, points.Length);
                
                var tris = mesh.triangles;
                var normals = mesh.normals;
                
                var bounded = MeshObject.Bounded(pointsf3, normals, tris, bounds, false);

                return GenerateCollisionMesh(bounded, settings);
            }
        }
        
        public static List<Mesh> GenerateCollisionMeshInBounds(
            MeshObject meshObject,
            Bounds bounds, 
            ConvexMeshSettings settings,
            bool closeHoles = true)
        {
            using (_PRF_GenerateCollisionMeshInBounds.Auto())
            {
                var nativeTris = (closeHoles ? meshObject.solidTriangleIndices : meshObject.triangleIndices);

                var tris = nativeTris.ToArray();
                var points = meshObject.vertexPositions.ToArray();
                
                var normals = new Vector3[points.Length];
                for (var i = 0; i < normals.Length; i++)
                {
                    normals[i] = Vector3.forward;
                }

                var bounded = MeshObject.Bounded(points, normals, tris, bounds, true);

                return GenerateCollisionMesh(bounded, settings);
            }
        }
    }
}
#endif