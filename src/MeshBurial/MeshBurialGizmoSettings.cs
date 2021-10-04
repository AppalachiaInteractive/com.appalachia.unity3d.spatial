#region

using System;
using Appalachia.Base.Scriptables;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial
{
    [Serializable]
    public class
        MeshBurialGizmoSettings : SelfSavingSingletonScriptableObject<MeshBurialGizmoSettings>
    {
        public bool drawAllTriangles;
        public bool drawBorders = true;
        public bool drawTerrainIntersection = true;
        public bool drawTerrainHeights = true;
        public Color drawTerrainColor = Color.red;
        [PropertyRange(1f, 10f)] public float drawTerrainHeightsRadius = 5.0f;

        [PropertyRange(.001, .1f)]
        public float drawTerrainHeightsSize = .02f;

        public bool drawVertices;

        public int triangleLimit;

        public bool drawSpecificTriangles;

        [ShowIf(nameof(drawSpecificTriangles))]
        [PropertyRange(0, nameof(triangleLimit))]
        public int triangle1;

        [ShowIf(nameof(drawSpecificTriangles))]
        [PropertyRange(0, nameof(triangleLimit))]
        public int triangle2;
    }
}