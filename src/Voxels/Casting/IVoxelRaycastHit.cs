namespace Appalachia.Spatial.Voxels.Casting
{
    public interface IVoxelRaycastHit
    {
        Voxel Voxel { get; }
        float Distance { get; }
    }
}
