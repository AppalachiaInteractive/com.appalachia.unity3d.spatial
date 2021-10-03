#region

using System;
using Appalachia.Core.MeshData;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialSharedState
    {
        private static int _terrainFrameCountLastRender;

        [SerializeField, HideInInspector]
        private GameObject _obj;

        [SerializeField, HideInInspector]
        private Mesh _meshAsset;

        [BoxGroup("Optimization"), InlineProperty, InlineEditor, HideLabel, ShowInInspector, HideReferenceObjectPicker]
        private MeshBurialOptimizationParameters _optimizationParams;

        [BoxGroup("Gizmos"), InlineProperty, InlineEditor, HideLabel, ShowInInspector, HideReferenceObjectPicker]
        private MeshBurialGizmoSettings _gizmos;

        public MeshBurialSharedState(GameObject instanceOrPrefab, MeshBurialOptimizationParameters op)
        {
            _optimizationParams = op;

            _obj = instanceOrPrefab;
            _meshAsset = MeshObjectManager.GetCheapestMesh(_obj);
        }

        /*public MeshObject meshObject
                 {
                     get => _meshObject;
                     private set => _meshObject = value;
                 }*/
        public MeshObjectWrapper meshObject => MeshObjectManager.GetByMesh(_meshAsset, true);

        public MeshBurialOptimizationParameters optimizationParams
        {
            get
            {
                if (_optimizationParams == null)
                {
                    _optimizationParams = MeshBurialOptimizationParameters.instance;
                }

                return _optimizationParams;
            }
        }

        public MeshBurialGizmoSettings gizmos
        {
            get
            {
                if (_gizmos == null)
                {
                    _gizmos = MeshBurialGizmoSettings.instance;
                }

                return _gizmos;
            }
        }

        /*public void OnDrawGizmos(Matrix4x4 matrix, TerrainThreadsafeData terrain)
        {
            if (_meshAsset == null || _meshObject == null)
            {
                _meshAsset = MeshObjectManager.GetCheapestNonBillboardMesh(_obj);
                _meshObject = MeshObjectManager.GetByMesh(_meshAsset);
            }
            else if (_meshObject.vertices == null || _meshObject.vertices.Length == 0)
            {
                _meshObject.Populate(_meshAsset, MeshObjectManager.groupingScale);
            }
            else if (_meshObject.RequiresRepopulation)
            {
                _meshObject.RepopulateNonSerializedFields();
            }

            var position = matrix.GetPositionFromMatrix();
            var rotation = matrix.GetRotationFromMatrix();
            var scale = matrix.GetScaleFromMatrix();

            if (gizmos.drawAllTriangles)
            {
                Gizmos.DrawWireMesh(_meshAsset, position, rotation, scale);
            }

            if (gizmos.drawSpecificTriangles)
            {
                meshObject.DrawTriangle(matrix, gizmos.triangle1, Color.cyan);
                if (gizmos.triangle2 != gizmos.triangle1) meshObject.DrawTriangle(matrix, gizmos.triangle2, Color.magenta);
            }

            if (gizmos.drawBorders)
            {
                meshObject.DrawBorders(matrix, terrain);
            }

            if (gizmos.drawTerrainIntersection)
            {
                meshObject.DrawTerrain(matrix, terrain);
            }

            if (gizmos.drawTerrainHeights)
            {
                var frameCount = Time.frameCount;
                
                if (frameCount > _terrainFrameCountLastRender)
                {
                    var center = matrix.MultiplyPoint(Vector3.zero);

                    var interpolationColor = Color.white - gizmos.drawTerrainColor;
                    interpolationColor.a = gizmos.drawTerrainColor.a;
                    
                    /*terrain.DrawGizmos(
                        center,
                        gizmos.drawTerrainHeightsRadius,
                        gizmos.drawTerrainHeightsSize,
                        gizmos.drawTerrainColor,
                        interpolationColor
                    );#1#
                    _terrainFrameCountLastRender = frameCount;
                }
            }
                
            if (gizmos.drawVertices)
            {
                meshObject.DrawVertexStatus(matrix, terrain);
            }
        }*/
    }
}
