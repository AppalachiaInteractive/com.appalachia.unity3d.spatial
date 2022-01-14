#if UNITY_EDITOR
using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Objects.Root;
using Sirenix.OdinInspector;

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    [Serializable]
    public class DecomposedColliderReplacementReviewItem : AppalachiaSimpleBase
    {
        #region Fields and Autoproperties

        [HorizontalGroup("A"), SmartLabel, ReadOnly]
        public int pieces;

        [HorizontalGroup("A"), SmartLabel, ReadOnly]
        public int vertices;

        [HorizontalGroup("A"), SmartLabel, ReadOnly]
        public int triangles;

        #endregion
    }
}

#endif
