#if UNITY_EDITOR
using UnityEngine;

namespace Appalachia.Spatial.Visualizers
{
    public sealed class HeightmapVisualizer : InstancedIndirectHeightmapMapVisualization<HeightmapVisualizer>
    {
        /// <inheritdoc />
        protected override bool CanGenerate => texture != null;

        /// <inheritdoc />
        protected override bool ShouldRegenerate => false;

        /// <inheritdoc />
        protected override void GetVisualizationInfo(
            Vector3 position,
            out Quaternion rotation,
            out Vector3 scale)
        {
            rotation = Quaternion.identity;
            scale = Vector3.one * visualizationSize;
        }

        /// <inheritdoc />
        protected override void PrepareInitialGeneration()
        {
        }

        /// <inheritdoc />
        protected override void PrepareSubsequentGenerations()
        {
        }
    }
}
#endif
