#if UNITY_EDITOR
using Appalachia.Core.Objects.Root;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.MeshCutting.Runtime
{
    public sealed class DrawBounds : AppalachiaBehaviour<DrawBounds>
    {
        #region Fields and Autoproperties

        private MeshFilter filter;

        #endregion

        #region Event Functions

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (enabled)
            {
                if (!filter)
                {
                    filter = GetComponent<MeshFilter>();
                }

                if (filter.sharedMesh == null)
                {
                    return;
                }

                var transform1 = transform;
                var modelMatrix = Matrix4x4.TRS(
                    transform1.position,
                    transform1.rotation,
                    transform1.lossyScale
                );
                Gizmos.matrix = modelMatrix;
                var sharedMesh = filter.sharedMesh;
                Gizmos.DrawWireCube(sharedMesh.bounds.center, sharedMesh.bounds.size);
            }
        }

        #endregion
    }
}

#endif
