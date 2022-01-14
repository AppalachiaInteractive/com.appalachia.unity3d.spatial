#if UNITY_EDITOR
using System;
using Appalachia.Core.Collections;
using Appalachia.Core.Preferences.Globals;
using Unity.Mathematics;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    [Serializable]
    public class DecomposedColliderDataReviewItemIndex : AppaLookup<DecomposedColliderData,
        DecomposedColliderDataReviewItem, AppaList_DecomposedColliderData,
        AppaList_DecomposedColliderDataReviewItem>
    {
        public DecomposedColliderDataReviewItemIndex(int capacity) : base(capacity)
        {
        }

        protected override Color GetDisplayColor(
            DecomposedColliderData key,
            DecomposedColliderDataReviewItem value)
        {
            var ratioScaledAndClamped = 1.0f - math.clamp(value.elements * .02f, 0f, 1f);
            var ratioColor = ColorPrefs.Instance.Quality_BadToGood.v.Evaluate(ratioScaledAndClamped);
            return ratioColor;
        }

        protected override string GetDisplaySubtitle(
            DecomposedColliderData key,
            DecomposedColliderDataReviewItem value)
        {
            return value.ToString();
        }

        protected override string GetDisplayTitle(
            DecomposedColliderData key,
            DecomposedColliderDataReviewItem value)
        {
            return key.name;
        }
    }
}
#endif
