#region

using System;
using Appalachia.Base.Scriptables;
using Appalachia.Core.Collections.Implementations.Lookups;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Constants;
using Appalachia.MeshData;
using Appalachia.Spatial.MeshBurial.Collections;
using Appalachia.Spatial.MeshBurial.Processing;
using Appalachia.Spatial.SpatialKeys;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialAdjustmentState : SelfSavingScriptableObject<MeshBurialAdjustmentState>
    {
        private const string _PRF_PFX = nameof(MeshBurialAdjustmentState) + ".";

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

        private static readonly ProfilerMarker _PRF_InitializeLookupStorage = new ProfilerMarker(_PRF_PFX + nameof(InitializeLookupStorage));
        public void InitializeLookupStorage(GameObject pf)
        {
            using (_PRF_InitializeLookupStorage.Auto())
            {
                prefab = pf;

                InitializeLookup();
            }
        }

        private static readonly ProfilerMarker _PRF_OnEnable = new ProfilerMarker(_PRF_PFX + nameof(OnEnable));
        private void OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
                InitializeLookup();
            }
        }

        private static readonly ProfilerMarker _PRF_OnDisable = new ProfilerMarker(_PRF_PFX + nameof(OnDisable));
        private void OnDisable()
        {
            using (_PRF_OnDisable.Auto())
            {
                _nativeAdjustments.SafeDispose();
            }
        }

        private static readonly ProfilerMarker _PRF_InitializeLookup = new ProfilerMarker(_PRF_PFX + nameof(InitializeLookup));
        private void InitializeLookup()
        {
            using (_PRF_InitializeLookup.Auto())
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentEntryWrapperLookup();
                    SetDirty();
                }

                _state.SetDirtyAction(SetDirty);

                if (_nativeAdjustments.ShouldAllocate())
                {
                    _nativeAdjustments = new NativeHashMap<Matrix4x4Key, MeshBurialAdjustment>(2048, Allocator.Persistent);
                    MeshObjectManager.RegisterDisposalDependency(
                        () => _nativeAdjustments.SafeDispose()
                    );
                }
            }
        }

        private static readonly ProfilerMarker _PRF_TryGetValue = new ProfilerMarker(_PRF_PFX + nameof(TryGetValue));
        public bool TryGetValue(Matrix4x4 matrix, bool adoptTerrainNormal, out MeshBurialAdjustmentEntryWrapper value)
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

        private static readonly ProfilerMarker _PRF_Contains = new ProfilerMarker(_PRF_PFX + nameof(Contains));
        public bool Contains(Matrix4x4 matrix, bool adoptTerrainNormal)
        {
            using (_PRF_Contains.Auto())
            {
                return TryGetValue(matrix, adoptTerrainNormal, out _);
            }
        }

        private static readonly ProfilerMarker _PRF_AddOrUpdate = new ProfilerMarker(_PRF_PFX + nameof(AddOrUpdate));
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

                    SetDirty();
                }
            }
        }

        private static readonly ProfilerMarker _PRF_Reset = new ProfilerMarker(_PRF_PFX + nameof(Reset));
        internal void Reset()
        {
            using (_PRF_Reset.Auto())
            {
                InitializeLookup();

                _state.Clear();
                if (_nativeAdjustments.IsCreated)
                {
                    _nativeAdjustments.Clear();
                }

                SetDirty();
            }
        }

        private static readonly ProfilerMarker _PRF_Consume = new ProfilerMarker(_PRF_PFX + nameof(Consume));
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

                    SetDirty();
                }
            }
        }

        private static readonly ProfilerMarker _PRF_GetNative = new ProfilerMarker(_PRF_PFX + nameof(GetNative));
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

                    var adjustment = new MeshBurialAdjustment {matrix = adj.entry.adjustment, error = adj.entry.error};

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
    }
}
