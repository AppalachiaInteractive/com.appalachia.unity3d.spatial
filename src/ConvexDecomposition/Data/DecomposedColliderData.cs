#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Appalachia.CI.Integration.Assets;
using Appalachia.CI.Integration.FileSystem;
using Appalachia.Core.Assets;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Core.Extensions;
using Appalachia.Core.Filtering;
using Appalachia.Core.Preferences;
using Appalachia.Core.Preferences.Globals;
using Appalachia.Core.Scriptables;
using Appalachia.Core.Shading;
using Appalachia.Editing.Assets;
using Appalachia.Editing.Core;
using Appalachia.Editing.Debugging.Handle;
using Appalachia.Editing.Scene.Prefabs;
using Appalachia.Jobs.MeshData;
using Appalachia.Rendering.Prefabs.Rendering;
using Appalachia.Simulation.Core.Metadata.Materials;
using Appalachia.Simulation.Core.Selections;
using Appalachia.Spatial.ConvexDecomposition.Data.Review;
using Appalachia.Spatial.ConvexDecomposition.Generation;
using Appalachia.Utility.Colors;
using Appalachia.Utility.Constants;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Data
{
    public delegate void OnPostDecompose();

    public class DecomposedColliderData : SelfSavingScriptableObject<DecomposedColliderData>
    {
#region UI Groups

        private const string _PRF_PFX = nameof(DecomposedColliderData) + ".";

        private const string _l_ = "/";
        private const string _TABS = "TABS";
        private const string _TABS_ = _TABS + _l_;

        private const string _SUBA = "SUBA";
        private const string _SUBB = "SUBB";
        private const string _SUBC = "SUBC";
        private const string _SUBD = "SUBD";
        private const string _SUBE = "SUBE";
        private const string _SUBF = "SUBF";
        private const string _SUBG = "SUBG";
        private const string _SUBH = "SUBH";
        private const string _SUBI = "SUBI";
        private const string _SUBJ = "SUBJ";

        private const string _SUBA_ = _SUBA + _l_;
        private const string _SUBB_ = _SUBB + _l_;
        private const string _SUBC_ = _SUBC + _l_;
        private const string _SUBD_ = _SUBD + _l_;
        private const string _SUBE_ = _SUBE + _l_;
        private const string _SUBF_ = _SUBF + _l_;
        private const string _SUBG_ = _SUBG + _l_;
        private const string _SUBH_ = _SUBH + _l_;
        private const string _SUBI_ = _SUBI + _l_;
        private const string _SUBJ_ = _SUBJ + _l_;

        private const string _SHARED = "Shared";
        private const string _SHARED_ = _TABS_ + _SHARED + _l_;
        private const string _SHARED_A = _SHARED_ + _SUBA;
        private const string _SHARED_B = _SHARED_ + _SUBB;
        private const string _SHARED_C = _SHARED_ + _SUBC;
        private const string _SHARED_D = _SHARED_ + _SUBD;
        private const string _SHARED_E = _SHARED_ + _SUBE;

        private const string _REFS = "Refs";
        private const string _REFS_ = _TABS_ + _REFS + _l_;
        private const string _REFS_A = _REFS_ + _SUBA;
        private const string _REFS_B = _REFS_ + _SUBB;
        private const string _REFS_C = _REFS_ + _SUBC;
        private const string _REFS_D = _REFS_ + _SUBD;
        private const string _REFS_E = _REFS_ + _SUBE;

        private const string _DECOMPOSE = "Decompose";
        private const string _DECOMPOSE_ = _TABS_ + _DECOMPOSE + _l_;
        private const string _DECOMPOSE_A = _DECOMPOSE_ + _SUBA;
        private const string _DECOMPOSE_B = _DECOMPOSE_ + _SUBB;
        private const string _DECOMPOSE_C = _DECOMPOSE_ + _SUBC;
        private const string _DECOMPOSE_D = _DECOMPOSE_ + _SUBD;
        private const string _DECOMPOSE_E = _DECOMPOSE_ + _SUBE;

        private const string _EXTERNAL = "External";
        private const string _EXTERNAL_ = _TABS_ + _EXTERNAL + _l_;
        private const string _EXTERNAL_A = _EXTERNAL_ + _SUBA;
        private const string _EXTERNAL_B = _EXTERNAL_ + _SUBB;
        private const string _EXTERNAL_C = _EXTERNAL_ + _SUBC;
        private const string _EXTERNAL_D = _EXTERNAL_ + _SUBD;
        private const string _EXTERNAL_E = _EXTERNAL_ + _SUBE;

        private const string _RESULTS = "Results";

        private const string _PHYSICS = "Physics";
        private const string _TABS_PHYSICS = _TABS + "/" + _PHYSICS + "/" + _TABS;
        private const string _MODEL = "Model";
        private const string _SWAP = "Swap";
        private const string _ASSIGN = "Assign";
        private const string _ASSIGNALL = "Assign All";
        private const string _TABS_PHYSICS_SWAP_ = _TABS_PHYSICS + "/" + _SWAP;
        private const string _TABS_PHYSICS_SWAP_FROM = _TABS_PHYSICS_SWAP_ + "/" + "Swap From";
        private const string _TABS_PHYSICS_SWAP_TO = _TABS_PHYSICS_SWAP_ + "/" + "Swap To";

        private const string _MAINTENANCE = "Maintenance";
        private const string _MAINTENANCE_ = _MAINTENANCE + _l_;
        private const string _MAINTENANCE1 = _MAINTENANCE + "1";
        private const string _MAINTENANCE2 = _MAINTENANCE + "2";
        private const string _MAINTENANCE3 = _MAINTENANCE + "3";
        private const string _MAINTENANCE4 = _MAINTENANCE + "4";
        private const string _MAINTENANCE5 = _MAINTENANCE + "5";
        private const string _MAINTENANCE6 = _MAINTENANCE + "6";
        private const string _MAINTENANCE1_ = _MAINTENANCE_ + _MAINTENANCE1;
        private const string _MAINTENANCE2_ = _MAINTENANCE_ + _MAINTENANCE2;
        private const string _MAINTENANCE3_ = _MAINTENANCE_ + _MAINTENANCE3;
        private const string _MAINTENANCE4_ = _MAINTENANCE_ + _MAINTENANCE4;
        private const string _MAINTENANCE5_ = _MAINTENANCE_ + _MAINTENANCE5;
        private const string _MAINTENANCE6_ = _MAINTENANCE_ + _MAINTENANCE6;

#endregion

#region Fields and Properties

        public const string childName = "COLLIDERS";

        private const int _PRI_LOCKED = -1000;
        private const int _PRI_SHARED = _PRI_LOCKED + 1000;
        private const int _PRI_REFS = _PRI_LOCKED + 2000;
        private const int _PRI_DECOMPOSE = _PRI_LOCKED + 3000;
        private const int _PRI_EXTERNAL = _PRI_LOCKED + 4000;
        private const int _PRI_RESULTS = _PRI_LOCKED + 5000;
        private const int _PRI_PHYSICS = _PRI_LOCKED + 6000;

        [GUIColor(nameof(_lockColor))]
        [ToggleLeft]
        [PropertyOrder(_PRI_LOCKED)]
        [OnValueChanged(nameof(OnLocked))]
        public bool locked;

        [NonSerialized]
        [ShowInInspector]
        [GUIColor(nameof(_indexColor))]
        [EnableIf(nameof(_domaxi))]
        [PropertyRange(0, nameof(_max_index))]
        [PropertyOrder(_PRI_LOCKED + 1)]
        public int selectedColliderIndex;

        private void OnLocked()
        {
            if (!locked)
            {
                return;
            }

            DeleteGizmoComponents();
        }

#region Shared

        [TabGroup(_TABS, _SHARED)]
        [HorizontalGroup(_SHARED_A)]
        [PropertyOrder(_PRI_SHARED + 0)]
        [GUIColor(nameof(limitationColors))]
        [SmartLabel]
        [ShowInInspector]
        public static PREF<bool> generationDisabled;

        [HorizontalGroup(_SHARED_A)]
        [PropertyOrder(_PRI_SHARED + 1)]
        [GUIColor(nameof(limitationColors))]
        [SmartLabel]
        [ShowInInspector]
        public static PREF<int> maximumIterations;

        [HorizontalGroup(_SHARED_B)]
        [PropertyOrder(_PRI_SHARED + 2)]
        [SmartLabel]
        [ShowInInspector]
        public static PREF<bool> basicLogging;

        [HorizontalGroup(_SHARED_B)]
        [PropertyOrder(_PRI_SHARED + 3)]
        [SmartLabel]
        [ShowInInspector]
        public static PREF<bool> performanceLogging;

        [HorizontalGroup(_SHARED_C)]
        [PropertyOrder(_PRI_SHARED + 4)]
        [SmartLabel]
        [ShowInInspector]
        public static PREF<bool> extraLogging;

        [HorizontalGroup(_SHARED_C)]
        [PropertyOrder(_PRI_SHARED + 5)]
        [SmartLabel]
        [ShowInInspector]
        public static PREF<bool> dirtyLogging;

#endregion

#region Refs

        [TabGroup(_TABS, _REFS)]
        [HorizontalGroup(_REFS_A)]
        [PropertyOrder(_PRI_REFS + 0)]
        [SmartLabel]
        [ToggleLeft]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(locked))]
        public bool migrated;

        [HorizontalGroup(_REFS_A)]
        [PropertyOrder(_PRI_REFS + 10)]
        [SmartLabel]
        [ToggleLeft]
        [SerializeField]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public bool externallyCreated;

        [TabGroup(_TABS, _REFS)]
        [PropertyOrder(_PRI_REFS + 20)]
        [SmartLabel]
        [SerializeField]
        [DisableIf(nameof(originalMeshDisabled))]
        [DisableIf(nameof(locked))]
        public Mesh originalMesh;

        [TabGroup(_TABS, _REFS)]
        [PropertyOrder(_PRI_REFS + 25)]
        [SmartLabel]
        [SerializeField]
        [DisableIf(nameof(originalMeshDisabled))]
        [DisableIf(nameof(locked))]
        [PropertyRange(0, 10)]
        [OnValueChanged(nameof(ResetOriginalMesh))]
        public int meshOffset;

        private void ResetOriginalMesh()
        {
            originalMesh = null;
        }

        [TabGroup(_TABS, _REFS)]
        [PropertyOrder(_PRI_REFS + 30)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public float3 localPosition;

        [TabGroup(_TABS, _REFS)]
        [PropertyOrder(_PRI_REFS + 40)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public quaternion localRotation;

        [TabGroup(_TABS, _REFS)]
        [PropertyOrder(_PRI_REFS + 50)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public float3 localScale;

        [TabGroup(_TABS, _REFS)]
        [PropertyOrder(_PRI_REFS + 60)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public float originalScale;

#endregion

#region Decompose

        [TabGroup(_TABS, _DECOMPOSE)]
        [PropertyOrder(_PRI_DECOMPOSE + 0)]
        [PropertyRange(1.0f, 5.0f)]
        [GUIColor(nameof(successColor))]
        [SmartLabel]
        [SerializeField]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public float successThreshold = 1.25f;

        [TabGroup(_TABS, _DECOMPOSE)]
        [PropertyOrder(_PRI_DECOMPOSE + 10)]
        [SmartLabel]
        [ToggleLeft]
        [DisableIf(nameof(externallyCreated))]
        [SerializeField]
        [DisableIf(nameof(locked))]
        public bool fillHoles = true;

        [TabGroup(_TABS, _DECOMPOSE)]
        [PropertyOrder(_PRI_DECOMPOSE + 20)]
        [InlineProperty]
        [HideLabel]
        [LabelWidth(0)]
        [SerializeField]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public ConvexMeshSettings settings;

#endregion

#region External

        private Color _buttonColor => Color.white;

        [TabGroup(_TABS, _EXTERNAL)]
        [PropertyOrder(_PRI_EXTERNAL + 0)]
        [DisableIf(nameof(locked))]
        [SmartLabel]
        [SmartInlineButton(
            nameof(UnassignExternalMeshes),
            "Remove",
            false,
            false,
            nameof(_buttonColor),
            nameof(_disableExternalUnassign)
        )]
        [SmartInlineButton(
            nameof(LoadExternalMeshes),
            "Load",
            false,
            false,
            nameof(_buttonColor),
            nameof(_disableExternalModify)
        )]
        [AssetsOnly]
        [SerializeField]
        [DisableIf(nameof(locked))]
        public GameObject externalModel;

        [TabGroup(_TABS, _EXTERNAL)]
        [PropertyOrder(_PRI_EXTERNAL + 10)]
        [ValueDropdown(nameof(_assets))]
        [SmartInlineButton(
            nameof(ReplaceExternalModel),
            "Replace",
            false,
            false,
            nameof(_buttonColor),
            nameof(_disableReplaceFunctions)
        )]
        [SmartInlineButton(
            nameof(SuggestExternal),
            "Suggest",
            false,
            false,
            nameof(_buttonColor),
            nameof(_disableSuggestExternal)
        )]
        [SmartInlineButton(
            nameof(SelectReplacement),
            "Select",
            false,
            false,
            nameof(_buttonColor),
            nameof(_disableReplaceFunctions)
        )]
        [SerializeField]
        [SmartLabel]
        [OnValueChanged(nameof(UpdateReplacementReview))]
        public GameObject suggestedReplacementModel;

        [TabGroup(_TABS, _EXTERNAL)]
        [PropertyOrder(_PRI_EXTERNAL + 20)]
        [DisableIf(nameof(_disableReplacementReview))]
        [HideReferenceObjectPicker]
        [SerializeField]
        [SmartLabel]
        [SmartInlineButton(
            nameof(UpdateReplacementReview),
            "Refresh",
            false,
            DisableIf = nameof(_disableReplaceFunctions)
        )]
        public DecomposedColliderReplacementReviewItem replacementReview;

#endregion

#region Results

        [TabGroup(_TABS, _RESULTS)]
        [PropertyOrder(_PRI_RESULTS + 0)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public float originalVolume;

        [TabGroup(_TABS, _RESULTS)]
        [PropertyOrder(_PRI_RESULTS + 20)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public float decomposedVolume;

        [TabGroup(_TABS, _RESULTS)]
        [PropertyOrder(_PRI_RESULTS + 30)]
        [SmartLabel]
        [SerializeField]
        [ReadOnly]
        [DisableIf(nameof(externallyCreated))]
        [DisableIf(nameof(locked))]
        public double executionTime;

        [Title("Selected")]
        [TabGroup(_TABS, _RESULTS)]
        [PropertyOrder(_PRI_RESULTS + 40)]
        [SmartLabel]
        [ShowInInspector]
        [HideDuplicateReferenceBox]
        [DisableIf(nameof(locked))]
        public DecomposedColliderElement selected
        {
            get
            {
                selectedColliderIndex = math.clamp(selectedColliderIndex, 0, elements?.Count ?? 0);

                if (elements?.Count == 0)
                {
                    return default;
                }

                return elements?[selectedColliderIndex];
            }
            set => elements[selectedColliderIndex] = value;
        }

        [Title("Results")]
        [TabGroup(_TABS, _RESULTS)]
        [PropertyOrder(_PRI_RESULTS + 50)]
        [ListDrawerSettings(
            NumberOfItemsPerPage = 5,
            HideAddButton = true,
            HideRemoveButton = true,
            DraggableItems = false
        )]
        [OnValueChanged(nameof(ClampColliderIndex), true)]
        [SmartLabel]
        [SerializeField]
        [HideDuplicateReferenceBox]
        [DisableIf(nameof(locked))]
        public List<DecomposedColliderElement> elements = new();

#endregion

#region Physics

        [TabGroup(_TABS, _PHYSICS)]
        [PropertyOrder(_PRI_PHYSICS + 0)]
        [TabGroup(_TABS_PHYSICS, _MODEL)]
        [GUIColor(nameof(colorSelectorModel))]
        [DisableIf(nameof(locked))]
        public PhysicMaterialWrapper materialModel;

        [TabGroup(_TABS_PHYSICS, _MODEL)]
        [PropertyOrder(_PRI_PHYSICS + 10)]
        [GUIColor(nameof(colorSelectorModel))]
        [ShowInInspector]
        [HideLabel]
        [DisableIf(nameof(locked))]
        public PhysicMaterialLookupSelection modelSelector;

        [TabGroup(_TABS_PHYSICS, _ASSIGN)]
        [PropertyOrder(_PRI_PHYSICS + 60)]
        [GUIColor(nameof(colorSelectorAssign))]
        [ShowInInspector]
        [HideLabel]
        [DisableIf(nameof(locked))]
        public PhysicMaterialLookupSelection assignSelector;

        [TabGroup(_TABS_PHYSICS, _ASSIGNALL)]
        [PropertyOrder(_PRI_PHYSICS + 70)]
        [GUIColor(nameof(colorSelectorAssignAll))]
        [ShowInInspector]
        [HideLabel]
        [DisableIf(nameof(locked))]
        public PhysicMaterialLookupSelection assignAllSelector;

        [TabGroup(_TABS_PHYSICS, _SWAP)]
        [PropertyOrder(_PRI_PHYSICS + 89)]
        [BoxGroup(_TABS_PHYSICS_SWAP_FROM)]
        [GUIColor(nameof(colorSelectorSwap))]
        [ShowInInspector]
        [SmartLabel]
        [DisableIf(nameof(locked))]
        public static PhysicMaterialWrapper swapFrom;

        [BoxGroup(_TABS_PHYSICS_SWAP_TO)]
        [PropertyOrder(_PRI_PHYSICS + 90)]
        [GUIColor(nameof(colorSelectorSwap))]
        [ShowInInspector]
        [HideLabel]
        [DisableIf(nameof(locked))]
        public static PhysicMaterialLookupSelection swapFromSelector;

        [BoxGroup(_TABS_PHYSICS_SWAP_TO)]
        [PropertyOrder(_PRI_PHYSICS + 99)]
        [GUIColor(nameof(colorSelectorSwap))]
        [ShowInInspector]
        [SmartLabel]
        [SmartInlineButton(nameof(Swap), DisableIf = nameof(_canNotSwap))]
        [DisableIf(nameof(locked))]
        public static PhysicMaterialWrapper swapTo;

        [BoxGroup(_TABS_PHYSICS_SWAP_TO)]
        [PropertyOrder(_PRI_PHYSICS + 100)]
        [GUIColor(nameof(colorSelectorSwap))]
        [ShowInInspector]
        [HideLabel]
        [DisableIf(nameof(locked))]
        public static PhysicMaterialLookupSelection swapToSelector;

#endregion

#region State

        private DecomposedCollider _parent;

        private MeshObjectWrapper _meshObject;

        internal MeshObjectWrapper meshObject
        {
            get
            {
                if ((_meshObject == null) || !_meshObject.data.isCreated)
                {
                    _meshObject = MeshObjectManager.GetByMesh(originalMesh, true);
                }

                return _meshObject;
            }
        }

#endregion

#region Obsolete

        [HideInInspector]
        [Obsolete]
        public List<DecomposedColliderPiece> pieces = new();

        [HideInInspector]
        [Obsolete]
        public bool _migratedPieces;

#endregion

#endregion

#region Profiling

        private static readonly ProfilerMarker _PRF_ClampColliderIndex =
            new(_PRF_PFX + nameof(ClampColliderIndex));

        private static readonly ProfilerMarker _PRF_CheckForMissingAssets =
            new(_PRF_PFX + nameof(CheckForMissingAssets));

        private static readonly ProfilerMarker _PRF_GetSaveDirectory =
            new(_PRF_PFX + nameof(GetSaveDirectory));

        private static readonly ProfilerMarker _PRF_ConfirmMeshNames =
            new(_PRF_PFX + nameof(ConfirmMeshNames));

        private static readonly ProfilerMarker _PRF_CheckOriginalMesh =
            new(_PRF_PFX + nameof(CheckOriginalMesh));

        private static readonly ProfilerMarker _PRF_GetOriginalMesh = new(_PRF_PFX + nameof(GetOriginalMesh));
        private static readonly ProfilerMarker _PRF_DeleteOldMeshes = new(_PRF_PFX + nameof(DeleteOldMeshes));

        private static readonly ProfilerMarker _PRF_UpdateTransformData =
            new(_PRF_PFX + nameof(UpdateTransformData));

        private static readonly ProfilerMarker _PRF_Save = new(_PRF_PFX + nameof(Save));
        private static readonly ProfilerMarker _PRF_ApplyMaterials = new(_PRF_PFX + nameof(ApplyMaterials));
        private static readonly ProfilerMarker _PRF_DeleteSelected = new(_PRF_PFX + nameof(DeleteSelected));

        private static readonly ProfilerMarker _PRF_InitializeColliders =
            new(_PRF_PFX + nameof(InitializeColliders));

        private static readonly ProfilerMarker _PRF_ExecuteDecompositionExplicit =
            new(_PRF_PFX + nameof(ExecuteDecompositionExplicit));

        private static readonly ProfilerMarker _PRF_DecompositionRequired =
            new(_PRF_PFX + nameof(DecompositionRequired));

        private static readonly ProfilerMarker _PRF_GenerateDecomposedMeshes =
            new(_PRF_PFX + nameof(GenerateDecomposedMeshes));

        private static readonly ProfilerMarker _PRF_SetExternalModel =
            new(_PRF_PFX + nameof(SetExternalModel));

        private static readonly ProfilerMarker _PRF_LoadExternalMeshes =
            new(_PRF_PFX + nameof(LoadExternalMeshes));

#endregion

#region UI | Ranges

        private bool _domaxi => elements?.Count > 0;
        private int _max_index => elements?.Count - 1 ?? 0;

#endregion

#region UI | Colors

        private Color _lockColor => locked ? Colors.CadmiumYellow : Color.white;
        private Color _indexColor => ColorPrefs.Instance.DecomposedColliderSelectedIndex.v;
        private Color limitationColors => ColorPrefs.Instance.DecomposedColliderLimitationColors.v;
        private Color successColor => ColorPrefs.Instance.DecomposedColliderSuccessThreshold.v;

#endregion

#region UI | Enabled/Disabled

        private bool originalMeshDisabled => externallyCreated || (originalMesh != null);

        private bool _canNotSwap => swapFrom == swapTo;
        private bool _disableExternalUnassign => locked || (externalModel == null);
        private bool _disableExternalModify => !locked && (externalModel != null);
        private bool _disableReplaceFunctions => !locked && (externalModel != null);
        private bool _disableSuggestExternal => locked || (externallyCreated && (externalModel != null));
        private bool _disableReplacementReview => suggestedReplacementModel == null;

#endregion

#region Unity Events

        private void OnEnable()
        {
            modelSelector = LookupSelectionGenerator.CreatePhysicMaterialSelector(
                AssignMaterialModel,
                ColorPrefs.Instance.DecomposedColliderSelectorModel
            );
            swapFromSelector = LookupSelectionGenerator.CreatePhysicMaterialSelector(
                mat => swapFrom = mat,
                ColorPrefs.Instance.DecomposedColliderSelectorSwap
            );
            swapToSelector = LookupSelectionGenerator.CreatePhysicMaterialSelector(
                mat => swapTo = mat,
                ColorPrefs.Instance.DecomposedColliderSelectorSwap
            );
            assignSelector = LookupSelectionGenerator.CreatePhysicMaterialSelector(
                AssignMaterialToSelected,
                ColorPrefs.Instance.DecomposedColliderSelectorAssign
            );
            assignAllSelector = LookupSelectionGenerator.CreatePhysicMaterialSelector(
                AssignMaterialToAll,
                ColorPrefs.Instance.DecomposedColliderSelectorAssignAll
            );

            InitializeLogging();

            if (materialModel == null)
            {
                AssignMaterialModel(elements.MostFrequent(p => p.material));
            }

            if (settings == default)
            {
                settings = ConvexMeshSettings.Default();

                migrated = true;

                SetDirty();
            }

            if (locked)
            {
                return;
            }

#pragma warning disable 612

            var noElements = (elements == null) || (elements.Count == 0);
            var hasOldPieces = (pieces != null) && (pieces.Count > 0);

            if (noElements && hasOldPieces && !_migratedPieces)
            {
                if (elements == null)
                {
                    elements = new List<DecomposedColliderElement>();
                }

                _migratedPieces = true;

                var newIndex = 0;

                for (var i = 0; i < pieces.Count; i++)
                {
                    var piece = pieces[i];
                    if (piece == null)
                    {
                        continue;
                    }

                    var element =
                        new DecomposedColliderElement(piece.mesh, piece.material, piece.externalMesh)
                        {
                            index = newIndex
                        };

                    newIndex += 1;

                    if (element.material == null)
                    {
                        element.material = materialModel;
                    }

                    elements.Add(element);
                }
            }
#pragma warning restore 612
        }

        private void OnDisable()
        {
            if ((_meshObject == null) || !_meshObject.data.isCreated)
            {
                return;
            }

            _meshObject = default;
        }

#endregion

#region Helpers

        public DataReviewState state => GetState(this);

        public static DataReviewState GetState(DecomposedColliderData d)
        {
            return d == null
                ? DataReviewState.Missing
                : (d.elements == null) || (d.elements.Count == 0)
                    ? DataReviewState.NotSet
                    : d.locked
                        ? DataReviewState.Locked
                        : ((d.externalModel == null) || !d.externallyCreated) &&
                          (d.suggestedReplacementModel != null)
                            ? DataReviewState.Suggested
                            : d.externallyCreated
                                ? DataReviewState.External
                                : DataReviewState.Basic;
        }

        public event OnPostDecompose OnPostDecompose;

        public void ResetMeshObject()
        {
            if (locked)
            {
                return;
            }

            _meshObject = default;
        }

        private void SetIndices()
        {
            if (locked)
            {
                return;
            }

            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                element.index = i;
                elements[i] = element;
            }
        }

        private void ClampColliderIndex()
        {
            if (locked)
            {
                return;
            }

            using (_PRF_ClampColliderIndex.Auto())
            {
                SetIndices();
                selectedColliderIndex = math.clamp(selectedColliderIndex, 0, _max_index);
            }
        }

        private void InitializeLogging()
        {
            if (generationDisabled == null)
            {
                generationDisabled = PREFS.REG("Decomposed Colliders", "Generation Disabled", false);
            }

            if (maximumIterations == null)
            {
                maximumIterations = PREFS.REG("Decomposed Colliders", "Maximum Iterations", 3);
            }

            if (basicLogging == null)
            {
                basicLogging = PREFS.REG("Decomposed Colliders", "Basic Logging", false);
            }

            if (performanceLogging == null)
            {
                performanceLogging = PREFS.REG("Decomposed Colliders", "Performance Logging", true);
            }

            if (extraLogging == null)
            {
                extraLogging = PREFS.REG("Decomposed Colliders", "Extra Logging", false);
            }

            if (dirtyLogging == null)
            {
                dirtyLogging = PREFS.REG("Decomposed Colliders", "Dirty Logging", false);
            }
        }

        public string GetSaveDirectory(Mesh m)
        {
            if (locked)
            {
                return null;
            }

            using (_PRF_GetSaveDirectory.Auto())
            {
                var originalMeshPath = AssetDatabaseManager.GetAssetPath(m);

                if (string.IsNullOrWhiteSpace(originalMeshPath))
                {
                    Debug.LogWarning($"Could not find mesh asset path for {name}.");

                    originalMeshPath = AssetPath;
                }

                var originalDirectory = AppaPath.GetDirectoryName(originalMeshPath);

                var newDirectory = AppaPath.Combine(originalDirectory, "Colliders");

                if (!AppaDirectory.Exists(newDirectory))
                {
                    AssetDatabaseManager.CreateFolder(originalDirectory, "Colliders");
                }

                newDirectory = newDirectory.Replace(Application.dataPath, "Assets");

                return newDirectory;
            }
        }

        public void PushParent(DecomposedCollider c)
        {
            _parent = c;
        }

        public void CheckOriginalMesh(GameObject go)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_CheckOriginalMesh.Auto())
            {
                InitializeLogging();

                if (basicLogging.v)
                {
                    Debug.Log($"Checking original mesh for {name}.");
                }

                if ((originalMesh == null) ||
                    (originalMesh == _gizmoMesh) ||
                    originalMesh.name.Contains("GIZMO"))
                {
                    GetOriginalMesh(go, meshOffset);
                }

                if (originalMesh == null)
                {
                    return;
                }

                var modelPath = AssetDatabaseManager.GetAssetPath(originalMesh);
                var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;

                if (importer is not null && (Math.Abs(originalScale - importer.globalScale) > float.Epsilon))
                {
                    originalScale = importer.globalScale;

                    ResetMeshObject();

                    originalVolume = fillHoles ? meshObject.data.SolidVolume : meshObject.data.Volume;
                }

                if (originalVolume == 0.0f)
                {
                    originalVolume = fillHoles ? meshObject.data.SolidVolume : meshObject.data.Volume;
                }

                UpdateExternalModelImportSettings();
            }
        }

        private void GetOriginalMesh(GameObject go, int meshOffset)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_GetOriginalMesh.Auto())
            {
                InitializeLogging();

                if (basicLogging.v)
                {
                    Debug.Log($"Getting original mesh for {name}.");
                }

                var renderer = go.FilterComponents<MeshRenderer>(true).CheapestRenderer(meshOffset);

                if (renderer == null)
                {
                    originalMesh = null;
                    localPosition = float3.zero;
                    localRotation = quaternion.identity;
                    localScale = float3c.one;
                    return;
                }

                originalMesh = renderer.GetSharedMesh();
                var rt = renderer.transform;
                UpdateTransformData(rt);
                var ct = _parent.colliderTransform;
                ct.localPosition = localPosition;
                ct.localRotation = localRotation;
                ct.localScale = localScale;
            }
        }

        private void UpdateTransformData(Transform t)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_UpdateTransformData.Auto())
            {
                InitializeLogging();

                if (basicLogging.v)
                {
                    Debug.Log($"Updating transform data for {name}.");
                }

                if (t == null)
                {
                    return;
                }

                if ((Vector3) localPosition != t.localPosition)
                {
                    localPosition = t.localPosition;
                    if (dirtyLogging.v)
                    {
                        Debug.LogWarning("Setting dirty: cached position updated.");
                    }

                    SetDirty();
                }

                if (localRotation != t.localRotation)
                {
                    localRotation = t.localRotation;
                    if (dirtyLogging.v)
                    {
                        Debug.LogWarning("Setting dirty: cached rotation updated.");
                    }

                    SetDirty();
                }

                if ((Vector3) localScale != t.localScale)
                {
                    localScale = t.localScale;
                    if (dirtyLogging.v)
                    {
                        Debug.LogWarning("Setting dirty: cached scale updated.");
                    }

                    SetDirty();
                }
            }
        }

        public static string GetSaveDirectory(DecomposedCollider c, out string name)
        {
            using (_PRF_GetSaveDirectory.Auto())
            {
                var originalMesh = c.FilterComponents<MeshFilter>(true).CheapestMesh();

                if (originalMesh == null)
                {
                    Debug.LogError("Must assign meshes to this", c);
                    c.enabled = false;
                    name = null;
                    return null;
                }

                var originalMeshPath = AssetDatabaseManager.GetAssetPath(originalMesh);
                var originalDirectory = AppaPath.GetDirectoryName(originalMeshPath);

                var newDirectory = AppaPath.Combine(originalDirectory, "Colliders");

                if (!AppaDirectory.Exists(newDirectory))
                {
                    AssetDatabaseManager.CreateFolder(originalDirectory, "Colliders");
                }

                newDirectory = newDirectory.Replace(Application.dataPath, "Assets");

                name = originalMesh.name;

                return newDirectory;
            }
        }

