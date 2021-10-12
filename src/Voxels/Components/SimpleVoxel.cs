#region

using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Behaviours;
using Appalachia.Core.Filtering;
using Appalachia.Spatial.Voxels.Gizmos;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Components
{
    [Serializable]
    public class SimpleVoxel : AppalachiaMonoBehaviour
    {
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

        [NonSerialized]
        [ShowInInspector]
        [InlineEditor]
        private VoxelDataGizmoSettings _gizmoSettings;

        private MeshRenderer[] _renderers;
        private VoxelTypes.Voxels _voxels;

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

        private bool _canProcessColliderStyle =>
            (style == VoxelPopulationStyle.Colliders) &&
            (_colliders != null) &&
            (_colliders.Length > 0);

        private bool _canProcessRendererStyle =>
            (style == VoxelPopulationStyle.Meshes) &&
            (_renderers != null) &&
            (_renderers.Length > 0);

        private bool _canProcess => _canProcessColliderStyle || _canProcessRendererStyle;

        private void OnEnable()
        {
            RefreshChildCollections();
        }

        private void OnDisable()
        {
            _voxels?.Dispose();
        }

        private void OnDestroy()
        {
            _voxels?.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            if (_voxels == null)
            {
                return;
            }

            if (_gizmoSettings == null)
            {
                _gizmoSettings = VoxelDataGizmoSettingsLookup.instance.GetOrLoadOrCreateNew(
                    VoxelDataGizmoStyle.Simple,
                    nameof(VoxelDataGizmoStyle.Simple)
                );
            }

            _voxels.DrawGizmos(_gizmoSettings);
        }

        [EnableIf(nameof(_canProcess))]
        [ButtonGroup]
        public void Process()
        {
            if (!_canProcess)
            {
                return;
            }

            _voxels = style == VoxelPopulationStyle.Colliders
                ? VoxelTypes.Voxels.Voxelize(_transform, _colliders, resolution)
                : VoxelTypes.Voxels.Voxelize(_transform, _renderers, resolution);
        }

        [ButtonGroup]
        private void RefreshChildCollections()
        {
            _colliders = _transform.FilterComponentsFromChildren<Collider>()
                                   .NoTriggers()
                                   .ActiveOnly()
                                   .RunFilter();
            _renderers = _transform.FilterComponentsFromChildren<MeshRenderer>().RunFilter();
        }
    }
}
