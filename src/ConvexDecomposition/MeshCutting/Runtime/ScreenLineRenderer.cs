#if UNITY_EDITOR
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Utility.Async;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.MeshCutting.Runtime
{
    public sealed class ScreenLineRenderer : AppalachiaBehaviour<ScreenLineRenderer>
    {
        // Line Drawn event handler
        public delegate void LineDrawnHandler(Vector3 begin, Vector3 end, Vector3 depth);

        public event LineDrawnHandler OnLineDrawn;

        #region Fields and Autoproperties

        private bool dragging;
        private Vector3 start;
        private Vector3 end;
        private Camera cam;

        public Material lineMaterial;

        #endregion

        #region Event Functions

        // Update is called once per frame
        private void Update()
        {
            if (ShouldSkipUpdate)
            {
                return;
            }

            if (!dragging && Input.GetMouseButtonDown(0))
            {
                start = cam.ScreenToViewportPoint(Input.mousePosition);
                dragging = true;
            }

            if (dragging)
            {
                end = cam.ScreenToViewportPoint(Input.mousePosition);
            }

            if (dragging && Input.GetMouseButtonUp(0))
            {
                // Finished dragging. We draw the line segment
                end = cam.ScreenToViewportPoint(Input.mousePosition);
                dragging = false;

                var startRay = cam.ViewportPointToRay(start);
                var endRay = cam.ViewportPointToRay(end);

                // Raise OnLineDrawnEvent
                OnLineDrawn?.Invoke(
                    startRay.GetPoint(cam.nearClipPlane),
                    endRay.GetPoint(cam.nearClipPlane),
                    endRay.direction.normalized
                );
            }
        }

        #endregion

        /// <inheritdoc />
        protected override async AppaTask Initialize(Initializer initializer)
        {
            await base.Initialize(initializer);

            cam = Camera.main;
            dragging = false;
        }

        /// <inheritdoc />
        protected override async AppaTask WhenDisabled()
        {
            await base.WhenDisabled();
            using (_PRF_WhenDisabled.Auto())
            {
                Camera.onPostRender -= PostRenderDrawLine;
            }
        }

        /// <inheritdoc />
        protected override async AppaTask WhenEnabled()
        {
            await base.WhenEnabled();
            using (_PRF_WhenEnabled.Auto())
            {
                Camera.onPostRender += PostRenderDrawLine;
            }
        }

        /// <summary>
        ///     Draws the line in viewport space using start and end variables
        /// </summary>
        private void PostRenderDrawLine(Camera c)
        {
            using (_PRF_PostRenderDrawLine.Auto())
            {
                if (dragging && lineMaterial)
                {
                    GL.PushMatrix();
                    lineMaterial.SetPass(0);
                    GL.LoadOrtho();
                    GL.Begin(GL.LINES);
                    GL.Color(Color.black);
                    GL.Vertex(start);
                    GL.Vertex(end);
                    GL.End();
                    GL.PopMatrix();
                }
            }
        }

        #region Profiling

        private static readonly ProfilerMarker _PRF_PostRenderDrawLine =
            new ProfilerMarker(_PRF_PFX + nameof(PostRenderDrawLine));

        #endregion
    }
}

#endif
