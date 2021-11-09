#if UNITY_EDITOR
using System;
using System.Diagnostics;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Preferences.Globals;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    [Serializable]
    public class DecomposedColliderDataReviewItemData
    {
        private const string _PRF_PFX = nameof(DecomposedColliderDataReviewItemData) + ".";
        private static readonly ProfilerMarker _PRF_ratio = new ProfilerMarker(_PRF_PFX + nameof(ratio));
        private static readonly ProfilerMarker _PRF_ToString = new ProfilerMarker(_PRF_PFX + nameof(ToString));
        private static readonly ProfilerMarker _PRF_ratioScaledAndClamped = new ProfilerMarker(_PRF_PFX + nameof(ratioScaledAndClamped));
        private static readonly ProfilerMarker _PRF_ratioColor = new ProfilerMarker(_PRF_PFX + nameof(ratioColor));
        
        [ReadOnly, SerializeField, SmartLabel(AlignWith = " Decomposed ")]
        public int original;
        
        [ReadOnly, SerializeField, SmartLabel(AlignWith = " Decomposed "), GUIColor(nameof(ratioColor))]
        public int decomposed;

        private float? _ratio;
        
        private float ratio
        {
            get
            {
                using (_PRF_ratio.Auto())
                {
                    if (!_ratio.HasValue)
                    {
                        _ratio = decomposed / (float) original;
                    }

                    return _ratio.Value;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_GetString = new ProfilerMarker(_PRF_PFX + nameof(GetString));
        private string _toString;
        public string GetString
        {
            get
            {
                using (_PRF_GetString.Auto())
                {
                    return ToString();
                }
            }
        }

        private const string _format = "{0:F1}%";
        [DebuggerStepThrough] public override string ToString()
        {
            using (_PRF_ToString.Auto())
            {
                if (_toString == null)
                {
                    _toString = string.Format(_format, ratio * 100f);
                }

                return _toString;
            }
        }

        private float? _ratioScaledAndClamped;
        private float ratioScaledAndClamped
        {
            get
            {
                using (_PRF_ratioScaledAndClamped.Auto())
                {
                    if (!_ratioScaledAndClamped.HasValue)
                    {
                        _ratioScaledAndClamped = 1.0f - math.clamp(ratio, 0f, 1f);
                    }

                    return _ratioScaledAndClamped.Value;
                }
            }
        }

        private Color? _ratioColor;
        
        public Color ratioColor
        {
            get
            {
                using (_PRF_ratioColor.Auto())
                {
                    if (!_ratioColor.HasValue)
                    {
                        _ratioColor = ColorPrefs.Instance.Quality_BadToGood.v.Evaluate(ratioScaledAndClamped);
                    }

                    return _ratioColor.Value;
                }
            }
        }

        public void Reset()
        {
            original = 0;
            decomposed = 0;
            
            _ratio = null;
            _toString = null;
            _ratioScaledAndClamped = null;
            _ratioColor = null;
        }
    }
}

#endif