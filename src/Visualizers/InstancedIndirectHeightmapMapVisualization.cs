#if UNITY_EDITOR
using Appalachia.Editing.Visualizers;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace Appalachia.Spatial.Visualizers
{
    public abstract class
        InstancedIndirectHeightmapMapVisualization<T> : InstancedIndirectGridVisualization<T>
        where T : InstancedIndirectHeightmapMapVisualization<T>
    {
        #region Fields and Autoproperties

        [HorizontalGroup("A")]
        [PropertyOrder(-100)]
        [OnValueChanged(nameof(Regenerate))]
        public Texture2D texture;

        public Vector3 size = Vector3.one;

        protected NativeArray<float> _data;

        #endregion

        [HorizontalGroup("A")]
        [PropertyOrder(-100)]
        [ShowInInspector]
        [HideLabel]
        [PreviewField(ObjectFieldAlignment.Right, Height = 128)]
        public Texture2D preview => texture;

        /// <inheritdoc />
        protected override bool CanVisualize => _data.IsCreated;

        protected abstract void GetVisualizationInfo(
            Vector3 position,
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
            if (!_data.IsCreated)
            {
                _data = HeightmapJobHelper.LoadHeightData(texture, Allocator.Persistent);
            }

            var mapPosition = Transform.position;

            height = HeightmapJobHelper.GetWorldSpaceHeight(
                position,
                mapPosition,
                _data,
                texture.width,
                texture.height,
                size
            );

            position.y = height;

            var normal = HeightmapJobHelper.GetHeightmapNormal(
                position,
                mapPosition,
                _data,
                texture.width,
                texture.height,
                size
            );

            GetVisualizationInfo(position, out rotation, out scale);
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
