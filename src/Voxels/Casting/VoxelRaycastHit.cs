#region

using System;

#endregion

namespace Appalachia.Spatial.Voxels.Casting
{
    [Serializable]
    public struct VoxelRaycastHit : IVoxelRaycastHit
    {
        public Voxel voxel;
        public float distance;

        public Voxel Voxel => voxel;
        public float Distance => distance;
    }

    [Serializable]
    public struct VoxelRaycastHit<T> : IVoxelRaycastHit
    {
        public Voxel voxel;
        public T data;
        public float distance;

        public Voxel Voxel => voxel;
        public T Data => data;
        public float Distance => distance;
    }
}
