#if UNITY_EDITOR
using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Objects.Root;
using Appalachia.Simulation.Core.Metadata.Materials;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.Data
{
    [Serializable]
    public class DecomposedColliderPiece : AppalachiaObject<DecomposedColliderPiece>
    {
        #region Fields and Autoproperties

        [SerializeField, SmartLabel(AlignWith = "Collider")]
        public Mesh mesh;

        [SerializeField, SmartLabel(AlignWith = "Collider")]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public PhysicMaterialWrapper material;

        [SerializeField, SmartLabel]
        public bool externalMesh;

        [HideInInspector] public int index;

        #endregion

        public Bounds bounds => mesh == null ? default : mesh.bounds;

        public Vector3 center => mesh == null ? default : mesh.bounds.center;
        public Vector3 max => mesh == null ? default : mesh.bounds.max;
        public Vector3 min => mesh == null ? default : mesh.bounds.min;
        public Vector3 size => mesh == null ? default : mesh.bounds.size;

        
    }
}

#endif
