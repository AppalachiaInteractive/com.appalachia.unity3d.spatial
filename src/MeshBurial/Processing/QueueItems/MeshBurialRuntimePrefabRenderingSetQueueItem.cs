/*
#region

using System;
using Appalachia.Core.Jobs.Transfers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public class MeshBurialRuntimePrefabRenderingSetQueueItem : MeshBurialManySameQueueItem
    {
        private const string _PRF_PFX = nameof(MeshBurialRuntimePrefabRenderingSetQueueItem) + ".";
        
        private PrefabRenderingSet _set;

        public MeshBurialRuntimePrefabRenderingSetQueueItem(PrefabRenderingSet set) : base(
            $"PrefabSet: {set.prefab.name}",
            set.prefab,
            set.instanceManager.element.Count,
            set.modelOptions.burialOptions.adoptTerrainNormal
        )
        {
            _set = set;
        }

        /// <inheritdoc />
protected override float GetDegreeAdjustmentStrengthInternal()
        {
            return _set.modelOptions.burialOptions.adjustmentStrength;
        }

        /// <inheritdoc />
protected override void OnCompleteInternal()
        {
            _set.instanceManager.transferOriginalToCurrent = true;
        }

        private static readonly ProfilerMarker _PRF_GetAllMatrices = new ProfilerMarker(_PRF_PFX + nameof(GetAllMatrices));
        /// <inheritdoc />
public override void GetAllMatrices(NativeList<float4x4> matrices)
        {
            using (_PRF_GetAllMatrices.Auto())
            {
                matrices.AddRange(_set.instanceManager.element.matrices_original);
            }
        }

        private static readonly ProfilerMarker _PRF_SetAllMatrices = new ProfilerMarker(_PRF_PFX + nameof(SetAllMatrices));
        /// <inheritdoc />
public override void SetAllMatrices(NativeArray<float4x4> matrices)
        {
            using (_PRF_SetAllMatrices.Auto())
            {
                var mats = _set.instanceManager.element.matrices_original;

                new TransferJob_float4x4_NA_float4x4_NA {input = matrices, output = mats}.Run(mats.Length);

                if (_set.useLocations)
                {
                    if ((_set.locations.locations == null) || (_set.locations.locations.Length != mats.Length))
                    {
                        _set.locations.locations = new float4x4[mats.Length];
                    }

                    mats.AsArray().CopyTo(_set.locations.locations);

                    _set.MarkAsModified();
                    _set.locations.MarkAsModified();
                }
            }
        }

        [DebuggerStepThrough] public override string ToString()
        {
            return _set.name + " Queue Item";
        }
    }
}
*/


