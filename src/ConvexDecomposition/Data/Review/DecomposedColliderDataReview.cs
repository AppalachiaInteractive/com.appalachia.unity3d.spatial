#if UNITY_EDITOR
using System;
using Appalachia.CI.Integration.Assets;
using Appalachia.Core.Attributes;
using Appalachia.Core.Objects.Root;
using Appalachia.Core.Preferences.Globals;
using Appalachia.Editing.Core;
using Appalachia.Spatial.ConvexDecomposition.Generation;
using Appalachia.Utility.Async;
using Appalachia.Utility.Execution;
using Appalachia.Utility.Extensions;
using Appalachia.Utility.Strings;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    [Serializable]
    [CallStaticConstructorInEditor]
    public class DecomposedColliderDataReview : SingletonAppalachiaObject<DecomposedColliderDataReview>
    {
        static DecomposedColliderDataReview()
        {
        }

        #region Fields and Autoproperties

        [SerializeField]
        [PropertyOrder(_PRI_ITEMS)]
        [ListDrawerSettings(
            NumberOfItemsPerPage = 4,
            HideAddButton = true,
            HideRemoveButton = true,
            DraggableItems = false
        )]
        [HideReferenceObjectPicker]
        public DecomposedColliderDataReviewItemIndex index;

        #endregion

        [PropertyOrder(_PRI_FILTER + 0)]
        [TabGroup(_TABS, _FILTER)]
        [ButtonGroup(_FILTER_A), LabelText("Show All")]
        public void Filter_All()
        {
            RebuildList();
        }

        [PropertyOrder(_PRI_FILTER + 60)]
        [ButtonGroup(_FILTER_C), LabelText("Basic")]
        [GUIColor(nameof(ButtonLevel2))]
        public void Filter_Basic()
        {
            RebuildList(true);
        }

        [PropertyOrder(_PRI_FILTER + 40)]
        [ButtonGroup(_FILTER_C), LabelText("External")]
        [GUIColor(nameof(ButtonLevel2))]
        public void Filter_External()
        {
            RebuildList(true, DataReviewState.External);
        }

        [PropertyOrder(_PRI_FILTER + 30)]
        [ButtonGroup(_FILTER_B), LabelText("Locked")]
        [GUIColor(nameof(ButtonLevel1))]
        public void Filter_Locked()
        {
            RebuildList(true, DataReviewState.Locked);
        }

        [PropertyOrder(_PRI_FILTER + 10)]
        [ButtonGroup(_FILTER_B), LabelText("Missing")]
        [GUIColor(nameof(ButtonLevel1))]
        public void Filter_Missing()
        {
            RebuildList(true, DataReviewState.Missing);
        }

        [PropertyOrder(_PRI_FILTER + 20)]
        [ButtonGroup(_FILTER_B), LabelText("Not Set")]
        [GUIColor(nameof(ButtonLevel1))]
        public void Filter_NotSet()
        {
            RebuildList(true, DataReviewState.NotSet);
        }

        [PropertyOrder(_PRI_FILTER + 50)]
        [ButtonGroup(_FILTER_C), LabelText("Suggested")]
        [GUIColor(nameof(ButtonLevel2))]
        public void Filter_Suggested()
        {
            RebuildList(true, DataReviewState.Suggested);
        }

        [PropertyOrder(_PRI_PROCESS + 30)]
        [TabGroup(_TABS, _PROCESS)]
        [ButtonGroup(_PROCESS_A)]
        public void ProcessAllNotSet()
        {
            ProcessAllNotSet(0);
        }

        [PropertyOrder(_PRI_PROCESS + 10)]
        [TabGroup(_TABS, _PROCESS)]
        [ButtonGroup(_PROCESS_A)]
        public void ProcessAllNotSet1()
        {
            ProcessAllNotSet(1);
        }

        [PropertyOrder(_PRI_PROCESS + 10)]
        [TabGroup(_TABS, _PROCESS)]
        [ButtonGroup(_PROCESS_A)]
        public void ProcessAllNotSet2()
        {
            ProcessAllNotSet(2);
        }

        [PropertyOrder(_PRI_PROCESS + 20)]
        [TabGroup(_TABS, _PROCESS)]
        [ButtonGroup(_PROCESS_A)]
        public void ProcessAllNotSet3()
        {
            ProcessAllNotSet(3);
        }

        [PropertyOrder(_PRI_PROCESS + 50)]
        [ButtonGroup(_PROCESS_B)]
        public void ProcessAllSuggested()
        {
            var eligible = index.CountKeys_NoAlloc(k => k.state == DataReviewState.Suggested);

            using (var progress = new EditorOnlyProgressBar(
                       "Updating to external model...",
                       eligible,
                       true,
                       1
                   ))
            {
                for (var i = 0; i < index.Count; i++)
                {
                    if (progress.Cancellable && progress.Cancelled)
                    {
                        break;
                    }

                    var item = index.at[i];

                    if (item.state == DataReviewState.Suggested)
                    {
                        progress.Increment1AndShowProgress(item.data.name);
                        item.SetExternalToSuggestion();
                    }
                }
            }
        }

        [PropertyOrder(_PRI_BASE)]
        [Button]
        public void Rebuild()
        {
            RebuildList();
        }

        [PropertyOrder(_PRI_SORT + 00)]
        [TabGroup(_TABS, _SORT)]
        [ButtonGroup(_SORT_A)]
        [GUIColor(nameof(ButtonLevel1))]
        public void SortPiecesAsc()
        {
            using (_PRF_SortPiecesAsc.Auto())
            {
                index.SortByValue(
                    (item1, item2) =>
                    {
                        if ((item1 == null) && (item2 == null))
                        {
                            return 0;
                        }

                        if (item1 == null)
                        {
                            return 1;
                        }

                        if (item2 == null)
                        {
                            return -1;
                        }

                        return item1.elements.CompareTo(item2.elements);
                    }
                );
            }
        }

        [PropertyOrder(_PRI_SORT + 30)]
        [ButtonGroup(_SORT_B)]
        [GUIColor(nameof(ButtonLevel2))]
        public void SortPiecesDesc()
        {
            using (_PRF_SortPiecesDesc.Auto())
            {
                index.SortByValue(
                    (item2, item1) =>
                    {
                        if ((item1 == null) && (item2 == null))
                        {
                            return 0;
                        }

                        if (item1 == null)
                        {
                            return 1;
                        }

                        if (item2 == null)
                        {
                            return -1;
                        }

                        return item1.elements.CompareTo(item2.elements);
                    }
                );
            }
        }

        [PropertyOrder(_PRI_SORT + 20)]
        [ButtonGroup(_SORT_A)]
        [GUIColor(nameof(ButtonLevel1))]
        public void SortTrianglesAsc()
        {
            using (_PRF_SortTrianglesAsc.Auto())
            {
                index.SortByValue(
                    (item1, item2) =>
                    {
                        if ((item1 == null) && (item2 == null))
                        {
                            return 0;
                        }

                        if (item1 == null)
                        {
                            return 1;
                        }

                        if (item2 == null)
                        {
                            return -1;
                        }

                        return item1.triangles.decomposed.CompareTo(item2.triangles.decomposed);
                    }
                );
            }
        }

        [PropertyOrder(_PRI_SORT + 50)]
        [ButtonGroup(_SORT_B)]
        [GUIColor(nameof(ButtonLevel2))]
        public void SortTrianglesDesc()
        {
            using (_PRF_SortTrianglesDesc.Auto())
            {
                index.SortByValue(
                    (item2, item1) =>
                    {
                        if ((item1 == null) && (item2 == null))
                        {
                            return 0;
                        }

                        if (item1 == null)
                        {
                            return 1;
                        }

                        if (item2 == null)
                        {
                            return -1;
                        }

                        return item1.triangles.decomposed.CompareTo(item2.triangles.decomposed);
                    }
                );
            }
        }

        [PropertyOrder(_PRI_SORT + 10)]
        [ButtonGroup(_SORT_A)]
        [GUIColor(nameof(ButtonLevel1))]
        public void SortVerticesAsc()
        {
            using (_PRF_SortVerticesAsc.Auto())
            {
                index.SortByValue(
                    (item1, item2) =>
                    {
                        if ((item1 == null) && (item2 == null))
                        {
                            return 0;
                        }

                        if (item1 == null)
                        {
                            return 1;
                        }

                        if (item2 == null)
                        {
                            return -1;
                        }

                        return item1.vertices.decomposed.CompareTo(item2.vertices.decomposed);
                    }
                );
            }
        }

        [PropertyOrder(_PRI_SORT + 40)]
        [ButtonGroup(_SORT_B)]
        [GUIColor(nameof(ButtonLevel2))]
        public void SortVerticesDesc()
        {
            using (_PRF_SortVerticesDesc.Auto())
            {
                index.SortByValue(
                    (item2, item1) =>
                    {
                        if ((item1 == null) && (item2 == null))
                        {
                            return 0;
                        }

                        if (item1 == null)
                        {
                            return 1;
                        }

                        if (item2 == null)
                        {
                            return -1;
                        }

                        return item1.vertices.decomposed.CompareTo(item2.vertices.decomposed);
                    }
                );
            }
        }

        /// <inheritdoc />
        protected override async AppaTask WhenEnabled()
        {
            await base.WhenEnabled();

            using (_PRF_WhenEnabled.Auto())
            {
                if (!AppalachiaApplication.IsPlayingOrWillPlay)
                {
                    RebuildList();
                }
            }
        }

        private void ProcessAllNotSet(int targetPieces)
        {
            var eligible = index.CountKeys_NoAlloc(k => k.state == DataReviewState.NotSet);

            using (var progress = new EditorOnlyProgressBar("Decomposing colliders...", eligible, true, 1))
            {
                for (var i = 0; i < index.Count; i++)
                {
                    if (progress.Cancellable && progress.Cancelled)
                    {
                        break;
                    }

                    var item = index.at[i];

                    if (item.state == DataReviewState.NotSet)
                    {
                        progress.Increment1AndShowProgress(item.data.name);

                        if (targetPieces > 0)
                        {
                            item.targetPieces = 1;
                        }

                        item.ProcessAgain();
                    }
                }
            }
        }

        private void RebuildList(bool filter = false, DataReviewState filterState = DataReviewState.Basic)
        {
            using (_PRF_RebuildList.Auto())
            {
                DecomposedColliderSuggestionHelper.RebuildAssetsList();

                var dataList = AssetDatabaseManager.FindAssets<DecomposedColliderData>();
                var colliders = FindObjectsOfType<DecomposedCollider>();

                if (index == null)
                {
                    index = new DecomposedColliderDataReviewItemIndex(dataList.Count);
                }

                index.Changed.Event += OnChanged;
                index.Clear();

                for (var i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];

                    if (data.originalMesh == null)
                    {
                        Context.Log.Warn(ZString.Format("No mesh for {0}", data.name), data);
                        continue;
                    }

                    if (filter)
                    {
                        if (data.state != filterState)
                        {
                            continue;
                        }
                    }

                    var item = new DecomposedColliderDataReviewItem
                    {
                        data = data,
                        dc = colliders.FirstOrDefault_NoAlloc(c => c.data == data),
                        vertices =
                            new DecomposedColliderDataReviewItemData
                            {
                                original = data.originalMesh.vertexCount
                            },
                        triangles =
                            new DecomposedColliderDataReviewItemData
                            {
                                original = data.originalMesh.triangles.Length / 3
                            },
                        replacementReview = new DecomposedColliderReplacementReviewItem()
                    };

                    for (var j = 0; j < data.elements.Count; j++)
                    {
                        var element = data.elements[j];

                        if (element == default)
                        {
                            continue;
                        }

                        if (element.mesh == null)
                        {
                            continue;
                        }

                        item.vertices.decomposed += element.mesh.vertexCount;
                        item.triangles.decomposed += element.mesh.triangles.Length / 3;
                    }

                    if (data.externalModel == null)
                    {
                        item.SuggestExternal();
                    }

                    item.targetPieces = 8;

                    index.Add(data, item);
                }

                SortPiecesDesc();
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_RebuildList =
            new ProfilerMarker(_PRF_PFX + nameof(RebuildList));

        private static readonly ProfilerMarker _PRF_SortPiecesAsc =
            new ProfilerMarker(_PRF_PFX + nameof(SortPiecesAsc));

        private static readonly ProfilerMarker _PRF_SortPiecesDesc =
            new ProfilerMarker(_PRF_PFX + nameof(SortPiecesDesc));

        private static readonly ProfilerMarker _PRF_SortTrianglesAsc =
            new ProfilerMarker(_PRF_PFX + nameof(SortTrianglesAsc));

        private static readonly ProfilerMarker _PRF_SortTrianglesDesc =
            new ProfilerMarker(_PRF_PFX + nameof(SortTrianglesDesc));

        private static readonly ProfilerMarker _PRF_SortVerticesAsc =
            new ProfilerMarker(_PRF_PFX + nameof(SortVerticesAsc));

        private static readonly ProfilerMarker _PRF_SortVerticesDesc =
            new ProfilerMarker(_PRF_PFX + nameof(SortVerticesDesc));

        #endregion

        #region UI

        private const int _PRI_BASE = 0;

        private const int _PRI_PROCESS = _PRI_BASE + 1000;
        private const int _PRI_FILTER = _PRI_BASE + 2000;
        private const int _PRI_SORT = _PRI_BASE + 3000;
        private const int _PRI_ITEMS = _PRI_BASE + 4000;

        private const string _l_ = "/";
        private const string _TABS = "TABS";
        private const string _TABS_ = _TABS + _l_;

        private const string _SUBA = "SUBA";
        private const string _SUBB = "SUBB";
        private const string _SUBC = "SUBC";

        private const string _PROCESS = "Process";
        private const string _PROCESS_ = _TABS_ + _PROCESS + _l_;
        private const string _PROCESS_A = _PROCESS_ + _SUBA;
        private const string _PROCESS_B = _PROCESS_ + _SUBB;
        private const string _PROCESS_C = _PROCESS_ + _SUBC;

        private const string _FILTER = "Filter";
        private const string _FILTER_ = _TABS_ + _FILTER + _l_;
        private const string _FILTER_A = _FILTER_ + _SUBA;
        private const string _FILTER_B = _FILTER_ + _SUBB;
        private const string _FILTER_C = _FILTER_ + _SUBC;

        private const string _SORT = "Sort";
        private const string _SORT_ = _TABS_ + _SORT + _l_;
        private const string _SORT_A = _SORT_ + _SUBA;
        private const string _SORT_B = _SORT_ + _SUBB;
        private const string _SORT_C = _SORT_ + _SUBC;

        private Color ButtonLevel1 => ColorPrefs.ButtonFade90;
        private Color ButtonLevel2 => ColorPrefs.ButtonFade80;
        private Color ButtonLevel3 => ColorPrefs.ButtonFade70;

        #endregion
    }
}

#endif
