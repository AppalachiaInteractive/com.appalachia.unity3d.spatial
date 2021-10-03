#region

using System;
using Appalachia.Core.Terrains;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialState
    {
        [FormerlySerializedAs("_optimizationState")]
        [SerializeField, BoxGroup("Optimization"), InlineProperty, HideLabel]
        private MeshBurialOptimizationState _optimized;

        [SerializeField, HideInInspector]
        private Matrix4x4 _originalLocalToWorld;

        [SerializeField, HideInInspector]
        private Matrix4x4 _localToWorld;

        [SerializeField, HideInInspector]
        private TerrainMetadata _terrain;

        [BoxGroup("Shared State"), InlineProperty, HideLabel, ShowInInspector, HideReferenceObjectPicker]
        private MeshBurialSharedState _shared;

        public MeshBurialState(MeshBurialSharedState shared, Matrix4x4 ltw, int terrainHashCode)
        {
            _shared = shared;
            _originalLocalToWorld = ltw;
            _localToWorld = ltw;

            optimized = new MeshBurialOptimizationState();

            if (!TerrainMetadataManager.Initialized)
            {
                TerrainMetadataManager.Initialize();
            }

            Terrain = TerrainMetadataManager.GetTerrain(terrainHashCode);
        }

        public MeshBurialOptimizationState optimized
        {
            get => _optimized;
            private set => _optimized = value;
        }

        public Matrix4x4 originalLocalToWorld => _originalLocalToWorld;

        public Matrix4x4 localToWorld
        {
            get => _localToWorld;
            set => _localToWorld = value;
        }

        public TerrainMetadata Terrain
        {
            get => _terrain;
            private set => _terrain = value;
        }

        public MeshBurialSharedState shared => _shared;

    }
}
