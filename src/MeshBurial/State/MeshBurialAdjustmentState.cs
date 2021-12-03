#if UNITY_EDITOR

#region

using System;
using Appalachia.CI.Constants;
using Appalachia.Core.Collections.Native;
using Appalachia.Core.Scriptables;
using Appalachia.Jobs.MeshData;
using Appalachia.Spatial.MeshBurial.Collections;
using Appalachia.Spatial.MeshBurial.Processing;
using Appalachia.Spatial.SpatialKeys;
using Appalachia.Utility.Extensions;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialAdjustmentState  : AppalachiaObject
    {
        private const string _PRF_PFX = nameof(MeshBurialAdjustmentState) + ".";

        private static readonly ProfilerMarker _PRF_InitializeLookupStorage =
            new(_PRF_PFX + nameof(InitializeLookupStorage));

        private static readonly ProfilerMarker _PRF_OnEnable = new(_PRF_PFX + nameof(OnEnable));

        private static readonly ProfilerMarker _PRF_OnDisable = new(_PRF_PFX + nameof(OnDisable));

        private static readonly ProfilerMarker _PRF_InitializeLookup =
            new(_PRF_PFX + nameof(InitializeLookup));

        private static readonly ProfilerMarker _PRF_TryGetValue =
            new(_PRF_PFX + nameof(TryGetValue));

        private static readonly ProfilerMarker _PRF_Contains = new(_PRF_PFX + nameof(Contains));

        private static readonly ProfilerMarker _PRF_AddOrUpdate =
            new(_PRF_PFX + nameof(AddOrUpdate));

        private static readonly ProfilerMarker _PRF_Reset = new(_PRF_PFX + nameof(Reset));

        private static readonly ProfilerMarker _PRF_Consume = new(_PRF_PFX + nameof(Consume));

        private static readonly ProfilerMarker _PRF_GetNative = new(_PRF_PFX + nameof(GetNative));

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

               this.MarkAsModified();
            }
        }

        protected override void OnEnable()
        {
            using (_PRF_OnEnable.Auto())
            {
                base.OnEnable();
                InitializeLookup();
            }
        }

        private void OnDisable()
        {
            using (_PRF_OnDisable.Auto())
            {
                _nativeAdjustments.SafeDispose();
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

        private void InitializeLookup()
        {
            using (_PRF_InitializeLookup.Auto())
            {
                if (_state == null)
                {
                    _state = new MeshBurialAdjustmentEntryWrapperLookup();
                   this.MarkAsModified();
                }

                _state.SetMarkModifiedAction(this.MarkAsModified);

                if (_nativeAdjustments.ShouldAllocate())
                {
                    _nativeAdjustments =
                        new NativeHashMap<Matrix4x4Key, MeshBurialAdjustment>(
                            2048,
                            Allocator.Persistent
                        );
                    MeshObjectManager.RegisterDisposalDependency(
                        () => _nativeAdjustments.SafeDispose()
                    );
                }
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

        public bool Contains(Matrix4x4 matrix, bool adoptTerrainNormal)
        {
            using (_PRF_Contains.Auto())
            {
                return TryGetValue(matrix, adoptTerrainNormal, out _);
            }
        }

        public void AddOrUpdate(
            Matrix4x4 matrix,
            bool adoptTerrainNormal,
            Matrix4x4 value,
            double error)
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

                   this.MarkAsModified();
                }
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

                        var key = new Matrix4x4Key(
                            otherUpdate.entry.input,
                            CONSTANTS.MatrixKeyGrouping
                        );

                        _state.AddIfKeyNotPresent(key, otherUpdate);
                    }

                   this.MarkAsModified();
                }
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
    }
}

#endif