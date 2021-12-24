﻿#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Root;
using Appalachia.Utility.Async;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

namespace Appalachia.Spatial.ConvexDecomposition.MeshCutting.Runtime
{
    public sealed class ObjectManager : AppalachiaBehaviour<ObjectManager>
    {
        #region Fields and Autoproperties

        public List<MeshRenderer> objects;

        public Dropdown dropdown;

        public Transform ObjectContainer;

        private CameraOrbit cameraOrbit;

        #endregion

        public void LoadObject()
        {
            if (dropdown.value >= objects.Count)
            {
                throw new UnityException("Error: selected object is out of range");
            }

            var SelectedObject = objects[dropdown.value];

            // Clear children
            foreach (Transform child in ObjectContainer)
            {
                Destroy(child.gameObject);
            }

            // Load new object in container and set to camera orbit
            cameraOrbit.target = Instantiate(SelectedObject, ObjectContainer).transform;
        }

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);

                dropdown.ClearOptions();
                dropdown.AddOptions(objects.Select(r => r.gameObject.name).ToList());

                cameraOrbit = FindObjectOfType<CameraOrbit>();
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(ObjectManager) + ".";

        private static readonly ProfilerMarker _PRF_Initialize =
            new ProfilerMarker(_PRF_PFX + nameof(Initialize));

        #endregion
    }
}

#endif
