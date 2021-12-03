#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Editing.Debugging.Handle;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.Gizmos;
using Appalachia.Utility.Constants;
using Appalachia.Utility.Extensions;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.VoxelTypes
{
    public abstract partial class VoxelsBase<TThis, TRaycastHit> : IDisposable,
                                                       IEquatable<VoxelsBase<TThis, TRaycastHit>>
        where TThis : VoxelsBase<TThis, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        #region Gizmos

        [BurstDiscard]
        [Conditional("UNITY_EDITOR")]
        public void DrawGizmos(VoxelDataGizmoSettings settings)
        {
            using (var scope = new SmartHandles.UnifiedDrawingScope(localToWorld))
            {
                if (settings.drawGrid)
                {
                    DrawGrid(settings, scope);
                }

                if (settings.drawBounds)
                {
                    DrawBounds(settings, scope);
                }

                if (settings.drawBoundsSubdivisions)
                {
                    DrawBoundsSubdivisions(settings, scope);
                }

                if (settings.drawSamplePoints)
                {
                    DrawSamplePoints(settings, scope);
                }

                if (settings.drawVoxels)
                {
                    DrawVoxels(settings, scope);
                }

                if (settings.drawNormals)
                {
                    DrawNormals(settings, scope);
                }

                if (settings.drawNormalFaces)
                {
                    DrawNormalFaces(settings, scope);
                }

                if (settings.drawFaces)
                {
                    DrawFaces(settings, scope);
                }

                if (settings.testRaycast)
                {
                    TestRaycast(settings, scope);
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DrawSamplePoints =
            new(_PRF_PFX + nameof(DrawSamplePoints));

        protected void DrawSamplePoints(
            VoxelDataGizmoSettings settings,
            SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawSamplePoints.Auto())
            {
                var gizmoColor = settings.samplePointsColor;

                if (!settings.colorSamplePointsWithTime)
                {
                    scope.color = gizmoColor;
                }

                var gizmoSize = resolution * settings.samplePointsGizmoScale;

                for (var ix = 0; ix < x; ix++)
                for (var iy = 0; iy < y; iy++)
                for (var iz = 0; iz < z; iz++)
                {
                    var samplePoint = samplePoints[ix, iy, iz];

                    if (samplePoint.populated && !settings.drawPopulatedSamplePoints)
                    {
                        continue;
                    }

                    if (settings.colorSamplePointsWithTime)
                    {
                        gizmoColor.r = samplePoint.time.x;
                        gizmoColor.g = samplePoint.time.y;
                        gizmoColor.b = samplePoint.time.z;
                        scope.color = gizmoColor;
                    }

                    SmartHandles.DrawWireCube(samplePoint.position, gizmoSize);
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DrawVoxels = new(_PRF_PFX + nameof(DrawVoxels));

        protected void DrawVoxels(VoxelDataGizmoSettings settings, SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawVoxels.Auto())
            {
                var gizmoColor = settings.voxelsColor;

                if (!settings.colorVoxelsWithTime)
                {
                    scope.color = gizmoColor;
                }

                var gizmoSize = resolution * settings.voxelsGizmoScale;

                for (var i = 0; i < count; i++)
                {
                    var voxel = voxels[i];

                    if (voxel.faceData.isFace && settings.drawFaces && !settings.drawFaceVoxels)
                    {
                        continue;
                    }

                    var samplePoint = samplePoints[voxel.indices];

                    if (settings.colorVoxelsWithTime)
                    {
                        gizmoColor.r = samplePoint.time.x;
                        gizmoColor.g = samplePoint.time.y;
                        gizmoColor.b = samplePoint.time.z;
                        scope.color = gizmoColor;
                    }

                    SmartHandles.DrawWireCube(voxel.position, gizmoSize);
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DrawFaces = new(_PRF_PFX + nameof(DrawFaces));

        protected void DrawFaces(VoxelDataGizmoSettings settings, SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawFaces.Auto())
            {
                var gizmoColor = settings.facesColor;

                if (!settings.colorFacesWithTime)
                {
                    scope.color = gizmoColor;
                }

                var gizmoSize = resolution * settings.facesGizmoScale;

                for (var i = 0; i < count; i++)
                {
                    var voxel = voxels[i];

                    if (!voxel.faceData.isFace)
                    {
                        continue;
                    }

                    var samplePoint = samplePoints[voxel.indices];

                    if (settings.colorFacesWithTime)
                    {
                        gizmoColor.r = samplePoint.time.x;
                        gizmoColor.g = samplePoint.time.y;
                        gizmoColor.b = samplePoint.time.z;
                        scope.color = gizmoColor;
                    }

                    SmartHandles.DrawWireCube(voxel.position, gizmoSize);
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DrawNormals = new(_PRF_PFX + nameof(DrawNormals));

        protected void DrawNormals(VoxelDataGizmoSettings settings, SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawNormals.Auto())
            {
                var gizmoColor = settings.normalsColor;

                if (!settings.colorNormalsWithNormal)
                {
                    scope.color = gizmoColor;
                }

                var gizmoSize = resolution * settings.normalsGizmoScale;

                for (var i = 0; i < count; i++)
                {
                    var voxel = voxels[i];

                    if (!voxel.faceData.isFace)
                    {
                        continue;
                    }

                    if (settings.colorNormalsWithNormal)
                    {
                        gizmoColor.r = voxel.faceData.normal.x;
                        gizmoColor.g = voxel.faceData.normal.y;
                        gizmoColor.b = voxel.faceData.normal.z;
                        scope.color = gizmoColor;
                    }

                    UnityEngine.Gizmos.DrawLine(
                        voxel.position,
                        voxel.position + (voxel.faceData.normal * gizmoSize)
                    );
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DrawNormalFaces = new(_PRF_PFX + nameof(DrawNormalFaces));

        protected void DrawNormalFaces(
            VoxelDataGizmoSettings settings,
            SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawNormalFaces.Auto())
            {
                var gizmoColor = settings.normalFacesColor;

                if (!settings.colorNormalFacesWithNormal)
                {
                    scope.color = gizmoColor;
                }

                var gizmoSize = resolution * settings.normalFacesGizmoScale;

                for (var i = 0; i < count; i++)
                {
                    var voxel = voxels[i];

                    if (!voxel.faceData.isFace)
                    {
                        continue;
                    }

                    if (settings.colorNormalFacesWithNormal)
                    {
                        gizmoColor.r = voxel.faceData.normal.x;
                        gizmoColor.g = voxel.faceData.normal.y;
                        gizmoColor.b = voxel.faceData.normal.z;
                        scope.color = gizmoColor;
                    }

                    if (voxel.faceData.forward)
                    {
                        SmartHandles.DrawWireCube(
                            voxel.position + (float3c.forward * settings.normalFacesGizmoOffset),
                            gizmoSize
                        );
                    }

                    if (voxel.faceData.back)
                    {
                        SmartHandles.DrawWireCube(
                            voxel.position + (float3c.back * settings.normalFacesGizmoOffset),
                            gizmoSize
                        );
                    }

                    if (voxel.faceData.left)
                    {
                        SmartHandles.DrawWireCube(
                            voxel.position + (float3c.left * settings.normalFacesGizmoOffset),
                            gizmoSize
                        );
                    }

                    if (voxel.faceData.right)
                    {
                        SmartHandles.DrawWireCube(
                            voxel.position + (float3c.right * settings.normalFacesGizmoOffset),
                            gizmoSize
                        );
                    }

                    if (voxel.faceData.down)
                    {
                        SmartHandles.DrawWireCube(
                            voxel.position + (float3c.down * settings.normalFacesGizmoOffset),
                            gizmoSize
                        );
                    }

                    if (voxel.faceData.up)
                    {
                        SmartHandles.DrawWireCube(
                            voxel.position + (float3c.up * settings.normalFacesGizmoOffset),
                            gizmoSize
                        );
                    }
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DrawBounds = new(_PRF_PFX + nameof(DrawBounds));

        protected void DrawBounds(VoxelDataGizmoSettings settings, SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawBounds.Auto())
            {
                scope.color = settings.boundsColor;

                SmartHandles.DrawWireCube(voxelBounds.center, voxelBounds.size);

                scope.color = settings.boundsColor * .5f;

                SmartHandles.DrawWireCube(rawBounds.center, rawBounds.size);
            }
        }

        [NonSerialized] private Bounds _gizmo_voxelBounds;
        [NonSerialized] private Vector3[] _gizmo_voxelBoundsSubdivisionLineSegments;

        private static readonly ProfilerMarker _PRF_DrawBoundsSubdivisions =
            new(_PRF_PFX + nameof(DrawBoundsSubdivisions));

        protected void DrawBoundsSubdivisions(
            VoxelDataGizmoSettings settings,
            SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawBoundsSubdivisions.Auto())
            {
                if ((_gizmo_voxelBounds != voxelBounds) ||
                    (_gizmo_voxelBoundsSubdivisionLineSegments == null) ||
                    (_gizmo_voxelBoundsSubdivisionLineSegments.Length == 0))
                {
                    _gizmo_voxelBounds = voxelBounds;

                    var gridCountX = (int) (voxelBounds.size.x / resolution.x) + 1;
                    var gridCountZ = (int) (voxelBounds.size.z / resolution.z) + 1;
                    var gridCount = gridCountX + gridCountZ;

                    if ((_gizmo_voxelBoundsSubdivisionLineSegments == null) ||
                        (_gizmo_voxelBoundsSubdivisionLineSegments.Length != (gridCount * 2)))
                    {
                        _gizmo_voxelBoundsSubdivisionLineSegments = new Vector3[4 + (gridCount * 2)];
                    }

                    var floor_y = -voxelBounds.extents.y;
                    var center = voxelBounds.center;

                    var index = 0;

                    for (var grid_x = -voxelBounds.extents.x;
                        grid_x <= voxelBounds.extents.x;
                        grid_x += resolution.x)
                    {
                        var seg1 = new Vector3(grid_x, floor_y, -voxelBounds.extents.z);
                        var seg2 = new Vector3(grid_x, floor_y, voxelBounds.extents.z);

                        _gizmo_voxelBoundsSubdivisionLineSegments[index] = center + seg1;
                        _gizmo_voxelBoundsSubdivisionLineSegments[index + 1] = center + seg2;

                        index += 2;
                    }

                    for (var grid_z = -voxelBounds.extents.z;
                        grid_z <= voxelBounds.extents.z;
                        grid_z += resolution.z)
                    {
                        var seg1 = new Vector3(-voxelBounds.extents.x, floor_y, grid_z);
                        var seg2 = new Vector3(voxelBounds.extents.x,  floor_y, grid_z);

                        _gizmo_voxelBoundsSubdivisionLineSegments[index] = center + seg1;
                        _gizmo_voxelBoundsSubdivisionLineSegments[index + 1] = center + seg2;

                        index += 2;
                    }
                }

                scope.color = settings.boundsSubdivisionColor;
                UnityEditor.Handles.DrawLines(_gizmo_voxelBoundsSubdivisionLineSegments);
            }
        }

        private static readonly ProfilerMarker _PRF_DrawGrid = new(_PRF_PFX + nameof(DrawGrid));

        protected void DrawGrid(VoxelDataGizmoSettings settings, SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_DrawGrid.Auto())
            {
                var gizmoColor = settings.gridColor;

                if (!settings.colorGridWithTime)
                {
                    scope.color = gizmoColor;
                }

                var gizmoSize = resolution;

                for (var ix = 0; ix < x; ix++)
                for (var iy = 0; iy < y; iy++)
                for (var iz = 0; iz < z; iz++)
                {
                    var samplePoint = samplePoints[ix, iy, iz];

                    if (settings.colorGridWithTime)
                    {
                        gizmoColor.r = samplePoint.time.x;
                        gizmoColor.g = samplePoint.time.y;
                        gizmoColor.b = samplePoint.time.z;
                        scope.color = gizmoColor;
                    }

                    SmartHandles.DrawWireCube(samplePoint.position, gizmoSize);
                }
            }
        }

        private static readonly ProfilerMarker _PRF_TestRaycast = new(_PRF_PFX + nameof(TestRaycast));

        [NonSerialized] private TRaycastHit[] _testHits;

        protected void TestRaycast(VoxelDataGizmoSettings settings, SmartHandles.UnifiedDrawingScope scope)
        {
            using (_PRF_TestRaycast.Auto())
            {
                if (_testHits == null)
                {
                    _testHits = new TRaycastHit[256];
                }

                var gizmoSize = resolution * settings.rayHitGizmoSize;

                var rayOrigin = settings.rayOrigin;
                var rayDirection = settings.rayDirection;
                var rayDistance = settings.rayMaximumDistance;

                if (settings.raySpace == Space.Self)
                {
                    var ltw = localToWorld;
                    rayOrigin = ltw.MultiplyPoint3x4(rayOrigin);
                    rayDirection = ltw.MultiplyVector(rayDirection);
                }

                if (settings.autoAimAtCenter)
                {
                    rayDirection = math.normalizesafe((float3) voxelWorldBounds.center - rayOrigin);
                }

                var ray = new Ray(rayOrigin, rayDirection * rayDistance);

                var hitCount = RaycastNonAlloc(ray, _testHits, rayDistance);

                SmartHandles.DrawRay(ray, hitCount == 0 ? settings.rayMissColor : settings.rayHitColor);

                for (var i = 0; i < hitCount; i++)
                {
                    var hit = _testHits[i];
                    var voxel = hit.Voxel;

                    if (settings.colorFacesWithTime)
                    {
                        scope.color = settings.rayHitVoxelColor;
                    }

                    SmartHandles.DrawWireCube(voxel.position, gizmoSize);
                }
            }
        }

        #endregion
    }
}

#endif