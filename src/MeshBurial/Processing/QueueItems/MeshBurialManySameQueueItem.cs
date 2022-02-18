#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Attributes;
using Appalachia.Core.Objects.Availability;
using Appalachia.Spatial.MeshBurial.State;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    [CallStaticConstructorInEditor]
    public abstract class MeshBurialManySameQueueItem : MeshBurialManyQueueItem
    {
        static MeshBurialManySameQueueItem()
        {
            RegisterInstanceCallbacks.For<MeshBurialManySameQueueItem>()
                                     .When.Object<MeshBurialAdjustmentCollection>()
                                     .IsAvailableThen(i => _meshBurialAdjustmentCollection = i);
        }

        protected MeshBurialManySameQueueItem(
            string name,
            GameObject model,
            int length,
            bool adoptTerrainNormal = true) : base(name, length, model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _model = model;
            _modelHashCode = _model.GetHashCode();
            _adoptTerrainNormal = adoptTerrainNormal;
        }

        #region Static Fields and Autoproperties

        private static MeshBurialAdjustmentCollection _meshBurialAdjustmentCollection;

        #endregion

        #region Fields and Autoproperties

        [SerializeField] private bool _adoptTerrainNormal;

        [SerializeField] private GameObject _model;
        [SerializeField] private int _modelHashCode;
        private MeshBurialAdjustmentState _adjustmentState;
        private MeshBurialSharedState _sharedState;

        #endregion

        /// <inheritdoc />
        protected override bool GetAdoptTerrainNormalInternal()
        {
            return _adoptTerrainNormal;
        }

        /// <inheritdoc />
        protected override MeshBurialAdjustmentState GetMeshBurialAdjustmentStateInternal()
        {
            if (_adjustmentState == null)
            {
                _adjustmentState = _meshBurialAdjustmentCollection.GetByPrefab(_model);
            }

            return _adjustmentState;
        }

        /// <inheritdoc />
        protected override MeshBurialSharedState GetMeshBurialSharedStateInternal()
        {
            if (_sharedState == null)
            {
                _sharedState = MeshBurialSharedStateManager.GetByPrefab(_model);
            }

            return _sharedState;
        }

        /// <inheritdoc />
        protected override int GetModelHashCodeInternal()
        {
            return _modelHashCode;
        }

        /// <inheritdoc />
        protected override void OnInitializeInternal()
        {
            using (_PRF_OnInitializeInternal.Auto())
            {
                base.OnInitializeInternal();

                _sharedState = MeshBurialSharedStateManager.GetByPrefab(_model);
                _adjustmentState = _meshBurialAdjustmentCollection.GetByPrefab(_model);
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(MeshBurialManySameQueueItem) + ".";

        private static readonly ProfilerMarker _PRF_OnInitializeInternal =
            new(_PRF_PFX + nameof(OnInitializeInternal));

        #endregion
    }
}

#endif
