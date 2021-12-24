#region

using System;
using Appalachia.Core.Objects.Root;
using UnityEngine;

#endregion

namespace Appalachia.Spatial
{
    [Serializable]
    public class TransformAnalysis : AppalachiaSimpleBase
    {
        public Vector3 minLocal;
        public Vector3 maxLocal;
        public Vector3 avgLocal;

        public Vector3 minWorld;
        public Vector3 maxWorld;
        public Vector3 avgWorld;

        public int children;

        public TransformAnalysis(GameObject go)
        {
            if (go.transform.childCount == 0)
            {
                return;
            }

            children = go.transform.childCount;

            for (var i = 0; i < go.transform.childCount; i++)
            {
                var localPosition = go.transform.GetChild(i).transform.localPosition;
                var worldPosition = go.transform.GetChild(i).transform.localPosition;

                avgLocal += localPosition;
                avgWorld += worldPosition;

                minLocal = Vector3.Min(minLocal, localPosition);
                minWorld = Vector3.Min(minWorld, worldPosition);

                maxLocal = Vector3.Max(maxLocal, localPosition);
                maxWorld = Vector3.Max(maxWorld, worldPosition);
            }

            avgLocal /= children;
            avgWorld /= children;
        }
    }
}
