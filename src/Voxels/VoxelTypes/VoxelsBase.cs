#region

using System;
using System.Diagnostics;
using Appalachia.Core.Collections.Extensions;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Collections.Native.Pointers;
using Appalachia.Core.Collections.NonSerialized;
using Appalachia.Core.Types.Exceptions;
using Appalachia.Jobs;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Utility.Constants;
using Appalachia.Utility.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.VoxelTypes
{
    [Serializable]
    public abstract partial class VoxelsBase<TThis, TRaycastHit> : IDisposable,
                                                                   IEquatable<VoxelsBase<TThis, TRaycastHit>>
        where TThis : VoxelsBase<TThis, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        #region Fields and Autoproperties

        public Collider[] colliders;
        public float activeRatio;
        public float3 resolution;
        public int faceCount;
        public int JOB_LOOP_SIZE = JOB_SIZE._LARGE;
        public MeshRenderer[] renderers;
        public NativeArray<Voxel> voxels;
        public NativeArray3D<VoxelSamplePoint> samplePoints;
        public NativeBitArray voxelsActive;
        public NativeIntPtr voxelsActiveCount;

        public VoxelPopulationStyle style;
        protected bool _voidCenterOfMassCalculation;

        protected Bounds _rawBounds;
        protected Bounds _rawWorldBounds;
        protected Bounds _voxelBounds;
        protected Bounds _voxelWorldBounds;
        protected float _volume;
        protected float _worldVolume;
        protected float3 _centerOfMass;
        protected float3 _worldCenterOfMass;

        protected JobHandle _voxelBaseJobHandle;

        protected NativeFloatPtr nativeVolume;
        protected NativeFloatPtr nativeWorldVolume;
        protected Transform _transform;

        #endregion

        public abstract bool IsPersistent { get; }

        public Bounds rawWorldBounds
        {
            get
            {
                if (_rawWorldBounds == default)
                {
                    throw new ClassNotProperlyInitializedException(nameof(TThis), nameof(_rawWorldBounds));
                }

                return _rawWorldBounds;
            }
        }

        public Bounds voxelWorldBounds
        {
            get
            {
                if (_voxelWorldBounds == default)
                {
                    throw new ClassNotProperlyInitializedException(nameof(TThis), nameof(_voxelWorldBounds));
                }

                return _voxelWorldBounds;
            }
        }

        public float worldVolume => _worldVolume;

        public float3 worldCenterOfMass => _worldCenterOfMass;

        public float3 worldResolution => resolution * _transform.lossyScale;

        public float4x4 localToWorld
        {
            get
            {
                if (_transform == default)
                {
                    throw new ClassNotProperlyInitializedException(nameof(TThis), nameof(_transform));
                }

                return _transform.localToWorldMatrix;
            }
        }

        public float4x4 worldToLocal
        {
            get
            {
                if (_transform == default)
                {
                    throw new ClassNotProperlyInitializedException(nameof(TThis), nameof(_transform));
                }

                return _transform.worldToLocalMatrix;
            }
        }

        public int count => voxels.IsCreated ? voxels.Length : 0;

        public int size => samplePoints.IsCreated ? samplePoints.TotalLength : 0;

        public int x => samplePoints.IsCreated ? samplePoints.Length0 : 0;

        public int y => samplePoints.IsCreated ? samplePoints.Length1 : 0;

        public int z => samplePoints.IsCreated ? samplePoints.Length2 : 0;

        public Bounds rawBounds
        {
            get
            {
                if (_rawBounds == default)
                {
                    throw new ClassNotProperlyInitializedException(nameof(TThis), nameof(_rawBounds));
                }

                return _rawBounds;
            }
            internal set => _rawBounds = value;
        }

        public Bounds voxelBounds
        {
            get
            {
                if (_voxelBounds == default)
                {
                    throw new ClassNotProperlyInitializedException(nameof(TThis), nameof(_voxelBounds));
                }

                return _voxelBounds;
            }
            internal set => _voxelBounds = value;
        }

        public float volume
        {
            get => _volume;
            set => _volume = value;
        }

        public float3 centerOfMass
        {
            get => _centerOfMass;
            set
            {
                if (!_centerOfMass.Equals(value))
                {
                    _voidCenterOfMassCalculation = true;
                    _centerOfMass = value;
                }
            }
        }

        public virtual void InitializeDataStore()
        {
        }

        #region Spatial Query

        public bool TryGetSamplePointIndices(float3 localPosition, out int3 samplePointIndex)
        {
            using (_PRF_TryGetSamplePointIndices.Auto())
            {
                samplePointIndex = int3.zero;

                if (!rawBounds.Contains(localPosition))
                {
                    return false;
                }

                var shifted = localPosition - (float3)voxelBounds.min;

                var rounded = RoundVector(shifted, resolution);
                var resolved = rounded / resolution;

                samplePointIndex.x = (int)resolved.x;
                samplePointIndex.y = (int)resolved.y;
                samplePointIndex.z = (int)resolved.z;

                samplePointIndex.x -= 1;
                samplePointIndex.y -= 1;
                samplePointIndex.z -= 1;

                samplePointIndex = math.clamp(samplePointIndex, int3.zero, samplePoints.Length - int3c.one);

                return true;
            }
        }

        #endregion

        #region Profiling

        private const string _PRF_PFX = nameof(VoxelsBase<TThis, TRaycastHit>) + ".";

        private static readonly TRaycastHit[] _empty = new TRaycastHit[0];
        private static readonly NonSerializedList<TRaycastHit> _hits = new();
        private static readonly NonSerializedList<int3> _traversedVoxels = new();
        private static readonly ProfilerMarker _PRF_Initialize = new(_PRF_PFX + nameof(Initialize));

        private static readonly ProfilerMarker _PRF_Synchronize = new(_PRF_PFX + nameof(Synchronize));

        private static readonly ProfilerMarker _PRF_UpdateBounds = new(_PRF_PFX + nameof(UpdateBounds));

        private static readonly ProfilerMarker _PRF_SynchronizeBounds =
            new(_PRF_PFX + nameof(SynchronizeBounds));

        private static readonly ProfilerMarker _PRF_UpdateVoxelActiveRatio =
            new(_PRF_PFX + nameof(UpdateVoxelActiveRatio));

        private static readonly ProfilerMarker _PRF_UpdateVoxelActiveRatio_CreateUpdateActiveJob =
            new(_PRF_PFX + nameof(UpdateVoxelActiveRatio) + ".CreateUpdateActiveJob");

        private static readonly ProfilerMarker _PRF_SetupPhysical = new(_PRF_PFX + nameof(SetupPhysical));

        private static readonly ProfilerMarker _PRF_SetupPhysical_CreateCenterOfMassSetupJob =
            new(_PRF_PFX + nameof(SetupPhysical) + ".CreateCenterOfMassSetupJob");

        private static readonly ProfilerMarker _PRF_UpdatePhysical = new(_PRF_PFX + nameof(UpdatePhysical));

        private static readonly ProfilerMarker _PRF_UpdatePhysical_CompleteVoxelToWorldJob =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".CompleteVoxelToWorldJob");

        private static readonly ProfilerMarker _PRF_UpdatePhysical_UpdateVolume =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".UpdateVolume");

        private static readonly ProfilerMarker _PRF_UpdatePhysical_CreateVoxelToWorldJob =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".CreateVoxelToWorldJob");

        private static readonly ProfilerMarker _PRF_UpdatePhysical_CreateVoxelVolumeUpdateJob =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".CreateVoxeVolumeUpdateJob");

        private static readonly ProfilerMarker _PRF_UpdatePhysical_ScheduleBatchedJobs =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".ScheduleBatchedJobs");

        private static readonly ProfilerMarker _PRF_UpdatePhysical_UpdateVolumeParams =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".UpdateVolumeParams");

        private static readonly ProfilerMarker _PRF_UpdatePhysical_CreateCenterOfMassSetupJob =
            new(_PRF_PFX + nameof(UpdatePhysical) + ".CreateCenterOfMassSetupJob");

        private static readonly ProfilerMarker _PRF_CompletePhysical =
            new(_PRF_PFX + nameof(CompletePhysical));

        private static readonly ProfilerMarker _PRF_TryGetSamplePointIndices =
            new(_PRF_PFX + nameof(TryGetSamplePointIndices));

        private static readonly ProfilerMarker _PRF_Raycast = new(_PRF_PFX + nameof(Raycast));
        private static readonly ProfilerMarker _PRF_Linecast = new(_PRF_PFX + nameof(Linecast));

        private static readonly ProfilerMarker _PRF_RaycastNonAlloc = new(_PRF_PFX + nameof(RaycastNonAlloc));

        private static readonly ProfilerMarker _PRF_RaycastAll = new(_PRF_PFX + nameof(RaycastAll));

        private static readonly ProfilerMarker _PRF_Internal_Raycast =
            new(_PRF_PFX + nameof(Internal_Raycast));

        private static readonly ProfilerMarker _PRF_Internal_RaycastNonAlloc =
            new(_PRF_PFX + nameof(Internal_RaycastNonAlloc));

        private static readonly ProfilerMarker _PRF_Internal_RaycastAll =
            new(_PRF_PFX + nameof(Internal_RaycastAll));

        private static readonly ProfilerMarker _PRF_RoundVector = new(_PRF_PFX + nameof(RoundVector));

        private static readonly ProfilerMarker _PRF_ShouldCastRay = new(_PRF_PFX + nameof(ShouldCastRay));

        private static readonly ProfilerMarker _PRF_GetTraversedVoxels =
            new(_PRF_PFX + nameof(GetTraversedVoxels));

        private static readonly ProfilerMarker _PRF_RoundValue = new(_PRF_PFX + nameof(RoundValue));

        #endregion

        #region General

        public void Initialize(Transform t)
        {
            using (_PRF_Initialize.Auto())
            {
                _transform = t;
                Synchronize();

                OnInitialize();
            }
        }

        public virtual void OnInitialize()
        {
            if (voxelsActiveCount.IsCreated)
            {
                voxelsActiveCount.SafeDispose();
            }

            voxelsActiveCount = new NativeIntPtr(Allocator.Persistent);
        }

        public virtual void InitializeElements(int elementCount)
        {
            if (voxelsActive.IsCreated)
            {
                voxelsActive.SafeDispose();
            }

            voxelsActive = new NativeBitArray(elementCount, Allocator.Persistent);

            voxelsActive.SetAllBits(true);
        }

        public void Synchronize()
        {
            using (_PRF_Synchronize.Auto())
            {
                SynchronizeBounds();
            }
        }

        public void UpdateBounds(Bounds raw, Bounds voxel)
        {
            using (_PRF_UpdateBounds.Auto())
            {
                _rawBounds = raw;
                _voxelBounds = voxel;
                SynchronizeBounds();
            }
        }

        private void SynchronizeBounds()
        {
            using (_PRF_SynchronizeBounds.Auto())
            {
                var rawCenter = localToWorld.MultiplyPoint3x4(_rawBounds.center);
                var rawMin = localToWorld.MultiplyPoint3x4(_rawBounds.min);
                var rawMax = localToWorld.MultiplyPoint3x4(_rawBounds.max);
                _rawWorldBounds = new Bounds(rawCenter, rawMax - rawMin);

                var voxelCenter = localToWorld.MultiplyPoint3x4(_voxelBounds.center);
                var voxelMin = localToWorld.MultiplyPoint3x4(_voxelBounds.min);
                var voxelMax = localToWorld.MultiplyPoint3x4(_voxelBounds.max);
                _voxelWorldBounds = new Bounds(voxelCenter, voxelMax - voxelMin);
            }
        }

        protected static float RoundValue(float value, float resolution)
        {
            using (_PRF_RoundValue.Auto())
            {
                return Mathf.Ceil(value / resolution) * resolution;
            }
        }

        protected static float3 RoundVector(float3 vector, float3 resolution)
        {
            using (_PRF_RoundVector.Auto())
            {
                return new Vector3(
                    RoundValue(vector.x, resolution.x),
                    RoundValue(vector.y, resolution.y),
                    RoundValue(vector.z, resolution.z)
                );
            }
        }

        protected void CalculateVoxelActiveRatio()
        {
            activeRatio = voxelsActiveCount.Value / (float)count;
        }

        public JobHandle UpdateVoxelActiveRatio(float ratio)
        {
            using (_PRF_UpdateVoxelActiveRatio.Auto())
            {
                activeRatio = math.clamp(activeRatio, 0.001f, 1f);
                ratio = math.clamp(ratio,             0.001f, 1f);

                if (Math.Abs(activeRatio - ratio) < float.Epsilon)
                {
                    return default;
                }

                activeRatio = ratio;

                if (!voxelsActiveCount.IsCreated)
                {
                    voxelsActiveCount = new NativeIntPtr(Allocator.Persistent);
                }

                voxelsActiveCount.Value = 0;

                using (_PRF_UpdateVoxelActiveRatio_CreateUpdateActiveJob.Auto())
                {
                    _voxelBaseJobHandle = new UpdateActiveJob
                    {
                        voxels = voxels,
                        voxelsActive = voxelsActive,
                        activeRatio = activeRatio,
                        voxelsActiveCount = voxelsActiveCount
                    }.Schedule(_voxelBaseJobHandle);
                }

                return _voxelBaseJobHandle;
            }
        }

        [BurstCompile]
        protected struct UpdateActiveJob : IJob
        {
            #region Fields and Autoproperties

            public float activeRatio;
            public NativeArray<Voxel> voxels;
            public NativeBitArray voxelsActive;
            public NativeIntPtr voxelsActiveCount;

            #endregion

            #region IJob Members

            public void Execute()
            {
                for (var index = 0; index < voxels.Length; index++)
                {
                    var voxel = voxels[index];

                    var active = voxel.normalizedDistanceToCenterOfMass < activeRatio;

                    voxelsActive.Set(index, active);

                    if (active)
                    {
                        voxelsActiveCount.Value += 1;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Voxel To World Job

        public JobHandle SetupPhysical()
        {
            using (_PRF_SetupPhysical.Auto())
            {
                Synchronize();

                _voxelBaseJobHandle = UpdatePhysical(0f);

                using (_PRF_SetupPhysical_CreateCenterOfMassSetupJob.Auto())
                {
                    _voxelBaseJobHandle =
                        new CenterOfMassSetupJob { voxels = voxels, centerOfMass = centerOfMass }.Schedule(
                            _voxelBaseJobHandle
                        );
                }

                return _voxelBaseJobHandle;
            }
        }

        public JobHandle UpdatePhysical(float deltaTime)
        {
            using (_PRF_UpdatePhysical.Auto())
            {
                using (_PRF_UpdatePhysical_CompleteVoxelToWorldJob.Auto())
                {
                    _voxelBaseJobHandle.Complete();
                }

                using (_PRF_UpdatePhysical_UpdateVolume.Auto())
                {
                    _volume = nativeVolume.IsCreated ? nativeVolume.Value : 0f;
                    _worldVolume = nativeWorldVolume.IsCreated ? nativeWorldVolume.Value : 0f;
                }

                using (_PRF_UpdatePhysical_CreateVoxelToWorldJob.Auto())
                {
                    _voxelBaseJobHandle = new VoxelWorldPositionUpdateJob
                    {
                        voxels = voxels,
                        voxelsActive = voxelsActive,
                        matrix = localToWorld,
                        resolution = resolution,
                        worldResolution = worldResolution,
                        deltaTime = deltaTime
                    }.Schedule();
                }

                _worldCenterOfMass = localToWorld.MultiplyPoint3x4(_centerOfMass);

                if (_voidCenterOfMassCalculation)
                {
                    using (_PRF_UpdatePhysical_CreateCenterOfMassSetupJob.Auto())
                    {
                        _voxelBaseJobHandle =
                            new CenterOfMassSetupJob { voxels = voxels, centerOfMass = centerOfMass }
                               .Schedule(_voxelBaseJobHandle);
                    }
                }

                using (_PRF_UpdatePhysical_UpdateVolumeParams.Auto())
                {
                    if (!nativeVolume.IsCreated)
                    {
                        nativeVolume = new NativeFloatPtr(Allocator.Persistent);
                    }
                    else
                    {
                        nativeVolume.Value = 0f;
                    }

                    if (!nativeWorldVolume.IsCreated)
                    {
                        nativeWorldVolume = new NativeFloatPtr(Allocator.Persistent);
                    }
                    else
                    {
                        nativeWorldVolume.Value = 0f;
                    }
                }

                using (_PRF_UpdatePhysical_CreateVoxelVolumeUpdateJob.Auto())
                {
                    _voxelBaseJobHandle = new VoxelVolumeUpdateJob
                    {
                        voxels = voxels,
                        voxelsActive = voxelsActive,
                        volume = nativeVolume.GetParallel(),
                        worldVolume = nativeWorldVolume.GetParallel()
                    }.Schedule(voxels.Length, JOB_LOOP_SIZE, _voxelBaseJobHandle);
                }

                using (_PRF_UpdatePhysical_ScheduleBatchedJobs.Auto())
                {
                    JobHandle.ScheduleBatchedJobs();
                }

                return _voxelBaseJobHandle;
            }
        }

        public void CompletePhysical()
        {
            using (_PRF_CompletePhysical.Auto())
            {
                _voxelBaseJobHandle.Complete();
            }
        }

        [BurstCompile]
        protected struct VoxelWorldPositionUpdateJob : IJob
        {
            #region Fields and Autoproperties

            [ReadOnly] public float deltaTime;
            [ReadOnly] public float3 resolution;
            [ReadOnly] public float3 worldResolution;
            [ReadOnly] public float4x4 matrix;

            public NativeArray<Voxel> voxels;
            [ReadOnly] public NativeBitArray voxelsActive;

            #endregion

            #region IJob Members

            public void Execute()
            {
                var inverseMatrix = matrix.Inverse();

                for (var i = 0; i < voxels.Length; i++)
                {
                    if (!voxelsActive.IsSet(i))
                    {
                        continue;
                    }

                    var voxel = voxels[i];

                    voxel.UpdatePhysical(matrix, inverseMatrix, resolution, worldResolution, deltaTime);

                    voxels[i] = voxel;
                }
            }

            #endregion
        }

        [BurstCompile]
        protected struct CenterOfMassSetupJob : IJob
        {
            #region Fields and Autoproperties

            public float3 centerOfMass;
            public NativeArray<Voxel> voxels;

            #endregion

            #region IJob Members

            public void Execute()
            {
                var maxDistance = float.MinValue;
                var minDistance = float.MaxValue;

                for (var i = 0; i < voxels.Length; i++)
                {
                    var voxel = voxels[i];

                    voxel.distanceToCenterOfMass = math.distance(voxel.position, centerOfMass);

                    maxDistance = math.max(voxel.distanceToCenterOfMass, maxDistance);
                    minDistance = math.min(voxel.distanceToCenterOfMass, minDistance);
                }

                var denom = math.clamp(maxDistance - minDistance, 0f, 5000f);

                if (denom <= 0)
                {
                    denom = 1;
                }

                for (var i = 0; i < voxels.Length; i++)
                {
                    var voxel = voxels[i];

                    voxel.normalizedDistanceToCenterOfMass =
                        (voxel.distanceToCenterOfMass - minDistance) / denom;
                }
            }

            #endregion
        }

        [BurstCompile]
        protected struct VoxelVolumeUpdateJob : IJobParallelFor
        {
            #region Fields and Autoproperties

            [ReadOnly] public NativeArray<Voxel> voxels;
            [ReadOnly] public NativeBitArray voxelsActive;
            public NativeFloatPtr.Parallel volume;
            public NativeFloatPtr.Parallel worldVolume;

            #endregion

            #region IJobParallelFor Members

            public void Execute(int index)
            {
                if (!voxelsActive.IsSet(index))
                {
                    return;
                }

                var vox = voxels[index];

                volume.Add(vox.volume);
                worldVolume.Add(vox.worldVolume);
            }

            #endregion
        }

        #endregion

        #region Raycast API

        public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(origin, direction, out _, maxDistance);
            }
        }

        public bool Raycast(Vector3 origin, Vector3 direction)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(origin, direction, out _, float.PositiveInfinity);
            }
        }

        public bool Raycast(Vector3 origin, Vector3 direction, out TRaycastHit hitInfo)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(origin, direction, out hitInfo, float.PositiveInfinity);
            }
        }

        public bool Raycast(Vector3 origin, Vector3 direction, out TRaycastHit hitInfo, float maxDistance)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(origin, direction, out hitInfo, maxDistance);
            }
        }

        public bool Raycast(Ray ray)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(ray.origin, ray.direction, out _, float.PositiveInfinity);
            }
        }

        public bool Raycast(Ray ray, float maxDistance)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(ray.origin, ray.direction, out _, maxDistance);
            }
        }

        public bool Raycast(Ray ray, out TRaycastHit hitInfo)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(ray.origin, ray.direction, out hitInfo, float.PositiveInfinity);
            }
        }

        public bool Raycast(Ray ray, out TRaycastHit hitInfo, float maxDistance)
        {
            using (_PRF_Raycast.Auto())
            {
                return Internal_Raycast(ray.origin, ray.direction, out hitInfo, maxDistance);
            }
        }

        public bool Linecast(Vector3 start, Vector3 end, out TRaycastHit hitInfo)
        {
            using (_PRF_Linecast.Auto())
            {
                var rayVector = end - start;
                return Internal_Raycast(start, end, out hitInfo, math.length(rayVector));
            }
        }

        public bool Linecast(Vector3 start, Vector3 end)
        {
            using (_PRF_Linecast.Auto())
            {
                var rayVector = end - start;
                return Internal_Raycast(start, end, out _, math.length(rayVector));
            }
        }

        public TRaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance)
        {
            using (_PRF_RaycastAll.Auto())
            {
                return Internal_RaycastAll(origin, direction, maxDistance);
            }
        }

        public TRaycastHit[] RaycastAll(Vector3 origin, Vector3 direction)
        {
            using (_PRF_RaycastAll.Auto())
            {
                return Internal_RaycastAll(origin, direction, float.PositiveInfinity);
            }
        }

        public TRaycastHit[] RaycastAll(Ray ray, float maxDistance)
        {
            using (_PRF_RaycastAll.Auto())
            {
                return Internal_RaycastAll(ray.origin, ray.direction, maxDistance);
            }
        }

        public TRaycastHit[] RaycastAll(Ray ray)
        {
            using (_PRF_RaycastAll.Auto())
            {
                return Internal_RaycastAll(ray.origin, ray.direction, float.PositiveInfinity);
            }
        }

        public int RaycastNonAlloc(Ray ray, TRaycastHit[] results, float maxDistance)
        {
            using (_PRF_RaycastNonAlloc.Auto())
            {
                return Internal_RaycastNonAlloc(ray.origin, ray.direction, results, maxDistance);
            }
        }

        public int RaycastNonAlloc(Ray ray, TRaycastHit[] results)
        {
            using (_PRF_RaycastNonAlloc.Auto())
            {
                return Internal_RaycastNonAlloc(ray.origin, ray.direction, results, float.PositiveInfinity);
            }
        }

        public int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            TRaycastHit[] results,
            float maxDistance)
        {
            using (_PRF_RaycastNonAlloc.Auto())
            {
                return Internal_RaycastNonAlloc(origin, direction, results, maxDistance);
            }
        }

        public int RaycastNonAlloc(Vector3 origin, Vector3 direction, TRaycastHit[] results)
        {
            using (_PRF_RaycastNonAlloc.Auto())
            {
                return Internal_RaycastNonAlloc(origin, direction, results, float.PositiveInfinity);
            }
        }

        #endregion

        #region Raycast Internals

        protected TRaycastHit[] Internal_RaycastAll(float3 origin, float3 direction, float maxDistance)
        {
            using (_PRF_Internal_RaycastAll.Auto())
            {
                var traversedVoxelCoordinates = GetTraversedVoxels(origin, direction);

                if ((traversedVoxelCoordinates == null) || (traversedVoxelCoordinates.Count == 0))
                {
                    return _empty;
                }

                var resultCount = 0;

                for (var i = 0; i < traversedVoxelCoordinates.Count; i++)
                {
                    var voxelCoordinates = traversedVoxelCoordinates[i];
                    var samplePoint = samplePoints[voxelCoordinates];
                    var voxelIndex = samplePoint.index;

                    var voxel = voxels[voxelIndex];
                    var distance = math.distance(voxel.worldPosition.value, origin);

                    if (distance > maxDistance)
                    {
                        break;
                    }

                    resultCount += 1;
                }

                if (resultCount == 0)
                {
                    return _empty;
                }

                var results = new TRaycastHit[resultCount];
                var resultIndex = 0;

                for (var i = 0; i < traversedVoxelCoordinates.Count; i++)
                {
                    var voxelCoordinates = traversedVoxelCoordinates[i];
                    var samplePoint = samplePoints[voxelCoordinates];
                    var voxelIndex = samplePoint.index;

                    var voxel = voxels[voxelIndex];
                    var distance = math.distance(voxel.worldPosition.value, origin);

                    if (distance > maxDistance)
                    {
                        break;
                    }

                    results[resultIndex] = PrepareRaycastHit(voxelIndex, voxel, distance);

                    resultIndex += 1;
                }

                return results;
            }
        }

        protected int Internal_RaycastNonAlloc(
            float3 origin,
            float3 direction,
            TRaycastHit[] results,
            float maxDistance)
        {
            using (_PRF_Internal_RaycastNonAlloc.Auto())
            {
                var traversedVoxelCoordinates = GetTraversedVoxels(origin, direction);

                if ((traversedVoxelCoordinates == null) || (traversedVoxelCoordinates.Count == 0))
                {
                    return 0;
                }

                var resultCount = 0;

                for (var i = 0; i < traversedVoxelCoordinates.Count; i++)
                {
                    var voxelCoordinates = traversedVoxelCoordinates[i];
                    var samplePoint = samplePoints[voxelCoordinates];
                    var voxelIndex = samplePoint.index;

                    var voxel = voxels[voxelIndex];
                    var distance = math.distance(voxel.worldPosition.value, origin);

                    if (distance > maxDistance)
                    {
                        break;
                    }

                    results[resultCount] = PrepareRaycastHit(voxelIndex, voxel, distance);

                    resultCount += 1;

                    if (resultCount >= results.Length)
                    {
                        break;
                    }
                }

                return resultCount;
            }
        }

        protected bool Internal_Raycast(
            float3 origin,
            float3 direction,
            out TRaycastHit hitInfo,
            float maxDistance)
        {
            using (_PRF_Internal_Raycast.Auto())
            {
                var traversedVoxelCoordinates = GetTraversedVoxels(origin, direction);

                for (var i = 0; i < traversedVoxelCoordinates.Count;)
                {
                    var voxelCoordinates = traversedVoxelCoordinates[i];
                    var samplePoint = samplePoints[voxelCoordinates];
                    var voxelIndex = samplePoint.index;

                    var voxel = voxels[voxelIndex];
                    var distance = math.distance(voxel.worldPosition.value, origin);

                    if (distance > maxDistance)
                    {
                        hitInfo = default;
                        return false;
                    }

                    hitInfo = PrepareRaycastHit(voxelIndex, voxel, distance);

                    return true;
                }

                hitInfo = default;
                return false;
            }
        }

        protected abstract TRaycastHit PrepareRaycastHit(int voxelIndex, Voxel voxel, float distance);

        protected bool ShouldCastRay(
            float3 origin,
            float3 direction,
            out float3 localRayStart,
            out float3 localRayDirection,
            out float rayDistance)
        {
            using (_PRF_ShouldCastRay.Auto())
            {
                var length = math.length(direction);

                if (length <= float.Epsilon)
                {
                    localRayStart = float3.zero;
                    localRayDirection = float3.zero;
                    rayDistance = 0f;
                    return false;
                }

                direction /= length;

                localRayStart = worldToLocal.MultiplyPoint3x4(origin);
                localRayDirection = worldToLocal.MultiplyVector(direction);

                if (!rawBounds.IntersectRay(
                        new Ray(localRayStart, localRayDirection),
                        out var distanceToBounds
                    ))
                {
                    localRayStart = float3.zero;
                    localRayDirection = float3.zero;
                    rayDistance = 0f;
                    return false;
                }

                localRayStart = RoundVector(
                    localRayStart + (localRayDirection * distanceToBounds),
                    resolution
                );

                var maximumIntersectingRayLength = math.distance(voxelBounds.max, voxelBounds.min);

                var oppositeSideRayStart = localRayStart +
                                           (localRayDirection * maximumIntersectingRayLength * -1.5f);

                if (!rawBounds.IntersectRay(
                        new Ray(oppositeSideRayStart, -localRayDirection),
                        out var oppositeDistanceToBounds
                    ))
                {
                    localRayStart = float3.zero;
                    localRayDirection = float3.zero;
                    rayDistance = 0f;
                    return false;
                }

                var rayEnd = oppositeSideRayStart * (-localRayDirection * oppositeDistanceToBounds);

                rayDistance = math.distance(localRayStart, rayEnd);
                rayDistance = math.clamp(rayDistance, 0f, maximumIntersectingRayLength);

                return true;
            }
        }

        protected NonSerializedList<int3> GetTraversedVoxels(float3 o, float3 v)
        {
            using (_PRF_GetTraversedVoxels.Auto())
            {
                _traversedVoxels.ClearFast();

                if (!ShouldCastRay(o, v, out var rayStart, out var rayDirection, out var rayDistance))
                {
                    return _traversedVoxels;
                }

                var rayEnd = RoundVector(rayStart + (rayDirection * rayDistance), resolution);

                // This id of the first/current voxel hit by the ray.
                // Using floor (round down) is actually very important,
                // the implicit int-casting will round up for negative numbers.
                var currentVoxelCoordinate = new int3(
                    (int)math.floor(rayStart.x / resolution.x),
                    (int)math.floor(rayStart.y / resolution.y),
                    (int)math.floor(rayStart.z / resolution.z)
                );

                // The id of the last voxel hit by the ray.
                var lastVoxelCoordinate = new int3(
                    (int)math.floor(rayEnd.x / resolution.x),
                    (int)math.floor(rayEnd.y / resolution.y),
                    (int)math.floor(rayEnd.z / resolution.z)
                );

                // In which direction the voxel ids are incremented.
                var step = new int3(
                    rayDirection.x >= 0 ? 1 : -1,
                    rayDirection.y >= 0 ? 1 : -1,
                    rayDirection.z >= 0 ? 1 : -1
                );

                // Distance along the ray to the next voxel border from the current position (tMaxX, tMaxY, tMaxZ).
                var nextVoxelBoundary = new float3(
                    (currentVoxelCoordinate.x + step.x) * resolution.x,
                    (currentVoxelCoordinate.y + step.y) * resolution.y,
                    (currentVoxelCoordinate.z + step.z) * resolution.z
                );

                // tMaxX, tMaxY, tMaxZ -- distance until next intersection with voxel-border
                // the value of t at which the ray crosses the first vertical voxel boundary
                var maximumTraversal = new float3(
                    math.abs(rayDirection.x) > float.Epsilon
                        ? (nextVoxelBoundary.x - rayStart.x) / rayDirection.x
                        : float.MaxValue,
                    math.abs(rayDirection.y) > float.Epsilon
                        ? (nextVoxelBoundary.y - rayStart.y) / rayDirection.y
                        : float.MaxValue,
                    math.abs(rayDirection.z) > float.Epsilon
                        ? (nextVoxelBoundary.z - rayStart.z) / rayDirection.z
                        : float.MaxValue
                );

                // tDeltaX, tDeltaY, tDeltaZ --
                // how far along the ray we must move for the horizontal component to equal the width of a voxel
                // the direction in which we traverse the grid
                // can only be FLT_MAX if we never go in that direction
                var traversalDelta = new float3(
                    math.abs(rayDirection.x) > float.Epsilon
                        ? (resolution.x / rayDirection.x) * step.x
                        : float.MaxValue,
                    math.abs(rayDirection.y) > float.Epsilon
                        ? (resolution.y / rayDirection.y) * step.y
                        : float.MaxValue,
                    math.abs(rayDirection.z) > float.Epsilon
                        ? (resolution.z / rayDirection.z) * step.z
                        : float.MaxValue
                );

                var diff = int3.zero;

                var neg_ray = false;
                if ((currentVoxelCoordinate.x != lastVoxelCoordinate.x) && (rayDirection.x < 0))
                {
                    diff.x -= 1;
                    neg_ray = true;
                }

                if ((currentVoxelCoordinate.y != lastVoxelCoordinate.y) && (rayDirection.y < 0))
                {
                    diff.y -= 1;
                    neg_ray = true;
                }

                if ((currentVoxelCoordinate.z != lastVoxelCoordinate.z) && (rayDirection.z < 0))
                {
                    diff.z -= 1;
                    neg_ray = true;
                }

                _traversedVoxels.Add(currentVoxelCoordinate);

                if (neg_ray)
                {
                    currentVoxelCoordinate += diff;
                    _traversedVoxels.Add(currentVoxelCoordinate);
                }

                while (!lastVoxelCoordinate.Equals(currentVoxelCoordinate))
                {
                    if (maximumTraversal.x < maximumTraversal.y)
                    {
                        if (maximumTraversal.x < maximumTraversal.z)
                        {
                            currentVoxelCoordinate.x += step.x;
                            maximumTraversal.x += traversalDelta.x;
                        }
                        else
                        {
                            currentVoxelCoordinate.z += step.z;
                            maximumTraversal.z += traversalDelta.z;
                        }
                    }
                    else
                    {
                        if (maximumTraversal.y < maximumTraversal.z)
                        {
                            currentVoxelCoordinate.y += step.y;
                            maximumTraversal.y += traversalDelta.y;
                        }
                        else
                        {
                            currentVoxelCoordinate.z += step.z;
                            maximumTraversal.z += traversalDelta.z;
                        }
                    }

                    _traversedVoxels.Add(currentVoxelCoordinate);
                }

                return _traversedVoxels;
            }
        }

        #endregion

        #region IEquatable

        [DebuggerStepThrough]
        public bool Equals(VoxelsBase<TThis, TRaycastHit> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(_transform, other._transform) &&
                   _rawBounds.Equals(other._rawBounds) &&
                   _rawWorldBounds.Equals(other._rawWorldBounds) &&
                   _voxelBounds.Equals(other._voxelBounds) &&
                   _voxelWorldBounds.Equals(other._voxelWorldBounds) &&
                   _volume.Equals(other._volume) &&
                   _worldVolume.Equals(other._worldVolume) &&
                   _worldCenterOfMass.Equals(other._worldCenterOfMass) &&
                   _centerOfMass.Equals(other._centerOfMass) &&
                   (_voidCenterOfMassCalculation == other._voidCenterOfMassCalculation) &&
                   (style == other.style) &&
                   (faceCount == other.faceCount) &&
                   resolution.Equals(other.resolution) &&
                   nativeVolume.Equals(other.nativeVolume) &&
                   nativeWorldVolume.Equals(other.nativeWorldVolume) &&
                   samplePoints.Equals(other.samplePoints) &&
                   voxels.Equals(other.voxels) &&
                   voxelsActive.Equals(other.voxelsActive) &&
                   voxelsActiveCount.Equals(other.voxelsActiveCount) &&
                   activeRatio.Equals(other.activeRatio) &&
                   _voxelBaseJobHandle.Equals(other._voxelBaseJobHandle) &&
                   (JOB_LOOP_SIZE == other.JOB_LOOP_SIZE)
#if UNITY_EDITOR
                 &&
                   _gizmo_voxelBounds.Equals(other._gizmo_voxelBounds) &&
                   Equals(
                       _gizmo_voxelBoundsSubdivisionLineSegments,
                       other._gizmo_voxelBoundsSubdivisionLineSegments
                   ) &&
                   Equals(_testHits, other._testHits)
#endif
                ;
        }

        [DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((VoxelsBase<TThis, TRaycastHit>)obj);
        }

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _transform != null ? _transform.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ _rawBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ _rawWorldBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ _voxelBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ _voxelWorldBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ _volume.GetHashCode();
                hashCode = (hashCode * 397) ^ _worldVolume.GetHashCode();
                hashCode = (hashCode * 397) ^ _worldCenterOfMass.GetHashCode();
                hashCode = (hashCode * 397) ^ _centerOfMass.GetHashCode();
                hashCode = (hashCode * 397) ^ _voidCenterOfMassCalculation.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)style;
                hashCode = (hashCode * 397) ^ faceCount;
                hashCode = (hashCode * 397) ^ resolution.GetHashCode();
                hashCode = (hashCode * 397) ^ nativeVolume.GetHashCode();
                hashCode = (hashCode * 397) ^ nativeWorldVolume.GetHashCode();
                hashCode = (hashCode * 397) ^ samplePoints.GetHashCode();
                hashCode = (hashCode * 397) ^ voxels.GetHashCode();
                hashCode = (hashCode * 397) ^ voxelsActive.GetHashCode();
                hashCode = (hashCode * 397) ^ voxelsActiveCount.GetHashCode();
                hashCode = (hashCode * 397) ^ activeRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ _voxelBaseJobHandle.GetHashCode();
                hashCode = (hashCode * 397) ^ JOB_LOOP_SIZE;
