#region

using System;
using Appalachia.Jobs.VegetationStudio.Transformations;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public class MeshBurialVegetationQueueItem : MeshBurialManySameQueueItem
    {
        private const string _PRF_PFX = nameof(MeshBurialVegetationQueueItem) + ".";
        private static VegetationSystemPro _system;

        public MeshBurialVegetationQueueItem(
            int cellIndex,
            int packageIndex,
            int itemIndex,
            GameObject prefab,
            int length,
            bool adoptTerrainNormal = true) : base(
            $"VSP: {prefab.name} : [Cell {cellIndex}] [Package {packageIndex}] [Item {itemIndex}]",
            prefab,
            length,
            adoptTerrainNormal
        )
        {
            this.cellIndex = cellIndex;
            this.packageIndex = packageIndex;
            this.itemIndex = itemIndex;

            if (_system == null)
            {
                _system = VegetationStudioManager.Instance.VegetationSystemList[0];
            }
        }

        public int cellIndex { get; }

        public int packageIndex { get; }

        public int itemIndex { get; }

        /*
        protected override bool TryGetMatrixInternal(int i, out float4x4 matrix)
        {
            var cell = _system.VegetationCellList[cellIndex];
            var packageInstances = cell.VegetationPackageInstancesList[packageIndex];
            var items = packageInstances.VegetationItemMatrixList[itemIndex];

            matrix = items[i].Matrix;
            return true;
        }

        protected override void SetMatrixInternal(int i, float4x4 m)
        {
            var cell = _system.VegetationCellList[cellIndex];
            var packageInstances = cell.VegetationPackageInstancesList[packageIndex];
            var items = packageInstances.VegetationItemMatrixList[itemIndex];

            var instance = items[i];
            instance.Matrix = m;
            items[i] = instance;
        }
        */

        protected override float GetDegreeAdjustmentStrengthInternal()
        {
            return 1.0f;
        }

        protected override void OnCompleteInternal()
        {
        }

        private static readonly ProfilerMarker _PRF_GetAllMatrices = new ProfilerMarker(_PRF_PFX + nameof(GetAllMatrices));
        public override void GetAllMatrices(NativeList<float4x4> matrices)
        {
            using (_PRF_GetAllMatrices.Auto())
            {
                var cell = _system.VegetationCellList[cellIndex];
                var packageInstances = cell.VegetationPackageInstancesList[packageIndex];
                var items = packageInstances.VegetationItemMatrixList[itemIndex];

                matrices.Length = items.Length;

                new TransformationJob_MatrixInstance_float4x4 {input = items, output = matrices}.Run(items.Length);
            }
        }

        private static readonly ProfilerMarker _PRF_SetAllMatrices = new ProfilerMarker(_PRF_PFX + nameof(SetAllMatrices));
        public override void SetAllMatrices(NativeArray<float4x4> matrices)
        {
            using (_PRF_SetAllMatrices.Auto())
            {
                var cell = _system.VegetationCellList[cellIndex];
                var packageInstances = cell.VegetationPackageInstancesList[packageIndex];
                var items = packageInstances.VegetationItemMatrixList[itemIndex];

                new TransformationJob_float4x4_MatrixInstance {input = matrices, output = items}.Run(items.Length);
            }
        }

        public override string ToString()
        {
            return _system.VegetationPackageProList[packageIndex].VegetationInfoList[itemIndex].VegetationPrefab.name + " Queue Item";
        }
    }
}
