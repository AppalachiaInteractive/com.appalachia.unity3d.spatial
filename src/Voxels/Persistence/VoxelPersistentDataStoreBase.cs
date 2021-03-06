#region

using System;
using Appalachia.Core.Collections.Extensions;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Collections.Native.Pointers;
using Appalachia.Core.Comparisons.ComponentEquality;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.VoxelTypes;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Strings;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.Persistence
{
    [Serializable]
    public abstract class
        VoxelPersistentDataStoreBase<TVoxelData, TThis, TRaycastHit> : AutonamedIdentifiableAppalachiaObject<
            TThis>
        where TVoxelData : PersistentVoxelsBase<TVoxelData, TThis, TRaycastHit>
        where TThis : VoxelPersistentDataStoreBase<TVoxelData, TThis, TRaycastHit>
        where TRaycastHit : struct, IVoxelRaycastHit
    {
        #region Fields and Autoproperties

        [SerializeField] public bool[] voxelsActive;

        [SerializeField] public Bounds rawBounds;
        [SerializeField] public Bounds voxelBounds;
        [SerializeField] public ColliderEqualityState[] colliderEqualityStates;
        [SerializeField] public float activeRatio;

        [SerializeField] public float volume;
        [SerializeField] public float3 centerOfMass;

        [SerializeField] public float3 resolution;
        [SerializeField] public int faceCount;
        [SerializeField] public int voxelsActiveCount;
        [SerializeField] public int3 samplePointDimensions;
        [SerializeField] public MeshEqualityState[] meshEqualityStates;
        [SerializeField] public string identifier;
        [SerializeField] public Voxel[] voxels;
        [SerializeField] public VoxelPopulationStyle style;

        [SerializeField] public VoxelSamplePoint[] samplePoints;

        #endregion

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

        public bool ShouldRestore(
            float3 resolution,
            VoxelPopulationStyle style,
            Collider[] colliders,
            MeshRenderer[] renderers)
        {
            if (!Equals(this.resolution, resolution))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Resolution has changed from [{0}] to [{1}]",
                        this.resolution,
                        resolution
                    )
                );
                return false;
            }

            if (style != this.style)
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Style has changed from [{0}] to [{1}]",
                        this.style,
                        style
                    )
                );
                return false;
            }

            if (((colliders == null) || (colliders.Length == 0)) &&
                (colliderEqualityStates != null) &&
                (colliderEqualityStates.Length > 0))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Collider amount has changed from [{0}] to [{1}]",
                        colliderEqualityStates?.Length ?? 0,
                        colliders?.Length ?? 0
                    )
                );
                return false;
            }

            if (((colliderEqualityStates == null) || (colliderEqualityStates.Length == 0)) &&
                (colliders != null) &&
                (colliders.Length > 0))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Collider amount has changed from [{0}] to [{1}]",
                        colliderEqualityStates?.Length ?? 0,
                        colliders?.Length ?? 0
                    )
                );
                return false;
            }

            if ((colliderEqualityStates != null) &&
                (colliders != null) &&
                (colliderEqualityStates.Length != colliders.Length))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Collider amount has changed from [{0}] to [{1}]",
                        colliderEqualityStates?.Length ?? 0,
                        colliders?.Length ?? 0
                    )
                );
                return false;
            }

            for (var i = 0; i < colliderEqualityStates.Length; i++)
            {
                if (!colliderEqualityStates[i].Equals(colliders[i]))
                {
                    Context.Log.Warn(
                        ZString.Format("Cannot reuse voxel data store.  Collider [{0}] has changed.", i)
                    );
                    return false;
                }
            }

            if (((renderers == null) || (renderers.Length == 0)) &&
                (meshEqualityStates != null) &&
                (meshEqualityStates.Length > 0))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Renderer amount has changed from [{0}] to [{1}]",
                        meshEqualityStates?.Length ?? 0,
                        renderers?.Length ?? 0
                    )
                );
                return false;
            }

            if (((meshEqualityStates == null) || (meshEqualityStates.Length == 0)) &&
                (renderers != null) &&
                (renderers.Length > 0))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Renderer amount has changed from [{0}] to [{1}]",
                        meshEqualityStates?.Length ?? 0,
                        renderers?.Length ?? 0
                    )
                );
                return false;
            }

            if ((meshEqualityStates != null) &&
                (renderers != null) &&
                (meshEqualityStates.Length != renderers.Length))
            {
                Context.Log.Warn(
                    ZString.Format(
                        "Cannot reuse voxel data store.  Renderer amount has changed from [{0}] to [{1}]",
                        meshEqualityStates?.Length ?? 0,
                        renderers?.Length ?? 0
                    )
                );
                return false;
            }

            for (var i = 0; i < meshEqualityStates.Length; i++)
            {
                if (!meshEqualityStates[i].Equals(renderers[i].GetSharedMesh()))
                {
                    Context.Log.Warn(
                        ZString.Format("Cannot reuse voxel data store.  Mesh [{0}] has changed.", i)
                    );
                    return false;
                }
            }

            return true;
        }

        protected abstract void RecordAdditional(TVoxelData data);

        protected abstract void RestoreAdditional(TVoxelData data);
    }
}
