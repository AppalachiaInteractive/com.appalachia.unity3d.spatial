#region

using System;
using System.Collections.Generic;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Collections.Native.Pointers;
using Appalachia.Core.Extensions;
using Appalachia.Jobs.Burstable;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;
using Appalachia.Utility.Constants;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Logging;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels
{
    public static class Voxelizer
    {
#region Profiling

        private const string _PRF_PFX = nameof(Voxelizer) + ".";

        private static readonly ProfilerMarker _PRF_VoxelizeSingleInternal =
            new(_PRF_PFX + nameof(VoxelizeSingleInternal));

        private static readonly ProfilerMarker _PRF_VoxelizeSingle =
            new(_PRF_PFX + nameof(VoxelizeSingle));

        private static readonly ProfilerMarker _PRF_VoxelizeInternal =
            new(_PRF_PFX + nameof(VoxelizeInternal));

        private static readonly ProfilerMarker _PRF_Voxelize = new(_PRF_PFX + nameof(Voxelize));

        private static readonly ProfilerMarker _PRF_CheckArguments =
            new(_PRF_PFX + nameof(CheckArguments));

        private static readonly ProfilerMarker _PRF_PopulateVoxels =
            new(_PRF_PFX + nameof(PopulateVoxels));

        private static readonly ProfilerMarker _PRF_SetFaceData =
            new(_PRF_PFX + nameof(SetFaceData));

        private static readonly ProfilerMarker _PRF_CheckFaceHit =
            new(_PRF_PFX + nameof(CheckFaceHit));

        private static readonly ProfilerMarker _PRF_SyncTransforms =
            new(_PRF_PFX + nameof(SyncTransforms));

        private static readonly ProfilerMarker _PRF_PointIsInsideCollider =
            new(_PRF_PFX + nameof(PointIsInsideCollider));

        private static readonly ProfilerMarker _PRF_VoxelBounds =
            new(_PRF_PFX + nameof(VoxelBounds));

        private static readonly ProfilerMarker _PRF_RoundFloat = new(_PRF_PFX + nameof(RoundFloat));

#endregion

#region Voxelize Public

        public static T VoxelizeSingle<T, TRaycastHit>(
            T voxelData,
            Transform transform,
            Bounds voxelBounds,
            float3 samplePoint,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_VoxelizeSingle.Auto())
            {
                return VoxelizeSingleInternal<T, TRaycastHit>(
                    voxelData,
                    transform,
                    voxelBounds,
                    samplePoint,
                    allocator
                );
            }
        }

        public static T Voxelize<T, TRaycastHit>(
            T voxelData,
            Transform transform,
            Collider[] colliders,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_Voxelize.Auto())
            {
                return Voxelize<T, TRaycastHit>(
                    voxelData,
                    transform,
                    VoxelPopulationStyle.Colliders,
                    colliders,
                    null,
                    resolution,
                    allocator
                );
            }
        }

        public static T Voxelize<T, TRaycastHit>(
            T voxelData,
            Transform transform,
            MeshRenderer[] renderers,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_Voxelize.Auto())
            {
                return Voxelize<T, TRaycastHit>(
                    voxelData,
                    transform,
                    VoxelPopulationStyle.Meshes,
                    null,
                    renderers,
                    resolution,
                    allocator
                );
            }
        }

        public static T Voxelize<T, TRaycastHit>(
            T voxelData,
            Transform transform,
            VoxelPopulationStyle style,
            Collider[] colliders,
            MeshRenderer[] renderers,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_Voxelize.Auto())
            {
                return VoxelizeInternal<T, TRaycastHit>(
                    voxelData,
                    transform,
                    style,
                    colliders,
                    renderers,
                    resolution,
                    allocator
                );
            }
        }

        public static T VoxelizeSingle<T, TRaycastHit>(
            Transform transform,
            Bounds voxelBounds,
            float3 samplePoint,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>, new()
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_VoxelizeSingle.Auto())
            {
                var voxelData = new T();

                return VoxelizeSingleInternal<T, TRaycastHit>(
                    voxelData,
                    transform,
                    voxelBounds,
                    samplePoint,
                    allocator
                );
            }
        }

        public static T Voxelize<T, TRaycastHit>(
            Transform transform,
            Collider[] colliders,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>, new()
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_Voxelize.Auto())
            {
                return Voxelize<T, TRaycastHit>(
                    transform,
                    VoxelPopulationStyle.Colliders,
                    colliders,
                    null,
                    resolution,
                    allocator
                );
            }
        }

        public static T Voxelize<T, TRaycastHit>(
            Transform transform,
            MeshRenderer[] renderers,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>, new()
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_Voxelize.Auto())
            {
                return Voxelize<T, TRaycastHit>(
                    transform,
                    VoxelPopulationStyle.Meshes,
                    null,
                    renderers,
                    resolution,
                    allocator
                );
            }
        }

        public static T Voxelize<T, TRaycastHit>(
            Transform transform,
            VoxelPopulationStyle style,
            Collider[] colliders,
            MeshRenderer[] renderers,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>, new()
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_Voxelize.Auto())
            {
                var voxelData = new T();
                return VoxelizeInternal<T, TRaycastHit>(
                    voxelData,
                    transform,
                    style,
                    colliders,
                    renderers,
                    resolution,
                    allocator
                );
            }
        }

