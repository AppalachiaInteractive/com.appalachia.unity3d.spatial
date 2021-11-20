#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Debugging;
using Appalachia.Core.Scriptables;
using Appalachia.Editing.Core.Behaviours;
using Appalachia.Rendering.Prefabs.Rendering;
using Appalachia.Simulation.Physical.Integration;
using Appalachia.Spatial.ConvexDecomposition.Data;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Logging;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Generation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public partial class DecomposedCollider : EditorOnlyFrustumCulledBehaviour
    {
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

        public override EditorOnlyExclusionStyle exclusionStyle => EditorOnlyExclusionStyle.Component;

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
                    if ((colliderTransform.localPosition != (Vector3) data.localPosition) ||
                        (colliderTransform.localRotation != data.localRotation) ||
                        (colliderTransform.localScale != (Vector3) data.localScale))
                    {
                        colliderTransform.localPosition = data.localPosition;
                        colliderTransform.localRotation = data.localRotation;
                        colliderTransform.localScale = data.localScale;
                        SetDirty();
                    }
                }

                colliders = colliderTransform.GetComponents<MeshCollider>().ToList();
            }
        }

        [Button, DisableIf(nameof(_disableDataChanges))]
        public void Refresh()
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
                data.SetDirty();
            }

            OnPostDecompose();
        }

        protected override void Internal_Awake()
        {
            if (visibilityEnabled)
            {
                return;
            }

            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }

            Setup();
        }

        protected override void Internal_OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
                if (visibilityEnabled)
                {
                    return;
                }

                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
                {
                    return;
                }

                Setup();
            }
        }

        protected override void Internal_Start()
        {
            if (visibilityEnabled)
            {
                return;
            }

            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }

            Setup();
        }

        private void OnPostDecompose()
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

        private void Setup()
        {
            using (_PRF_Setup.Auto())
            {
                if (_isEnabled)
                {
                    return;
                }

                if (PrefabRenderingManager.instance.ActiveNow)
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

                    data = AppalachiaObject.LoadOrCreateNew<DecomposedColliderData>(directory, assetName);

                    if (DecomposedColliderData.dirtyLogging.v)
                    {
                        AppaLog.Warn("Setting dirty: created data.");
                    }

                    data.SetDirty();
                    SetDirty();
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
                    data.SetDirty();
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

        private static readonly ProfilerMarker
            _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_Setup = new ProfilerMarker(_PRF_PFX + nameof(Setup));

        private static readonly ProfilerMarker _PRF_FindColliderRoot =
            new ProfilerMarker(_PRF_PFX + nameof(FindColliderRoot));

        private static readonly ProfilerMarker _PRF_OnDrawGizmosSelected =
            new ProfilerMarker(_PRF_PFX + nameof(OnDrawGizmosSelected));

        #endregion
    }
}
#endif
