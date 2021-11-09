/*
namespace Appalachia.Spatial.Octree
{
    /*public struct OctreeNode<TK, TV> : IEquatable<OctreeNode<TK, TV>>
    {
        public TK key;
        public TV value;

        [DebuggerStepThrough] public bool Equals(OctreeNode<TK, TV> other)
        {
            return EqualityComparer<TK>.Default.Equals(key, other.key) &&
                EqualityComparer<TV>.Default.Equals(value, other.value);
        }

        [DebuggerStepThrough] public override bool Equals(object obj)
        {
            return obj is OctreeNode<TK, TV> other && Equals(other);
        }

        [DebuggerStepThrough] public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<TK>.Default.GetHashCode(key) * 397) ^
                    EqualityComparer<TV>.Default.GetHashCode(value);
            }
        }

        [DebuggerStepThrough] public static bool operator ==(OctreeNode<TK, TV> left, OctreeNode<TK, TV> right)
        {
            return left.Equals(right);
        }

        [DebuggerStepThrough] public static bool operator !=(OctreeNode<TK, TV> left, OctreeNode<TK, TV> right)
        {
            return !left.Equals(right);
        }
    }#1#
}
*/