#endregion

#region Maintenance

        [FoldoutGroup(_MAINTENANCE)]
        [ButtonGroup(_MAINTENANCE1_)]
        [DisableIf(nameof(locked))]
        public void CheckForMissingAssets()
        {
            if (locked)
            {
                return;
            }

            using (_PRF_CheckForMissingAssets.Auto())
            {
                if (!AssetDatabaseSaveManager.RequestSuspendImport(out var scope))
                {
                    return;
                }

                using (scope)
                {
                    InitializeLogging();

                    if (basicLogging.v)
                    {
                        Debug.Log($"Confirming validity for {name}.");
                    }

                    for (var i = elements.Count - 1; i >= 0; i--)
                    {
                        var element = elements[i];

                        if (!element.valid)
                        {
                            elements[i].Delete();
                            elements.RemoveAt(i);
                            if (dirtyLogging.v)
                            {
                                Debug.LogWarning("Setting dirty: Invalid element removed");
                            }

                            SetDirty();
                        }
                    }
                }
            }
        }

        [ButtonGroup(_MAINTENANCE1_)]
        [DisableIf(nameof(locked))]
        private void ConfirmMeshNames()
        {
            if (locked)
            {
                return;
            }

            using (_PRF_ConfirmMeshNames.Auto())
            {
                using (new AssetEditingScope())
                {
                    SetIndices();

                    for (var i = 0; i < elements.Count; i++)
                    {
                        elements[i].ConfirmMeshName(originalMesh, GetSaveDirectory(originalMesh));
                    }

                    var renderer = _parent.FilterComponents<MeshRenderer>(true).CheapestRenderer();
                    UpdateTransformData(renderer == null ? null : renderer.transform);
                }
            }
        }

        [ButtonGroup(_MAINTENANCE2_)]
        [DisableIf(nameof(locked))]
        private void DeleteOldMeshes(string saveDirectory)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_DeleteOldMeshes.Auto())
            {
                InitializeLogging();

                if (basicLogging.v)
                {
                    Debug.Log($"Deleting old meshes for {name}.");
                }

                var meshHash = elements.Select(p => p.mesh).ToHashSet();

                var existingMeshes = AssetDatabaseManager.FindAssets(
                    $"t:Mesh {originalMesh.name}",
                    new[] {saveDirectory}
                );

                for (var i = 0; i < existingMeshes.Length; i++)
                {
                    var path = AssetDatabaseManager.GUIDToAssetPath(existingMeshes[i]);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    var asset = AssetDatabaseManager.LoadAssetAtPath<Mesh>(path);

                    if (!meshHash.Contains(asset) && !AssetDatabaseManager.IsSubAsset(asset))
                    {
                        if (extraLogging.v)
                        {
                            Debug.LogWarning($"Would delete mesh at {path}");
                        }
                        else
                        {
                            AssetDatabaseManager.DeleteAsset(path);
                        }
                    }
                }
            }
        }

        [ButtonGroup(_MAINTENANCE2_)]
        public void SelectReview()
        {
            Selection.activeObject = DecomposedColliderDataReview.instance;
        }

        [ButtonGroup(_MAINTENANCE3_)]
        [DisableIf(nameof(locked))]
        public void ApplyMaterials()
        {
            if (locked)
            {
                return;
            }

            using (_PRF_ApplyMaterials.Auto())
            {
                InitializeLogging();

                if (basicLogging.v)
                {
                    Debug.Log($"Applying material for {name}.");
                }

                for (var i = 0; i < elements.Count; i++)
                {
                    elements[i].Apply(_parent.colliders[i]);
                }

                if (_gizmoMesh != null)
                {
                    _gizmoMesh.Clear();
                }
            }
        }

        [DisableIf(nameof(externallyCreated))]
        [ButtonGroup(_MAINTENANCE3_)]
        [DisableIf(nameof(locked))]
        private void DeleteSelected()
        {
            if (locked)
            {
                return;
            }

            using (_PRF_DeleteSelected.Auto())
            {
                selected.Delete();

                for (var i = elements.Count - 1; i >= 0; i--)
                {
                    var element = elements[i];

                    if ((element == default) || (element.mesh == null))
                    {
                        elements.RemoveAt(i);
                    }
                }

                if (dirtyLogging.v)
                {
                    Debug.LogWarning("Setting dirty");
                }

                SetDirty();

                SetIndices();
                CheckForMissingAssets();
                ApplyMaterials();
            }
        }

        [ButtonGroup(_MAINTENANCE4_)]
        [DisableIf(nameof(locked))]
        public void InitializeColliders()
        {
            InitializeColliders(_parent.gameObject);
        }

        [ButtonGroup(_MAINTENANCE4_)]
        [DisableIf(nameof(locked))]
        private void UpdateExternalModelImportSettings()
        {
            UpdateExternalModelImportSettings(externalModel, originalMesh);
        }

        public void InitializeColliders(GameObject go)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_InitializeColliders.Auto())
            {
                if (basicLogging.v)
                {
                    Debug.Log($"Initializing colliders for {name}.");
                }

                var foundColliderObj = false;
                var duplicates = false;

                for (var i = 0; i < go.transform.childCount; i++)
                {
                    var child = go.transform.GetChild(i);

                    if (child.name != childName)
                    {
                        continue;
                    }

                    if (foundColliderObj)
                    {
                        duplicates = true;
                    }
                    else
                    {
                        foundColliderObj = true;
                    }
                }

                if (duplicates)
                {
                    foundColliderObj = false;

                    if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
                    {
                        var asset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                        var pf = AssetDatabaseManager.LoadAssetAtPath<GameObject>(asset);

                        using (var mutable = pf.ToMutable())
                        {
                            for (var i = mutable.Mutable.transform.childCount - 1; i >= 0; i--)
                            {
                                var child = mutable.Mutable.transform.GetChild(i);

                                if (child.name != childName)
                                {
                                    continue;
                                }

                                if (foundColliderObj)
                                {
                                    try
                                    {
                                        child.gameObject.DestroySafely();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError(ex, pf);
                                    }
                                }
                                else
                                {
                                    foundColliderObj = true;
                                }
                            }
                        }

                        PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
                    }
                    else
                    {
                        for (var i = go.transform.childCount - 1; i >= 0; i--)
                        {
                            var child = go.transform.GetChild(i);

                            if (child.name != childName)
                            {
                                continue;
                            }

                            if (foundColliderObj)
                            {
                                try
                                {
                                    child.gameObject.DestroySafely();
                                    i -= 1;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex, go);
                                }
                            }
                            else
                            {
                                foundColliderObj = true;
                            }
                        }
                    }
                }
            }
        }

        [ButtonGroup(_MAINTENANCE5_)]
        public void ResetGizmoMaterial()
        {
            _gizmoMaterial.DestroySafely();
            _gizmoMaterial = null;

            if (_gizmoMesh != null)
            {
                _gizmoMesh.Clear();
            }
        }

        [ButtonGroup(_MAINTENANCE5_)]
        public void DeleteGizmoComponents()
        {
            /*
             if (_gizmoMaterial != null) _gizmoMaterial.DestroySafely();
             if (_gizmoMesh != null) _gizmoMesh.DestroySafely();
             */
            if (_gizmoMeshFilter == null)
            {
                _gizmoMeshFilter = _parent.colliderTransform.GetComponent<MeshFilter>();
            }

            if (_gizmoMeshRenderer == null)
            {
                _gizmoMeshRenderer = _parent.colliderTransform.GetComponent<MeshRenderer>();
            }

            if (_gizmoMeshFilter != null)
            {
                _gizmoMeshFilter.DestroySafely();
            }

            if (_gizmoMeshRenderer != null)
            {
                _gizmoMeshRenderer.DestroySafely();
            }

            _gizmoMaterial = null;
            _gizmoMaterials = null;
            _gizmoMesh = null;
            _gizmoSubmeshes = null;
            _gizmoMeshFilter = null;
            _gizmoMeshRenderer = null;
        }

