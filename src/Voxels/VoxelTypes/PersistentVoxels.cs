#region

using Appalachia.Voxels.Casting;
using Appalachia.Voxels.Persistence;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Voxels.VoxelTypes
{
    public sealed class PersistentVoxels : PersistentVoxelsBase<PersistentVoxels, VoxelPersistentDataStore, VoxelRaycastHit>
    {
        private PersistentVoxels(string identifier) : base(identifier)
        {
        }

        protected override VoxelRaycastHit PrepareRaycastHit(int voxelIndex, Voxel voxel, float distance)
        {
            return new VoxelRaycastHit {distance = distance, voxel = voxel};
        }

        public static PersistentVoxels VoxelizeSingle(string identifier, Transform t, Bounds b, float3 p)
        {
            return Voxelizer.VoxelizeSingle<PersistentVoxels, VoxelRaycastHit>(new PersistentVoxels(identifier), t, b, p);
        }

        public static PersistentVoxels Voxelize(string identifier, VoxelPopulationStyle style, Transform t, Collider[] c, MeshRenderer[] r, float3 res)
        {
            return Voxelizer.Voxelize<PersistentVoxels, VoxelRaycastHit>(
                new PersistentVoxels(identifier),
                t,
                style,
                c,
                r,
                res
            );
        }
    }
}
