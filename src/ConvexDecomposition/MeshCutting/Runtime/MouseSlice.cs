#if UNITY_EDITOR
using System.Collections.Generic;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.ConvexDecomposition.MeshCutting.Core;
using Appalachia.Utility.Async;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.MeshCutting.Runtime
{
    public sealed class MouseSlice : AppalachiaBehaviour<MouseSlice>
    {
        #region Fields and Autoproperties

        public GameObject plane;
        public Transform ObjectContainer;

        // How far away from the slice do we separate resulting objects
        public float separation;

        // Do we draw a plane object associated with the slice
        private Plane slicePlane;
        public bool drawPlane;

        // Reference to the line renderer
        public ScreenLineRenderer lineRenderer;

        private MeshCutter meshCutter;
        private TempMesh biggerMesh, smallerMesh;

        #endregion

        protected override async AppaTask Initialize(Initializer initializer)
        {
            await base.Initialize(initializer);

            meshCutter = new MeshCutter(256);
        }

        protected override async AppaTask WhenDisabled()
        {
            await base.WhenDisabled();
            lineRenderer.OnLineDrawn -= OnLineDrawn;
        }

        protected override async AppaTask WhenEnabled()
        {
            await base.WhenEnabled();
            lineRenderer.OnLineDrawn += OnLineDrawn;
        }

        #region Utility Functions

        private void DrawPlane(Vector3 start, Vector3 end, Vector3 normalVec)
        {
            var rotate = Quaternion.FromToRotation(Vector3.up, normalVec);

            plane.transform.localRotation = rotate;
            plane.transform.position = (end + start) / 2;
            plane.SetActive(true);
        }

        #endregion

        private void OnLineDrawn(Vector3 start, Vector3 end, Vector3 depth)
        {
            var planeTangent = (end - start).normalized;

            // if we didn't drag, we set tangent to be on x
            if (planeTangent == Vector3.zero)
            {
                planeTangent = Vector3.right;
            }

            var normalVec = Vector3.Cross(depth, planeTangent);

            if (drawPlane)
            {
                DrawPlane(start, end, normalVec);
            }

            SliceObjects(start, normalVec);
        }

        /// <summary>
        ///     Replace the mesh with tempMesh.
        /// </summary>
        private void ReplaceMesh(Mesh mesh, TempMesh tempMesh, MeshCollider c = null)
        {
            mesh.Clear();
            mesh.SetVertices(tempMesh.vertices);
            mesh.SetTriangles(tempMesh.triangles, 0);
            mesh.SetNormals(tempMesh.normals);
            mesh.SetUVs(0, tempMesh.uvs);

            //mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            if ((c != null) && c.enabled)
            {
                c.sharedMesh = mesh;
                c.convex = true;
            }
        }

        private void SeparateMeshes(Transform posTransform, Transform negTransform, Vector3 localPlaneNormal)
        {
            // Bring back normal in world space
            var worldNormal = ((Vector3)(posTransform.worldToLocalMatrix.transpose * localPlaneNormal))
               .normalized;

            var separationVec = worldNormal * separation;

            // Transform direction in world coordinates
            posTransform.position += separationVec;
            negTransform.position -= separationVec;
        }

        private void SeparateMeshes(
            List<Transform> positives,
            List<Transform> negatives,
            Vector3 worldPlaneNormal)
        {
            int i;
            var separationVector = worldPlaneNormal * separation;

            for (i = 0; i < positives.Count; ++i)
            {
                positives[i].transform.position += separationVector;
            }

            for (i = 0; i < negatives.Count; ++i)
            {
                negatives[i].transform.position -= separationVector;
            }
        }

        private bool SliceObject(
            ref Plane sliceObjectPlane,
            GameObject obj,
            List<Transform> positiveObjects,
            List<Transform> negativeObjects)
        {
            var mesh = obj.GetComponent<MeshFilter>().mesh;

            if (!meshCutter.SliceMesh(mesh, ref sliceObjectPlane))
            {
                // Put object in the respective list
                if (sliceObjectPlane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
                {
                    positiveObjects.Add(obj.transform);
                }
                else
                {
                    negativeObjects.Add(obj.transform);
                }

                return false;
            }

            // TODO: Update center of mass

            // Silly condition that labels which mesh is bigger to keep the bigger mesh in the original gameobject
            var posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
            if (posBigger)
            {
                biggerMesh = meshCutter.PositiveMesh;
                smallerMesh = meshCutter.NegativeMesh;
            }
            else
            {
                biggerMesh = meshCutter.NegativeMesh;
                smallerMesh = meshCutter.PositiveMesh;
            }

            // Create new Sliced object with the other mesh
            var newObject = Instantiate(obj, ObjectContainer);
            newObject.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
            var newObjMesh = newObject.GetComponent<MeshFilter>().mesh;

            // Put the bigger mesh in the original object
            // TODO: Enable collider generation (either the exact mesh or compute smallest enclosing sphere)
            ReplaceMesh(mesh,       biggerMesh);
            ReplaceMesh(newObjMesh, smallerMesh);

            (posBigger ? positiveObjects : negativeObjects).Add(obj.transform);
            (posBigger ? negativeObjects : positiveObjects).Add(newObject.transform);

            return true;
        }

        private void SliceObjects(Vector3 point, Vector3 normal)
        {
            var toSlice = GameObject.FindGameObjectsWithTag("Sliceable");

            // Put results in positive and negative array so that we separate all meshes if there was a cut made
            List<Transform> positive = new List<Transform>(), negative = new List<Transform>();

            GameObject obj;
            var slicedAny = false;
            for (var i = 0; i < toSlice.Length; ++i)
            {
                obj = toSlice[i];

                // We multiply by the inverse transpose of the worldToLocal Matrix, a.k.a the transpose of the localToWorld Matrix
                // Since this is how normal are transformed
                var transformedNormal =
                    ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

                //Convert plane in object's local frame
                slicePlane.SetNormalAndPosition(
                    transformedNormal,
                    obj.transform.InverseTransformPoint(point)
                );

                slicedAny = SliceObject(ref slicePlane, obj, positive, negative) || slicedAny;
            }

            // Separate meshes if a slice was made
            if (slicedAny)
            {
                SeparateMeshes(positive, negative, normal);
            }
        }

        #region Profiling

        

        #endregion
    }
}

#endif
