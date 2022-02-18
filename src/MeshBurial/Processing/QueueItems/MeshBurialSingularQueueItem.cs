#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Attributes;
using Appalachia.Core.Objects.Availability;
using Appalachia.Spatial.MeshBurial.State;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    [CallStaticConstructorInEditor]
    public abstract class MeshBurialSingularQueueItem : MeshBurialQueueItem
    {
        static MeshBurialSingularQueueItem()
        {
            RegisterInstanceCallbacks.For<MeshBurialSingularQueueItem>()
                                     .When.Object<MeshBurialAdjustmentCollection>()
                                     .IsAvailableThen(i => _meshBurialAdjustmentCollection = i);
        }

        protected MeshBurialSingularQueueItem(
            string name,
            GameObject model,
            float4x4 matrix,
            UnityEngine.Object owner,
            bool adoptTerrainNormal = true) : base(name, 1, owner)
        {
            _model = model;
            _hashCode = _model.GetHashCode();
            _adjustmentState = _meshBurialAdjustmentCollection.GetByPrefab(_model);
            _sharedState = MeshBurialSharedStateManager.Get(_model);
            _adoptTerrainNormal = adoptTerrainNormal;
            _matrices = new float4x4[0];
            _matrices[0] = matrix;
        }

        #region Static Fields and Autoproperties

        private static MeshBurialAdjustmentCollection _meshBurialAdjustmentCollection;

        #endregion

        #region Fields and Autoproperties

        protected GameObject _model;

        private bool _adoptTerrainNormal;

        //private int[] _terrainHashCode;
        private float4x4[] _matrices;
        private int _hashCode;
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
            return _adjustmentState;
        }

        /// <inheritdoc />
        protected override MeshBurialSharedState GetMeshBurialSharedStateInternal()
        {
            return _sharedState;
        }

        /// <inheritdoc />
        protected override int GetModelHashCodeInternal()
        {
            return _hashCode;
        }

        /// <inheritdoc />
        protected override void OnInitializeInternal()
        {
        }
    }
}

#endif
