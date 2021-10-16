/*
#region

using System.Linq;
using Appalachia.Core.Base;
using Appalachia.Core.Attributes;
using Appalachia.Core.Editing.Coloring;
using Appalachia.Core.Editing.Preferences;
using Appalachia.Core.Extensions;
using Appalachia.Jobs.MeshData;
using Appalachia.Core.Physic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

using Appalachia.Core.Editing.Prefabs;
using UnityEditor.SceneManagement;

#endregion

namespace Appalachia.Core.ConvexDecomposition
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public partial class DecomposedCollider : InternalFrustumCulledMonoBehaviour<DecomposedCollider>
    {
        private const string _TABS = "TABS";
        private const string _REFS = "Refs";
        private const string _SETUP = "Setup";
        private const string _SETUP_ = _TABS + "/Setup";
        private const string _SETUP_A = _SETUP_ + "/A";
        private const string _SETUP_B = _SETUP_ + "/B";
        private const string _PHYSX = "Physics";
        private const string _PHYSX_MATS1 = _TABS + "/" + _PHYSX + "/Materials1";
        private const string _PHYSX_MATS2 = _TABS + "/" + _PHYSX + "/Materials2";
        private const string _PHYSX_TABS = _TABS + "/" + _PHYSX + "/TABS";
        private const string _PHYSX_SUB_UPDATE = "Update";
        private const string _PHYSX_SUB_SWAP = "Swap";
        private const string _PHYSX_SUB_ALL = "Assign All";
        private const string _PHYSX_SUB_UPDATE_ = _PHYSX_TABS + "/" + _PHYSX_SUB_UPDATE;
        private const string _PHYSX_SUB_SWAP_ = _PHYSX_TABS + "/" + _PHYSX_SUB_SWAP;
        private const string _PHYSX_SUB_ALL_ = _PHYSX_TABS + "/" + _PHYSX_SUB_ALL;
        
        private const string _PHYSX_SUB_UPDATE_1 = _PHYSX_SUB_UPDATE_ + "/Buttons1";
        private const string _PHYSX_SUB_UPDATE_2 = _PHYSX_SUB_UPDATE_ + "/Buttons2";
        private const string _PHYSX_SUB_UPDATE_3 = _PHYSX_SUB_UPDATE_ + "/Buttons3";
        private const string _PHYSX_SUB_UPDATE_4 = _PHYSX_SUB_UPDATE_ + "/Buttons4";
        private const string _PHYSX_SUB_UPDATE_5 = _PHYSX_SUB_UPDATE_ + "/Buttons5";
        private const string _PHYSX_SUB_UPDATE_6 = _PHYSX_SUB_UPDATE_ + "/Buttons6";
        private const string _PHYSX_SUB_SWAP_1 = _PHYSX_SUB_SWAP_ + "/Buttons1";
        private const string _PHYSX_SUB_SWAP_2 = _PHYSX_SUB_SWAP_ + "/Buttons2";
        private const string _PHYSX_SUB_SWAP_3 = _PHYSX_SUB_SWAP_ + "/Buttons3";
        private const string _PHYSX_SUB_SWAP_4 = _PHYSX_SUB_SWAP_ + "/Buttons4";
        private const string _PHYSX_SUB_SWAP_5 = _PHYSX_SUB_SWAP_ + "/Buttons5";
        private const string _PHYSX_SUB_SWAP_6 = _PHYSX_SUB_SWAP_ + "/Buttons6";
        private const string _PHYSX_SUB_ALL_1 = _PHYSX_SUB_ALL_ + "/Buttons1";
        private const string _PHYSX_SUB_ALL_2 = _PHYSX_SUB_ALL_ + "/Buttons2";
        private const string _PHYSX_SUB_ALL_3 = _PHYSX_SUB_ALL_ + "/Buttons3";
        private const string _PHYSX_SUB_ALL_4 = _PHYSX_SUB_ALL_ + "/Buttons4";
        private const string _PHYSX_SUB_ALL_5 = _PHYSX_SUB_ALL_ + "/Buttons5";
        private const string _PHYSX_SUB_ALL_6 = _PHYSX_SUB_ALL_ + "/Buttons6";
        
        private const string _RUNTIME = "Runtime";
        private const string _PROC = "Processing";
        private const string _ORIGINALS = _PROC + "/Originals";
        private const string _PROC_PROC = _PROC + "/Process";
        private const string _PROC_MORE = _PROC + "/More";
        private const string _PROC_LESS = _PROC + "/Less";
        private const string _RESULTS = "Results";
        private const string _RES_DIGS = _RESULTS + "/Digits";
        private const string _RES_COMPS = _RESULTS + "/Components";

        [SmartLabel, ToggleLeft]
        [TabGroup(_TABS, _SETUP), PropertyOrder(3)]
        [HorizontalGroup(_SETUP_A, .3f)]
        [SerializeField]
        public bool fillHoles = true;

        [SmartLabel]
        [TabGroup(_TABS, _REFS), PropertyOrder(-1)]
        [SerializeField]
        public Mesh originalMesh;

        [SmartLabel]
        [TabGroup(_TABS, _REFS), PropertyOrder(1)]
        [SerializeField]
        public MeshRenderer originalMeshRenderer;

        [SmartLabel]
        [TabGroup(_TABS, _REFS), PropertyOrder(2)]
        [SerializeField]
        [ReadOnly]
        public float originalScale;

        [SmartLabel]
        [TabGroup(_TABS, _SETUP), PropertyOrder(3)]
        [HorizontalGroup(_SETUP_A, .7f)]
        [PropertyRange(1, 50)]
        [SerializeField]
        public int maximumParts = 6;

        [SmartLabel]
        [TabGroup(_TABS, _SETUP), PropertyOrder(3)]
        [HorizontalGroup(_SETUP_B, .5f)]
        [PropertyRange(10000, 1000000)]
        [SerializeField]
        public int resolutionPerMesh = 60000;

        [SmartLabel]
        [TabGroup(_TABS, _SETUP), PropertyOrder(3)]
        [HorizontalGroup(_SETUP_B, .5f)]
        [PropertyRange(100000, 10000000)]
        [SerializeField]
        public int maxResolution = 500000;

        [SmartLabel]
        [TabGroup(_TABS, _SETUP), PropertyOrder(4)]
        [PropertyRange(1.0f, 5.0f)]
        [SerializeField]
        public float successThreshold = 1.25f;

        [SmartLabel]
        [TabGroup(_TABS, _REFS), PropertyOrder(5)]
        [SerializeField]
        public string childName = "COLLIDERS";

        [SmartLabel]
        [TabGroup(_TABS, _REFS), PropertyOrder(5)]
        [SerializeField]
        public Transform colliderTransform;

        [SmartLabel]
        [TabGroup(_TABS, _REFS), PropertyOrder(6), ReadOnly]
        [SerializeField]
        public Collider[] originals;

        [SmartLabel]
        [TabGroup(_TABS, _PHYSX), PropertyOrder(7)]
        [SerializeField]
        public DensityMetadata density;

        [SmartLabel]
        [TabGroup(_TABS, _RUNTIME), PropertyOrder(50)]
        [SerializeField]
        public ColliderBehavior decomposedBehavior = ColliderBehavior.AlwaysEnabled;

        [SmartLabel]
        [TabGroup(_TABS, _RUNTIME), PropertyOrder(51)]
        [SerializeField]
        public ColliderBehavior originalBehavior = ColliderBehavior.NeverEnabled;

        [SmartLabel]
        [BoxGroup(_RESULTS)]
        [HorizontalGroup(_RES_DIGS, .33f), PropertyOrder(100), ReadOnly]
        [SerializeField]
        public float originalVolume;

        [SmartLabel]
        [HorizontalGroup(_RES_DIGS, .33f), PropertyOrder(101), ReadOnly]
        [SerializeField]
        public float decomposedVolume;

        [SmartLabel]
        [HorizontalGroup(_RES_DIGS, .33f), PropertyOrder(102), ReadOnly]
        [SerializeField]
        public double executionTime;

        [SmartLabel]
        [HorizontalGroup(_RES_COMPS, .5f), PropertyOrder(110), ReadOnly, ListDrawerSettings(NumberOfItemsPerPage = 3)]
        [SerializeField]
        public Mesh[] decomposedMeshes;

        [SmartLabel]
        [HorizontalGroup(_RES_COMPS, .5f), PropertyOrder(120), ReadOnly, ListDrawerSettings(NumberOfItemsPerPage = 3)]
        [SerializeField]
        public MeshCollider[] colliders;

        [SmartLabel]
        [TabGroup(_TABS, _PHYSX), PropertyOrder(150)]
        [SerializeField]
        public DensityRigidbodyManager densityRigidbodyManager;

        [SmartLabel, ListDrawerSettings(ShowPaging = false)]
        [TabGroup(_TABS, _PHYSX), PropertyOrder(151)]
        [SerializeField]
        [OnValueChanged(nameof(ClampPhysicIndex), true)]
        public List<DecomposedColliderPhysics> physicMaterials = new List<DecomposedColliderPhysics>();

        private int _maxPhysicMaterial => physicMaterials == null || physicMaterials.Count == 0 ? 0 : physicMaterials.Count - 1;
        
        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_UPDATE), PropertyOrder(155)]
        [PropertyRange(0, nameof(_maxPhysicMaterial))]
        public int highlightedPhysicMaterial = 0;

        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_UPDATE), PropertyOrder(155), ShowInInspector]
        public static PREF<float> gizmoTransparency;

        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_SWAP), PropertyOrder(160), ShowInInspector, SmartLabel]
        public static PhysicMaterial swapFrom;

        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_ALL)]
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Twigs")] 
        private void TwigsAll() =>      AssignAll(PhysicsMaterials.instance.twigs);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Wood")] 
        private void WoodAll() =>       AssignAll(PhysicsMaterials.instance.wood);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Dirt")] 
        private void DirtAll() =>       AssignAll(PhysicsMaterials.instance.dirt);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Grass")] 
        private void GrassAll() =>      AssignAll(PhysicsMaterials.instance.grass);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Gravel")] 
        private void GravelAll() =>     AssignAll(PhysicsMaterials.instance.gravel);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Terrain")] 
        private void TerrainAll() =>    AssignAll(PhysicsMaterials.instance.terrain);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Ice")] 
        private void IceAll() =>        AssignAll(PhysicsMaterials.instance.ice);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Rock")] 
        private void RockAll() =>       AssignAll(PhysicsMaterials.instance.rock);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Mossy")] 
        private void MossyAll() =>  AssignAll(PhysicsMaterials.instance.Mossy);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Sand")] 
        private void SandAll() =>       AssignAll(PhysicsMaterials.instance.sand);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Water")] 
        private void WaterAll() =>      AssignAll(PhysicsMaterials.instance.water);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Wet Ground")] 
        private void WetGroundAll() =>  AssignAll(PhysicsMaterials.instance.wetGround);
        
        
        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_SWAP), PropertyOrder(161), ShowInInspector, SmartLabel]
        [SmartInlineButton(nameof(Swap), DisableIfMemberName = nameof(_canNotSwap))]
        public static PhysicMaterial swapTo;

        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_ALL)]
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Twigs")] 
        private void TwigsAll() =>      AssignAll(PhysicsMaterials.instance.twigs);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Wood")] 
        private void WoodAll() =>       AssignAll(PhysicsMaterials.instance.wood);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Dirt")] 
        private void DirtAll() =>       AssignAll(PhysicsMaterials.instance.dirt);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Grass")] 
        private void GrassAll() =>      AssignAll(PhysicsMaterials.instance.grass);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Gravel")] 
        private void GravelAll() =>     AssignAll(PhysicsMaterials.instance.gravel);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Terrain")] 
        private void TerrainAll() =>    AssignAll(PhysicsMaterials.instance.terrain);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Ice")] 
        private void IceAll() =>        AssignAll(PhysicsMaterials.instance.ice);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Rock")] 
        private void RockAll() =>       AssignAll(PhysicsMaterials.instance.rock);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Mossy")] 
        private void MossyAll() =>  AssignAll(PhysicsMaterials.instance.Mossy);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Sand")] 
        private void SandAll() =>       AssignAll(PhysicsMaterials.instance.sand);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Water")] 
        private void WaterAll() =>      AssignAll(PhysicsMaterials.instance.water);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Wet Ground")] 
        private void WetGroundAll() =>  AssignAll(PhysicsMaterials.instance.wetGround);
        
        private bool _canNotSwap => swapFrom == swapTo;
        
        private void ClampPhysicIndex() => highlightedPhysicMaterial = math.clamp(highlightedPhysicMaterial, 0, _maxPhysicMaterial);
        
        [TabGroup(_TABS, _PHYSX), PropertyOrder(153)]
        [Button]
        public void Populate()
        {
            if (physicMaterials == null)
            {
                physicMaterials = new List<DecomposedColliderPhysics>();                
            }
            
            var colls = GetComponentsInChildren<Collider>();

            while (physicMaterials.Count > colls.Length)
            {
                physicMaterials.RemoveAt(physicMaterials.Count - 1);
            }

            while (physicMaterials.Count < colls.Length)
            {
                physicMaterials.Add(new DecomposedColliderPhysics());
            }

            PhysicMaterial modelMaterial = null;
            
            for (var i = 0; i < colls.Length; i++)
            {
                var c = colls[i];
                
                var physicMaterial = physicMaterials[i];

                if (modelMaterial == null && physicMaterial.material != null)
                {
                    modelMaterial = physicMaterial.material;
                }

                if (physicMaterial.material == null)
                {
                    physicMaterial.material = modelMaterial;
                }

                physicMaterial.collider = c;

                var mc = c as MeshCollider;
                var verts = mc.sharedMesh.vertices;

                for (var j = 0; j < verts.Length; j++)
                {
                    physicMaterial.center += verts[j];
                }

                physicMaterial.center /= (float)verts.Length;

                physicMaterials[i] = physicMaterial;
            }
        }

        [TabGroup(_TABS, _PHYSX), PropertyOrder(153)]
        [Button]
        public void AssignToColliders()
        {
            for (var i = 0; i < physicMaterials.Count; i++)
            {
                var physics = physicMaterials[i];

                physics.collider.sharedMaterial = physics.material;
            }
        }

        [HorizontalGroup(_PHYSX_SUB_UPDATE_1), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Twigs() =>      Assign(PhysicsMaterials.instance.twigs);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_1), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Wood() =>       Assign(PhysicsMaterials.instance.wood);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_1), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Dirt() =>       Assign(PhysicsMaterials.instance.dirt);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_1), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Grass() =>      Assign(PhysicsMaterials.instance.grass);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_2), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Gravel() =>     Assign(PhysicsMaterials.instance.gravel);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_2), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Terrain() =>     Assign(PhysicsMaterials.instance.terrain);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_2), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Ice() =>        Assign(PhysicsMaterials.instance.ice);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_2), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Rock() =>       Assign(PhysicsMaterials.instance.rock);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_3), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Mossy() =>  Assign(PhysicsMaterials.instance.Mossy);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_3), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Sand() =>       Assign(PhysicsMaterials.instance.sand);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_3), PropertyOrder(156), ResponsiveButtonGroup] 
        private void Water() =>      Assign(PhysicsMaterials.instance.water);
        
        [HorizontalGroup(_PHYSX_SUB_UPDATE_3), PropertyOrder(156), ResponsiveButtonGroup] 
        private void WetGround() =>  Assign(PhysicsMaterials.instance.wetGround);
            
        private void Assign(PhysicMaterial mat)
        {
            var matx = physicMaterials[highlightedPhysicMaterial];
            matx.material = mat;
            physicMaterials[highlightedPhysicMaterial] = matx;
        }
        
        [TabGroup(_PHYSX_TABS, _PHYSX_SUB_ALL)]
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Twigs")] 
        private void TwigsAll() =>      AssignAll(PhysicsMaterials.instance.twigs);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Wood")] 
        private void WoodAll() =>       AssignAll(PhysicsMaterials.instance.wood);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Dirt")] 
        private void DirtAll() =>       AssignAll(PhysicsMaterials.instance.dirt);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_1), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Grass")] 
        private void GrassAll() =>      AssignAll(PhysicsMaterials.instance.grass);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Gravel")] 
        private void GravelAll() =>     AssignAll(PhysicsMaterials.instance.gravel);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Terrain")] 
        private void TerrainAll() =>    AssignAll(PhysicsMaterials.instance.terrain);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Ice")] 
        private void IceAll() =>        AssignAll(PhysicsMaterials.instance.ice);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_2), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Rock")] 
        private void RockAll() =>       AssignAll(PhysicsMaterials.instance.rock);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Mossy")] 
        private void MossyAll() =>  AssignAll(PhysicsMaterials.instance.Mossy);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Sand")] 
        private void SandAll() =>       AssignAll(PhysicsMaterials.instance.sand);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Water")] 
        private void WaterAll() =>      AssignAll(PhysicsMaterials.instance.water);
        
        [HorizontalGroup(_PHYSX_SUB_ALL_3), PropertyOrder(156), ResponsiveButtonGroup, LabelText("Wet Ground")] 
        private void WetGroundAll() =>  AssignAll(PhysicsMaterials.instance.wetGround);
        
        private void AssignAll(PhysicMaterial mat)
        {
            for (int i = 0; i < physicMaterials.Count; i++)
            {
                var matx = physicMaterials[i];
                matx.material = mat;
                physicMaterials[i] = matx;
            }
        }

        private void Swap()
        {
            for (var i = 0; i < physicMaterials.Count; i++)
            {
                if (physicMaterials[i].material == swapFrom)
                {
                    var mat = physicMaterials[i];
                    mat.material = swapTo;
                    physicMaterials[i] = mat;
                }
            }
        }
        
        private static readonly ProfilerMarker _PRF_DecomposedCollider_OnEnable = new ProfilerMarker("DecomposedCollider.OnEnable");

        private void OnEnable()
        {
            using (_PRF_DecomposedCollider_OnEnable.Auto())
            {
                InitializeComponents(gameObject, this);

                CheckOriginalsStatus();

                var playing = Application.isPlaying;

                var decomposedEnabled = false;
                var originalEnabled = false;

                if ((decomposedBehavior == ColliderBehavior.AlwaysEnabled) ||
                    ((decomposedBehavior == ColliderBehavior.EnabledAtRuntime) && playing) ||
                    ((decomposedBehavior == ColliderBehavior.EnabledInEditMode) && !playing))
                {
                    decomposedEnabled = true;
                }

                if ((originalBehavior == ColliderBehavior.AlwaysEnabled) ||
                    ((originalBehavior == ColliderBehavior.EnabledAtRuntime) && playing) ||
                    ((originalBehavior == ColliderBehavior.EnabledInEditMode) && !playing))
                {
                    originalEnabled = true;
                }

                UpdateDecomposedColliders(decomposedEnabled);
                UpdateOriginalColliders(originalEnabled);
                OnEnableDensityCheck();
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_OnEnableDensityCheck = new ProfilerMarker("DecomposedCollider.OnEnableDensityCheck");
        private void OnEnableDensityCheck()
        {
            using (_PRF_DecomposedCollider_OnEnableDensityCheck.Auto())
            {
                if (density != null)
                {

                    var asset = gameObject.GetAsset();
                    var labels = AssetDatabaseManager.GetLabels(asset);

                    for (var i = 0; i < labels.Length; i++)
                    {
                        if (labels[i].Contains("Assembly"))
                        {
                            density = null;

                            if (densityRigidbodyManager != null)
                            {
                                densityRigidbodyManager.DestroySafely();
                                densityRigidbodyManager = null;
                            }

                            return;
                        }
                    }
                    if (densityRigidbodyManager == null)
                    {
                        densityRigidbodyManager = gameObject.GetComponent<DensityRigidbodyManager>();
                    }

                    if (densityRigidbodyManager != null)
                    {
                        if (densityRigidbodyManager.density != density)
                        {
                            densityRigidbodyManager.Initialize(density);
                        }
                    }
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_OnDisable = new ProfilerMarker("DecomposedCollider.OnDisable");

        private void OnDisable()
        {
            using (_PRF_DecomposedCollider_OnDisable.Auto())
            {
                if ((_meshObject == null) || !_meshObject.data.isCreated)
                {
                    return;
                }

                _meshObject = default;
                /*_buildHandles.SafeDispose();
            _inputTrianglesCollection.SafeDispose();
            _outputTrianglesCollection.SafeDispose();#1#
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_UpdateDecomposedColliders =
            new ProfilerMarker("DecomposedCollider.UpdateDecomposedColliders");

        private void UpdateDecomposedColliders(bool enabled)
        {
            using (_PRF_DecomposedCollider_UpdateDecomposedColliders.Auto())
            {
                if (colliders == null)
                {
                    return;
                }

                for (var i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = enabled;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_UpdateOriginalColliders =
            new ProfilerMarker("DecomposedCollider.UpdateOriginalColliders");

        private void UpdateOriginalColliders(bool enabled)
        {
            using (_PRF_DecomposedCollider_UpdateOriginalColliders.Auto())
            {
                if (originals == null)
                {
                    return;
                }

                for (var i = 0; i < originals.Length; i++)
                {
                    originals[i].enabled = enabled;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_InitializeColliders = new ProfilerMarker("DecomposedCollider.InitializeColliders");

        public void InitializeColliders()
        {
            using (_PRF_DecomposedCollider_InitializeColliders.Auto())
            {
                var foundColliderObj = false;
                var duplicates = false;

                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);

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

                    if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
                    {
                        var asset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
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
                    }
                    else
                    {
                        for (var i = transform.childCount - 1; i >= 0; i--)
                        {
                            var child = transform.GetChild(i);

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
                                    Debug.LogError(ex, gameObject);
                                }
                            }
                            else
                            {
                                foundColliderObj = true;
                            }
                        }
                    }
                }

                if (colliderTransform == null)
                {
                    colliderTransform = transform.Find(childName);

                    if (colliderTransform == null)
                    {
                        var colliderRoot = new GameObject(childName);

                        colliderRoot.transform.SetParent(transform, false);

                        colliderTransform = colliderRoot.transform;
                    }
                }

                if (colliderTransform == null)
                {
                    return;
                }

                if (colliders != null)
                {
                    for (var i = 0; i < colliders.Length; i++)
                    {
                        var c = colliders[i];

                        if ((c == null) || (c.sharedMesh == null))
                        {
                            colliders = colliderTransform.GetComponents<MeshCollider>();
                            break;
                        }
                    }
                }
                else
                {
                    colliders = colliderTransform.GetComponents<MeshCollider>();
                }
            }
        }


#pragma warning disable CS0414
        private bool _originalsStatusEnabled;
        private bool _originalsStatusDisabled;
#pragma warning restore

        private static readonly ProfilerMarker _PRF_DecomposedCollider_CheckOriginalsStatus = new ProfilerMarker("DecomposedCollider.CheckOriginalsStatus");

        [OnInspectorGUI]
        private void CheckOriginalsStatus()
        {
            using (_PRF_DecomposedCollider_CheckOriginalsStatus.Auto())
            {
                if ((originals != null) && (originals.Length > 0))
                {
                    for (var i = 0; i < originals.Length; i++)
                    {
                        if (originals[i] == null)
                        {
                            originals = originals.Where(o => o != null).ToArray();
                            break;
                        }
                    }
                }
                else
                {
                    originals = GetComponentsInChildren<Collider>(true)
                               .Where(
                                    c => (c != null) &&
                                         !c.isTrigger &&
                                         (c.gameObject.name != childName) &&
                                         (!(c is MeshCollider mc) || ((mc.sharedMesh != null) && !mc.sharedMesh.name.Contains("_convex_"))) &&
                                         ((colliders == null) || !colliders.Contains(c))
                                )
                               .ToArray();
                }

                _originalsStatusEnabled = false;
                _originalsStatusDisabled = false;

                for (var i = 0; i < originals.Length; i++)
                {
                    if (originals[i].enabled)
                    {
                        _originalsStatusEnabled = true;
                    }
                    else
                    {
                        _originalsStatusDisabled = true;
                    }
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_DisableOriginalColliders =
            new ProfilerMarker("DecomposedCollider.DisableOriginalColliders");

        [BoxGroup(_PROC), PropertyOrder(50)]
        [ButtonGroup(_ORIGINALS), Button, EnableIf(nameof(_originalsStatusEnabled))]
        public void DisableOriginalColliders()
        {
            using (_PRF_DecomposedCollider_DisableOriginalColliders.Auto())
            {
                if ((originals == null) || (colliders == null))
                {
                    return;
                }

                for (var i = 0; i < originals.Length; i++)
                {
                    originals[i].enabled = false;
                }

                CheckOriginalsStatus();
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_EnableOriginalColliders =
            new ProfilerMarker("DecomposedCollider.EnableOriginalColliders");

        [ButtonGroup(_ORIGINALS), Button, EnableIf(nameof(_originalsStatusDisabled))]
        public void EnableOriginalColliders()
        {
            using (_PRF_DecomposedCollider_EnableOriginalColliders.Auto())
            {
                if ((originals == null) || (colliders == null))
                {
                    return;
                }

                originals = GetComponentsInChildren<Collider>(true).Where(c => !c.isTrigger && !colliders.Contains(c)).ToArray();

                for (var i = 0; i < originals.Length; i++)
                {
                    originals[i].enabled = true;
                }

                CheckOriginalsStatus();
            }
        }

        private bool _canProcess => originalMesh != null;
        private bool _canProcessMore1 => (originalMesh != null) && (decomposedMeshes != null) /*&& decomposedMeshes.Length < 20#1#;
        private bool _canProcessMore2 => (originalMesh != null) && (decomposedMeshes != null) /*&& decomposedMeshes.Length < 19#1#;
        private bool _canProcessMore3 => (originalMesh != null) && (decomposedMeshes != null) /*&& decomposedMeshes.Length < 18#1#;
        private bool _canProcessMore5 => (originalMesh != null) && (decomposedMeshes != null) /*&& decomposedMeshes.Length < 18#1#;
        private bool _canProcessMore10 => (originalMesh != null) && (decomposedMeshes != null) /*&& decomposedMeshes.Length < 18#1#;
        private bool _canProcessLess1 => (originalMesh != null) && (decomposedMeshes != null) && (decomposedMeshes.Length > 1);
        private bool _canProcessLess2 => (originalMesh != null) && (decomposedMeshes != null) && (decomposedMeshes.Length > 2);
        private bool _canProcessLess3 => (originalMesh != null) && (decomposedMeshes != null) && (decomposedMeshes.Length > 3);
        private bool _canProcessLess5 => (originalMesh != null) && (decomposedMeshes != null) && (decomposedMeshes.Length > 5);
        private bool _canProcessLess10 => (originalMesh != null) && (decomposedMeshes != null) && (decomposedMeshes.Length > 10);

        [ButtonGroup(_PROC_PROC), Button, EnableIf(nameof(_canProcess))]
        public void DecomposeMeshes()
        {
            ExecuteDecomposition(ExecutionStyle.Normal);
        }

        [ButtonGroup(_PROC_PROC), Button, EnableIf(nameof(_canProcess))]
        public void ForceDecompose()
        {
            ExecuteDecomposition(ExecutionStyle.Forced);
        }

        [ButtonGroup(_PROC_PROC), Button, EnableIf(nameof(_canProcess))]
        public void RebuildDecompose()
        {
            ExecuteDecomposition(ExecutionStyle.Rebuild);
        }

        [ButtonGroup(_PROC_MORE), Button("More (+1)"), EnableIf(nameof(_canProcessMore1))]
        public void MoreComplex1()
        {
            ExecuteDecomposition(ExecutionStyle.IncreaseParts1);
        }

        [ButtonGroup(_PROC_MORE), Button("More (+2)"), EnableIf(nameof(_canProcessMore2))]
        public void MoreComplex2()
        {
            ExecuteDecomposition(ExecutionStyle.IncreaseParts2);
        }

        [ButtonGroup(_PROC_MORE), Button("More (+3)"), EnableIf(nameof(_canProcessMore3))]
        public void MoreComplex3()
        {
            ExecuteDecomposition(ExecutionStyle.IncreaseParts3);
        }

        [ButtonGroup(_PROC_MORE), Button("More (+5)"), EnableIf(nameof(_canProcessMore5))]
        public void MoreComplex5()
        {
            ExecuteDecomposition(ExecutionStyle.IncreaseParts5);
        }

        [ButtonGroup(_PROC_MORE), Button("More (+10)"), EnableIf(nameof(_canProcessMore10))]
        public void MoreComplex10()
        {
            ExecuteDecomposition(ExecutionStyle.IncreaseParts10);
        }

        [ButtonGroup(_PROC_LESS), Button("Less (-1)"), EnableIf(nameof(_canProcessLess1))]
        public void LessComplex1()
        {
            ExecuteDecomposition(ExecutionStyle.DecreaseParts1);
        }

        [ButtonGroup(_PROC_LESS), Button("Less (-2)"), EnableIf(nameof(_canProcessLess2))]
        public void LessComplex2()
        {
            ExecuteDecomposition(ExecutionStyle.DecreaseParts2);
        }

        [ButtonGroup(_PROC_LESS), Button("Less (-3)"), EnableIf(nameof(_canProcessLess3))]
        public void LessComplex3()
        {
            ExecuteDecomposition(ExecutionStyle.DecreaseParts3);
        }

        [ButtonGroup(_PROC_LESS), Button("Less (-5)"), EnableIf(nameof(_canProcessLess5))]
        public void LessComplex5()
        {
            ExecuteDecomposition(ExecutionStyle.DecreaseParts3);
        }

        [ButtonGroup(_PROC_LESS), Button("Less (-10)"), EnableIf(nameof(_canProcessLess10))]
        public void LessComplex10()
        {
            ExecuteDecomposition(ExecutionStyle.DecreaseParts3);
        }

        private enum ExecutionStyle
        {
            Normal,
            Forced,
            Rebuild,
            IncreaseParts1,
            IncreaseParts2,
            IncreaseParts3,
            IncreaseParts5,
            IncreaseParts10,
            DecreaseParts1,
            DecreaseParts2,
            DecreaseParts3,
            DecreaseParts5,
            DecreaseParts10
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_ExecuteDecomposition = new ProfilerMarker("DecomposedCollider.ExecuteDecomposition");

        private void ExecuteDecomposition(ExecutionStyle style)
        {
            using (_PRF_DecomposedCollider_ExecuteDecomposition.Auto())
            {
                var leveragedParts = 0;

                switch (style)
                {
                    case ExecutionStyle.Normal:
                        break;
                    case ExecutionStyle.Forced:
                        break;
                    case ExecutionStyle.Rebuild:
                        leveragedParts = decomposedMeshes?.Length ?? 0;
                        break;
                    case ExecutionStyle.IncreaseParts1:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) + 1;
                        break;
                    case ExecutionStyle.IncreaseParts2:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) + 2;
                        break;
                    case ExecutionStyle.IncreaseParts3:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) + 3;
                        break;
                    case ExecutionStyle.IncreaseParts5:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) + 5;
                        break;
                    case ExecutionStyle.IncreaseParts10:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) + 10;
                        break;
                    case ExecutionStyle.DecreaseParts1:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) - 1;
                        break;
                    case ExecutionStyle.DecreaseParts2:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) - 2;
                        break;
                    case ExecutionStyle.DecreaseParts3:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) - 3;
                        break;
                    case ExecutionStyle.DecreaseParts5:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) - 5;
                        break;
                    case ExecutionStyle.DecreaseParts10:
                        leveragedParts = (decomposedMeshes?.Length ?? 0) - 10;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(style), style, null);
                }

                leveragedParts = Mathf.Clamp(leveragedParts, 0, 50);

                InitializeComponents(gameObject, this);

                if (originalMesh == null)
                {
                    return;
                }

                var successVolume = originalVolume * successThreshold;

                if ((style == ExecutionStyle.Normal) && !DecompositionRequired(successVolume))
                {
                    return;
                }

                DisableOriginalColliders();

                var startTime = DateTime.Now;

                var meshes = GenerateDecomposedMeshes(leveragedParts, successVolume);

                if (meshes == null)
                {
                    return;
                }

                DestroyExisting();

                AssignAndSaveMeshes(meshes);

                var duration = DateTime.Now - startTime;
                executionTime = duration.TotalSeconds;
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_DecompositionRequired =
            new ProfilerMarker("DecomposedCollider.DecompositionRequired");

        private bool DecompositionRequired(float successVolume)
        {
            using (_PRF_DecomposedCollider_DecompositionRequired.Auto())
            {
                if ((colliders != null) &&
                    decomposedMeshes.All(c => c != null) &&
                    colliders.All(c => c != null) &&
                    (decomposedVolume > 0) &&
                    ((decomposedVolume < successVolume) || (decomposedMeshes.Length == maximumParts)))
                {
                    return false;
                }

                return true;
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_GenerateDecomposedMeshes =
            new ProfilerMarker("DecomposedCollider.GenerateDecomposedMeshes");

        private List<Mesh> GenerateDecomposedMeshes(int leveragedParts, float successVolume)
        {
            using (_PRF_DecomposedCollider_GenerateDecomposedMeshes.Auto())
            {
                decomposedVolume = successVolume * 10.0f;

                var m = meshObject;

                originalVolume = fillHoles ? m.data.SolidVolume : m.data.Volume;

                List<Mesh> meshes = null;

                if (leveragedParts == 0)
                {
                    if (successVolume == originalVolume)
                    {
                        leveragedParts = maximumParts - 1;
                    }

                    while ((decomposedVolume > successVolume) && (leveragedParts < maximumParts))
                    {
                        leveragedParts += 1;

                        var resolution = Mathf.Clamp(resolutionPerMesh, 10000, maxResolution / leveragedParts);

                        if (fillHoles)
                        {
                            meshes = ConvexMeshColliderGenerator.GenerateCollisionMesh(meshObject.data, leveragedParts, true, resolution);
                        }
                        else
                        {
                            meshes = ConvexMeshColliderGenerator.GenerateCollisionMesh(originalMesh, leveragedParts, resolution);
                        }

                        decomposedVolume = meshes.GetVolume();
                    }
                }
                else
                {
                    var pending = true;

                    while (pending)
                    {
                        var resolution = Mathf.Clamp(resolutionPerMesh, 10000, maxResolution / leveragedParts);

                        if (fillHoles)
                        {
                            meshes = ConvexMeshColliderGenerator.GenerateCollisionMesh(meshObject.data, leveragedParts, true, resolution);
                        }
                        else
                        {
                            meshes = ConvexMeshColliderGenerator.GenerateCollisionMesh(originalMesh, leveragedParts, resolution);
                        }

                        var diff = leveragedParts - meshes.Count;

                        if (diff > 0)
                        {
                            resolutionPerMesh += resolutionPerMesh / leveragedParts;
                            maxResolution = resolutionPerMesh * maximumParts;
                        }
                        else
                        {
                            pending = false;
                        }
                    }

                    decomposedVolume = meshes.GetVolume();
                }

                maximumParts = Mathf.Max(maximumParts, leveragedParts);

                return meshes;
            }
        }

        private MeshObjectWrapper _meshObject;

        private MeshObjectWrapper meshObject
        {
            get
            {
                if ((_meshObject == null) || !_meshObject.data.isCreated)
                {
                    _meshObject = MeshObjectManager.GetByMesh(originalMesh);
                }

                return _meshObject;
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_DestroyExisting = new ProfilerMarker("DecomposedCollider.DestroyExisting");

        private void DestroyExisting()
        {
            using (_PRF_DecomposedCollider_DestroyExisting.Auto())
            {
                AssetDatabaseManager.Refresh();

                if (decomposedMeshes != null)
                {
                    for (var i = decomposedMeshes.Length - 1; i >= 0; i--)
                    {
                        var dec = decomposedMeshes[i];

                        if (dec == null)
                        {
                            continue;
                        }

                        var path = AssetDatabaseManager.GetAssetPath(dec);

                        if (string.IsNullOrWhiteSpace(path))
                        {
                            dec.DestroySafely();
                        }
                        else
                        {
                            AssetDatabaseManager.DeleteAsset(path);
                        }
                    }
                }

                InitializeColliders();

                if (colliders != null)
                {
                    for (var i = colliders.Length - 1; i >= 0; i--)
                    {
                        colliders[i].DestroySafely();
                    }

                    colliders = null;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_AssignAndSaveMeshes = new ProfilerMarker("DecomposedCollider.AssignAndSaveMeshes");

        private void AssignAndSaveMeshes(List<Mesh> meshes)
        {
            using (_PRF_DecomposedCollider_AssignAndSaveMeshes.Auto())
            {
                decomposedMeshes = new Mesh[meshes.Count];
                colliders = new MeshCollider[meshes.Count];

                var originalMeshPath = AssetDatabaseManager.GetAssetPath(originalMesh);
                var originalDirectory = Path.GetDirectoryName(originalMeshPath);

                var newDirectory = Path.Combine(originalDirectory, "Colliders");

                if (!AppaDirectory.Exists(newDirectory))
                {
                    AssetDatabaseManager.CreateFolder(originalDirectory, "Colliders");
                }

                newDirectory = newDirectory.Replace(Application.dataPath, "Assets");

                var colliderRoot = colliderTransform.gameObject;

                var meshPaths = new string[meshes.Count];

                for (var i = 0; i < meshes.Count; i++)
                {
                    var meshName = $"{originalMesh.name}_convex_{i}.mesh";

                    meshPaths[i] = Path.Combine(newDirectory, meshName);

                    AssetDatabaseManager.CreateAsset(meshes[i], meshPaths[i]);

                    colliders[i] = colliderRoot.AddComponent<MeshCollider>();
                    colliders[i].convex = true;
                }

                AssetDatabaseManager.SaveAssets();

                for (var i = 0; i < meshes.Count; i++)
                {
                    decomposedMeshes[i] = AssetDatabaseManager.LoadAssetAtPath<Mesh>(meshPaths[i]);
                    colliders[i].sharedMesh = decomposedMeshes[i];
                }

                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private void ResetMeshObject()
        {
            _meshObject = default;
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_InitializeComponents = new ProfilerMarker("DecomposedCollider.InitializeComponents");

        private static void InitializeComponents(GameObject go, DecomposedCollider decomposed)
        {
            using (_PRF_DecomposedCollider_InitializeComponents.Auto())
            {
                if (decomposed.originalMesh == null)
                {
                    var renderers = go.GetComponentsInChildren<MeshRenderer>().OrderByDescending(r => r.GetSharedMesh().vertexCount).ToArray();

                    var last = renderers[renderers.Length - 1];

                    decomposed.originalMeshRenderer = last;

                    decomposed.originalMesh = last.GetSharedMesh();

                    decomposed.originalVolume = 0.0f;
                    decomposed.originalScale = 0.0f;
                }

                if (decomposed.originalMesh == null)
                {
                    return;
                }

                var modelPath = AssetDatabaseManager.GetAssetPath(decomposed.originalMesh);
                var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;

                if (decomposed.originalScale != importer.globalScale)
                {
                    decomposed.originalScale = importer.globalScale;

                    decomposed.ResetMeshObject();

                    decomposed.originalVolume = decomposed.fillHoles ? decomposed.meshObject.data.SolidVolume : decomposed.meshObject.data.Volume;
                }

                if (decomposed.originalVolume == 0.0f)
                {
                    decomposed.originalVolume = decomposed.fillHoles ? decomposed.meshObject.data.SolidVolume : decomposed.meshObject.data.Volume;
                }

                if (decomposed.originalMeshRenderer == null)
                {
                    decomposed.originalMeshRenderer = decomposed.GetComponentsInChildren<MeshRenderer>()
                                                                .FirstOrDefault(mr => mr.GetSharedMesh() == decomposed.originalMesh);
                }

                decomposed.InitializeColliders();

                if (decomposed.originalMeshRenderer == null)
                {
                    return;
                }

                var mrt = decomposed.originalMeshRenderer.transform;

                decomposed.colliderTransform.position = mrt.position;
                decomposed.colliderTransform.rotation = mrt.rotation;
                decomposed.colliderTransform.localScale = mrt.localScale;
            }
        }

        private const string MENU_BASE = "GameObject/Collisions/Convex Decompose/";
        private const string CREATE_BASE = MENU_BASE + "Create/";
        private const string TOGGLE_BASE = MENU_BASE + "Toggle/";
        private const string RIGID_BASE = MENU_BASE + "Rigidbodies/";
        private const string UTILITY_BASE = MENU_BASE + "Utility/";

        [MenuItem(CREATE_BASE + "1x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_1(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1f);
        }

        [MenuItem(CREATE_BASE + "2x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_2(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 2f);
        }

        [MenuItem(CREATE_BASE + "3x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_3(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 3f);
        }

        [MenuItem(CREATE_BASE + "4x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_4(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 4f);
        }

        [MenuItem(CREATE_BASE + "5x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_5(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 5f);
        }

        [MenuItem(CREATE_BASE + "7.5x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_75(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 7.5f);
        }

        [MenuItem(CREATE_BASE + "10.0x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_10(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 10f);
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_Decompose = new ProfilerMarker("DecomposedCollider.Decompose");

        public static void Decompose(MenuCommand menuCommand, float tolerance)
        {
            using (_PRF_DecomposedCollider_Decompose.Auto())
            {
                var go = menuCommand.context as GameObject;
                if (go == null)
                {
                    return;
                }

                var decomposed = go.GetComponent<DecomposedCollider>();

                if (decomposed == null)
                {
                    decomposed = go.AddComponent<DecomposedCollider>();
                }

                decomposed.maximumParts = 20;
                decomposed.successThreshold = tolerance;

                decomposed.ExecuteDecomposition(ExecutionStyle.Normal);
            }
        }

        [MenuItem(TOGGLE_BASE + "Enabled Decomposed Colliders", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Toggle_DecOn_OrigOff(MenuCommand menuCommand)
        {
            ToggleColliders(menuCommand, true, true, true, false);
        }

        [MenuItem(TOGGLE_BASE + "Enabled Original Colliders", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Toggle_DecOff_OrigOn(MenuCommand menuCommand)
        {
            ToggleColliders(menuCommand, true, false, true, true);
        }

        [MenuItem(TOGGLE_BASE + "Disable All Colliders", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Toggle_DecOff_OrigOff(MenuCommand menuCommand)
        {
            ToggleColliders(menuCommand, true, false, true, false);
        }

        private static readonly ProfilerMarker _PRF_DecomposedCollider_ToggleColliders = new ProfilerMarker("DecomposedCollider.ToggleColliders");

        private static void ToggleColliders(
            MenuCommand menuCommand,
            bool updateDecomposed,
            bool enableDecomposed,
            bool updateOriginal,
            bool enableOriginal)
        {
            using (_PRF_DecomposedCollider_ToggleColliders.Auto())
            {
                var go = menuCommand.context as GameObject;
                if (go == null)
                {
                    return;
                }

                var decomposedColliders = go.GetComponentsInChildren<DecomposedCollider>();

                for (var i = 0; i < decomposedColliders.Length; i++)
                {
                    var decomposed = decomposedColliders[i];

                    if (updateDecomposed && (decomposed.colliders != null))
                    {
                        for (var j = 0; j < decomposed.colliders.Length; j++)
                        {
                            decomposed.originals[j].enabled = enableDecomposed;
                        }
                    }

                    if (updateOriginal && (decomposed.originals != null))
                    {
                        for (var j = 0; j < decomposed.originals.Length; j++)
                        {
                            decomposed.originals[j].enabled = enableOriginal;
                        }
                    }
                }
            }
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/1 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_1(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 1.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/2 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_2(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 2.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/3 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_3(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 3.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/4 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_4(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 4.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/5 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_5(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 5.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/10 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_10(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 10.0f);
        }

        public static void AddRigidbody(MenuCommand menuCommand, float mass)
        {
            var go = menuCommand.context as GameObject;
            if (go == null)
            {
                return;
            }

            var decomposedColliders = go.GetComponentsInChildren<DecomposedCollider>();

            for (var i = 0; i < decomposedColliders.Length; i++)
            {
                var decomposed = decomposedColliders[i];

                var rigidbody = decomposed.GetComponent<Rigidbody>();

                if (!rigidbody)
                {
                    rigidbody = decomposed.gameObject.AddComponent<Rigidbody>();
                }

                rigidbody.detectCollisions = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.mass = mass;
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
            }
        }

        [MenuItem(RIGID_BASE + "Remove Rigidbody", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void RemoveRigidbody(MenuCommand menuCommand)
        {
            var go = menuCommand.context as GameObject;
            if (go == null)
            {
                return;
            }

            var decomposedColliders = go.GetComponentsInChildren<DecomposedCollider>();

            for (var i = 0; i < decomposedColliders.Length; i++)
            {
                var decomposed = decomposedColliders[i];

                var rigidbody = decomposed.GetComponent<Rigidbody>();

                if (!rigidbody)
                {
                    return;
                }

                if (Application.isPlaying)
                {
                    Destroy(rigidbody);
                }
                else
                {
                    DestroyImmediate(rigidbody);
                }
            }
        }

       
        [Serializable]
        public struct DecomposedColliderPhysics
        {
            [SerializeField, HorizontalGroup("A"), SmartLabel] 
            public PhysicMaterial material;
            
            [SerializeField, HorizontalGroup("A"), SmartLabel] 
            public Collider collider;

            [HideInInspector] public Vector3 center;
        }

        private HashSet<PhysicMaterial> _materials;
        private Dictionary<PhysicMaterial, Color> _colorDict;
        private static Transform[] _cachedSelections;
        private static int _cacheFrameCount;
            
        private void OnDrawGizmosSelected()
        {
            var frameCount = Time.frameCount;
            if (Time.frameCount > _cacheFrameCount)
            {
                _cachedSelections = UnityEditor.Selection.transforms;
                _cacheFrameCount = frameCount;
            }

            var found = false;

            var t = transform;
            
            for (var i = 0; i < _cachedSelections.Length; i++)
            {
                if (_cachedSelections[i] == t)
                {
                    found = true;
                }
            }

            if (!found) return;

            if (gizmoTransparency == null)
            {
                gizmoTransparency = ColorPrefs.DecomposedColliderAlpha;
            }
            
            if (_materials == null)
            {
                _materials = new HashSet<PhysicMaterial>();
            }
                        
            for (var i = 0; i < physicMaterials.Count; i++)
            {
                var phymat = physicMaterials[i];

                _materials.Add(phymat.material);
            }
            
            if (_colorDict == null)
            {
                _colorDict = new Dictionary<PhysicMaterial, Color>();
            }

            _colorDict.Clear();

            var colorCount = _materials.Count;
            
            for (var i = 0; i < physicMaterials.Count; i++)
            {
                var phymat = physicMaterials[i];

                if (_colorDict.ContainsKey(phymat.material))
                {
                    continue;
                }

                var hue = _colorDict.Count / (float) _materials.Count;
                
                _colorDict.Add(phymat.material, Color.HSVToRGB(hue, 1.0f, 1.0f));
            }

            var gc = Gizmos.color;
            var position = transform.position;
            var rotation = transform.rotation;
            var scale = transform.lossyScale;
            
            for (var i = 0; i < physicMaterials.Count; i++)
            {
                var physicMaterial = physicMaterials[i];

                var color = highlightedPhysicMaterial == i
                    ? Color.white
                    : physicMaterial.material == null
                        ? Color.black
                        : _colorDict[physicMaterial.material];

                var alpha = highlightedPhysicMaterial == i
                    ? gizmoTransparency.v * 2.0f
                    : physicMaterial.material == null
                        ? gizmoTransparency.v * .5f
                        : gizmoTransparency.v;

                color.a = alpha;

                Gizmos.color = color;

                var coll = physicMaterial.collider as MeshCollider;

                if (coll != null)
                {
                    Gizmos.DrawWireMesh(coll.sharedMesh, position, rotation, scale);

                    var labelPosition = t.TransformPoint(physicMaterial.center);
                    UnityEditor.Handles.Label(labelPosition, physicMaterial.material.name);
                }
            }

            Gizmos.color = gc;
        }

    }
}
*/
