#region

using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Filtering;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Components
{
    [Serializable]
    public sealed partial class SimpleVoxel : AppalachiaBehaviour<SimpleVoxel>
    {
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
                    ? VoxelTypes.Voxels.Voxelize(Transform, _colliders, resolution)
                    : VoxelTypes.Voxels.Voxelize(Transform, _renderers, resolution);
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            await base.Initialize(initializer);

            using (_PRF_Initialize.Auto())
            {
                RefreshChildCollections();
            }

#if UNITY_EDITOR
            await InitializeEditor(initializer);
#endif
        }

        protected override async AppaTask WhenDestroyed()
        {
            await base.WhenDestroyed();

            using (_PRF_WhenDestroyed.Auto())
            {
                _voxels?.Dispose();
            }
        }

        protected override async AppaTask WhenDisabled()
        {
            await base.WhenDisabled();

            using (_PRF_WhenDisabled.Auto())
            {
                _voxels?.Dispose();
            }
        }

        [ButtonGroup]
        private void RefreshChildCollections()
        {
            using (_PRF_RefreshChildCollections.Auto())
            {
                _colliders = Transform.FilterComponentsFromChildren<Collider>()
                                       .NoTriggers()
                                       .ActiveOnly()
                                       .RunFilter();
                _renderers = Transform.FilterComponentsFromChildren<MeshRenderer>().RunFilter();
            }
        }

        #region Profiling

        private static readonly ProfilerMarker
            _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_Process = new ProfilerMarker(_PRF_PFX + nameof(Process));

        private static readonly ProfilerMarker _PRF_RefreshChildCollections =
            new ProfilerMarker(_PRF_PFX + nameof(RefreshChildCollections));

        #endregion
    }
}