#endregion

#region Physics

        private Color colorSelectorModel => ColorPrefs.Instance.DecomposedColliderSelectorModel.v;
        private Color colorSelectorSwap => ColorPrefs.Instance.DecomposedColliderSelectorSwap.v;
        private Color colorSelectorAssign => ColorPrefs.Instance.DecomposedColliderSelectorAssign.v;
        private Color colorSelectorAssignAll => ColorPrefs.Instance.DecomposedColliderSelectorAssignAll.v;

        private void Swap()
        {
            if (locked)
            {
                return;
            }

            SwapMaterial(swapFrom, swapTo);
        }

        public void SwapMaterial(PhysicMaterialWrapper old, PhysicMaterialWrapper newM)
        {
            if (locked)
            {
                return;
            }

            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].SwapMaterial(old, newM);
            }

            ApplyMaterials();
        }

        public void AssignMaterialModel(PhysicMaterialWrapper mat)
        {
            if (locked)
            {
                return;
            }

            if (materialModel == mat)
            {
                return;
            }

            materialModel = mat;
            if (dirtyLogging.v)
            {
                Debug.LogWarning("Setting dirty: material model updating");
            }

            SetDirty();
        }

        public void AssignMaterialToSelected(PhysicMaterialWrapper mat)
        {
            if (locked)
            {
                return;
            }

            if (selected.material == mat)
            {
                ApplyMaterials();
                return;
            }

            var s = selected;

            s.material = mat;

            selected = s;

            if (dirtyLogging.v)
            {
                Debug.LogWarning("Setting selected dirty: material assigned");
            }

            ApplyMaterials();
        }

        public void AssignMaterialToAll(PhysicMaterialWrapper mat)
        {
            if (locked)
            {
                return;
            }

            AssignMaterialModel(mat);

            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].AssignRecursive(mat);
            }

            ApplyMaterials();
        }

