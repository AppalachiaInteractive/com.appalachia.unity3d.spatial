#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.Terrains;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialState : AppalachiaSimpleBase
    {
        // [CallStaticConstructorInEditor] should be added to the class (initsingletonattribute)
        static MeshBurialState()
        {
            TerrainMetadataManager.InstanceAvailable += i => _terrainMetadataManager = i;
        }

        public MeshBurialState(MeshBurialSharedState shared, Matrix4x4 ltw, int terrainHashCode)
        {
            _shared = shared;
            _originalLocalToWorld = ltw;
            _localToWorld = ltw;

            optimized = new MeshBurialOptimizationState();

            Terrain = _terrainMetadataManager.GetTerrain(terrainHashCode);
        }

        #region Static Fields and Autoproperties

        private static TerrainMetadataManager _terrainMetadataManager;

        #endregion

        #region Fields and Autoproperties

        [FormerlySerializedAs("_optimizationState")]
        [SerializeField]
        [BoxGroup("Optimization")]
        [InlineProperty]
        [HideLabel]
        private MeshBurialOptimizationState _optimized;

        [SerializeField]
        [HideInInspector]
        private Matrix4x4 _originalLocalToWorld;

        [SerializeField]
        [HideInInspector]
        private Matrix4x4 _localToWorld;

        [SerializeField]
        [HideInInspector]
        private TerrainMetadata _terrain;

        [BoxGroup("Shared State")]
        [InlineProperty]
        [HideLabel]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        private MeshBurialSharedState _shared;

        #endregion

        public Matrix4x4 originalLocalToWorld => _originalLocalToWorld;

        public MeshBurialSharedState shared => _shared;

        public Matrix4x4 localToWorld
        {
            get => _localToWorld;
            set => _localToWorld = value;
        }

        public MeshBurialOptimizationState optimized
        {
            get => _optimized;
            private set => _optimized = value;
        }

        public TerrainMetadata Terrain
        {
            get => _terrain;
            private set => _terrain = value;
        }
    }
}

#endif
