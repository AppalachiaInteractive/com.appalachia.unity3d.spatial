#region

using System;
using Appalachia.Core.Collections.Extensions;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Collections.Native.Pointers;
using Appalachia.Core.Comparisons.ComponentEquality;
using Appalachia.Core.Extensions;
using Appalachia.Core.Scriptables;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Logging;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Persistence
{
    [Serializable]
    public abstract class
        VoxelPersistentDataStoreBase<TVoxelData, TDataStore, TRaycastHit> :
            AutonamedIdentifiableAppalachiaObject<TDataStore>
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TDataStore, TRaycastHit>
        where TDataStore : VoxelPersistentDataStoreBase<TVoxelData, TDataStore, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        [SerializeField] public string identifier;

        [SerializeField] public Bounds rawBounds;
        [SerializeField] public Bounds voxelBounds;

        [SerializeField] public float volume;
        [SerializeField] public float3 centerOfMass;
        [SerializeField] public int faceCount;

        [SerializeField] public VoxelSamplePoint[] samplePoints;
        [SerializeField] public int3 samplePointDimensions;
        [SerializeField] public Voxel[] voxels;
        [SerializeField] public bool[] voxelsActive;
        [SerializeField] public int voxelsActiveCount;
        [SerializeField] public float activeRatio;

        [SerializeField] public float3 resolution;
        [SerializeField] public VoxelPopulationStyle style;
        [SerializeField] public ColliderEqualityState[] colliderEqualityStates;
        [SerializeField] public MeshEqualityState[] meshEqualityStates;

        public void Record(TVoxelData data)
        {
            identifier = data.identifier;
            rawBounds = data.rawBounds;
            voxelBounds = data.voxelBounds;
            volume = data.volume;
            centerOfMass = data.centerOfMass;
            faceCount = data.faceCount;
            resolution = data.resolution;
            samplePoints = data.samplePoints.ToArrayFlat(out samplePointDimensions);
            voxels = data.voxels.IsCreated ? data.voxels.ToArray() : null;
            voxelsActive = data.voxelsActive.IsCreated ? data.voxelsActive.ToArray() : null;
            voxelsActiveCount = data.voxelsActiveCount.IsCreated ? data.voxelsActiveCount.Value : 0;
            activeRatio = data.activeRatio;
            style = data.style;
            colliderEqualityStates = data.colliders.ToEqualityStates();
            meshEqualityStates = data.renderers.ToEqualityStates();

            RecordAdditional(data);
        }

        protected abstract void RecordAdditional(TVoxelData data);

        public void Restore(TVoxelData data)
        {
            data.identifier = identifier;
            data.rawBounds = rawBounds;
            data.voxelBounds = voxelBounds;
            data.volume = volume;
            data.centerOfMass = centerOfMass;
            data.faceCount = faceCount;
            data.resolution = resolution;
            data.samplePoints = new NativeArray3D<VoxelSamplePoint>(
                samplePoints,
                samplePointDimensions,
                Allocator.Persistent
            );
            data.voxels = new NativeArray<Voxel>(voxels, Allocator.Persistent);
            data.voxelsActive = voxelsActive.FromArray();
            data.voxelsActiveCount = new NativeIntPtr(Allocator.Persistent, voxelsActiveCount);
            data.activeRatio = activeRatio;
            data.style = style;

            RestoreAdditional(data);
        }

        protected abstract void RestoreAdditional(TVoxelData data);

        public bool ShouldRestore(
            float3 resolution,
            VoxelPopulationStyle style,
            Collider[] colliders,
            MeshRenderer[] renderers)
        {
            if (!Equals(this.resolution, resolution))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Resolution has changed from [{this.resolution}] to [{resolution}]"
                );
                return false;
            }

            if (style != this.style)
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Style has changed from [{this.style}] to [{style}]"
                );
                return false;
            }

            if (((colliders == null) || (colliders.Length == 0)) &&
                (colliderEqualityStates != null) &&
                (colliderEqualityStates.Length > 0))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Collider amount has changed from [{colliderEqualityStates?.Length ?? 0}] to [{colliders?.Length ?? 0}]"
                );
                return false;
            }

            if (((colliderEqualityStates == null) || (colliderEqualityStates.Length == 0)) &&
                (colliders != null) &&
                (colliders.Length > 0))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Collider amount has changed from [{colliderEqualityStates?.Length ?? 0}] to [{colliders?.Length ?? 0}]"
                );
                return false;
            }

            if ((colliderEqualityStates != null) &&
                (colliders != null) &&
                (colliderEqualityStates.Length != colliders.Length))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Collider amount has changed from [{colliderEqualityStates?.Length ?? 0}] to [{colliders?.Length ?? 0}]"
                );
                return false;
            }

            for (var i = 0; i < colliderEqualityStates.Length; i++)
            {
                if (!colliderEqualityStates[i].Equals(colliders[i]))
                {
                   AppaLog.Warn(
                        $"Cannot reuse voxel data store.  Collider [{i}] has changed."
                    );
                    return false;
                }
            }

            if (((renderers == null) || (renderers.Length == 0)) &&
                (meshEqualityStates != null) &&
                (meshEqualityStates.Length > 0))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Renderer amount has changed from [{meshEqualityStates?.Length ?? 0}] to [{renderers?.Length ?? 0}]"
                );
                return false;
            }

            if (((meshEqualityStates == null) || (meshEqualityStates.Length == 0)) &&
                (renderers != null) &&
                (renderers.Length > 0))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Renderer amount has changed from [{meshEqualityStates?.Length ?? 0}] to [{renderers?.Length ?? 0}]"
                );
                return false;
            }

            if ((meshEqualityStates != null) &&
                (renderers != null) &&
                (meshEqualityStates.Length != renderers.Length))
            {
               AppaLog.Warn(
                    $"Cannot reuse voxel data store.  Renderer amount has changed from [{meshEqualityStates?.Length ?? 0}] to [{renderers?.Length ?? 0}]"
                );
                return false;
            }

            for (var i = 0; i < meshEqualityStates.Length; i++)
            {
                if (!meshEqualityStates[i].Equals(renderers[i].GetSharedMesh()))
                {
                   AppaLog.Warn($"Cannot reuse voxel data store.  Mesh [{i}] has changed.");
                    return false;
                }
            }

            return true;
        }
    }
}