#endregion

#region Voxelize Internals

        private static T VoxelizeSingleInternal<T, TRaycastHit>(
            T voxelData,
            Transform transform,
            Bounds voxelBounds,
            float3 samplePoint,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_VoxelizeSingleInternal.Auto())
            {
                voxelData.resolution = float3.zero;
                voxelData.faceCount = 1;
                voxelData.samplePoints = new NativeArray3D<VoxelSamplePoint>(1, 1, 1, allocator);
                voxelData.voxels = new NativeArray<Voxel>(1, allocator);
                voxelData.voxelsActive = new NativeBitArray(1, allocator);
                voxelData.voxelsActiveCount = new NativeIntPtr(allocator, 1);
                voxelData.style = VoxelPopulationStyle.SinglePoint;

                voxelData.Initialize(transform);
                voxelData.UpdateBounds(voxelBounds, voxelBounds);
                voxelData.Synchronize();

                var time = (samplePoint - (float3) voxelBounds.min) /
                           (voxelBounds.max - voxelBounds.min);

                var voxelSamplePoint = new VoxelSamplePoint
                {
                    populated = true, position = samplePoint, time = time
                };

                voxelData.samplePoints[0] = voxelSamplePoint;

                var voxel = new Voxel
                {
                    position = samplePoint, faceData = VoxelFaceData.FullyExposed()
                };

                voxelData.voxels[0] = voxel;

                if (voxelData.IsPersistent)
                {
                    voxelData.InitializeDataStore();
                }

                return voxelData;
            }
        }

        private static T VoxelizeInternal<T, TRaycastHit>(
            T voxelData,
            Transform transform,
            VoxelPopulationStyle style,
            Collider[] colliders,
            MeshRenderer[] renderers,
            float3 resolution,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_VoxelizeInternal.Auto())
            {
                var t = transform;
                var rotation = t.rotation;
                var position = t.position;
                var localScale = t.localScale;

                CheckArguments(resolution, style, colliders, renderers);

                try
                {
                    t.SetPositionAndRotation(float3.zero, Quaternion.identity);
                    t.localScale = float3c.one;

                    SyncTransforms(4);

                    var rawBounds = style == VoxelPopulationStyle.Colliders
                        ? VoxelBounds(colliders)
                        : VoxelBounds(renderers);

                    var voxelBounds = rawBounds;

                    voxelBounds.size = RoundFloat(voxelBounds.size, resolution);

                    voxelData.resolution = resolution;
                    voxelData.style = style;
                    voxelData.renderers = renderers;
                    voxelData.colliders = colliders;

                    voxelData.Initialize(t);
                    voxelData.UpdateBounds(rawBounds, voxelBounds);
                    voxelData.Synchronize();

                    PopulateSamplePoints<T, TRaycastHit>(voxelData, allocator);

                    if (style == VoxelPopulationStyle.Colliders)
                    {
                        PopulateVoxels<T, TRaycastHit>(voxelData, colliders, allocator);
                    }
                    else
                    {
                        PopulateVoxels<T, TRaycastHit>(voxelData, renderers, allocator);
                    }

                    SetFaceData<T, TRaycastHit>(voxelData);

                    t.SetPositionAndRotation(position, rotation);
                    t.localScale = localScale;

                    if (voxelData.IsPersistent)
                    {
                        voxelData.InitializeDataStore();
                    }

                    return voxelData;
                }
                catch (Exception ex)
                {
                    t.SetPositionAndRotation(position, rotation);
                    t.localScale = localScale;

                    SyncTransforms(4);
                    AppaLog.Exception(ex);
                    throw;
                }
            }
        }

#endregion

