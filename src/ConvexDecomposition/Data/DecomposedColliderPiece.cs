#if UNITY_EDITOR
using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Scriptables;
using Appalachia.Simulation.Core.Metadata.Materials;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.Data
{
    [Serializable]
    public class DecomposedColliderPiece : AppalachiaObject
    {
        [SerializeField, SmartLabel(AlignWith = "Collider")] 
        public Mesh mesh;
        
        [SerializeField, SmartLabel(AlignWith = "Collider")] 
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)] 
        public PhysicMaterialWrapper material;
            
        [SerializeField, SmartLabel] 
        public bool externalMesh;

        [HideInInspector] public int index;

        public Vector3 center => mesh == null ? default : mesh.bounds.center;
        public Vector3 size => mesh == null ? default : mesh.bounds.size;
        public Vector3 min => mesh == null ? default : mesh.bounds.min;
        public Vector3 max => mesh == null ? default : mesh.bounds.max;
        public Bounds bounds => mesh == null ? default : mesh.bounds;

    }
}

#endif