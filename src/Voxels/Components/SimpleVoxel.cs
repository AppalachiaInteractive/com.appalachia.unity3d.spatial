#region

using System;
using Appalachia.Core.Attributes;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Filtering;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.Voxels.Gizmos;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Components
{
    [Serializable]
    [CallStaticConstructorInEditor]
    public sealed class SimpleVoxel : AppalachiaBehaviour<SimpleVoxel>
    {
        static SimpleVoxel()
        {
#if UNITY_EDITOR
            RegisterDependency<MainVoxelDataGizmoSettingsCollection>(
                i => _mainVoxelDataGizmoSettingsCollection = i
            );
#endif
        }

        #region Static Fields and Autoproperties

#if UNITY_EDITOR
        private static MainVoxelDataGizmoSettingsCollection _mainVoxelDataGizmoSettingsCollection;
#endif

        #endregion

        #region Fields and Autoproperties

        [OnValueChanged(nameof(RefreshChildCollections))]
        public VoxelPopulationStyle style;

        [SerializeField] private float3 _resolution;

        [OnValueChanged(nameof(RefreshChildCollections))]
        [SerializeField]
        [PropertyRange(0.01f, 10f)]
        [SmartLabel(Text = "X")]
        [HorizontalGroup("Res")]
        public float resolutionX = .25f;

        [OnValueChanged(nameof(RefreshChildCollections))]
        [SerializeField]
        [PropertyRange(0.01f, 10f)]
        [SmartLabel(Text = "Y")]
        [HorizontalGroup("Res")]
        public float resolutionY = .25f;

        [OnValueChanged(nameof(RefreshChildCollections))]
        [SerializeField]
        [PropertyRange(0.01f, 10f)]
        [SmartLabel(Text = "Z")]
        [HorizontalGroup("Res")]
        public float resolutionZ = .25f;

        private Collider[] _colliders;

        private MeshRenderer[] _renderers;
        private VoxelTypes.Voxels _voxels;

#if UNITY_EDITOR
        [NonSerialized]
        [ShowInInspector]
        [InlineEditor]
        private VoxelDataGizmoSettings _gizmoSettings;

#endif

        #endregion

        [ShowInInspector]
        public float3 resolution
        {
            get
            {
                _resolution.x = resolutionX;
                _resolution.y = resolutionY;
                _resolution.z = resolutionZ;

                return _resolution;
            }
        }

        private bool _canProcess => _canProcessColliderStyle || _canProcessRendererStyle;

        private bool _canProcessColliderStyle =>
            (style == VoxelPopulationStyle.Colliders) && (_colliders != null) && (_colliders.Length > 0);

        private bool _canProcessRendererStyle =>
            (style == VoxelPopulationStyle.Meshes) && (_renderers != null) && (_renderers.Length > 0);

        #region Event Functions

#if UNITY_EDITOR

        private static readonly ProfilerMarker _PRF_OnDrawGizmosSelected =
            new ProfilerMarker(_PRF_PFX + nameof(OnDrawGizmosSelected));
        
        private void OnDrawGizmosSelected()
        {
            using (_PRF_OnDrawGizmosSelected.Auto())
            {
                if (_voxels == null)
                {
                    return;
                }

                if (_gizmoSettings == null)
                {
                    return;
                }

                _voxels.DrawGizmos(_gizmoSettings);
            }
        }
#endif

        #endregion

        [EnableIf(nameof(_canProcess))]
        [ButtonGroup]
        public void Process()
        {
            using (_PRF_Process.Auto())
            {
                if (!_canProcess)
                {
                    return;
                }

                _voxels = style == VoxelPopulationStyle.Colliders
                    ? VoxelTypes.Voxels.Voxelize(_transform, _colliders, resolution)
                    : VoxelTypes.Voxels.Voxelize(_transform, _renderers, resolution);
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);

                RefreshChildCollections();

#if UNITY_EDITOR
                _gizmoSettings = _mainVoxelDataGizmoSettingsCollection.Lookup.GetOrLoadOrCreateNew(
                    VoxelDataGizmoStyle.Simple,
                    nameof(VoxelDataGizmoStyle.Simple)
                );
#endif
            }
        }

        protected override async AppaTask WhenDestroyed()
        {
            using (_PRF_OnDestroy.Auto())
            {
                await base.WhenDestroyed();

                _voxels?.Dispose();
            }
        }

        protected override async AppaTask WhenDisabled()

        {
            using (_PRF_OnDisable.Auto())
            {
                await base.WhenDisabled();

                _voxels?.Dispose();
            }
        }

        [ButtonGroup]
        private void RefreshChildCollections()
        {
            using (_PRF_RefreshChildCollections.Auto())
            {
                _colliders = _transform.FilterComponentsFromChildren<Collider>()
                                       .NoTriggers()
                                       .ActiveOnly()
                                       .RunFilter();
                _renderers = _transform.FilterComponentsFromChildren<MeshRenderer>().RunFilter();
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(SimpleVoxel) + ".";

        private static readonly ProfilerMarker _PRF_Initialize =
            new ProfilerMarker(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_Process = new ProfilerMarker(_PRF_PFX + nameof(Process));

        private static readonly ProfilerMarker _PRF_RefreshChildCollections =
            new ProfilerMarker(_PRF_PFX + nameof(RefreshChildCollections));

        private static readonly ProfilerMarker
            _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_OnDisable =
            new ProfilerMarker(_PRF_PFX + nameof(OnDisable));

        private static readonly ProfilerMarker _PRF_OnDestroy =
            new ProfilerMarker(_PRF_PFX + nameof(OnDestroy));

        #endregion
    }
}
