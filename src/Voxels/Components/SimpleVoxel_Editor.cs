#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Attributes;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Spatial.Voxels.Gizmos;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.Voxels.Components
{
    [CallStaticConstructorInEditor]
    public sealed partial class SimpleVoxel
    {
        static SimpleVoxel()
        {
            RegisterDependency<MainVoxelDataGizmoSettingsCollection>(
                i => _mainVoxelDataGizmoSettingsCollection = i
            );
        }

        #region Static Fields and Autoproperties

        private static MainVoxelDataGizmoSettingsCollection _mainVoxelDataGizmoSettingsCollection;

        #endregion

        #region Fields and Autoproperties

        [NonSerialized]
        [ShowInInspector]
        [InlineEditor]
        private VoxelDataGizmoSettings _gizmoSettings;

        #endregion

        #region Event Functions

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

        #endregion

        private async AppaTask InitializeEditor(Initializer initializer)
        {
            using (_PRF_InitializeEditor.Auto())
            {
                _gizmoSettings = _mainVoxelDataGizmoSettingsCollection.Lookup.GetOrLoadOrCreateNew(
                    VoxelDataGizmoStyle.Simple,
                    nameof(VoxelDataGizmoStyle.Simple)
                );

                await AppaTask.CompletedTask;
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_InitializeEditor =
            new ProfilerMarker(_PRF_PFX + nameof(InitializeEditor));

        #endregion
    }
}

#endif
