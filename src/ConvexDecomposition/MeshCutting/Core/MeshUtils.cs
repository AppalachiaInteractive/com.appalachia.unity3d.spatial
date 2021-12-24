#if UNITY_EDITOR
using System.Collections.Generic;
using Appalachia.CI.Constants;
using Appalachia.Jobs.MeshData;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.MeshCutting.Core
{
    public static class MeshUtils
    {
        private static AppaContext _context;

        private static AppaContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = new AppaContext(typeof(MeshUtils));
                }

                return _context;
            }
        }
        
        /// <summary>
        /// Find center of polygon by averaging vertices
        /// </summary>
        public static Vector3 FindCenter(List<Vector3> pairs)
        {
            Vector3 center = Vector3.zero;
            int count = 0;

            for (int i = 0; i < pairs.Count; i += 2)
            {
                center += pairs[i];
                count++;
            }

            return center / count;
        }

        /// <summary>
        /// Reorder a list of pairs of vectors (one dimension list where i and i + 1 defines a line segment)
        /// So that it forms a closed polygon 
        /// </summary>
        public static void ReorderList(MeshVertex[] pairs)
        {
            //int nbFaces = 0;
            int faceStart = 0;
            int i = 0;

            while (i < pairs.Length)
            {
                // Find next adjacent edge
                for (int j = i + 2; j < pairs.Length; j += 2)
                {
                    if (pairs[j] == pairs[i + 1])
                    {
                        // Put j at i+2
                        SwitchPairs(pairs, i + 2, j);
                        break;
                    }
                }


                if ((i + 3) >= pairs.Length)
                {
                    // Why does this happen?
                    Context.Log.Info("Huh?");
                    break;
                }
                else if (pairs[i + 3] == pairs[faceStart])
                {
                    // A face is complete.
                    //nbFaces++;
                    i += 4;
                    faceStart = i;
                }
                else
                {
                    i += 2;
                }
            }
        }

        /// <summary>
        /// Reorder a list of pairs of vectors (one dimension list where i and i + 1 defines a line segment)
        /// So that it forms a closed polygon 
        /// </summary>
        public static void ReorderList(List<Vector3> pairs)
        {
            //int nbFaces = 0;
            int faceStart = 0;
            int i = 0;

            while (i < pairs.Count)
            {
                // Find next adjacent edge
                for (int j = i + 2; j < pairs.Count; j += 2)
                {
                    if (pairs[j] == pairs[i + 1])
                    {
                        // Put j at i+2
                        SwitchPairs(pairs, i + 2, j);
                        break;
                    }
                }


                if ((i + 3) >= pairs.Count)
                {
                    // Why does this happen?
                    Context.Log.Info("Huh?");
                    break;
                }
                else if (pairs[i + 3] == pairs[faceStart])
                {
                    // A face is complete.
                    //nbFaces++;
                    i += 4;
                    faceStart = i;
                }
                else
                {
                    i += 2;
                }
            }
        }
    
        private static void SwitchPairs<T>(IList<T> pairs, int pos1, int pos2)
        {
            if (pos1 == pos2) return;

            var temp1 = pairs[pos1];
            var temp2 = pairs[pos1 + 1];
            pairs[pos1] = pairs[pos2];
            pairs[pos1 + 1] = pairs[pos2 + 1];
            pairs[pos2] = temp1;
            pairs[pos2 + 1] = temp2;
        }
    
        /// <summary>
        /// Extract polygon from the pairs of vertices.
        /// Per example, two vectors that are colinear is redundant and only forms one side of the polygon
        /// </summary>
        public static List<MeshVertex> FindRealPolygon(MeshVertex[] pairs)
        {
            List<MeshVertex> vertices = new List<MeshVertex>();
            Vector3 edge1, edge2;

            // List should be ordered in the correct way
            for (int i = 0; i < pairs.Length; i += 2)
            {
                edge1 = (pairs[i + 1].position - pairs[i].position);
                if (i == (pairs.Length - 2))
                    edge2 = pairs[1].position - pairs[0].position;
                else
                    edge2 = pairs[i + 3].position - pairs[i + 2].position;

                // Normalize edges
                edge1.Normalize();
                edge2.Normalize();

                if (Vector3.Angle(edge1, edge2) > MeshCutter.threshold)
                    // This is a corner
                    vertices.Add(pairs[i + 1]);
            }

            return vertices;
        }

        /// <summary>
        /// Extract polygon from the pairs of vertices.
        /// Per example, two vectors that are colinear is redundant and only forms one side of the polygon
        /// </summary>
        public static List<Vector3> FindRealPolygon(List<Vector3> pairs)
        {
            List<Vector3> vertices = new List<Vector3>();
            Vector3 edge1, edge2;

            // List should be ordered in the correct way
            for (int i = 0; i < pairs.Count; i += 2)
            {
                edge1 = (pairs[i + 1] - pairs[i]);
                if (i == (pairs.Count - 2))
                    edge2 = pairs[1] - pairs[0];
                else
                    edge2 = pairs[i + 3] - pairs[i + 2];

                // Normalize edges
                edge1.Normalize();
                edge2.Normalize();

                if (Vector3.Angle(edge1, edge2) > MeshCutter.threshold)
                    // This is a corner
                    vertices.Add(pairs[i + 1]);
            }

            return vertices;
        }
    
    }
}

#endif