#region Helpers

        private static void CheckArguments(
            float3 resolution,
            VoxelPopulationStyle style,
            Collider[] colliders,
            MeshRenderer[] renderers)
        {
            using (_PRF_CheckArguments.Auto())
            {
                if ((resolution.x <= 0f) || (resolution.y <= 0f) || (resolution.z <= 0f))
                {
                    throw new NotSupportedException(
                        $"Voxel resolution must be positive!  Provided: {resolution}"
                    );
                }

                if ((style == VoxelPopulationStyle.Meshes) &&
                    ((renderers == null) || (renderers.Length == 0)))
                {
                    throw new NotSupportedException("Must provide renderers!");
                }

                if ((style == VoxelPopulationStyle.Colliders) &&
                    ((colliders == null) || (colliders.Length == 0)))
                {
                    throw new NotSupportedException("Must provide colliders!");
                }
            }
        }

        private static readonly ProfilerMarker _PRF_PopulateSamplePoints =
            new(_PRF_PFX + nameof(PopulateSamplePoints));

        public static void PopulateSamplePoints<T, TRaycastHit>(
            T voxelData,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_PopulateSamplePoints.Auto())
            {
                var boundsSize = voxelData.voxelBounds.size;
                var boundsCenter = voxelData.voxelBounds.center;
                var boundsExtents = voxelData.voxelBounds.extents;
                var boundsMin = voxelData.voxelBounds.min;

                var x = (int) math.round(boundsSize.x / voxelData.resolution.x);
                var y = (int) math.round(boundsSize.y / voxelData.resolution.y);
                var z = (int) math.round(boundsSize.z / voxelData.resolution.z);

                if (voxelData.samplePoints.IsCreated)
                {
                    voxelData.samplePoints.SafeDispose();
                }

                if ((x == 0) || (y == 0) || (z == 0) || (x < 0) || (y < 0) || (z < 0))
                {
                    throw new NotSupportedException(
                        $"Voxels not possible, bounds: [{voxelData.voxelBounds}]"
                    );
                }

                voxelData.samplePoints = new NativeArray3D<VoxelSamplePoint>(x, y, z, allocator);

                var halfRez = voxelData.resolution * .5f;

                for (var indexX = 0; indexX < voxelData.x; indexX++)
                {
                    for (var indexY = 0; indexY < voxelData.y; indexY++)
                    {
                        for (var indexZ = 0; indexZ < voxelData.z; indexZ++)
                        {
                            var samplePoint = new VoxelSamplePoint();

                            samplePoint.position = new float3(
                                -boundsExtents.x +
                                halfRez.x +
                                (indexX * voxelData.resolution.x) +
                                boundsCenter.x,
                                -boundsExtents.y +
                                halfRez.y +
                                (indexY * voxelData.resolution.y) +
                                boundsCenter.y,
                                -boundsExtents.z +
                                halfRez.z +
                                (indexZ * voxelData.resolution.z) +
                                boundsCenter.z
                            );

                            samplePoint.time = new float3(
                                (samplePoint.position.x - boundsMin.x) / boundsSize.x,
                                (samplePoint.position.y - boundsMin.y) / boundsSize.y,
                                (samplePoint.position.z - boundsMin.z) / boundsSize.z
                            );

                            voxelData.samplePoints[indexX, indexY, indexZ] = samplePoint;
                        }
                    }
                }

                if (voxelData.voxels.IsCreated)
                {
                    voxelData.voxels.SafeDispose();
                }
            }
        }

        public static void PopulateVoxels<T, TRaycastHit>(
            T voxelData,
            Collider[] colliders,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_PopulateVoxels.Auto())
            {
                var populatedVoxels = 0;

                for (var indexX = 0; indexX < voxelData.x; indexX++)
                {
                    for (var indexY = 0; indexY < voxelData.y; indexY++)
                    {
                        for (var indexZ = 0; indexZ < voxelData.z; indexZ++)
                        {
                            var samplePoint = voxelData.samplePoints[indexX, indexY, indexZ];

                            for (var colliderIndex = 0;
                                colliderIndex < colliders.Length;
                                colliderIndex++)
                            {
                                var testingCollider = colliders[colliderIndex];

                                if (!testingCollider.enabled)
                                {
                                    continue;
                                }

                                if (PointIsInsideCollider(testingCollider, samplePoint.position))
                                {
                                    samplePoint.populated = true;
                                    samplePoint.index = populatedVoxels;
                                    populatedVoxels += 1;
                                    voxelData.samplePoints[indexX, indexY, indexZ] = samplePoint;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (voxelData.voxels.IsCreated)
                {
                    voxelData.voxels.SafeDispose();
                }

                if (voxelData.voxelsActive.IsCreated)
                {
                    voxelData.voxelsActive.SafeDispose();
                }

                if (voxelData.voxelsActiveCount.IsCreated)
                {
                    voxelData.voxelsActiveCount.SafeDispose();
                }

                voxelData.voxels = new NativeArray<Voxel>(populatedVoxels, allocator);
                voxelData.voxelsActive = new NativeBitArray(populatedVoxels, allocator);
                voxelData.voxelsActiveCount = new NativeIntPtr(allocator, populatedVoxels);

                voxelData.InitializeElements(populatedVoxels);

                var populationIndex = 0;

                for (var indexX = 0; indexX < voxelData.x; indexX++)
                {
                    for (var indexY = 0; indexY < voxelData.y; indexY++)
                    {
                        for (var indexZ = 0; indexZ < voxelData.z; indexZ++)
                        {
                            var samplePoint = voxelData.samplePoints[indexX, indexY, indexZ];

                            if (samplePoint.populated)
                            {
                                var voxel = new Voxel
                                {
                                    position = samplePoint.position,
                                    indices = new int3(indexX, indexY, indexZ)
                                };

                                voxelData.voxels[populationIndex] = voxel;
                                populationIndex += 1;
                            }
                        }
                    }
                }
            }
        }

        public static void PopulateVoxels<T, TRaycastHit>(
            T voxelData,
            MeshRenderer[] renderers,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_PopulateVoxels.Auto())
            {
                var populatedSamplePointIndices = new HashSet<int3>();
                var samplePointBounds = new BoundsBurst();

                for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    var renderer = renderers[rendererIndex];
                    var rendererLTW = renderer.transform.localToWorldMatrix;

                    var mesh = renderer.GetSharedMesh();

                    var triangleIndices = mesh.triangles;
                    var vertices = mesh.vertices;

                    for (var i = 0; i < triangleIndices.Length; i += 3)
                    {
                        var tx = triangleIndices[i];
                        var ty = triangleIndices[i + 1];
                        var tz = triangleIndices[i + 2];

                        var vx = rendererLTW.MultiplyPoint3x4(vertices[tx]);
                        var vy = rendererLTW.MultiplyPoint3x4(vertices[ty]);
                        var vz = rendererLTW.MultiplyPoint3x4(vertices[tz]);

                        var tri = new Triangle(vx, vy, vz, float3c.forward);

                        int3 xyz;
                        for (var indexX = 0; indexX < voxelData.x; indexX++)
                        {
                            for (var indexY = 0; indexY < voxelData.y; indexY++)
                            {
                                for (var indexZ = 0; indexZ < voxelData.z; indexZ++)
                                {
                                    xyz.x = indexX;
                                    xyz.y = indexY;
                                    xyz.z = indexZ;

                                    if (!populatedSamplePointIndices.Contains(xyz))
                                    {
                                        var samplePoint =
                                            voxelData.samplePoints[indexX, indexY, indexZ];

                                        samplePointBounds.center = samplePoint.position;
                                        samplePointBounds.size = voxelData.resolution;

                                        if (Intersects(tri, samplePointBounds))
                                        {
                                            populatedSamplePointIndices.Add(xyz);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (voxelData.voxels.IsCreated)
                {
                    voxelData.voxels.SafeDispose();
                }

                if (voxelData.voxelsActive.IsCreated)
                {
                    voxelData.voxelsActive.SafeDispose();
                }

                if (voxelData.voxelsActiveCount.IsCreated)
                {
                    voxelData.voxelsActiveCount.SafeDispose();
                }

                voxelData.voxels = new NativeArray<Voxel>(
                    populatedSamplePointIndices.Count,
                    allocator
                );
                voxelData.voxelsActive = new NativeBitArray(
                    populatedSamplePointIndices.Count,
                    allocator
                );
                voxelData.voxelsActiveCount = new NativeIntPtr(Allocator.Persistent, 1);
                voxelData.InitializeElements(populatedSamplePointIndices.Count);

                var populationIndex = 0;

                foreach (var samplePointIndices in populatedSamplePointIndices)
                {
                    var spi = samplePointIndices;

                    var samplePoint = voxelData.samplePoints[spi.x, spi.y, spi.z];
                    var voxel = new Voxel
                    {
                        position = samplePoint.position, indices = new int3(spi.x, spi.y, spi.z)
                    };

                    samplePoint.populated = true;
                    samplePoint.index = populationIndex;

                    voxelData.samplePoints[spi.x, spi.y, spi.z] = samplePoint;
                    voxelData.voxels[populationIndex] = voxel;

                    populationIndex += 1;
                }
            }
        }

        public static void PopulateVoxels<T, TRaycastHit>(
            T voxelData,
            Collider[] colliders,
            MeshRenderer[] renderers,
            Allocator allocator = Allocator.Persistent)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_PopulateVoxels.Auto())
            {
                var populatedSamplePointIndices = new HashSet<int3>();
                var samplePointBounds = new BoundsBurst();
                int3 xyz;

                for (var indexX = 0; indexX < voxelData.x; indexX++)
                {
                    for (var indexY = 0; indexY < voxelData.y; indexY++)
                    {
                        for (var indexZ = 0; indexZ < voxelData.z; indexZ++)
                        {
                            xyz.x = indexX;
                            xyz.y = indexY;
                            xyz.z = indexZ;

                            var samplePoint = voxelData.samplePoints[indexX, indexY, indexZ];

                            for (var colliderIndex = 0;
                                colliderIndex < colliders.Length;
                                colliderIndex++)
                            {
                                var testingCollider = colliders[colliderIndex];

                                if (!testingCollider.enabled)
                                {
                                    continue;
                                }

                                if (PointIsInsideCollider(testingCollider, samplePoint.position))
                                {
                                    samplePoint.populated = true;
                                    voxelData.samplePoints[indexX, indexY, indexZ] = samplePoint;
                                    populatedSamplePointIndices.Add(xyz);

                                    break;
                                }
                            }
                        }
                    }
                }

                for (var rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    var renderer = renderers[rendererIndex];
                    var rendererLTW = renderer.transform.localToWorldMatrix;

                    var mesh = renderer.GetSharedMesh();

                    var triangleIndices = mesh.triangles;
                    var vertices = mesh.vertices;

                    for (var i = 0; i < triangleIndices.Length; i += 3)
                    {
                        var tx = triangleIndices[i];
                        var ty = triangleIndices[i + 1];
                        var tz = triangleIndices[i + 2];

                        var vx = rendererLTW.MultiplyPoint3x4(vertices[tx]);
                        var vy = rendererLTW.MultiplyPoint3x4(vertices[ty]);
                        var vz = rendererLTW.MultiplyPoint3x4(vertices[tz]);

                        var tri = new Triangle(vx, vy, vz, float3c.forward);

                        for (var indexX = 0; indexX < voxelData.x; indexX++)
                        {
                            for (var indexY = 0; indexY < voxelData.y; indexY++)
                            {
                                for (var indexZ = 0; indexZ < voxelData.z; indexZ++)
                                {
                                    xyz.x = indexX;
                                    xyz.y = indexY;
                                    xyz.z = indexZ;

                                    if (!populatedSamplePointIndices.Contains(xyz))
                                    {
                                        var samplePoint =
                                            voxelData.samplePoints[indexX, indexY, indexZ];

                                        if (Intersects(tri, samplePointBounds))
                                        {
                                            samplePoint.populated = true;
                                            voxelData.samplePoints[indexX, indexY, indexZ] =
                                                samplePoint;
                                            populatedSamplePointIndices.Add(xyz);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (voxelData.voxels.IsCreated)
                {
                    voxelData.voxels.SafeDispose();
                }

                if (voxelData.voxelsActive.IsCreated)
                {
                    voxelData.voxelsActive.SafeDispose();
                }

                if (voxelData.voxelsActiveCount.IsCreated)
                {
                    voxelData.voxelsActiveCount.SafeDispose();
                }

                voxelData.voxels = new NativeArray<Voxel>(
                    populatedSamplePointIndices.Count,
                    allocator
                );
                voxelData.voxelsActive = new NativeBitArray(
                    populatedSamplePointIndices.Count,
                    allocator
                );
                voxelData.voxelsActiveCount = new NativeIntPtr(
                    allocator,
                    populatedSamplePointIndices.Count
                );
                voxelData.InitializeElements(populatedSamplePointIndices.Count);

                var populationIndex = 0;

                foreach (var samplePointIndices in populatedSamplePointIndices)
                {
                    var spi = samplePointIndices;

                    var samplePoint = voxelData.samplePoints[spi.x, spi.y, spi.z];
                    var voxel = new Voxel
                    {
                        position = samplePoint.position, indices = new int3(spi.x, spi.y, spi.z)
                    };

                    samplePoint.populated = true;
                    samplePoint.index = populationIndex;

                    voxelData.samplePoints[spi.x, spi.y, spi.z] = samplePoint;
                    voxelData.voxels[populationIndex] = voxel;

                    populationIndex += 1;
                }
            }
        }

        public static void SetFaceData<T, TRaycastHit>(T voxelData)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_SetFaceData.Auto())
            {
                for (var x = 0; x < voxelData.x; x++)
                {
                    for (var y = 0; y < voxelData.y; y++)
                    {
                        var alreadyHit = false;
                        var face = VoxelFace.Back;

                        for (var z = 0; z < voxelData.z; z++)
                        {
                            CheckFaceHit<T, TRaycastHit>(voxelData, x, y, z, face, ref alreadyHit);
                        }

                        alreadyHit = false;
                        face = VoxelFace.Forward;

                        for (var z = voxelData.z - 1; z >= 0; z--)
                        {
                            CheckFaceHit<T, TRaycastHit>(voxelData, x, y, z, face, ref alreadyHit);
                        }
                    }
                }

                for (var y = 0; y < voxelData.y; y++)
                {
                    for (var z = 0; z < voxelData.z; z++)
                    {
                        var alreadyHit = false;
                        var face = VoxelFace.Left;

                        for (var x = 0; x < voxelData.x; x++)
                        {
                            CheckFaceHit<T, TRaycastHit>(voxelData, x, y, z, face, ref alreadyHit);
                        }

                        alreadyHit = false;
                        face = VoxelFace.Right;

                        for (var x = voxelData.x - 1; x >= 0; x--)
                        {
                            CheckFaceHit<T, TRaycastHit>(voxelData, x, y, z, face, ref alreadyHit);
                        }
                    }
                }

                for (var x = 0; x < voxelData.x; x++)
                {
                    for (var z = 0; z < voxelData.z; z++)
                    {
                        var alreadyHit = false;
                        var face = VoxelFace.Down;

                        for (var y = 0; y < voxelData.y; y++)
                        {
                            CheckFaceHit<T, TRaycastHit>(voxelData, x, y, z, face, ref alreadyHit);
                        }

                        alreadyHit = false;
                        face = VoxelFace.Up;

                        for (var y = voxelData.y - 1; y >= 0; y--)
                        {
                            CheckFaceHit<T, TRaycastHit>(voxelData, x, y, z, face, ref alreadyHit);
                        }
                    }
                }

                for (var i = 0; i < voxelData.count; i++)
                {
                    var voxel = voxelData.voxels[i];

                    voxel.faceData.RecalculateNormal();

                    if (voxel.faceData.isFace)
                    {
                        voxelData.faceCount += 1;
                    }
                }
            }
        }

        private static void CheckFaceHit<T, TRaycastHit>(
            T voxelData,
            int x,
            int y,
            int z,
            VoxelFace face,
            ref bool alreadyHit)
            where T : VoxelsBase<T, TRaycastHit>
            where TRaycastHit : struct, IVoxelRaycastHit
        {
            using (_PRF_CheckFaceHit.Auto())
            {
                var samplePoint = voxelData.samplePoints[x, y, z];

                if (samplePoint.populated)
                {
                    var voxel = voxelData.voxels[samplePoint.index];

                    voxel.faceData[face] = !alreadyHit;
                    alreadyHit = true;

                    voxelData.voxels[samplePoint.index] = voxel;
                }
                else
                {
                    alreadyHit = false;
                }
            }
        }

        private static void SyncTransforms(int count)
        {
            using (_PRF_SyncTransforms.Auto())
            {
                for (var i = 0; i < count; i++)
                {
                    Physics.SyncTransforms();
                }
            }
        }

        private static bool PointIsInsideCollider(Collider c, float3 p)
        {
            using (_PRF_PointIsInsideCollider.Auto())
            {
                var ct = c.transform;
                var cp = Physics.ClosestPoint(p, c, ct.position, ct.rotation);
                return math.distance(cp, p) < 0.01f;
            }
        }

        private static BoundsBurst VoxelBounds(IReadOnlyList<Collider> colliders)
        {
            using (_PRF_VoxelBounds.Auto())
            {
                var bounds = new BoundsBurst();

                for (var index = 0; index < colliders.Count; index++)
                {
                    var nextCollider = colliders[index];

                    if (index == 0)
                    {
                        bounds.center = nextCollider.bounds.center;
                    }

                    bounds.Encapsulate(nextCollider.bounds);
                }

                return bounds;
            }
        }

        private static BoundsBurst VoxelBounds(IReadOnlyList<MeshRenderer> renderers)
        {
            using (_PRF_VoxelBounds.Auto())
            {
                var bounds = new BoundsBurst();

                for (var index = 0; index < renderers.Count; index++)
                {
                    var nextRenderer = renderers[index];

                    if (index == 0)
                    {
                        bounds.center = nextRenderer.bounds.center;
                    }

                    bounds.Encapsulate(nextRenderer.bounds);
                }

                return bounds;
            }
        }

        private static float3 RoundFloat(float3 vec, float3 rounding)
        {
            using (_PRF_RoundFloat.Auto())
            {
                return new float3(
                    math.ceil(vec.x / rounding.x) * rounding.x,
                    math.ceil(vec.y / rounding.y) * rounding.y,
                    math.ceil(vec.z / rounding.z) * rounding.z
                );
            }
        }

#endregion

#region Mesh Voxelization Options

        private static bool Intersects(Triangle tri, BoundsBurst aabb)
        {
            float p0, p1, p2, r;

            float3 center = aabb.center, extents = aabb.max - center;

            float3 v0 = tri.a - center, v1 = tri.b - center, v2 = tri.c - center;

            float3 f0 = v1 - v0, f1 = v2 - v1, f2 = v0 - v2;

            float3 a00 = new(0, -f0.z, f0.y),
                   a01 = new(0, -f1.z, f1.y),
                   a02 = new(0, -f2.z, f2.y),
                   a10 = new(f0.z, 0, -f0.x),
                   a11 = new(f1.z, 0, -f1.x),
                   a12 = new(f2.z, 0, -f2.x),
                   a20 = new(-f0.y, f0.x, 0),
                   a21 = new(-f1.y, f1.x, 0),
                   a22 = new(-f2.y, f2.x, 0);

            // Test axis a00
            p0 = math.dot(v0, a00);
            p1 = math.dot(v1, a00);
            p2 = math.dot(v2, a00);
            r = (extents.y * math.abs(f0.z)) + (extents.z * math.abs(f0.y));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a01
            p0 = math.dot(v0, a01);
            p1 = math.dot(v1, a01);
            p2 = math.dot(v2, a01);
            r = (extents.y * math.abs(f1.z)) + (extents.z * math.abs(f1.y));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a02
            p0 = math.dot(v0, a02);
            p1 = math.dot(v1, a02);
            p2 = math.dot(v2, a02);
            r = (extents.y * math.abs(f2.z)) + (extents.z * math.abs(f2.y));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a10
            p0 = math.dot(v0, a10);
            p1 = math.dot(v1, a10);
            p2 = math.dot(v2, a10);
            r = (extents.x * math.abs(f0.z)) + (extents.z * math.abs(f0.x));
            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a11
            p0 = math.dot(v0, a11);
            p1 = math.dot(v1, a11);
            p2 = math.dot(v2, a11);
            r = (extents.x * math.abs(f1.z)) + (extents.z * math.abs(f1.x));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a12
            p0 = math.dot(v0, a12);
            p1 = math.dot(v1, a12);
            p2 = math.dot(v2, a12);
            r = (extents.x * math.abs(f2.z)) + (extents.z * math.abs(f2.x));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a20
            p0 = math.dot(v0, a20);
            p1 = math.dot(v1, a20);
            p2 = math.dot(v2, a20);
            r = (extents.x * math.abs(f0.y)) + (extents.y * math.abs(f0.x));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a21
            p0 = math.dot(v0, a21);
            p1 = math.dot(v1, a21);
            p2 = math.dot(v2, a21);
            r = (extents.x * math.abs(f1.y)) + (extents.y * math.abs(f1.x));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a22
            p0 = math.dot(v0, a22);
            p1 = math.dot(v1, a22);
            p2 = math.dot(v2, a22);
            r = (extents.x * math.abs(f2.y)) + (extents.y * math.abs(f2.x));

            if (math.max(-mathex.max(p0, p1, p2), mathex.min(p0, p1, p2)) > r)
            {
                return false;
            }

            if ((mathex.max(v0.x, v1.x, v2.x) < -extents.x) ||
                (mathex.min(v0.x, v1.x, v2.x) > extents.x))
            {
                return false;
            }

            if ((mathex.max(v0.y, v1.y, v2.y) < -extents.y) ||
                (mathex.min(v0.y, v1.y, v2.y) > extents.y))
            {
                return false;
            }

            if ((mathex.max(v0.z, v1.z, v2.z) < -extents.z) ||
                (mathex.min(v0.z, v1.z, v2.z) > extents.z))
            {
                return false;
            }

            var normal = math.normalize(math.cross(f1, f0));
            var pl = new PlaneBurst(normal, math.dot(normal, tri.a));
            return Intersects(pl, aabb);
        }

        private static bool Intersects(PlaneBurst pl, BoundsBurst aabb)
        {
            var center = aabb.center;
            var extents = aabb.max - center;

            var r = (extents.x * math.abs(pl.normal.x)) +
                    (extents.y * math.abs(pl.normal.y)) +
                    (extents.z * math.abs(pl.normal.z));
            var s = math.dot(pl.normal, center) - pl.distance;

            return math.abs(s) <= r;
        }

        private struct Triangle
        {
            public readonly float3 a;
            public readonly float3 b;
            public readonly float3 c;
            public readonly BoundsBurst bounds;
            public bool frontFacing;

            public Triangle(float3 a, float3 b, float3 c, float3 dir)
            {
                this.a = a;
                this.b = b;
                this.c = c;

                var cross = math.cross(b - a, c - a);
                frontFacing = math.dot(cross, dir) <= 0f;

                var min = math.min(math.min(a, b), c);
                var max = math.max(math.max(a, b), c);
                bounds = new BoundsBurst();
                bounds.SetMinMax(min, max);
            }

            public float2 GetUV(float3 p, float2 uva, float2 uvb, float2 uvc)
            {
                float u, v, w;
                Barycentric(p, out u, out v, out w);
                return (uva * u) + (uvb * v) + (uvc * w);
            }

            // https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
            public void Barycentric(float3 p, out float u, out float v, out float w)
            {
                float3 v0 = b - a, v1 = c - a, v2 = p - a;
                var d00 = math.dot(v0, v0);
                var d01 = math.dot(v0, v1);
                var d11 = math.dot(v1, v1);
                var d20 = math.dot(v2, v0);
                var d21 = math.dot(v2, v1);
                var denom = 1f / ((d00 * d11) - (d01 * d01));
                v = ((d11 * d20) - (d01 * d21)) * denom;
                w = ((d00 * d21) - (d01 * d20)) * denom;
                u = 1.0f - v - w;
            }
        }

        /*
        private static Mesh BuildMesh(Voxel_t[] voxels, float unit, bool useUV = false)
        {
            var vertices = new List<float3>();
            var uvs = new List<float2>();
            var triangles = new List<int>();
            var normals = new List<float3>();
            var centers = new List<float4>();

            var up = float3.up * unit;
            var hup = up * 0.5f;
            var hbottom = -hup;

            var right = float3.right * unit;
            var hright = right * 0.5f;

            var left = -right;
            var hleft = left * 0.5f;

            var forward = float3.forward * unit;
            var hforward = forward * 0.5f;
            var back = -forward;
            var hback = back * 0.5f;

            for (int i = 0, n = voxels.Length; i < n; i++)
            {
                var v = voxels[i];
                if (v.fill > 0)
                {
                    // back
                    CalculatePlane(vertices, normals, centers, uvs, triangles, v, useUV, hback, right, up, float3.back);

                    // right
                    CalculatePlane(vertices, normals, centers, uvs, triangles, v, useUV, hright, forward, up, float3.right);

                    // forward
                    CalculatePlane(vertices, normals, centers, uvs, triangles, v, useUV, hforward, left, up, float3.forward);

                    // left
                    CalculatePlane(vertices, normals, centers, uvs, triangles, v, useUV, hleft, back, up, float3.left);

                    // up
                    CalculatePlane(vertices, normals, centers, uvs, triangles, v, useUV, hup, right, forward, float3.up);

                    // down
                    CalculatePlane(vertices, normals, centers, uvs, triangles, v, useUV, hbottom, right, back, float3.down);
                }
            }

            var mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();
            mesh.tangents = centers.ToArray();
            mesh.SetTriangles(triangles.ToArray(), 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CalculatePlane(
            List<float3> vertices,
            List<float3> normals,
            List<float4> centers,
            List<float2> uvs,
            List<int> triangles,
            Voxel_t voxel,
            bool useUV,
            float3 offset,
            float3 right,
            float3 up,
            float3 normal,
            int rSegments = 2,
            int uSegments = 2)
        {
            var rInv = 1f / (rSegments - 1);
            var uInv = 1f / (uSegments - 1);

            var triangleOffset = vertices.Count;
            var center = voxel.position;

            var transformed = center + offset;
            for (var y = 0; y < uSegments; y++)
            {
                var ru = y * uInv;
                for (var x = 0; x < rSegments; x++)
                {
                    var rr = x * rInv;
                    vertices.Add(transformed + (right * (rr - 0.5f)) + (up * (ru - 0.5f)));
                    normals.Add(normal);
                    centers.Add(center);
                    if (useUV)
                    {
                        uvs.Add(voxel.uv);
                    }
                    else
                    {
                        uvs.Add(new float2(rr, ru));
                    }
                }

                if (y < (uSegments - 1))
                {
                    var ioffset = (y * rSegments) + triangleOffset;
                    for (int x = 0, n = rSegments - 1; x < n; x++)
                    {
                        triangles.Add(ioffset + x);
                        triangles.Add(ioffset + x + rSegments);
                        triangles.Add(ioffset + x + 1);

                        triangles.Add(ioffset + x + 1);
                        triangles.Add(ioffset + x + rSegments);
                        triangles.Add(ioffset + x + 1 + rSegments);
                    }
                }
            }
        }
        private struct Voxel_t
        {
            public float3 position;
            public float2 uv;
            public uint fill;
            public uint front;

            public bool IsFrontFace()
            {
                return (fill > 0) && (front > 0);
            }

            public bool IsBackFace()
            {
                return (fill > 0) && (front < 1);
            }

            public bool IsEmpty()
            {
                return fill < 1;
            }
        }

        */

#endregion
    }
}