#endregion

#region Processing

        public void ExecuteDecompositionExplicit(
            GameObject go,
            int leveragedParts,
            int modifications,
            bool normal = false)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_ExecuteDecompositionExplicit.Auto())
            {
                if (!AssetDatabaseSaveManager.RequestSuspendImport(out var scope))
                {
                    return;
                }

                using (scope)
                {
                    if (generationDisabled.v)
                    {
                        return;
                    }

                    if (externallyCreated)
                    {
                        return;
                    }

                    leveragedParts += modifications;
                    leveragedParts = Mathf.Clamp(
                        leveragedParts,
                        1,
                        ConvexMeshSettings.Ranges.maxConvexHulls_MAX
                    );

                    CheckOriginalMesh(go);
                    InitializeColliders(go);

                    settings.Clamp();

                    if (originalMesh == null)
                    {
                        return;
                    }

                    var successVolume = originalVolume * successThreshold;

                    if (normal && !DecompositionRequired(successVolume))
                    {
                        return;
                    }

                    var startTime = DateTime.Now;

                    var meshes = GenerateDecomposedMeshes(leveragedParts, successVolume);

                    if (meshes == null)
                    {
                        return;
                    }

                    Save(meshes);

                    var duration = DateTime.Now - startTime;
                    executionTime = duration.TotalSeconds;

                    if (executionTime > 300)
                    {
                        settings.DefaultSettings();
                    }

                    CheckForMissingAssets();
                    ApplyMaterials();
                }
            }
        }

        private bool DecompositionRequired(float successVolume)
        {
            if (locked)
            {
                return false;
            }

            using (_PRF_DecompositionRequired.Auto())
            {
                if ((elements != null) &&
                    (elements.Count > 0) &&
                    elements.None_NoAlloc(d => d.mesh == null) &&
                    (decomposedVolume > 0) &&
                    ((decomposedVolume < successVolume) || (elements.Count == settings.maxConvexHulls)))
                {
                    return false;
                }

                return true;
            }
        }

        private List<Mesh> GenerateDecomposedMeshes(int leveragedParts, float successVolume)
        {
            if (locked)
            {
                return null;
            }

            using (_PRF_GenerateDecomposedMeshes.Auto())
            {
                if (!AssetDatabaseSaveManager.RequestSuspendImport(out var scope))
                {
                    return null;
                }

                using (scope)
                {
                    if (basicLogging.v)
                    {
                        Debug.Log($"Generating decomposed meshes for {name}.");
                    }

                    decomposedVolume = successVolume * 10.0f;

                    var m = meshObject.data;
                    originalVolume = fillHoles ? m.SolidVolume : m.Volume;

                    List<Mesh> meshes = null;

                    var initialLeveragedParts = leveragedParts;

                    if (initialLeveragedParts == 0)
                    {
                        leveragedParts = ExecuteMinimizingProcessingLoop(m, successVolume, ref meshes);
                    }
                    else
                    {
                        leveragedParts = ExecuteMaximizingProcessingLoop(
                            m,
                            initialLeveragedParts,
                            successVolume,
                            ref meshes
                        );
                    }

                    settings.maxConvexHulls = Mathf.Max(settings.maxConvexHulls, leveragedParts);

                    return meshes;
                }
            }
        }

        private int ExecuteMinimizingProcessingLoop(MeshObject m, float successVolume, ref List<Mesh> meshes)
        {
            if (locked)
            {
                return meshes.Count;
            }

            var iteration = 0;

            var previousVolumes = new float[maximumIterations.v];
            var previousCounts = new int[maximumIterations.v];
            var significantChangeThreshold = successVolume * -.04f;

            var shouldContinue = true;
            while (shouldContinue)
            {
                settings.Clamp();

                ExecuteGenerationIteration(name, fillHoles, settings, m, originalMesh, ref meshes);

                decomposedVolume = meshes.GetVolume();
                previousVolumes[iteration] = decomposedVolume;
                previousCounts[iteration] = meshes.Count;

                iteration += 1;

                shouldContinue = !ShouldAbandonGeneration(
                    name,
                    iteration,
                    previousCounts,
                    successVolume,
                    decomposedVolume,
                    previousVolumes,
                    significantChangeThreshold,
                    settings
                );
            }

            return meshes.Count;
        }

        public int ExecuteMaximizingProcessingLoop(
            MeshObject m,
            int leveragedParts,
            float successVolume,
            ref List<Mesh> meshes)
        {
            if (locked)
            {
                return meshes.Count;
            }

            var iteration = 0;

            var previousVolumes = new float[maximumIterations.v];
            var previousCounts = new int[maximumIterations.v];
            var significantChangeThreshold = successVolume * -.04f;

            settings.maxConvexHulls = math.max(settings.maxConvexHulls, leveragedParts);
            SetDirty();

            var shouldContinue = true;
            while (shouldContinue)
            {
                settings.Clamp();

                ExecuteGenerationIteration(name, fillHoles, settings, m, originalMesh, ref meshes);

                decomposedVolume = meshes.GetVolume();
                previousVolumes[iteration] = decomposedVolume;
                previousCounts[iteration] = meshes.Count;

                iteration += 1;

                shouldContinue = !ShouldAbandonGeneration(
                    name,
                    iteration,
                    previousCounts,
                    successVolume,
                    decomposedVolume,
                    previousVolumes,
                    significantChangeThreshold,
                    settings
                );

                if (shouldContinue)
                {
                    var diff = settings.maxConvexHulls - meshes.Count;
                    IncreaseResolution(ref settings, diff, false);
                    SetDirty();
                }
            }

            return meshes.Count;
        }

        private const string _generation_start = "{0}: Generating collision mesh starting: {1}";
        private const string _generation_complete = "{0}: Generating collision mesh complete.";

        private const string _abandon = "{0}: Abandoning before round {1}. {2}. | {3} | Settings: {4}";
        private const string _abandon_volume = "Volume Not Improving";
        private const string _abandon_hulls = "Hulls Not Increasingly Utilized";

        private const string _break = "{0}: Breaking before round {1}. {2}. | {3} | Settings: {4}";
        private const string _break_disabled = "Generation Disabled";
        private const string _break_resolution = "Maximum Resolution Reached";
        private const string _break_hulls = "Maximum Hulls Reached";
        private const string _break_iterations = "Maximum Iterations Reached";

        private const string _round = "{0}: Round {1}. | Settings: {2}";

        private static void ExecuteGenerationIteration(
            string name,
            bool fillHoles,
            ConvexMeshSettings settings,
            MeshObject meshWrapper,
            Mesh originalMesh,
            ref List<Mesh> meshes)
        {
            if (meshes != null)
            {
                for (var i = 0; i < meshes.Count; i++)
                {
                    meshes[i].DestroySafely();
                }

                meshes.Clear();
            }

            if (performanceLogging.v)
            {
                Debug.LogFormat(_generation_start, name, settings);
            }

            meshes = fillHoles
                ? ConvexMeshColliderGenerator.GenerateCollisionMesh(meshWrapper,  settings)
                : ConvexMeshColliderGenerator.GenerateCollisionMesh(originalMesh, settings);

            if (performanceLogging.v)
            {
                Debug.LogFormat(_generation_complete, name);
            }
        }

        private static bool ShouldAbandonGeneration(
            string name,
            int nextIter,
            int[] previousCounts,
            float successVol,
            float vol,
            float[] previousVol,
            float changeThreshold,
            ConvexMeshSettings settings)
        {
            if (vol < successVol)
            {
                return true;
            }

            var volume_string = $"{vol / successVol:F2}x vol.";

            if (generationDisabled.v)
            {
                Debug.LogWarningFormat(_break, name, nextIter, _break_disabled, volume_string, settings);
                return true;
            }

            if (settings.resolution >= ConvexMeshSettings.Ranges.resolution_MAX)
            {
                if (performanceLogging.v)
                {
                    Debug.LogWarningFormat(
                        _break,
                        name,
                        nextIter,
                        _break_resolution,
                        volume_string,
                        settings
                    );
                }

                return true;
            }

            var lastHullCount = previousCounts[nextIter - 1];

            if (lastHullCount >= settings.maxConvexHulls)
            {
                if (performanceLogging.v)
                {
                    Debug.LogWarningFormat(_break, name, nextIter, _break_hulls, volume_string, settings);
                }

                return true;
            }

            if (nextIter > maximumIterations.v)
            {
                if (performanceLogging.v)
                {
                    Debug.LogWarningFormat(
                        _break,
                        name,
                        nextIter,
                        _break_iterations,
                        volume_string,
                        settings
                    );
                }

                return true;
            }

            if (nextIter >= 2)
            {
                var volumeA = previousVol[nextIter - 2] / successVol;
                var volumeB = previousVol[nextIter - 1] / successVol;

                var volumeDelta = volumeB - volumeA;

                if (volumeDelta > changeThreshold)
                {
                    if (performanceLogging.v)
                    {
                        Debug.LogWarningFormat(
                            _abandon,
                            name,
                            nextIter,
                            _abandon_volume,
                            volume_string,
                            settings
                        );
                    }

                    return true;
                }

                var countA = previousCounts[nextIter - 2];
                var countB = previousCounts[nextIter - 1];
                if (countA >= countB)
                {
                    if (performanceLogging.v)
                    {
                        Debug.LogWarningFormat(
                            _abandon,
                            name,
                            nextIter,
                            _abandon_hulls,
                            volume_string,
                            settings
                        );
                    }

                    return true;
                }
            }

            if (performanceLogging.v)
            {
                Debug.LogFormat(_round, name, nextIter, settings);
            }

            return false;
        }

        private static void IncreaseResolution(
            ref ConvexMeshSettings settings,
            int diff,
            bool canIncreaseHulls)
        {
            var multiplier = 1.0f + (diff * .5f);

            var oldResolution = settings.resolution;
            var oldHulls = settings.maxConvexHulls;

            settings.resolution = math.clamp(
                (int) (settings.resolution * multiplier),
                ConvexMeshSettings.Ranges.resolution_MIN,
                ConvexMeshSettings.Ranges.resolution_MAX
            );

            if (canIncreaseHulls)
            {
                settings.maxConvexHulls = math.max(
                    settings.maxConvexHulls,
                    settings.SuggestedHullsByResolution
                );
            }

            settings.maximumVerticesPerHull = math.max(
                settings.maximumVerticesPerHull,
                settings.SuggestedVerticesPerHull
            );

            settings.convexHullDownsampling = math.max(
                settings.convexHullDownsampling,
                settings.SuggestedHullDownsamplingByResolution
            );

            settings.Clamp();

            if (basicLogging.v)
            {
                Debug.Log(
                    $"Parameter change: [{oldResolution}] res. to [{settings.resolution}] res. | [{oldHulls}] max hulls to [{settings.maxConvexHulls}] max hulls"
                );
            }
        }

        public void Save(List<Mesh> meshes)
        {
            if (locked)
            {
                return;
            }

            using (_PRF_Save.Auto())
            {
                if (elements == null)
                {
                    elements = new List<DecomposedColliderElement>();
                }

                if (basicLogging.v)
                {
                    Debug.Log($"Saving data for {name}.");
                }

                var saveDirectory = GetSaveDirectory(originalMesh);

                var existingMeshes = AssetDatabaseManager.FindAssets(
                    $"t:Mesh {originalMesh.name}",
                    new[] {saveDirectory}
                );

                var progressItems = existingMeshes.Length + (2 * meshes.Count);
                progressItems *= 2;
                var progressItemQuarter = progressItems / 4;

                using (var bar = new EditorOnlyProgressBar(
                    name,
                    progressItems,
                    false,
                    (int) (progressItems / 20f)
                ))
                {
                    if (basicLogging.v)
                    {
                        Debug.Log($"Deleting old meshes for {name}.");
                    }

                    using (new AssetEditingScope())
                    {
                        var temp = elements;

                        elements = new List<DecomposedColliderElement>();

                        for (var i = 0; i < existingMeshes.Length; i++)
                        {
                            var path = AssetDatabaseManager.GUIDToAssetPath(existingMeshes[i]);

                            if (!string.IsNullOrWhiteSpace(path) &&
                                !AssetDatabaseManager.IsSubAsset(
                                    AssetDatabaseManager.LoadAssetAtPath<Mesh>(path)
                                ))
                            {
                                if (extraLogging.v)
                                {
                                    Debug.Log($"Deleting asset at [{path}].");
                                }

                                AssetDatabaseManager.DeleteAsset(path);
                            }

                            bar.Increment1AndShowProgressBasic();
                        }

                        if (basicLogging.v)
                        {
                            Debug.Log($"Initializing elements for {name}.");
                        }

                        for (var i = 0; i < meshes.Count; i++)
                        {
                            var mesh = meshes[i];

                            if (string.IsNullOrWhiteSpace(mesh.name))
                            {
                                mesh.name = $"dc_{i}";
                            }

                            DecomposedColliderElement newElement;

                            if ((temp != null) && (temp.Count > 0))
                            {
                                var center = mesh.vertices.Center_NoAlloc();
                                var nearest = temp.WithMin_NoAlloc(d => (d.center - center).magnitude);

                                if (nearest == default)
                                {
#pragma warning disable 612
                                    var oldNearest =
                                        pieces.WithMin_NoAlloc(d => (d.center - center).magnitude);
#pragma warning restore 612

                                    if (oldNearest != null)
                                    {
                                        newElement = new DecomposedColliderElement(
                                            mesh,
                                            oldNearest.material,
                                            false
                                        );
                                    }
                                    else
                                    {
                                        newElement = new DecomposedColliderElement(
                                            mesh,
                                            materialModel,
                                            false
                                        );
                                    }
                                }
                                else
                                {
                                    newElement = new DecomposedColliderElement(mesh, nearest.material, false);
                                }
                            }
                            else
                            {
                                newElement = new DecomposedColliderElement(mesh, materialModel, false);
                            }

                            if (newElement.material == null)
                            {
                                newElement.material = materialModel;
                            }

                            elements.Add(newElement);
                            bar.Increment1AndShowProgressBasic();
                        }

                        if (dirtyLogging.v)
                        {
                            Debug.LogWarning("Setting dirty: elements added");
                        }

                        SetDirty();

                        bar.IncrementAndShowProgressBasic(progressItemQuarter);

                        if ((temp != null) && (temp.Count > 0))
                        {
                            for (var i = 0; i < temp.Count; i++)
                            {
                                temp[i].Delete();
                            }
                        }
                    }

                    using (new AssetEditingScope())
                    {
                        SetIndices();

                        if (basicLogging.v)
                        {
                            Debug.Log($"Saving elements for {name}.");
                        }

                        for (var i = 0; i < meshes.Count; i++)
                        {
                            elements[i].Save(originalMesh, saveDirectory);
                            bar.Increment1AndShowProgressBasic();
                        }

                        ConfirmMeshNames();

                        bar.IncrementAndShowProgressBasic(progressItemQuarter);
                    }

                    AssetDatabaseSaveManager.SaveAssetsNextFrame();
                }

                OnPostDecompose?.Invoke();
            }
        }

