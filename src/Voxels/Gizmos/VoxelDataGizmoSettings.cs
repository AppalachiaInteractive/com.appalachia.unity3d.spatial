#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Scriptables;
using Appalachia.Utility.Constants;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Gizmos
{
    [Serializable]
    public class VoxelDataGizmoSettings : AppalachiaObject<VoxelDataGizmoSettings>
    {
        public VoxelDataGizmoStyle style;

        [FoldoutGroup("Bounds")]
        [SmartLabel(Postfix = true)]
        public bool drawBounds;

        [FoldoutGroup("Bounds")]
        [EnableIf(nameof(drawBounds))]
        [SmartLabel]
        public Color boundsColor = Color.white;

        [FoldoutGroup("Bounds")]
        [SmartLabel(Postfix = true)]
        public bool drawBoundsSubdivisions;

        [FoldoutGroup("Bounds")]
        [EnableIf(nameof(drawBoundsSubdivisions))]
        [SmartLabel]
        public Color boundsSubdivisionColor = Color.white;

        //[SmartLabel(Postfix = true)] public bool drawBoundsGrid;
        //[EnableIf(nameof(drawBoundsGrid))] public Color boundsGridColor;

        [FoldoutGroup("Grid")]
        [SmartLabel(Postfix = true)]
        public bool drawGrid;

        [FoldoutGroup("Grid")]
        [SmartLabel(Postfix = true)]
        public bool colorGridWithTime;

        [FoldoutGroup("Grid")]
        [EnableIf(nameof(drawGrid))]
        [SmartLabel]
        public Color gridColor = Color.white;

        [FoldoutGroup("Samples")]
        [SmartLabel(Postfix = true)]
        public bool drawSamplePoints;

        [FoldoutGroup("Samples")]
        [EnableIf(nameof(drawSamplePoints))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float samplePointsGizmoScale;

        [FoldoutGroup("Samples")]
        [SmartLabel(Postfix = true)]
        public bool colorSamplePointsWithTime;

        [FoldoutGroup("Samples")]
        [EnableIf(nameof(drawSamplePoints))]
        [SmartLabel]
        public Color samplePointsColor = Color.white;

        [FoldoutGroup("Samples")]
        [EnableIf(nameof(drawSamplePoints))]
        [SmartLabel(Postfix = true)]
        public bool drawPopulatedSamplePoints;

        [FoldoutGroup("Voxels")]
        [SmartLabel(Postfix = true)]
        public bool drawVoxels;

        [FoldoutGroup("Voxels")]
        [EnableIf(nameof(drawVoxels))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float voxelsGizmoScale;

        [FoldoutGroup("Voxels")]
        [SmartLabel(Postfix = true)]
        public bool colorVoxelsWithTime;

        [FoldoutGroup("Voxels")]
        [EnableIf(nameof(drawVoxels))]
        [SmartLabel]
        public Color voxelsColor = Color.white;

        [FoldoutGroup("Voxels")]
        [EnableIf(nameof(drawVoxels))]
        [SmartLabel(Postfix = true)]
        public bool drawFaceVoxels;

        [FoldoutGroup("Faces")]
        [SmartLabel(Postfix = true)]
        public bool drawFaces;

        [FoldoutGroup("Faces")]
        [EnableIf(nameof(drawFaces))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float facesGizmoScale;

        [FoldoutGroup("Faces")]
        [SmartLabel(Postfix = true)]
        public bool colorFacesWithTime;

        [FoldoutGroup("Faces")]
        [EnableIf(nameof(drawFaces))]
        [SmartLabel]
        public Color facesColor = Color.white;

        [FoldoutGroup("Normals")]
        [SmartLabel(Postfix = true)]
        public bool drawNormals;

        [FoldoutGroup("Normals")]
        [EnableIf(nameof(drawNormals))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float normalsGizmoScale;

        [FoldoutGroup("Normals")]
        [SmartLabel(Postfix = true)]
        public bool colorNormalsWithNormal;

        [FoldoutGroup("Normals")]
        [EnableIf(nameof(drawNormals))]
        [SmartLabel]
        public Color normalsColor = Color.white;

        [FoldoutGroup("Normal Faces")]
        [SmartLabel(Postfix = true)]
        public bool drawNormalFaces;

        [FoldoutGroup("Normal Faces")]
        [EnableIf(nameof(drawNormalFaces))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float normalFacesGizmoScale;

        [FoldoutGroup("Normal Faces")]
        [EnableIf(nameof(drawNormalFaces))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float normalFacesGizmoOffset;

        [FoldoutGroup("Normal Faces")]
        [SmartLabel(Postfix = true)]
        public bool colorNormalFacesWithNormal;

        [FoldoutGroup("Normal Faces")]
        [EnableIf(nameof(drawNormalFaces))]
        [SmartLabel]
        public Color normalFacesColor = Color.white;

        [FoldoutGroup("Raycast")]
        [SmartLabel(Postfix = true)]
        public bool testRaycast;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public Space raySpace;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public float3 rayOrigin;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public float3 rayDirection = float3c.forward;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel(Postfix = true)]
        public bool autoAimAtCenter = true;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public float rayMaximumDistance = 128f;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public Color rayHitColor = Color.green;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public Color rayMissColor = Color.red;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        public Color rayHitVoxelColor = Color.yellow;

        [FoldoutGroup("Raycast")]
        [EnableIf(nameof(testRaycast))]
        [SmartLabel]
        [PropertyRange(0f, 1f)]
        public float rayHitGizmoSize;
    }
}

#endif