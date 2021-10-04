#region

using System;
using Appalachia.Core.Collections.Native;
using Appalachia.Spatial.Voxels.Casting;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.VoxelTypes
{
    public class Voxels : VoxelsBase<Voxels, VoxelRaycastHit>
    {
        public override bool IsPersistent => false;
        
        protected override VoxelRaycastHit PrepareRaycastHit(int voxelIndex, Voxel voxel, float distance)
        {
            return new VoxelRaycastHit {distance = distance, voxel = voxel};
        }

        public static Voxels VoxelizeSingle(Transform t, Bounds b, float3 p)
        {
            return Voxelizer.VoxelizeSingle<Voxels, VoxelRaycastHit>(t, b, p);
        }

        public static Voxels Voxelize(Transform t, MeshRenderer[] c, float3 res)
        {
            return Voxelizer.Voxelize<Voxels, VoxelRaycastHit>(t, c, res);
        }

        public static Voxels Voxelize(Transform t, Collider[] c, float3 res)
        {
            return Voxelizer.Voxelize<Voxels, VoxelRaycastHit>(t, c, res);
        }

        public static Voxels Voxelize(Transform t, Collider[] c, MeshRenderer[] r, float3 res)
        {
            return Voxelizer.Voxelize<Voxels, VoxelRaycastHit>(t, VoxelPopulationStyle.CollidersAndMeshes, c, r, res);
        }
    }

    public class Voxels<TElement> : VoxelsBase<Voxels<TElement>, VoxelRaycastHit<TElement>>
        where TElement : struct
    {
        public override bool IsPersistent => false;
        
        [NonSerialized] public NativeArray<TElement> elementDatas;

        public override void InitializeElements(int elementCount)
        {
            elementDatas = new NativeArray<TElement>(elementCount, Allocator.Persistent);
            base.InitializeElements(elementCount);
        }

        protected override VoxelRaycastHit<TElement> PrepareRaycastHit(int voxelIndex, Voxel voxel, float distance)
        {
            return new VoxelRaycastHit<TElement> {data = elementDatas[voxelIndex], voxel = voxel, distance = distance};
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (elementDatas.IsCreated)
                {
                    elementDatas.SafeDispose();
                }
            }
        }

        public static Voxels<TElement> VoxelizeSingle(Transform t, Bounds b, float3 p)
        {
            return Voxelizer.VoxelizeSingle<Voxels<TElement>, VoxelRaycastHit<TElement>>(t, b, p);
        }

        public static Voxels<TElement> Voxelize(Transform t, MeshRenderer[] c, float3 res)
        {
            return Voxelizer.Voxelize<Voxels<TElement>, VoxelRaycastHit<TElement>>(t, c, res);
        }

        public static Voxels<TElement> Voxelize(Transform t, Collider[] c, float3 res)
        {
            return Voxelizer.Voxelize<Voxels<TElement>, VoxelRaycastHit<TElement>>(t, c, res);
        }

        public static Voxels<TElement> Voxelize(Transform t, Collider[] c, MeshRenderer[] r, float3 res)
        {
            return Voxelizer.Voxelize<Voxels<TElement>, VoxelRaycastHit<TElement>>(t, VoxelPopulationStyle.CollidersAndMeshes, c, r, res);
        }
    }

    public class Voxels<T, TElement> : VoxelsBase<Voxels<T, TElement>, VoxelRaycastHit<TElement>>
        where TElement : struct
        where T : IVoxelsInit, new()
    {
        public override bool IsPersistent => false;
        
        public T objectData;

        [NonSerialized] public NativeArray<TElement> elementDatas;

        public override void OnInitialize()
        {
            base.OnInitialize();

            if (objectData == null)
            {
                objectData = new T();
            }

            objectData.Initialize();
        }

        public override void InitializeElements(int elementCount)
        {
            elementDatas = new NativeArray<TElement>(elementCount, Allocator.Persistent);
            base.InitializeElements(elementCount);
        }

        protected override VoxelRaycastHit<TElement> PrepareRaycastHit(int voxelIndex, Voxel voxel, float distance)
        {
            return new VoxelRaycastHit<TElement> {data = elementDatas[voxelIndex], voxel = voxel, distance = distance};
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (elementDatas.IsCreated)
                {
                    elementDatas.SafeDispose();
                }

                if (objectData is IDisposable d)
                {
                    d.SafeDispose();
                }
            }
        }

        public static Voxels<T, TElement> VoxelizeSingle(Transform t, Bounds b, float3 p)
        {
            return Voxelizer.VoxelizeSingle<Voxels<T, TElement>, VoxelRaycastHit<TElement>>(t, b, p);
        }

        public static Voxels<T, TElement> Voxelize(VoxelPopulationStyle style, Transform t, Collider[] c, MeshRenderer[] r, float3 res)
        {
            
            return Voxelizer.Voxelize<Voxels<T, TElement>, VoxelRaycastHit<TElement>>(t, style, c, r, res);
        }
    }
}