#endregion

#region External Processing

        private ValueDropdownList<GameObject> _assets => DecomposedColliderSuggestionHelper.assets;

        private static readonly ProfilerMarker _PRF_SuggestExternal = new(_PRF_PFX + nameof(SuggestExternal));

        public void SuggestExternal()
        {
            using (_PRF_SuggestExternal.Auto())
            {
                if (_disableSuggestExternal)
                {
                    return;
                }

                if (suggestedSearchTerm == null)
                {
                    return;
                }

                var match = DecomposedColliderSuggestionHelper.SuggestExternal(suggestedSearchTerm);

                suggestedReplacementModel = match;
                UpdateReplacementReview();
            }
        }

        private void UpdateReplacementReview()
        {
            DecomposedColliderSuggestionHelper.UpdateReplacementReview(
                ref replacementReview,
                suggestedReplacementModel
            );
        }

        private static readonly ProfilerMarker _PRF_ReplaceExternalModel =
            new(_PRF_PFX + nameof(ReplaceExternalModel));

        private void ReplaceExternalModel()
        {
            using (_PRF_ReplaceExternalModel.Auto())
            {
                SetExternalModel(this, suggestedReplacementModel);
            }
        }

        private static readonly ProfilerMarker _PRF_SelectReplacement =
            new(_PRF_PFX + nameof(SelectReplacement));

        private void SelectReplacement()
        {
            using (_PRF_SelectReplacement.Auto())
            {
                Selection.activeObject = suggestedReplacementModel;
            }
        }

        [NonSerialized] private string ___suggestedSearchTerm;

        internal string suggestedSearchTerm
        {
            get
            {
                if (___suggestedSearchTerm == null)
                {
                    var splits = name.Split('_');

                    if (splits.Length <= 1)
                    {
                        return ___suggestedSearchTerm;
                    }

                    ___suggestedSearchTerm = splits[splits.Length - 2];
                }

                return ___suggestedSearchTerm;
            }
        }

        public static void SetExternalModel(DecomposedColliderData data, GameObject newModel)
        {
            using (_PRF_SetExternalModel.Auto())
            {
                data.externalModel = newModel;
                data.LoadExternalMeshes();
            }
        }

        private static void UpdateExternalModelImportSettings(GameObject model, Mesh original)
        {
            if (model == null)
            {
                return;
            }

            var importer = AssetImporter.GetAtPath(AssetDatabaseManager.GetAssetPath(model)) as ModelImporter;

            if (importer == null)
            {
                return;
            }

            importer.addCollider = false;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
            importer.animationType = ModelImporterAnimationType.None;
            importer.generateAnimations = ModelImporterGenerateAnimations.None;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importConstraints = false;
            importer.importLights = false;

            //importer.importNormals = ModelImporterNormals.None;
            importer.importTangents = ModelImporterTangents.None;
            importer.importVisibility = false;
            importer.isReadable = false;
            importer.keepQuads = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.meshCompression = ModelImporterMeshCompression.Low;
            importer.preserveHierarchy = false;
            importer.weldVertices = true;
            importer.importBlendShapes = false;
            importer.meshOptimizationFlags = MeshOptimizationFlags.Everything;

            if (original == null)
            {
                importer.SaveAndReimport();
                return;
            }

            var originalImporter =
                AssetImporter.GetAtPath(AssetDatabaseManager.GetAssetPath(original)) as ModelImporter;

            if (originalImporter == null)
            {
                importer.SaveAndReimport();
                return;
            }

            importer.globalScale = originalImporter.globalScale;

            importer.SaveAndReimport();
        }

        private void UnassignExternalMeshes()
        {
            if (locked)
            {
                return;
            }

            externalModel = null;
            externallyCreated = false;
        }

        public void LoadExternalMeshes()
        {
            if (locked)
            {
                return;
            }

            using (_PRF_LoadExternalMeshes.Auto())
            {
                using (new AssetEditingScope())
                {
                    var meshes = new List<Mesh>();

                    var path = AssetDatabaseManager.GetAssetPath(externalModel);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        Debug.LogWarning($"Object [{externalModel.name}] is not an asset.");
                        return;
                    }

                    UpdateExternalModelImportSettings(externalModel, originalMesh);

                    var subAssets = AssetDatabaseManager.LoadAllAssetsAtPath(path).FilterCast2<Mesh>();

                    meshes.AddRange(subAssets);

                    if (meshes.Count == 0)
                    {
                        Debug.LogWarning($"No meshes found in asset [{externalModel.name}].");
                        return;
                    }

                    if (elements == null)
                    {
                        elements = new List<DecomposedColliderElement>();
                    }

                    externallyCreated = true;

                    if (basicLogging.v)
                    {
                        Debug.Log($"Loading external meshes for {name}.");
                    }

                    var progressItems = meshes.Count;
                    progressItems *= 3;
                    var progressThird = progressItems / 3;

                    using (var bar = new EditorOnlyProgressBar(
                        name,
                        progressItems,
                        false,
                        (int) (progressItems / 20f)
                    ))
                    {
                        if (basicLogging.v)
                        {
                            Debug.Log($"Deleting old meshes for {name}.");
                        }

                        var temp = elements;
                        elements = new List<DecomposedColliderElement>();

                        if (basicLogging.v)
                        {
                            Debug.Log($"Initializing elements for {name}.");
                        }

                        for (var i = 0; i < meshes.Count; i++)
                        {
                            var mesh = meshes[i];

                            if (string.IsNullOrWhiteSpace(mesh.name))
                            {
                                mesh.name = $"convextemp{i}";
                            }

                            DecomposedColliderElement newElement;

                            if ((temp != null) && (temp.Count > 0))
                            {
                                var center = mesh.vertices.Center_NoAlloc();

                                var nearest = temp.WithMin_NoAlloc(d => (d.center - center).magnitude);

                                if (nearest == default)
                                {
#pragma warning disable 612
                                    var oldNearest =
                                        pieces.WithMin_NoAlloc(d => (d.center - center).magnitude);
#pragma warning restore 612

                                    if (oldNearest != null)
                                    {
                                        newElement = new DecomposedColliderElement(
                                            mesh,
                                            oldNearest.material,
                                            true
                                        );
                                    }
                                    else
                                    {
                                        newElement = new DecomposedColliderElement(mesh, materialModel, true);
                                    }
                                }
                                else
                                {
                                    newElement = new DecomposedColliderElement(mesh, nearest.material, true);
                                }
                            }
                            else
                            {
                                newElement = new DecomposedColliderElement(mesh, materialModel, true);
                            }

                            if (newElement.material == null)
                            {
                                newElement.material = materialModel;
                            }

                            newElement.externalMesh = true;
                            elements.Add(newElement);
                            bar.Increment1AndShowProgressBasic();
                        }

                        if (dirtyLogging.v)
                        {
                            Debug.LogWarning("Setting dirty: elements added");
                        }

                        SetDirty();

                        SetIndices();

                        ConfirmMeshNames();
                        bar.IncrementAndShowProgressBasic(progressThird);

                        AssetDatabaseSaveManager.SaveAssetsNextFrame();
                    }

                    OnPostDecompose?.Invoke();
                }
            }
        }

