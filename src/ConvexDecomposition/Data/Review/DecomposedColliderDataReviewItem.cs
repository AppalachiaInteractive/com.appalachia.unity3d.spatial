#if UNITY_EDITOR

#region

using System;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Preferences.Globals;
using Appalachia.Spatial.ConvexDecomposition.Generation;
using Appalachia.Utility.Colors;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    [Serializable, HideReferenceObjectPicker]
    public class DecomposedColliderDataReviewItem
    {
        [HideInInspector] public DecomposedCollider dc;

        [SmartTitle(_TITLE,  _SUBTITLE,  TitleAlignments.Split, false, true, nameof(_titleColor))]
        [SmartTitle(_TITLE2, _SUBTITLE2, TitleAlignments.Split, false, true, nameof(reviewColor))]
        [PropertyOrder(0)]
        [SerializeField]
        [SmartLabel]
        [SmartInlineButton(nameof(SuggestExternal), "Suggest", false, false, _fbc, nameof(_disableSuggest))]
        [SmartInlineButton(nameof(SyncExternal),    "Sync",    false, false, _fbc, nameof(_disableSyncExternal))]
        [SmartInlineButton(nameof(Frame),           "Frame",   false, false, _fbc, nameof(_disableFrame))]
        [SmartInlineButton(nameof(SelectComponent), "Select",  false, false, _fbc, nameof(_disableSelectComponent))]
        public DecomposedColliderData data;

        [PropertyOrder(1)]
        [TabGroup(_TABS, _REVIEW, UseFixedHeight = true, Order = 0)]
        [HorizontalGroup(_REVIEW__, .5F)]
        [InfoBox(vertexInfoStringPointer, InfoMessageType.None)]
        [SerializeField, HideReferenceObjectPicker, HideLabel, LabelWidth(0), InlineProperty]
        public DecomposedColliderDataReviewItemData vertices;

        [PropertyOrder(2)]
        [HorizontalGroup(_REVIEW__, .5F)]
        [InfoBox(triangleInfoStringPointer, InfoMessageType.None)]
        [SerializeField, HideReferenceObjectPicker, HideLabel, LabelWidth(0), InlineProperty]
        public DecomposedColliderDataReviewItemData triangles;

        [TabGroup(_TABS, _MODIFY, UseFixedHeight = true, Order = 10)]
        [GUIColor(nameof(_modColor))]
        [PropertyOrder(200)]
        [PropertyRange(1, 64)]
        [SmartInlineButton(nameof(ProcessAgain), "Process Again", false, false, nameof(_modColor), nameof(_disableProcessAgain))]
        [SerializeField, SmartLabel]
        public int targetPieces;

        [TabGroup(_TABS, _EXTERNAL, UseFixedHeight = true, Order = 20)]
        [GUIColor(nameof(_modColor))]
        [PropertyOrder(250)]
        [ValueDropdown(nameof(assetsDropdown))]
        [SmartInlineButton(nameof(SetExternalToSuggestion), "Replace", false, false, nameof(_modColor), nameof(_disableReplace))]
        [SmartInlineButton(nameof(SuggestExternal),     "Suggest", false, false, nameof(_modColor), nameof(_disableSuggest))]
        [SmartInlineButton(nameof(SelectReplacement),   "Select",  false, false, nameof(_modColor), nameof(_disableReplace))]
        [ShowInInspector, HideDuplicateReferenceBox, HideReferenceObjectPicker, SmartLabel(Color = nameof(_modColor))]
        [OnValueChanged(nameof(UpdateReplacementReview))]
        public GameObject replacementModel
        {
            get => data.suggestedReplacementModel;
            set => data.suggestedReplacementModel = value;
        }

        [TabGroup(_TABS, _EXTERNAL, UseFixedHeight = true, Order = 20)]
        [GUIColor(nameof(_modColor))]
        [PropertyOrder(260)]
        [EnableIf(nameof(_enableReplacementReview))]
        [HideReferenceObjectPicker, HideDuplicateReferenceBox]
        [ShowInInspector, SmartLabel(Color = nameof(_modColor))]
        [SmartInlineButton(
            nameof(UpdateReplacementReview),
            "Refresh",
            false,
            color: nameof(_modColor),
            DisableIf = nameof(_disableReplace)
        )]
        public DecomposedColliderReplacementReviewItem replacementReview
        {
            get => data.replacementReview;
            set => data.replacementReview = value;
        }

#region Helpers

        public void Reset()
        {
            dc = null;
            data = null;
            targetPieces = 8;
            replacementModel = null;
            _toString = null;
            vertices.Reset();
            triangles.Reset();
        }

        public DataReviewState state => DecomposedColliderData.GetState(data);

        public int elements
        {
            get
            {
                if (data != null)
                {
                    return data.elements?.Count ?? 0;
                }

                return 0;
            }
        }

        private void UpdateReplacementReview()
        {
            DecomposedColliderSuggestionHelper.UpdateReplacementReview(ref data.replacementReview, replacementModel);
        }

        public void ProcessAgain()
        {
            using (_PRF_ProcessAgain.Auto())
            {
                data.ExecuteDecompositionExplicit(dc.gameObject, targetPieces, 0);
            }
        }

        private void SelectComponent()
        {
            using (_PRF_SelectComponent.Auto())
            {
                Selection.SetActiveObjectWithContext(dc, data);
            }
        }

        private void Frame()
        {
            using (_PRF_Frame.Auto())
            {
                dc.Frame();
            }
        }

        private void SyncExternal()
        {
            using (_PRF_SyncExternal.Auto())
            {
                if (_disableSyncExternal)
                {
                    return;
                }

                data.LoadExternalMeshes();
            }
        }

        public void SuggestExternal()
        {
            using (_PRF_SuggestExternal.Auto())
            {
                data.SuggestExternal();
            }
        }

        public void SetExternalToSuggestion()
        {
            using (_PRF_Replace.Auto())
            {
                DecomposedColliderData.SetExternalModel(data, replacementModel);
            }
        }

        private void SelectReplacement()
        {
            using (_PRF_SelectReplacement.Auto())
            {
                Selection.activeObject = replacementModel;
            }
        }

#endregion

#region ToString

        private string _toString;
        private static readonly ProfilerMarker _PRF_ToString = new ProfilerMarker(_PRF_PFX + nameof(ToString));

        public override string ToString()
        {
            using (_PRF_ToString.Auto())
            {
                if (_toString == null)
                {
                    _toString = $"{nameof(elements)}: {elements}, {nameof(vertices)}: {vertices}, {nameof(triangles)}: {triangles}";
                }

                return _toString;
            }
        }

#endregion

#region Profiling

        private const string _PRF_PFX = nameof(DecomposedColliderDataReviewItem) + ".";

        private static readonly ProfilerMarker _PRF_SyncExternal = new ProfilerMarker(_PRF_PFX + nameof(SyncExternal));
        private static readonly ProfilerMarker _PRF_SuggestExternal = new ProfilerMarker(_PRF_PFX + nameof(SuggestExternal));
        private static readonly ProfilerMarker _PRF_ProcessAgain = new ProfilerMarker(_PRF_PFX + nameof(ProcessAgain));
        private static readonly ProfilerMarker _PRF_SelectComponent = new ProfilerMarker(_PRF_PFX + nameof(SelectComponent));
        private static readonly ProfilerMarker _PRF_Replace = new ProfilerMarker(_PRF_PFX + nameof(SetExternalToSuggestion));
        private static readonly ProfilerMarker _PRF_SelectReplacement = new ProfilerMarker(_PRF_PFX + nameof(SelectReplacement));

        private static readonly ProfilerMarker _PRF_Frame = new ProfilerMarker(_PRF_PFX + nameof(Frame));

#endregion

#region UI

        private const string _TITLE = "$" + nameof(_title);
        private const string _SUBTITLE = "$" + nameof(_subtitle);
        private const string _TITLE2 = "$" + nameof(_title2);
        private const string _SUBTITLE2 = "$" + nameof(_subtitle2);

        private ValueDropdownList<GameObject> assetsDropdown => DecomposedColliderSuggestionHelper.assets;

        private const string _fbc = nameof(_functionButtons);
        private Color _functionButtons => ColorPrefs.Instance.DecomposedColliderReviewFunction.v;

        private Color _titleColor =>
            data == null
                ? ColorPrefs.Instance.DecomposedColliderReviewMissing.v
                : (data.elements == null) || (data.elements.Count == 0)
                    ? ColorPrefs.Instance.DecomposedColliderReviewNotSet.v
                    : data.locked
                        ? ColorPrefs.Instance.DecomposedColliderReviewLocked.v
                        : data.externallyCreated
                            ? ColorPrefs.Instance.DecomposedColliderReviewExternal.v
                            : ColorPrefs.Instance.DecomposedColliderReviewBasic.v;

        private string _title => data == null ? string.Empty : data.name;

        private string _subtitle =>
            data == null
                ? "Missing"
                : (data.elements == null) || (data.elements.Count == 0)
                    ? "Not Set"
                    : data.locked
                        ? "Locked"
                        : data.externallyCreated
                            ? "External"
                            : "Basic";

        private string _title2 => $"{elements} Meshes";
        private string _subtitle2 => $"{vertexInfoString} | {triangleInfoString}";

        private const string _TABS = "Tabs";
        private const string _REVIEW = "Review";
        private const string _REVIEW_ = _TABS + "/" + _REVIEW;
        private const string _REVIEW__ = _REVIEW_ + "/" + "A";
        private const string _MODIFY = "Modify";
        private const string _MODIFY_ = _TABS + "/" + _MODIFY;
        private const string _EXTERNAL = "External";
        private const string _EXTERNAL_ = _TABS + "/" + _EXTERNAL;
        private const string _BUTTONS = "Buttons";

        private Color _modColor => ColorPrefs.Instance.DecomposedColliderReviewModification.v;

        private bool _enableReplacementReview => replacementModel != null;

        private Color _buttonColor => Color.white;
        private bool _disableProcessAgain => (targetPieces < 1) || (data == null) || data.locked;

        private bool _disableReplace => replacementModel == null;
        private bool _disableSelectComponent => dc == null;
        private bool _disableSyncExternal => (dc == null) || (data == null) || !data.externallyCreated || (data.externalModel == null) || data.locked;
        private bool _disableSuggest => (dc == null) || (data == null) || data.externallyCreated || (data.externalModel != null);

        private bool _disableFrame => dc == null;

        private Color _reviewColor;

        private Color reviewColor
        {
            get
            {
                if (_reviewColor == default)
                {
                    _reviewColor = vertices.ratioColor.BlendHSV(triangles.ratioColor);
                }

                return _reviewColor;
            }
        }

        private const string vertexInfoStringPointer = "$" + nameof(vertexInfoString);
        private string _vertexInfoString;

        private string vertexInfoString
        {
            get
            {
                if (_vertexInfoString == null)
                {
                    _vertexInfoString = $"Vertices: {vertices.GetString}";
                }

                return _vertexInfoString;
            }
        }

        private const string triangleInfoStringPointer = "$" + nameof(triangleInfoString);
        private string _triangleInfoString;

        private string triangleInfoString
        {
            get
            {
                if (_triangleInfoString == null)
                {
                    _triangleInfoString = $"Triangles: {triangles.GetString}";
                }

                return _triangleInfoString;
            }
        }

#endregion
    }
}

#endif
