#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using Appalachia.CI.Integration.Assets;
using Appalachia.Core.Attributes;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Debugging;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Preferences;
using Appalachia.Editing.Core.Behaviours;
using Appalachia.Editing.Debugging.Handle;
using Appalachia.Jobs.MeshData;
using Appalachia.Jobs.Optimization.Options;
using Appalachia.Jobs.Optimization.Utilities;
using Appalachia.Spatial.MeshBurial.Processing;
using Appalachia.Spatial.MeshBurial.State;
using Appalachia.Spatial.Terrains;
using Appalachia.Spatial.Terrains.Utilities;
using Appalachia.Utility.Async;
using Appalachia.Utility.Colors;
using Appalachia.Utility.Constants;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Strings;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial
{
    [CallStaticConstructorInEditor]
    [ExecuteAlways]
    public class MeshBury : EditorOnlyAppalachiaBehaviour<MeshBury>
    {
        #region Constants and Static Readonly

        private const string G_ = "Mesh Burial Gizmos";
        private const string GR = "Gizmos";
        private const string GR_ = GR + "/";

        #endregion

        static MeshBury()
        {
            RegisterDependency<MeshBurialAdjustmentCollection>(i => _meshBurialAdjustmentCollection = i);
            RegisterDependency<TerrainMetadataManager>(i => _terrainMetadataManager = i);
            RegisterDependency<MeshObjectManager>(i => _meshObjectManager = i);
        }

        #region Preferences

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> adjustmentColor = PREFS.REG(G_, "Rotation Color", Colors.Cornsilk1);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<HandleCapType> adjustmentHandle = PREFS.REG(G_, "Adjustment Handle", HandleCapType.Cube);

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> baseRotationColor = PREFS.REG(G_, "Rotation Color", Colors.Blue2);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> borderColor = PREFS.REG(G_, "Border Color", Colors.Orange);

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> calculatedAdjustmentColor = PREFS.REG(
            G_,
            "Rotation Color Calculated",
            Colors.Cornsilk3
        );

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<bool> drawBorderNormals = PREFS.REG(G_, "Draw Border Vertices", false);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<bool> drawBorders = PREFS.REG(G_, "Draw Borders", false);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<bool> drawOriginalVertices = PREFS.REG(G_, "Draw Original Vertices", false);

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<bool> drawRotation = PREFS.REG(G_, "Draw Rotation", false);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<bool> drawTriangles = PREFS.REG(G_, "Draw Triangles", false);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<bool> drawVertices = PREFS.REG(G_, "Draw Vertices", false);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> faceNormalColor = PREFS.REG(G_, "Face Normal Color", Colors.Orange);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> gizmoDistance = PREFS.REG(G_, "Gizmo Distance", 2f, 1f, 5f);

        [FoldoutGroup(GR)]
        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> gizmoRadius = PREFS.REG(G_, "Gizmo Radius", .25f, .1f, 1f);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> meshBorderColor = PREFS.REG(G_, "Mesh Border Color", Colors.Magenta);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<HandleCapType> meshBorderHandle = PREFS.REG(
            G_,
            "Mesh Border Handle",
            HandleCapType.Sphere
        );

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<HandleCapType> meshFaceHandle = PREFS.REG(G_, "Face Normal Handle", HandleCapType.Dot);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> originalVertexColor = PREFS.REG(G_, "Original Vertex Color", Colors.Orange);

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> rotationBaseColor = PREFS.REG(G_, "Rotation Base Color", Colors.Cornsilk4);

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> rotationGizmoDistance = PREFS.REG(G_, "Rotation Distance", 2f, 1f, 5f);

        [FoldoutGroup(GR_ + "Adjustment"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> rotationSpeed = PREFS.REG(G_, "Draw Rotation", .5f, 0f, 5f);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> terrainColor = PREFS.REG(G_, "Terrain Color", Color.cyan);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<HandleCapType> terrainHandle = PREFS.REG(G_, "Terrain Handle", HandleCapType.Sphere);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> triangleColor = PREFS.REG(G_, "Triangle Color", Colors.Orange);

        [FoldoutGroup(GR_ + "Debug"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<Color> vertexColor = PREFS.REG(G_, "Vertex Color", Colors.Orange);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> worldDistanceScale = PREFS.REG(G_, "World Distance Scale", .5f, .1f, 1f);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<HandleCapType> worldHandle = PREFS.REG(G_, "World Handle", HandleCapType.Arrow);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> worldHandleScale = PREFS.REG(G_, "World Handle Scale", .5f, .1f, 1f);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> worldSaturation = PREFS.REG(G_, "World Saturation", 0.5f, 0.1f, 1.0f);

        [FoldoutGroup(GR_ + "Axis"), ShowInInspector, InlineProperty]
        [NonSerialized]
        private PREF<float> worldValue = PREFS.REG(G_, "World Value", 0.5f, 0.1f, 1.0f);

        #endregion

        #region Static Fields and Autoproperties

        private static MeshBurialAdjustmentCollection _meshBurialAdjustmentCollection;
        private static MeshObjectManager _meshObjectManager;

        private static TerrainMetadataManager _terrainMetadataManager;

        #endregion

        #region Fields and Autoproperties

        [BoxGroup("Execution"), PropertyRange(1, 5000)]
        public int iterations = 128;

        [BoxGroup("Execution"), PropertyRange(0.01f, 1.0f)]
        public double errorThreshold = .3;

        [BoxGroup("Execution"), ToggleLeft]
        public bool accountForMeshNormal = true;

        [BoxGroup("Execution"), ToggleLeft]
        public bool matchTerrainNormal = true;

        [BoxGroup("Execution"), ToggleLeft]
        public bool adjustHeight = true;

        [BoxGroup("Execution"), ToggleLeft]
        public bool applyParameters = true;

        [BoxGroup("Execution"), PropertyRange(1, nameof(maxPermissiveness))]
        public int permissiveness = 1;

        [BoxGroup("Execution"), PropertyRange(1, 18)]
        public int maxPermissiveness = 10;

        [BoxGroup("Results"), Sirenix.OdinInspector.ReadOnly, PropertyRange(0f, 1.0f)]
        public double error = 1.0;

        [NonSerialized] private GameObject _gameObject;
        [NonSerialized] private MeshObjectWrapper _meshObject;
        [NonSerialized] private MeshBurialInstanceData _instanceData;
        [NonSerialized] private JobHandle _pendingHandle;
        [NonSerialized] private NativeList<JobHandle> _dependencyList;
        [NonSerialized] private JobRandoms _randoms;
        [NonSerialized] private OptimizationOptions _optimizationOptions;
        [NonSerialized] private MeshBurialOptions _burialOptions;
        [NonSerialized] private MeshBurialSharedState _state;
        [NonSerialized] private MeshBurialAdjustmentState _adjustment;

        [NonSerialized]
        private NativeArray<float4x4> _matrices = new NativeArray<float4x4>(1, Allocator.Persistent);

        [NonSerialized] private int[] _terrainHashCodes = new int[1];
        [NonSerialized] private MeshBurialInstanceTracking _recent;
        [NonSerialized] private bool _hasCachedUpdateValues;
        [NonSerialized] private float _degree;

        [NonSerialized] private List<Vector3> _borderList;
        [NonSerialized] private Vector3[] _borders;
        [NonSerialized] private Vector3[] _borderNormals;
        [NonSerialized] private Vector3[] _triVerts;
        [NonSerialized] private Vector3[] _triVertsOriginal;
        [NonSerialized] private Vector3[] _verts;

        #endregion

        //private Queue<float4x4> _last;

        private bool _canReinitialize => _gameObject != null;

        #region Event Functions

        protected void Update()
        {
            using (_PRF_Update.Auto())
            {
                if (ShouldSkipUpdate)
                {
                    return;
                }

                try
                {
                    if (_hasCachedUpdateValues)
                    {
                        return;
                    }

                    if (!_pendingHandle.IsCompleted)
                    {
                        return;
                    }

                    _hasCachedUpdateValues = true;

                    var degreeAdjustment = _burialOptions.permissiveness *
                                           (_burialOptions.minimalRotation
                                               ? _state.optimizationParams.xzDegreeAdjustmentMinimal
                                               : _state.optimizationParams.xzDegreeAdjustment);

                    _pendingHandle = MeshBurialJobManager.ScheduleMeshBurialJobs(
                        _instanceData,
                        _meshObject.data,
                        _adjustment,
                        _matrices,

                        //_terrainHashCodes,
                        _state.optimizationParams,
                        _optimizationOptions,
                        _burialOptions,
                        degreeAdjustment,
                        _randoms,
                        _dependencyList
                    );
                }
                catch (Exception ex)
                {
                    Context.Log.Error(nameof(Update).GenericMethodException(this), this, ex);
                    enabled = false;
                }
            }
        }

        private void LateUpdate()
        {
            using (_PRF_LateUpdate.Auto())
            {
                try
                {
                    if (!_hasCachedUpdateValues || !enabled)
                    {
                        return;
                    }

                    if (!_pendingHandle.IsCompleted)
                    {
                        return;
                    }

                    _pendingHandle.Complete();

                    var result = _instanceData.bestResults[0];
                    _recent = _instanceData.instances[0, result.iterationIndex];

                    if (_recent.Equals(default))
                    {
                        return;
                    }

                    _hasCachedUpdateValues = false;

                    error = _recent.proposed.error;
                    var matrix = _recent.proposed.matrix;

                    if (math.isnan(matrix.c0.x))
                    {
                        return;
                    }

                    transform.SetMatrix4x4ToTransform(matrix);

                    if (_recent.proposed.error < errorThreshold)
                    {
                    }
                    else
                    {
                        permissiveness += 1;
                    }

                    permissiveness = math.clamp(permissiveness, 1, maxPermissiveness);
                }
                catch (Exception ex)
                {
                    Context.Log.Error(nameof(LateUpdate).GenericMethodException(this), this, ex);
                    _hasCachedUpdateValues = false;
                    enabled = false;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            using (_PRF_OnDrawGizmosSelected.Auto())
            {
                if (!enabled ||
                    (_terrainHashCodes == null) ||
                    (_terrainHashCodes.Length == 0) ||
                    (_terrainHashCodes[0] == 0) ||
                    !_meshObject.data.isCreated)
                {
                    return;
                }

                if (!GizmoCameraChecker.ShouldRenderGizmos())
                {
                    return;
                }

                var t = transform;
                var m = (float4x4)t.localToWorldMatrix;
                var pos = (float3)t.position;
                var dist = gizmoDistance.v;
                var size = gizmoRadius.v;

                var verts = _meshObject.data.vertices;
                var tris = _meshObject.data.triangles;
                var edges = _meshObject.data.edges;

                if ((_triVerts == null) || (_triVerts.Length != (tris.Length * 6)))
                {
                    _triVerts = new Vector3[tris.Length * 6];
                }

                if (drawTriangles.v)
                {
                    for (var i = 0; i < tris.Length; i++)
                    {
                        var baseIndex = i * 6;
                        var tri = tris[i];

                        var x = m.MultiplyPoint3x4(verts[tri.xIndex].position);
                        var y = m.MultiplyPoint3x4(verts[tri.yIndex].position);
                        var z = m.MultiplyPoint3x4(verts[tri.zIndex].position);

                        _triVerts[baseIndex + 0] = x;
                        _triVerts[baseIndex + 1] = y;
                        _triVerts[baseIndex + 2] = y;
                        _triVerts[baseIndex + 3] = z;
                        _triVerts[baseIndex + 4] = z;
                        _triVerts[baseIndex + 5] = x;
                    }

                    Handles.color = triangleColor.v;
                    Handles.DrawLines(_triVerts);
                }

                if (drawVertices.v)
                {
                    if ((_verts == null) || (_verts.Length != (verts.Length * 2)))
                    {
                        _verts = new Vector3[verts.Length * 2];
                    }

                    for (var i = 0; i < verts.Length; i++)
                    {
                        var destination = i * 2;

                        var source = i;
                        var source2 = i + 1;

                        if (source2 >= verts.Length)
                        {
                            source2 = 0;
                        }

                        _verts[destination] = m.MultiplyPoint3x4(verts[source].position);
                        _verts[destination + 1] = m.MultiplyPoint3x4(verts[source2].position);
                    }

                    Handles.color = vertexColor.v;

                    Handles.DrawLines(_verts);
                }

                if (drawBorders.v)
                {
                    if (_borderList == null)
                    {
                        _borderList = new List<Vector3>();
                    }

                    _borderList.Clear();

                    for (var i = 0; i < edges.Length; i++)
                    {
                        var edge = edges[i];

                        if (edge.triangleCount != 1)
                        {
                            continue;
                        }

                        var v1 = _meshObject.data.vertices[edge.aIndex];
                        var v2 = _meshObject.data.vertices[edge.bIndex];

                        _borderList.Add(m.MultiplyPoint3x4(v1.position));
                        _borderList.Add(m.MultiplyPoint3x4(v2.position));
                    }

                    if ((_borders == null) || (_borders.Length != _borderList.Count))
                    {
                        _borders = new Vector3[_borderList.Count];
                    }

                    _borderList.CopyTo(_borders);

                    Handles.color = borderColor.v;
                    Handles.DrawLines(_borders);
                }

                if (drawBorderNormals.v)
                {
                    var edgeIndices = _meshObject.data.borderEdgeIndices;

                    if ((_borderNormals == null) || (_borderNormals.Length != (edgeIndices.Length * 2)))
                    {
                        _borderNormals = new Vector3[edgeIndices.Length * 2];
                    }

                    for (var i = 0; i < edgeIndices.Length; i++)
                    {
                        var edgeIndex = edgeIndices[i];
                        var normals = _meshObject.data.borderEdgeNormals[i];

                        var edge = edges[edgeIndex];

                        var vertexA = verts[edge.aIndex];
                        var vertexB = verts[edge.bIndex];

                        var center = (vertexA.position + vertexB.position) / 2f;

                        var point = m.MultiplyPoint3x4(center);

                        var drawIndex = i * 2;
                        _borderNormals[drawIndex + 0] = point;
                        _borderNormals[drawIndex + 1] = point + m.MultiplyVector(normals);
                    }

                    Handles.color = borderColor.v;
                    Handles.DrawLines(_borderNormals);
                }

                var mesh = _meshObject.mesh;
                var meshT = mesh.triangles;
                var meshV = mesh.vertices;

                if (drawOriginalVertices.v)
                {
                    if ((_triVertsOriginal == null) || (_triVertsOriginal.Length != (meshT.Length * 2)))
                    {
                        _triVertsOriginal = new Vector3[meshT.Length * 2];
                    }

                    for (var i = 0; i < (meshT.Length / 3); i++)
                    {
                        var triI = i * 3;
                        var vertI = i * 6;

                        var x = m.MultiplyPoint3x4(meshV[meshT[triI + 0]]);
                        var y = m.MultiplyPoint3x4(meshV[meshT[triI + 1]]);
                        var z = m.MultiplyPoint3x4(meshV[meshT[triI + 2]]);

                        _triVertsOriginal[vertI + 0] = x;
                        _triVertsOriginal[vertI + 1] = y;
                        _triVertsOriginal[vertI + 2] = y;
                        _triVertsOriginal[vertI + 3] = z;
                        _triVertsOriginal[vertI + 4] = x;
                        _triVertsOriginal[vertI + 5] = z;
                    }

                    Handles.color = originalVertexColor.v;
                    Handles.DrawLines(_triVertsOriginal);
                }

                {
                    var ah = adjustmentHandle.v;
                    var u = (float3)t.up;
                    var r = (float3)t.right;
                    var f = (float3)t.forward;
                    var endu = pos + (dist * u);
                    var endf = pos + (dist * f);
                    var endr = pos + (dist * r);
                    SmartHandles.DrawHandleLine(pos, endu, ah, r, u, size, Color.green);
                    SmartHandles.DrawHandleLine(pos, endf, ah, u, f, size, Color.blue);
                    SmartHandles.DrawHandleLine(pos, endr, ah, u, r, size, Color.red);
                }

                {
                    var wh = worldHandle.v;
                    var Dist = worldDistanceScale.v * dist;
                    var Size = worldHandleScale.v * size;
                    var U = (float3)Vector3.up;
                    var R = (float3)Vector3.right;
                    var F = (float3)Vector3.forward;
                    var endU = pos + (Dist * U);
                    var endF = pos + (Dist * F);
                    var endR = pos + (Dist * R);
                    Color.RGBToHSV(Color.green, out var wUHUE, out var wUSAT, out var wUVAL);
                    Color.RGBToHSV(Color.blue,  out var wFHUE, out var wFSAT, out var wFVAL);
                    Color.RGBToHSV(Color.red,   out var wRHUE, out var wRSAT, out var wRVAL);
                    var wUC = Color.HSVToRGB(wUHUE, worldSaturation.v * wUSAT, worldValue.v * wUVAL);
                    var wFC = Color.HSVToRGB(wFHUE, worldSaturation.v * wFSAT, worldValue.v * wFVAL);
                    var wRC = Color.HSVToRGB(wRHUE, worldSaturation.v * wRSAT, worldValue.v * wRVAL);
                    SmartHandles.DrawHandleLine(pos, endU, wh, R, U, Size, wUC);
                    SmartHandles.DrawHandleLine(pos, endF, wh, U, F, Size, wFC);
                    SmartHandles.DrawHandleLine(pos, endR, wh, U, R, Size, wRC);
                }

                var terrainData = _terrainMetadataManager.GetTerrain(_terrainHashCodes[0]);
                var terrainNormal = terrainData.GetTerrainNormal(pos);

                {
                    var rh = terrainHandle.v;
                    var T = terrainNormal;
                    var tRot = Quaternion.LookRotation(T, Vector3.up);
                    var Tf = (float3)tRot.Forward();
                    var Tu = (float3)tRot.Up();
                    var endT = pos + (dist * T);
                    SmartHandles.DrawHandleLine(pos, endT, rh, Tu, Tf, size, terrainColor.v);
                }

                {
                    var h = meshBorderHandle.v;
                    var vec = m.MultiplyVector(_meshObject.data.BorderNormal);
                    var rot = Quaternion.LookRotation(vec, Vector3.up);
                    var f = (float3)rot.Forward();
                    var up = (float3)rot.Up();
                    var end = pos + (dist * vec);
                    SmartHandles.DrawHandleLine(pos, end, h, up, f, size, meshBorderColor.v);
                }

                {
                    var h = meshFaceHandle.v;
                    var vec = m.MultiplyVector(_meshObject.data.AverageFaceNormal);
                    var rot = Quaternion.LookRotation(vec, Vector3.up);
                    var f = (float3)rot.Forward();
                    var up = (float3)rot.Up();
                    var end = pos + (dist * vec);
                    SmartHandles.DrawHandleLine(pos, end, h, up, f, size, faceNormalColor.v);
                }

                SmartHandles.DrawWireSphere(
                    pos,
                    gizmoRadius.v * 2.0f,
                    _recent.proposed.error < errorThreshold ? Color.green : Color.red
                );

                if (drawRotation.v)
                {
                    var slerpTime = Mathf.PingPong(Time.time * rotationSpeed.v, 1.0f);

                    var gDist = rotationGizmoDistance.v;
                    var terrainRotation = quaternion.LookRotationSafe(terrainNormal, float3c.up);
                    var meshBorderRotation = quaternion.LookRotationSafe(
                        m.MultiplyVector(_meshObject.data.BorderNormal),
                        float3c.up
                    );

                    var dummy = m.MultiplyVector(_meshObject.data.BorderNormal)
                                 .fromToRotation(terrainNormal, true);

                    var terrainIndicatorPosition = pos + (gDist * terrainRotation.forward());
                    var meshIndicatorPosition = pos + (gDist * meshBorderRotation.forward());
                    var dummyIndicatorPosition = pos + (1.1f * gDist * dummy.forward());

                    SmartHandles.DrawWireSphere(terrainIndicatorPosition, gizmoRadius.v, terrainColor.v);
                    SmartHandles.DrawWireSphere(meshIndicatorPosition,    gizmoRadius.v, meshBorderColor.v);
                    SmartHandles.DrawWireSphere(dummyIndicatorPosition,   gizmoRadius.v, rotationBaseColor.v);

                    var calculatedAdjustment = math.slerp(terrainRotation, meshBorderRotation, slerpTime);
                    var calculatedIndicatorPosition = pos + (gDist * .9f * calculatedAdjustment.forward());

                    SmartHandles.DrawWireSphere(
                        calculatedIndicatorPosition,
                        gizmoRadius.v,
                        calculatedAdjustmentColor.v
                    );

                    /*var adjustmentPositionBase = pos + (gDist *.7f * new quaternion(m).forward());  
                SmartHandles.DrawWireSphere(adjustmentPositionBase, gizmoRadius.v, baseRotationColor.v);

                var adjMatrix = MeshBurialJobUtilities.MatchTerrainNormal(m, terrainData.JobData);
                var adjustedMatrix = math.mul(m, adjMatrix);
                
                var adjustmentPosition = pos + (gDist *.8f * new quaternion(adjustedMatrix).forward());
                SmartHandles.DrawWireSphere(adjustmentPosition, gizmoRadius.v, adjustmentColor.v);*/
                }
            }
        }

        #endregion

        [Button, EnableIf(nameof(_canReinitialize))]
        public void ReinitializeMesh()
        {
            using (_PRF_ReinitializeMesh.Auto())
            {
                _meshObject.data.Dispose();

                _meshObject = _meshObjectManager.GetCheapestMeshWrapper(_gameObject, true);
            }
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            await base.Initialize(initializer);

            if (_gameObject == null)
            {
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                _gameObject = AssetDatabaseManager.LoadAssetAtPath<GameObject>(path);

                if (_gameObject == null)
                {
                    enabled = false;
                    return;
                }
            }

            var t = transform;
            var p = t.parent;

            if (p == null)
            {
                var newParent = new GameObject(ZString.Format("PARENT_{0}", (object)name));
                p = newParent.transform;
                p.SetMatrix4x4ToTransform(t.localToWorldMatrix);

                t.SetParent(p, true);
            }

            if (_matrices.ShouldAllocate() || (_matrices.Length == 0))
            {
                _matrices.SafeDispose();
                _matrices = new NativeArray<float4x4>(1, Allocator.Persistent);
            }

            if ((_terrainHashCodes == null) || (_terrainHashCodes.Length == 0))
            {
                _terrainHashCodes = new int[1];
            }

            if ((_meshObject == null) || !_meshObject.data.isCreated)
            {
                _meshObject = _meshObjectManager.GetCheapestMeshWrapper(gameObject, true);
            }

            if (_instanceData == null)
            {
                _instanceData = new MeshBurialInstanceData();
            }

            if (_dependencyList.ShouldAllocate())
            {
                _dependencyList = new NativeList<JobHandle>(Allocator.Persistent);
            }
            else
            {
                SafeNative.SafeClear(ref _dependencyList);
            }

            if (_state == null)
            {
                _state = MeshBurialSharedStateManager.GetByPrefab(_gameObject);
            }

            if (_adjustment == null)
            {
                _adjustment = _meshBurialAdjustmentCollection.GetByPrefab(_gameObject);
            }

            if (_randoms.ShouldAllocate())
            {
                _randoms = new JobRandoms(Allocator.Persistent);
            }

            var randomSearch = _optimizationOptions.randomSearch;
            randomSearch.iterations = iterations;

            _optimizationOptions.randomSearch = randomSearch;

            _burialOptions.threshold = errorThreshold;
            _burialOptions.accountForMeshNormal = accountForMeshNormal;
            _burialOptions.matchTerrainNormal = matchTerrainNormal;
            permissiveness = math.clamp(permissiveness, 1, maxPermissiveness);
            _burialOptions.permissiveness = permissiveness;
            _burialOptions.adjustHeight = adjustHeight;
            _burialOptions.applyParameters = applyParameters;
            _burialOptions.testValue = float4x4.identity;

            _matrices[0] = p.localToWorldMatrix;

            _terrainHashCodes[0] = _terrainMetadataManager.GetTerrainHashCodeAt(p.position);
        }

        protected override async AppaTask WhenDisabled()
        {
            await base.WhenDisabled();

            using (_PRF_WhenDisabled.Auto())
            {
                _hasCachedUpdateValues = false;

                _pendingHandle.Complete();

                _randoms.SafeDispose();
                _dependencyList.SafeDisposeAll();
                _instanceData?.Dispose();
                _state = null;
                _adjustment = null;
                _matrices.SafeDispose();
                _terrainHashCodes = null;
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_ReinitializeMesh =
            new ProfilerMarker(_PRF_PFX + nameof(ReinitializeMesh));

        #endregion
    }
}
#endif
