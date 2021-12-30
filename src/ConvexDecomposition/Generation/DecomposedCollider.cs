#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.Core.Attributes;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Debugging;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Editing.Core.Behaviours;
using Appalachia.Rendering.Prefabs.Rendering;
using Appalachia.Simulation.Physical.Integration;
using Appalachia.Spatial.ConvexDecomposition.Data;
using Appalachia.Utility.Async;
using Appalachia.Utility.Extensions;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Generation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [CallStaticConstructorInEditor]
    public partial class DecomposedCollider : EditorOnlyFrustumCulledBehaviour<DecomposedCollider>
    {
        static DecomposedCollider()
        {
            PrefabRenderingManager.InstanceAvailable += i => _prefabRenderingManager = i;
        }

        #region Static Fields and Autoproperties

        private static PrefabRenderingManager _prefabRenderingManager;

        #endregion

        #region Fields and Autoproperties

        [PropertyOrder(-150)]
        [HideLabel, LabelWidth(0), InlineProperty, SerializeField, InlineEditor(Expanded = true)]
        public DecomposedColliderData data;

        [PropertyOrder(150)]
        [SerializeField, ReadOnly]
        public List<MeshCollider> colliders;

        [SmartLabel]
        [BoxGroup("Transform"), PropertyOrder(140)]
        [SerializeField]
        public Transform colliderTransform;

        [NonSerialized] private bool _isEnabled;

        #endregion

        private bool _disableDataChanges => (data == null) || data.locked;

        #region Event Functions

        private void OnDrawGizmosSelected()
        {
            using (_PRF_OnDrawGizmosSelected.Auto())
            {
                if (!GizmoCameraChecker.ShouldRenderGizmos())
                {
                    return;
                }

                if (data != null)
                {
                    data.OnDrawGizmosSelected(this);
                }
            }
        }

        #endregion

        public void FindColliderRoot()
        {
            using (_PRF_FindColliderRoot.Auto())
            {
                if (colliderTransform == null)
                {
                    colliderTransform = _transform.Find(DecomposedColliderData.childName);

                    if (colliderTransform == null)
                    {
                        var colliderRoot = new GameObject(DecomposedColliderData.childName);

                        colliderRoot.transform.SetParent(_transform, false);

                        colliderTransform = colliderRoot.transform;
                    }
                }

                if (data != null)
                {
                    if ((colliderTransform.localPosition != (Vector3)data.localPosition) ||
                        (colliderTransform.localRotation != data.localRotation) ||
                        (colliderTransform.localScale != (Vector3)data.localScale))
                    {
                        colliderTransform.localPosition = data.localPosition;
                        colliderTransform.localRotation = data.localRotation;
                        colliderTransform.localScale = data.localScale;
                        MarkAsModified();
                    }
                }

                colliders = colliderTransform.GetComponents<MeshCollider>().ToList();
            }
        }

        [Button, DisableIf(nameof(_disableDataChanges))]
        public void Refresh()
        {
            using (_PRF_Refresh.Auto())
            {
                if (_disableDataChanges)
                {
                    return;
                }

                data.PushParent(this);

                data.CheckOriginalMesh(gameObject);

                data.InitializeColliders(gameObject);

                data.CheckForMissingAssets();

                if (data.settings.dirty)
                {
                    data.MarkAsModified();
                }

                OnPostDecompose();
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);

                if (visibilityEnabled)
                {
                    return;
                }

                if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
                {
                    return;
                }

                Setup();
            }
        }

        private void OnPostDecompose()
        {
            using (_PRF_OnPostDecompose.Auto())
            {
                if ((colliders == null) || (colliderTransform == null))
                {
                    FindColliderRoot();
                }

                while (colliders.Count > data.elements.Count)
                {
                    var i = colliders.Count - 1;
                    var c = colliders[i];
                    colliders.RemoveAt(i);
                    c.DestroySafely();
                }

                while (colliders.Count < data.elements.Count)
                {
                    colliders.Add(colliderTransform.gameObject.AddComponent<MeshCollider>());
                }

                for (var i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i] == null)
                    {
                        colliders[i] = colliderTransform.gameObject.AddComponent<MeshCollider>();
                    }
                }

                for (var i = 0; i < colliders.Count; i++)
                {
                    data.elements[i].Apply(colliders[i]);
                }
            }
        }

        private void Setup()
        {
            using (_PRF_Setup.Auto())
            {
                if (_isEnabled)
                {
                    return;
                }

                if (_prefabRenderingManager.ActiveNow)
                {
                    return;
                }

                _isEnabled = true;

                FindColliderRoot();

                if (data == null)
                {
                    var directory = DecomposedColliderData.GetSaveDirectory(this, out var assetName);

                    if (string.IsNullOrWhiteSpace(directory))
                    {
                        return;
                    }

                    data = DecomposedColliderData.LoadOrCreateNew(directory, assetName);

                    if (DecomposedColliderData.dirtyLogging.v)
                    {
                        Context.Log.Warn("Setting dirty: created data.");
                    }

                    data.MarkAsModified();
                    MarkAsModified();
                }

                var rdm = GetComponent<RigidbodyDensityManager>();

                if ((rdm != null) && (rdm.density == null))
                {
                    var bestGuess = data.elements.MostFrequent(e => e.material);

                    if (bestGuess != null)
                    {
                        rdm.density = bestGuess.defaultDensity;
                    }
                }

                data.PushParent(this);

                if (data.settings.dirty)
                {
                    data.MarkAsModified();
                }

                if (_disableDataChanges)
                {
                    return;
                }

                data.OnPostDecompose -= OnPostDecompose;
                data.OnPostDecompose += OnPostDecompose;

                data.CheckOriginalMesh(gameObject);

                data.InitializeColliders(gameObject);

                data.CheckForMissingAssets();
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_OnPostDecompose =
            new ProfilerMarker(_PRF_PFX + nameof(OnPostDecompose));

        private static readonly ProfilerMarker _PRF_Refresh = new ProfilerMarker(_PRF_PFX + nameof(Refresh));

        private static readonly ProfilerMarker _PRF_Initialize =
            new ProfilerMarker(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_Setup = new ProfilerMarker(_PRF_PFX + nameof(Setup));

        private static readonly ProfilerMarker _PRF_FindColliderRoot =
            new ProfilerMarker(_PRF_PFX + nameof(FindColliderRoot));

        private static readonly ProfilerMarker _PRF_OnDrawGizmosSelected =
            new ProfilerMarker(_PRF_PFX + nameof(OnDrawGizmosSelected));

        #endregion
    }
}
#endif
