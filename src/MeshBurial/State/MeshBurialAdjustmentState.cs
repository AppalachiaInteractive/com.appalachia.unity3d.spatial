#if UNITY_EDITOR

#region

using System;
using Appalachia.CI.Constants;
using Appalachia.Core.Attributes;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Execution;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.MeshBurial.Collections;
using Appalachia.Spatial.MeshBurial.Processing;
using Appalachia.Spatial.SpatialKeys;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    [CallStaticConstructorInEditor]
    public class MeshBurialAdjustmentState : AppalachiaObject<MeshBurialAdjustmentState>
    {
        #region Fields and Autoproperties

        [SerializeField]
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            NumberOfItemsPerPage = 10
        )]
        [HideReferenceObjectPicker]
        private MeshBurialAdjustmentEntryWrapperLookup _state;

        public GameObject prefab;

        private NativeHashMap<Matrix4x4Key, MeshBurialAdjustment> _nativeAdjustments;

        #endregion

        public void AddOrUpdate(Matrix4x4 matrix, bool adoptTerrainNormal, Matrix4x4 value, double error)
        {
            using (_PRF_AddOrUpdate.Auto())
            {
                InitializeLookup();

                var key = new Matrix4x4Key(matrix, CONSTANTS.MatrixKeyGrouping);

                lock (this)
                {
                    if (_state.ContainsKey(key))
                    {
                        var existing = _state[key];

                        existing.entry.adoptTerrainNormal = adoptTerrainNormal;
                        existing.entry.adjustment = value;
                        existing.entry.error = error;

                        _state[key] = existing;
                    }
                    else
                    {
                        var newAdjustment = new MeshBurialAdjustmentEntryWrapper
                        {
                            entry = new MeshBurialAdjustmentEntry
                            {
                                input = matrix,
                                adjustment = value,
                                adoptTerrainNormal = adoptTerrainNormal,
                                error = error
                            }
                        };

                        _state.AddOrUpdate(key, newAdjustment);
                    }

                    MarkAsModified();
                }
            }
        }

        public bool Contains(Matrix4x4 matrix, bool adoptTerrainNormal)
        {
            using (_PRF_Contains.Auto())
            {
                return TryGetValue(matrix, adoptTerrainNormal, out _);
            }
        }

        public NativeHashMap<Matrix4x4Key, MeshBurialAdjustment> GetNative()
        {
            using (_PRF_GetNative.Auto())
            {
                InitializeLookup();

                _nativeAdjustments.Clear();

                for (var i = 0; i < _state.Count; i++)
                {
                    var key = _state.GetKeyByIndex(i);
                    var adj = _state.at[i];

                    var adjustment = new MeshBurialAdjustment
                    {
                        matrix = adj.entry.adjustment, error = adj.entry.error
                    };

                    if (!_nativeAdjustments.ContainsKey(key))
                    {
                        _nativeAdjustments.Add(key, adjustment);
                    }
                    else
                    {
                        _nativeAdjustments[key] = adjustment;
                    }
                }

                return _nativeAdjustments;
            }
        }

        public void InitializeLookupStorage(GameObject pf)
        {
            using (_PRF_InitializeLookupStorage.Auto())
            {
                prefab = pf;

                InitializeLookup();
            }
        }

        public void ResetState()
        {
            using (_PRF_ResetState.Auto())
            {
                InitializeLookup();

                _state.Clear();
                if (_nativeAdjustments.IsCreated)
                {
                    _nativeAdjustments.Clear();
                }

                MarkAsModified();
            }
        }

        public bool TryGetValue(
            Matrix4x4 matrix,
            bool adoptTerrainNormal,
            out MeshBurialAdjustmentEntryWrapper value)
        {
            using (_PRF_TryGetValue.Auto())
            {
                InitializeLookup();

                var key = new Matrix4x4Key(matrix, CONSTANTS.MatrixKeyGrouping);

                if (_state.ContainsKey(key))
                {
                    lock (this)
                    {
                        if (_state.ContainsKey(key))
                        {
                            var testValue = _state[key];

                            if (testValue.entry.adoptTerrainNormal == adoptTerrainNormal)
                            {
                                value = testValue;
                                return true;
                            }
                        }
                    }
                }

                value = default;
                return false;
            }
        }

        internal void Consume(MeshBurialAdjustmentState other)
        {
            using (_PRF_Consume.Auto())
            {
                lock (this)
                {
                    for (var i = 0; i < other._state.Count; i++)
                    {
                        var otherUpdate = other._state.at[i];

                        var key = new Matrix4x4Key(otherUpdate.entry.input, CONSTANTS.MatrixKeyGrouping);

                        _state.AddIfKeyNotPresent(key, otherUpdate);
                    }

                    MarkAsModified();
                }
            }
        }

        /// <inheritdoc />
        protected override async AppaTask Initialize(Initializer initializer)
        {
            await base.Initialize(initializer);

            InitializeLookup();
        }

        /// <inheritdoc />
        protected override async AppaTask WhenDisabled()
        {
            await base.WhenDisabled();
            using (_PRF_WhenDisabled.Auto())
            {
                _nativeAdjustments.SafeDispose();
            }
        }

        private void InitializeLookup()
        {
            using (_PRF_InitializeLookup.Auto())
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentEntryWrapperLookup();
                    MarkAsModified();
                }

                _state.Changed.Event +=(OnChanged);

                if (_nativeAdjustments.ShouldAllocate())
                {
                    _nativeAdjustments =
                        new NativeHashMap<Matrix4x4Key, MeshBurialAdjustment>(2048, Allocator.Persistent);

                    CleanupManager.Store(() => _nativeAdjustments.SafeDispose());
                }
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_AddOrUpdate = new(_PRF_PFX + nameof(AddOrUpdate));
        private static readonly ProfilerMarker _PRF_Consume = new(_PRF_PFX + nameof(Consume));
        private static readonly ProfilerMarker _PRF_Contains = new(_PRF_PFX + nameof(Contains));

        private static readonly ProfilerMarker _PRF_GetNative = new(_PRF_PFX + nameof(GetNative));

        private static readonly ProfilerMarker _PRF_InitializeLookup =
            new(_PRF_PFX + nameof(InitializeLookup));

        private static readonly ProfilerMarker _PRF_InitializeLookupStorage =
            new(_PRF_PFX + nameof(InitializeLookupStorage));

        private static readonly ProfilerMarker _PRF_OnDisable = new(_PRF_PFX + nameof(OnDisable));

        private static readonly ProfilerMarker _PRF_OnEnable = new(_PRF_PFX + nameof(OnEnable));
        private static readonly ProfilerMarker _PRF_ResetState = new(_PRF_PFX + nameof(ResetState));

        private static readonly ProfilerMarker _PRF_TryGetValue = new(_PRF_PFX + nameof(TryGetValue));

        #endregion
    }
}

#endif
