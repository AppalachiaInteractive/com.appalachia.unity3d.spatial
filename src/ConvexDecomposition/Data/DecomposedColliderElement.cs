#if UNITY_EDITOR
using System;
using Appalachia.CI.Integration.Assets;
using Appalachia.CI.Integration.FileSystem;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Simulation.Core.Metadata.Materials;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Appalachia.Spatial.ConvexDecomposition.Data
{
    [Serializable, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class DecomposedColliderElement : IEquatable<DecomposedColliderElement>
    {
        private const string _PRF_PFX = nameof(DecomposedColliderElement) + ".";
        
        public DecomposedColliderElement(Mesh m, PhysicMaterialWrapper mat, bool external)
        {
            mesh = m;
            material = mat;
            this.externalMesh = external;
        }

        [SerializeField, SmartLabel(AlignWith = "Collider")] 
        public Mesh mesh;
        
        [SerializeField, SmartLabel(AlignWith = "Collider")] 
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)] 
        public PhysicMaterialWrapper material;
            
        [SerializeField, SmartLabel] 
        public bool externalMesh;

        [SerializeField, HideInInspector] public int index;

        private static readonly ProfilerMarker _PRF_Apply = new ProfilerMarker(_PRF_PFX + nameof(Apply));

        public void Apply(MeshCollider c)
        {
            using (_PRF_Apply.Auto())
            {
                if (c == null) return;

                c.enabled = true;
                c.convex = true;
                c.isTrigger = false;
                c.sharedMesh = mesh;
                c.sharedMaterial = material == null ? null : material.material;
            }
        }
        
        public void SwapMaterial(PhysicMaterialWrapper old, PhysicMaterialWrapper newM)
        {
            if (material == old)
            {
                material = newM;
            }
        }

        private static readonly ProfilerMarker _PRF_AssignRecursive = new ProfilerMarker(_PRF_PFX + nameof(AssignRecursive));
        
        public void AssignRecursive(PhysicMaterialWrapper mat)
        {
            using (_PRF_AssignRecursive.Auto())
            {
                material = mat;
            }
        }

        public bool valid => mesh != null;

        public Vector3 center => mesh == null ? default : mesh.bounds.center;

        public string ProposedNameSuffix =>
            mesh == null
                ? null
                : $"dc_{index}";

        private static readonly ProfilerMarker _PRF_Save = new ProfilerMarker(_PRF_PFX + nameof(Save));
        public void Save(Mesh originalMesh, string directory)
        {
            using (_PRF_Save.Auto())
            {
                var targetName = $"{originalMesh.name}_{ProposedNameSuffix}";

                SaveMeshAsset(directory, targetName);
            }
        }

        public void ConfirmMeshName(Mesh originalMesh, string directory)
        {
            using (_PRF_Save.Auto())
            {
                var targetName = $"{originalMesh.name}_{ProposedNameSuffix}";

                SaveMeshAsset(directory, targetName);
            }
        }

        private void SaveMeshAsset(string directory, string targetName)
        {
            if (externalMesh)
            {
                return;
            }

            if (mesh.name != targetName)
            {
                mesh.name = targetName;                
            }
            
            var newTargetFileName = $"{targetName}.mesh.asset";
            var newSaveTargetPath = AppaPath.Combine(directory, newTargetFileName).Replace("\\", "/");

            var existingMeshPath = AssetDatabaseManager.GetAssetPath(mesh);
            
            var isThisMeshAlreadyAnAsset = !string.IsNullOrWhiteSpace(existingMeshPath);

            var isThisMeshAlreadySavedAtTargetPath = isThisMeshAlreadyAnAsset && (existingMeshPath == newSaveTargetPath);
            
            if (isThisMeshAlreadySavedAtTargetPath)
            {
                return;
            }
            
            var isTargetPopulatedWithAnotherAsset = AssetDatabaseManager.LoadAssetAtPath<Object>(newSaveTargetPath) != null;

            if (isTargetPopulatedWithAnotherAsset)
            {
                newSaveTargetPath = AssetDatabaseManager.GenerateUniqueAssetPath(newSaveTargetPath);
            }

            if (isThisMeshAlreadyAnAsset)
            {
                if (DecomposedColliderData.extraLogging.v) Debug.Log($"MOVING asset from [{existingMeshPath}] to [{newSaveTargetPath}].");
                AssetDatabaseManager.MoveAsset(existingMeshPath, newSaveTargetPath);
            }
            else
            {
                if (DecomposedColliderData.extraLogging.v) Debug.Log($"CREATING asset at [{newSaveTargetPath}].");
                AssetDatabaseManager.CreateAsset(mesh, newSaveTargetPath);
            }
        }

        private static readonly ProfilerMarker _PRF_Delete = new ProfilerMarker(_PRF_PFX + nameof(Delete));
        public void Delete()
        {
            using (_PRF_Delete.Auto())
            {
                if ((mesh != null) && !AssetDatabaseManager.IsSubAsset(mesh) && !externalMesh)
                {
                    var meshPath = AssetDatabaseManager.GetAssetPath(mesh);
                    if (DecomposedColliderData.extraLogging.v) Debug.Log($"DELETING asset at [{meshPath}].");
                    AssetDatabaseManager.DeleteAsset(meshPath);
                }

                material = null;
            }
        }
        
#region IEquatable

        public bool Equals(DecomposedColliderElement other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(mesh, other.mesh) && Equals(material, other.material) && (externalMesh == other.externalMesh) && (index == other.index);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DecomposedColliderElement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (mesh != null ? mesh.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (material != null ? material.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ externalMesh.GetHashCode();
                hashCode = (hashCode * 397) ^ index;
                return hashCode;
            }
        }

        public static bool operator ==(DecomposedColliderElement left, DecomposedColliderElement right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DecomposedColliderElement left, DecomposedColliderElement right)
        {
            return !Equals(left, right);
        }

#endregion
    }
}

#endif