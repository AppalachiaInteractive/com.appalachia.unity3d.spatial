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

        #region Fields and Autoproperties

        private bool dragging;
        private Vector3 start;
        private Vector3 end;
        private Camera cam;

        public Material lineMaterial;

        #endregion

        public event LineDrawnHandler OnLineDrawn;

        #region Event Functions

        // Update is called once per frame
        private void Update()
        {
            if (!DependenciesAreReady || !FullyInitialized)
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

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);

                cam = Camera.main;
                dragging = false;
            }
        }

        protected override async AppaTask WhenDisabled()

        {
            await base.WhenDisabled();
            Camera.onPostRender -= PostRenderDrawLine;
        }

        protected override async AppaTask WhenEnabled()
        {
            await base.WhenEnabled();
            Camera.onPostRender += PostRenderDrawLine;
        }

        /// <summary>
        ///     Draws the line in viewport space using start and end variables
        /// </summary>
        private void PostRenderDrawLine(Camera c)
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

        #region Profiling

        private const string _PRF_PFX = nameof(ScreenLineRenderer) + ".";

        private static readonly ProfilerMarker _PRF_Initialize =
            new ProfilerMarker(_PRF_PFX + nameof(Initialize));

        #endregion
    }
}

#endif
