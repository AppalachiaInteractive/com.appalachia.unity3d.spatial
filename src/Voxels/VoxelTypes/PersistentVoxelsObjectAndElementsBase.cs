#region

using System;
using Appalachia.Core.Collections.Native;
using Appalachia.Spatial.Voxels.Casting;
using Appalachia.Spatial.Voxels.Persistence;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels.VoxelTypes
{
    [Serializable]
    public abstract class
        PersistentVoxelsObjectAndElementsBase<TVoxelData, TDataStore, TObject, TElement> :
            PersistentVoxelsBase<TVoxelData, TDataStore, VoxelRaycastHit<TElement>>
        where TVoxelData : PersistentVoxelsObjectAndElementsBase<TVoxelData, TDataStore, TObject, TElement>
        where TDataStore : VoxelPersistentObjectAndElementsDataStore<TVoxelData, TDataStore, TObject,
            TElement>
        where TObject : IVoxelsInit, new()
        where TElement : struct
    {
        protected PersistentVoxelsObjectAndElementsBase(string identifier) : base(identifier)
        {
        }

        #region Fields and Autoproperties

        [NonSerialized] public NativeArray<TElement> elementDatas;
        [NonSerialized] public TObject objectData;

        #endregion

        public static TVoxelData Voxelize(
            TVoxelData instance,
            VoxelPopulationStyle style,
            Transform t,
            Collider[] c,
            MeshRenderer[] r,
            float3 res)
        {
            return Voxelizer.Voxelize<TVoxelData, VoxelRaycastHit<TElement>>(instance, t, style, c, r, res);
        }

        public static TVoxelData VoxelizeSingle(TVoxelData instance, Transform t, Bounds b, float3 p)
        {
            return Voxelizer.VoxelizeSingle<TVoxelData, VoxelRaycastHit<TElement>>(instance, t, b, p);
        }

        /// <inheritdoc />
        public override void InitializeElements(int elementCount)
        {
            base.InitializeElements(elementCount);
            elementDatas = new NativeArray<TElement>(elementCount, Allocator.Persistent);
        }

        /// <inheritdoc />
        public override void OnInitialize()
        {
            base.OnInitialize();

            if (objectData == null)
            {
                objectData = new TObject();
            }

            objectData.Initialize();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override VoxelRaycastHit<TElement> PrepareRaycastHit(
            int voxelIndex,
            Voxel voxel,
            float distance)
        {
            return new() { data = elementDatas[voxelIndex], voxel = voxel, distance = distance };
        }
    }
}
