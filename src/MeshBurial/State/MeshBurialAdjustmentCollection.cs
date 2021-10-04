#region

using Appalachia.Base.Scriptables;
using Appalachia.Core.Collections.Interfaces;
using Appalachia.Editing.Assets;
using Appalachia.Spatial.MeshBurial.Collections;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;

#if UNITY_EDITOR

#endif

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    public class
        MeshBurialAdjustmentCollection : SelfSavingSingletonScriptableObject<
            MeshBurialAdjustmentCollection>
    {
        private const string _PRF_PFX = nameof(MeshBurialAdjustmentCollection) + ".";

        private static readonly ProfilerMarker _PRF_WhenEnabled =
            new(_PRF_PFX + nameof(WhenEnabled));

        private static readonly ProfilerMarker _PRF_GetByPrefab =
            new(_PRF_PFX + nameof(GetByPrefab));

        private static readonly ProfilerMarker _PRF_Reset = new(_PRF_PFX + nameof(Reset));

        [SerializeField]
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            NumberOfItemsPerPage = 1
        )]
        private MeshBurialAdjustmentStateLookup _state;

        public IAppaLookupReadOnly<GameObject, MeshBurialAdjustmentState,
            AppaList_MeshBurialAdjustmentState> State
        {
            get
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentStateLookup();
                    SetDirty();

                    _state.SetDirtyAction(SetDirty);
                }

                return _state;
            }
        }

        public void Reset()
        {
            using (_PRF_Reset.Auto())
            {
                for (var i = 0; i < _state.Count; i++)
                {
                    _state.at[i].Reset();
                }

                _state.Clear();

                SetDirty();

                AssetDatabaseSaveManager.SaveAssetsNextFrame();
            }
        }

        protected override void WhenEnabled()
        {
            using (_PRF_WhenEnabled.Auto())
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentStateLookup();
                    SetDirty();
                }

                _state.SetDirtyAction(SetDirty);
            }
        }

        public MeshBurialAdjustmentState GetByPrefab(GameObject prefab)
        {
            using (_PRF_GetByPrefab.Auto())
            {
                if (_state.ContainsKey(prefab))
                {
                    var adjustment = _state[prefab];

                    if (adjustment == null)
                    {
                        //adjustment = new MeshBurialAdjustmentState() {prefab = prefab, assetGUID = assetGUID};
                        adjustment = MeshBurialAdjustmentState.LoadOrCreateNew(prefab.name);
                        adjustment.InitializeLookupStorage(prefab);

                        _state.AddOrUpdate(prefab, adjustment);
                        SetDirty();
                    }

                    return adjustment;
                }

                var newState = MeshBurialAdjustmentState.LoadOrCreateNew(prefab.name);
                newState.InitializeLookupStorage(prefab);

                _state.AddOrUpdate(prefab, newState);

                SetDirty();

                return newState;
            }
        }
    }
}