#if UNITY_EDITOR
                hashCode = (hashCode * 397) ^ _gizmo_voxelBounds.GetHashCode();
                hashCode = (hashCode * 397) ^
                           (_gizmo_voxelBoundsSubdivisionLineSegments != null
                               ? _gizmo_voxelBoundsSubdivisionLineSegments.GetHashCode()
                               : 0);
                hashCode = (hashCode * 397) ^ (_testHits != null ? _testHits.GetHashCode() : 0);
#endif
                return hashCode;
            }
        }

        [DebuggerStepThrough]
        public static bool operator ==(
            VoxelsBase<TThis, TRaycastHit> left,
            VoxelsBase<TThis, TRaycastHit> right)
        {
            return Equals(left, right);
        }

        [DebuggerStepThrough]
        public static bool operator !=(
            VoxelsBase<TThis, TRaycastHit> left,
            VoxelsBase<TThis, TRaycastHit> right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                samplePoints.SafeDispose();
                voxels.SafeDispose();
                voxelsActive.SafeDispose();
                voxelsActiveCount.SafeDispose();
                nativeVolume.SafeDispose();
                nativeWorldVolume.SafeDispose();

                _transform = null;

#if UNITY_EDITOR
                _gizmo_voxelBoundsSubdivisionLineSegments = default;
#endif
                _rawBounds = default;
                _rawWorldBounds = default;
                _voxelBounds = default;
                _voxelWorldBounds = default;
                faceCount = default;
                resolution = default;
                _voxelBaseJobHandle = default;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                Dispose();
            }
        }

        #endregion

        #region ISerializationCallbackReceiver

