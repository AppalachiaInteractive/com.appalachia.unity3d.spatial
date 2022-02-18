#if UNITY_EDITOR
using Appalachia.Editing.Visualizers;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Appalachia.Spatial.Visualizers
{
    public abstract class InstancedIndirectSpatialMapVisualization<T> : InstancedIndirectGridVisualization<T>
        where T : InstancedIndirectSpatialMapVisualization<T>
    {
        #region Fields and Autoproperties

        [PropertyOrder(-150)]
        [OnValueChanged(nameof(Regenerate))]
        public Texture2D texture;

        [PropertyOrder(-149)] public Vector3 size = Vector3.one;

        private NativeArray<float4> _data;

        #endregion

        protected abstract void GetVisualizationInfo(
            Vector3 position,
            float4 color,
            out float height,
            out Quaternion rotation,
            out Vector3 scale);

        /// <inheritdoc />
        protected override Bounds GetBounds()
        {
            var center = Transform.position + (.5f * size);
            return new Bounds(center, size);
        }

        /// <inheritdoc />
        protected override void GetGridPosition(
            Vector3 position,
            out float height,
            out Quaternion rotation,
            out Vector3 scale)
        {
            if (texture == null)
            {
                height = default;
                rotation = default;
                scale = default;
                return;
            }

            if (!_data.IsCreated)
            {
                _data = SpatialMapJobHelper.LoadMapData(texture, Allocator.Persistent);
            }

            var valueAtPosition = SpatialMapJobHelper.GetWorldSpaceValue(
                position,
                Transform.position,
                _data,
                texture.width,
                texture.height,
                size
            );

            GetVisualizationInfo(position, valueAtPosition, out height, out rotation, out scale);
        }

        /// <inheritdoc />
        protected override async AppaTask WhenDisabled()
        {
            await base.WhenDisabled();

            using (_PRF_WhenDisabled.Auto())
            {
                texture = null;
            }
        }
    }
}

#endif