#endregion

#region Gizmos

        private static Transform[] _cachedSelections;
        private static int _cacheFrameCount;
        [NonSerialized] private Material _gizmoMaterial;
        [NonSerialized] private Material[] _gizmoMaterials;

        [SerializeField]
        [HideInInspector]
        private Mesh _gizmoMesh;

        [NonSerialized] private MeshFilter _gizmoMeshFilter;
        [NonSerialized] private MeshRenderer _gizmoMeshRenderer;
        [NonSerialized] private PhysicMaterialWrapper[] _gizmoSubmeshes;
        private GameViewSelectionManager _selectionManager;

        public void OnDrawGizmosSelected(DecomposedCollider c)
        {
            if (locked && (_gizmoMaterial == null))
            {
                return;
            }

            var frameCount = Time.frameCount;
            if (Time.frameCount > _cacheFrameCount)
            {
                _cachedSelections = Selection.transforms;
                _cacheFrameCount = frameCount;
            }

            var found = false;

            var t = c.transform;

            for (var i = 0; i < _cachedSelections.Length; i++)
            {
                if (_cachedSelections[i] == t)
                {
                    found = true;
                }
            }

            if (!found)
            {
                return;
            }

            var gc = Gizmos.color;

            Gizmos.color = ColorPrefs.Instance.DecomposedColliderMesh.v;

            PushParent(c);

            var multiple = Selection.objects.Length > 1;

            var firstCollider = _parent.colliders.FirstOrDefault_NoAlloc();

            if (firstCollider == default)
            {
                return;
            }

            DrawGizmoMesh(c);

            DrawGizmoLabels(multiple, firstCollider);

            Gizmos.color = gc;

            HandleColliderSelection(multiple);
        }

        private void DrawGizmoMesh(DecomposedCollider c)
        {
            if (_gizmoMaterial == null)
            {
                _gizmoMaterial =
                    Instantiate(PhysicsMaterialsCollection.instance.physicsVisualizationMaterial);
                GSPL.Include(_gizmoMaterial);
            }

            if (_gizmoSubmeshes == null)
            {
                _gizmoSubmeshes = elements.OrderByFrequencyDescending(p => p.material).ToArray();
            }

            if ((_gizmoMesh == null) || (_gizmoMesh.vertexCount == 0))
            {
                _gizmoSubmeshes = elements.OrderByFrequencyDescending(p => p.material).ToArray();

                var submeshes = new Mesh[_gizmoSubmeshes.Length];

                for (var submeshIndex = 0; submeshIndex < submeshes.Length; submeshIndex++)
                {
                    var submeshMaterial = _gizmoSubmeshes[submeshIndex];
                    var submeshElements = elements.Where(p => p.material == submeshMaterial).ToArray();
                    var submeshCombine = new CombineInstance[submeshElements.Length];

                    for (var submeshElementIndex = 0;
                        submeshElementIndex < submeshElements.Length;
                        submeshElementIndex++)
                    {
                        submeshCombine[submeshElementIndex].mesh = submeshElements[submeshElementIndex].mesh;
                    }

                    submeshes[submeshIndex] = new Mesh();
                    submeshes[submeshIndex].CombineMeshes(submeshCombine, true, false);
                }

                var combine = new CombineInstance[submeshes.Length];

                for (var submeshIndex = 0; submeshIndex < submeshes.Length; submeshIndex++)
                {
                    combine[submeshIndex].mesh = submeshes[submeshIndex];
                }

                if (_gizmoMesh == null)
                {
                    var subassets = AssetDatabaseManager.LoadAllAssetsAtPath(AssetPath)
                                                        .FilterCast2<Mesh>()
                                                        .ToArray();

                    if (subassets.Length == 0)
                    {
                        _gizmoMesh = new Mesh();
                        AssetDatabaseManager.AddObjectToAsset(_gizmoMesh, this);
                    }
                    else
                    {
                        _gizmoMesh = subassets[0];
                        _gizmoMesh.Clear();
                    }
                }
                else
                {
                    _gizmoMesh.Clear();
                }

                _gizmoMesh.name = $"{originalMesh.name}_GIZMO";
                _gizmoMesh.CombineMeshes(combine, false, false);
                if (dirtyLogging.v)
                {
                    Debug.LogWarning("Setting dirty: Saving gizmo mesh");
                }

                EditorUtility.SetDirty(_gizmoMesh);
                EditorUtility.SetDirty(this);
            }

            if (_gizmoMeshFilter == null)
            {
                _gizmoMeshFilter = c.colliderTransform.gameObject.GetComponent<MeshFilter>();

                if (_gizmoMeshFilter == null)
                {
                    _gizmoMeshFilter = c.colliderTransform.gameObject.AddComponent<MeshFilter>();
                }
            }

            _gizmoMeshFilter.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

            if (_gizmoMeshRenderer == null)
            {
                _gizmoMeshRenderer = c.colliderTransform.gameObject.GetComponent<MeshRenderer>();

                if (_gizmoMeshRenderer == null)
                {
                    _gizmoMeshRenderer = c.colliderTransform.gameObject.AddComponent<MeshRenderer>();
                }
            }

            _gizmoMeshRenderer.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

            if ((_gizmoMaterials == null) || (_gizmoMaterials.Length != _gizmoSubmeshes.Length))
            {
                if (_gizmoMaterials != null)
                {
                    for (var i = 0; i < _gizmoMaterials.Length; i++)
                    {
                        if (_gizmoMaterials[i] != null)
                        {
                            _gizmoMaterials[i].DestroySafely();
                        }
                    }
                }

                _gizmoMaterials = new Material[_gizmoSubmeshes.Length];
            }

            var mask =
                PhysicsMaterialsCollection.instance.physicsVisualizationMaterial
                                          .GetTexture(GSPL.Get("_Mask"));

            for (var i = 0; i < _gizmoMaterials.Length; i++)
            {
                var mat = _gizmoMaterials[i];
                var submesh = _gizmoSubmeshes[i];

                if (mat == null)
                {
                    mat = Instantiate(_gizmoMaterial);
                }

                mat.SetFloat(GSPL.Get("_TRANSPARENCY"), submesh.meshTransparency);
                mat.SetColor(GSPL.Get("_COLOR"), submesh.wireColor);
                mat.SetTexture(GSPL.Get("_Mask"),   mask);
                mat.SetTexture(GSPL.Get("_Normal"), submesh.surface.GetTexture(GSPL.Get("_BumpMap")));

                _gizmoMaterials[i] = mat;
            }

            _gizmoMeshFilter.sharedMesh = _gizmoMesh;
            _gizmoMeshRenderer.materials = _gizmoMaterials;
        }

        // ReSharper disable once FunctionComplexityOverflow
        private void DrawGizmoLabels(bool multiple, Collider firstCollider)
        {
            var ltw = firstCollider.transform.localToWorldMatrix;

            var worldPosition = ltw.GetPositionFromMatrix();
            var worldRotation = ltw.GetRotationFromMatrix();
            var worldScale = ltw.GetScaleFromMatrix();

            var gizmoTransparency = ColorPrefs.Instance.DecomposedColliderAlpha;

            var gizmoLabelForegroundTransparency = ColorPrefs.Instance.DecomposedColliderLabelForegroundAlpha;

            var gizmoLabelBackgroundTransparency = ColorPrefs.Instance.DecomposedColliderLabelBackgroundAlpha;

            if (gizmoTransparency.v > 0f)
            {
                for (var i = 0; i < _gizmoSubmeshes.Length; i++)
                {
                    var submesh = _gizmoSubmeshes[i];

                    var color = submesh.material == null
                        ? Color.black
                        : submesh.ifnull(m => m.wireColor, Color.magenta);
                    var alpha = submesh.material == null ? gizmoTransparency.v * .6f : gizmoTransparency.v;
                    color.a = math.clamp(alpha, 0, 1);

                    Gizmos.color = color;
                    Gizmos.DrawWireMesh(_gizmoMesh, i, worldPosition, worldRotation, worldScale);
                }
            }

            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if ((element == default) || (element.material == null))
                {
                    continue;
                }

                var labelBackgroundAlpha = element.material == null
                    ? gizmoLabelBackgroundTransparency.v * .6f
                    : gizmoLabelBackgroundTransparency.v;

                var labelForegroundAlpha = element.material == null
                    ? gizmoLabelForegroundTransparency.v * .6f
                    : gizmoLabelForegroundTransparency.v;

                if (selectedColliderIndex == i)
                {
                    labelBackgroundAlpha *= 2.0f;
                    labelForegroundAlpha *= 2.0f;
                }

                labelBackgroundAlpha = math.clamp(labelBackgroundAlpha, 0, 1);
                labelForegroundAlpha = math.clamp(labelForegroundAlpha, 0, 1);

                var labelBackground = element.material == null
                    ? Color.black
                    : element.material.ifnull(m => m.labelBackgroundColor, Color.magenta);

                var labelForeground = element.material == null
                    ? Color.white
                    : element.material.ifnull(m => m.labelTextColor, Color.magenta);

                labelBackground.a = labelBackgroundAlpha;
                labelForeground.a = labelForegroundAlpha;

                if (!multiple && (selectedColliderIndex == i))
                {
                    var gizco = ColorPrefs.Instance.DecomposedColliderSelected.v;
                    Gizmos.color = gizco;
                    Gizmos.DrawWireMesh(
                        element.mesh,
                        worldPosition,
                        worldRotation,
                        worldScale * ColorPrefs.Instance.DecomposedColliderSelectedScale.v
                    );
                }

                if (!multiple)
                {
                    var elementCenter = ltw.MultiplyPoint3x4(element.center);
                    SmartHandles.Label(
                        elementCenter,
                        element.material.name,
                        selectedColliderIndex == i ? Color.white : labelForeground,
                        selectedColliderIndex == i ? Color.black : labelBackground
                    );
                }
            }
        }

        private void HandleColliderSelection(bool multiple)
        {
            if (!multiple)
            {
                if (_selectionManager == null)
                {
                    _selectionManager = new GameViewSelectionManager();
                }

                var colliders = _parent.colliders;

                if (_selectionManager.TryGameViewSelection(Camera.main, colliders, out var hit))
                {
                    for (var i = 0; i < colliders.Count; i++)
                    {
                        if (hit == colliders[i])
                        {
                            selectedColliderIndex = i;
                        }
                    }
                }
            }
        }

#endregion
    }
}

#endif
