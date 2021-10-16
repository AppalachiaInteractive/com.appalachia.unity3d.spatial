using System;
using Appalachia.Core.Attributes.Editing;
using Sirenix.OdinInspector;

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    [Serializable]
    public class DecomposedColliderReplacementReviewItem
    {
        [HorizontalGroup("A"), SmartLabel, ReadOnly]
        public int pieces;
        [HorizontalGroup("A"), SmartLabel, ReadOnly]
        public int vertices;
        [HorizontalGroup("A"), SmartLabel, ReadOnly]
        public int triangles;
    }
}
