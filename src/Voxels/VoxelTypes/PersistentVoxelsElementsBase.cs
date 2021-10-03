#region

using System;
using Appalachia.Core.Collections.Native;
using Appalachia.Voxels.Casting;
using Appalachia.Voxels.Persistence;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Voxels.VoxelTypes
{
    public abstract class
        PersistentVoxelsElementsBase<TVoxelData, TDataStore, TElement> : PersistentVoxelsBase<TVoxelData, TDataStore, VoxelRaycastHit<TElement>>
        where TVoxelData : PersistentVoxelsElementsBase<TVoxelData, TDataStore, TElement>
        where TDataStore : VoxelPersistentElementsDataStore<TVoxelData, TDataStore, TElement>
        where TElement : struct
    {
        [NonSerialized] public NativeArray<TElement> elementDatas;

        protected PersistentVoxelsElementsBase(string identifier) : base(identifier)
        {
        }

        public override void InitializeElements(int elementCount)
        {
            base.InitializeElements(elementCount);
            elementDatas = new NativeArray<TElement>(elementCount, Allocator.Persistent);
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

        public static TVoxelData VoxelizeSingle(TVoxelData instance, Transform t, Bounds b, float3 p)
        {
            return Voxelizer.VoxelizeSingle<TVoxelData, VoxelRaycastHit<TElement>>(instance, t, b, p);
        }

        public static TVoxelData Voxelize(TVoxelData instance, VoxelPopulationStyle style, Transform t, Collider[] c, MeshRenderer[] r, float3 res)
        {
            return Voxelizer.Voxelize<TVoxelData, VoxelRaycastHit<TElement>>(instance, t, VoxelPopulationStyle.CollidersAndMeshes, c, r, res);
        }
    }
}