/*
        [SerializeField] private VoxelSamplePoint[] __samplePoints;
        [SerializeField] private int3 __samplePointsDimensions;
        [SerializeField] private Voxel[] __voxels;
        
        public void OnBeforeSerialize()
        {
            if (samplePoints.IsCreated)
            {
                __samplePoints = samplePoints.ToArrayFlat(out __samplePointsDimensions);                
            }

            if (voxels.IsCreated)
            {
                __voxels = voxels.ToArray();                
            }
        }

        public void OnAfterDeserialize()
        {
                APPADEBUG.SERIALIZING = true;
            if (__samplePoints != null && __samplePoints.Length > 0)
            {
                samplePoints.SafeDispose();
                samplePoints = new NativeArray3D<VoxelSamplePoint>(__samplePoints, __samplePointsDimensions, Allocator.Persistent);                
            }

            if (__voxels != null && __voxels.Length > 0)
            {
                voxels.SafeDispose();
                voxels = new NativeArray<Voxel>(__voxels, Allocator.Persistent); 
                voxelsActive = new NativeBitArray(__voxels.Length, Allocator.Persistent);               
            }
        }
        */

        #endregion

        #region Algorithms

/*
* https://github.com/francisengelmann/fast_voxel_traversal/blob/master/main.cpp
// C/C++ includes
#include <cfloat>
#include <vector>
#include <iostream>

//Eigen includes
#include <Eigen/Core>

double _bin_size = 1;

 /\*
 * @brief returns all the voxels that are traversed by a ray going from start to end
 * @param start : continous world position where the ray starts
 * @param end   : continous world position where the ray end
 * @return vector of voxel ids hit by the ray in temporal order
 *
 * J. Amanatides, A. Woo. A Fast Voxel Traversal Algorithm for Ray Tracing. Eurographics '87
 *\/

std::vector<Eigen::Vector3i> voxel_traversal(Eigen::Vector3d ray_start, Eigen::Vector3d ray_end) {
  std::vector<Eigen::Vector3i> visited_voxels;

  // This id of the first/current voxel hit by the ray.
  // Using floor (round down) is actually very important,
  // the implicit int-casting will round up for negative numbers.
  Eigen::Vector3i current_voxel(std::floor(ray_start[0]/_bin_size),
                                std::floor(ray_start[1]/_bin_size),
                                std::floor(ray_start[2]/_bin_size));

  // The id of the last voxel hit by the ray.
  // TODO: what happens if the end point is on a border?
  Eigen::Vector3i last_voxel(std::floor(ray_end[0]/_bin_size),
                             std::floor(ray_end[1]/_bin_size),
                             std::floor(ray_end[2]/_bin_size));

  // Compute normalized ray direction.
  Eigen::Vector3d ray = ray_end-ray_start;
  //ray.normalize();

  // In which direction the voxel ids are incremented.
  double stepX = (ray.x >= 0) ? 1:-1; // correct
  double stepY = (ray[1] >= 0) ? 1:-1; // correct
  double stepZ = (ray[2] >= 0) ? 1:-1; // correct

  // Distance along the ray to the next voxel border from the current position (tMaxX, tMaxY, tMaxZ).
  double next_voxel_boundary_x = (current_voxel[0]+stepX)*_bin_size; // correct
  double next_voxel_boundary_y = (current_voxel[1]+stepY)*_bin_size; // correct
  double next_voxel_boundary_z = (current_voxel[2]+stepZ)*_bin_size; // correct

  // tMaxX, tMaxY, tMaxZ -- distance until next intersection with voxel-border
  // the value of t at which the ray crosses the first vertical voxel boundary
  double tMaxX = (ray[0]!=0) ? (next_voxel_boundary_x - ray_start[0])/ray[0] : DBL_MAX; //
  double tMaxY = (ray[1]!=0) ? (next_voxel_boundary_y - ray_start[1])/ray[1] : DBL_MAX; //
  double tMaxZ = (ray[2]!=0) ? (next_voxel_boundary_z - ray_start[2])/ray[2] : DBL_MAX; //

  // tDeltaX, tDeltaY, tDeltaZ --
  // how far along the ray we must move for the horizontal component to equal the width of a voxel
  // the direction in which we traverse the grid
  // can only be FLT_MAX if we never go in that direction
  double tDeltaX = (ray[0]!=0) ? _bin_size/ray[0]*stepX : DBL_MAX;
  double tDeltaY = (ray[1]!=0) ? _bin_size/ray[1]*stepY : DBL_MAX;
  double tDeltaZ = (ray[2]!=0) ? _bin_size/ray[2]*stepZ : DBL_MAX;

  Eigen::Vector3i diff(0,0,0);
  bool neg_ray=false;
  if (current_voxel[0]!=last_voxel[0] && ray[0]<0) { diff[0]--; neg_ray=true; }
  if (current_voxel[1]!=last_voxel[1] && ray[1]<0) { diff[1]--; neg_ray=true; }
  if (current_voxel[2]!=last_voxel[2] && ray[2]<0) { diff[2]--; neg_ray=true; }
  visited_voxels.push_back(current_voxel);
  if (neg_ray) {
    current_voxel+=diff;
    visited_voxels.push_back(current_voxel);
  }

  while(last_voxel != current_voxel) {
    if (tMaxX < tMaxY) {
      if (tMaxX < tMaxZ) {
        current_voxel[0] += stepX;
        tMaxX += tDeltaX;
      } else {
        current_voxel[2] += stepZ;
        tMaxZ += tDeltaZ;
      }
    } else {
      if (tMaxY < tMaxZ) {
        current_voxel[1] += stepY;
        tMaxY += tDeltaY;
      } else {
        current_voxel[2] += stepZ;
        tMaxZ += tDeltaZ;
      }
    }
    visited_voxels.push_back(current_voxel);
  }
  return visited_voxels;
}

int main (int, char**) {
  Eigen::Vector3d ray_start(0,0,0);
  Eigen::Vector3d ray_end(3,2,2);
  std::cout << "Voxel size: " << _bin_size << std::endl;
  std::cout << "Starting position: " << ray_start.transpose() << std::endl;
  std::cout << "Ending position: " << ray_end.transpose() << std::endl;
  std::cout << "Voxel ID's from start to end:" << std::endl;
  std::vector<Eigen::Vector3i> ids = voxel_traversal(ray_start,ray_end);

  for (auto& i : ids) {
    std::cout << "> " << i.transpose() << std::endl;
  }
  std::cout << "Total number of traversed voxels: " << ids.size() << std::endl;
  return 0;
}

*/

        #endregion
    }
}
