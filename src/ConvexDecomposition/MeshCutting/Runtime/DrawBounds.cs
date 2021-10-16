#if UNITY_EDITOR
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.MeshCutting.Runtime
{
    public class DrawBounds : MonoBehaviour {

        private MeshFilter filter;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (enabled)
            {
                if (!filter) filter = GetComponent<MeshFilter>();
                if (filter.sharedMesh == null) return;

                var transform1 = transform;
                var modelMatrix = Matrix4x4.TRS(transform1.position, transform1.rotation, transform1.lossyScale);
                Gizmos.matrix = modelMatrix;
                var sharedMesh = filter.sharedMesh;
                Gizmos.DrawWireCube(sharedMesh.bounds.center, sharedMesh.bounds.size);
            }
        }
    }
}

#endif