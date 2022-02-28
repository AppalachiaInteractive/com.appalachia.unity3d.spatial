#if UNITY_EDITOR

#region

using Appalachia.Core.Assets;
using Appalachia.Core.Collections.Interfaces;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.MeshBurial.Collections;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    public class MeshBurialAdjustmentCollection : SingletonAppalachiaObject<MeshBurialAdjustmentCollection>
    {
        #region Fields and Autoproperties

        [SerializeField]
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            NumberOfItemsPerPage = 1
        )]
        private MeshBurialAdjustmentStateLookup _state;

        #endregion

        public IAppaLookupReadOnly<GameObject, MeshBurialAdjustmentState, AppaList_MeshBurialAdjustmentState>
            State
        {
            get
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentStateLookup();
                    MarkAsModified();

                    _state.Changed.Event += OnChanged;
                }

                return _state;
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
                        MarkAsModified();
                    }

                    return adjustment;
                }

                var newState = MeshBurialAdjustmentState.LoadOrCreateNew(prefab.name);
                newState.InitializeLookupStorage(prefab);

                _state.AddOrUpdate(prefab, newState);

                MarkAsModified();

                return newState;
            }
        }

        public void ResetState()
        {
            using (_PRF_ResetState.Auto())
            {
                if (_state == null)
                {
                    return;
                }

                for (var i = 0; i < _state.Count; i++)
                {
                    _state.at[i].ResetState();
                }

                _state.Clear();

                MarkAsModified();

                AssetDatabaseSaveManager.SaveAssetsNextFrame();
            }
        }

        /// <inheritdoc />
        protected override async AppaTask WhenEnabled()
        {
            await base.WhenEnabled();

            using (_PRF_WhenEnabled.Auto())
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentStateLookup();
                    MarkAsModified();
                }

                _state.Changed.Event += OnChanged;
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_GetByPrefab = new(_PRF_PFX + nameof(GetByPrefab));

        private static readonly ProfilerMarker _PRF_ResetState =
            new ProfilerMarker(_PRF_PFX + nameof(ResetState));

        #endregion
    }
}

#endif
