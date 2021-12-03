#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Core.Attributes.Editing;
using Appalachia.Spatial.ConvexDecomposition.API;
using Appalachia.Utility.Extensions;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine.Serialization;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition
{
    [Serializable]
    public struct ConvexMeshSettings : IEquatable<ConvexMeshSettings>
    {
        #region Ranges
        public struct Ranges
        {
            public const int maxConvexHulls_MIN = 1;

            //public const int maxConvexHulls_MAX = 1024;
            public const int maxConvexHulls_MAX = 30;

            //public const int resolution_MIN = 10000;
            public const int resolution_MIN = 10000;

            //public const int resolution_MAX = 64000000;
            public const int resolution_MAX = 1500000;

            public const int planeDownsampling_MIN = 1;
            public const int planeDownsampling_MAX = 16;

            public const int convexHullDownsampling_MIN = 1;
            public const int convexHullDownsampling_MAX = 16;

            //public const int maximumVerticesPerHull_MIN = 4;
            public const int maximumVerticesPerHull_MIN = 64;
            public const int maximumVerticesPerHull_MAX = 256;

            public const double concavity_MIN = 0.0;

            //public const double concavity_MAX = 1.0;
            public const double concavity_MAX = 0.01;

            public const double alpha_MIN = 0.0;

            //public const double alpha_MAX = 1.0;
            public const double alpha_MAX = 0.01;

            public const double beta_MIN = 0.0;

            //public const double beta_MAX = 1.0;
            public const double beta_MAX = 0.01;

            public const double minimumVolumePerHull_MIN = 0.0;

            //public const double minimumVolumePerHull_MAX = 0.1;
            public const double minimumVolumePerHull_MAX = 0.01;
        }
        #endregion
        
        // 0: voxel - based approximate convex decomposition, 1 : tetrahedron - based
        // approximate convex decomposition (default = 0, range = { 0,1 })
        [SmartLabel]
        [HorizontalGroup("A", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public ConvexMeshDecompositionMode mode;

        // Maximum number of convex hulls to produce (default = 64, range = 1 - 1024)
        [SmartLabel, PropertyRange(Ranges.maxConvexHulls_MIN, Ranges.maxConvexHulls_MAX)]
        [HorizontalGroup("A", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int maxConvexHulls;
        
        // Enable / disable normalizing the mesh before applying the convex
        // decomposition (default = false)
        [SmartLabel(Postfix = true)]
        [HorizontalGroup("B", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public bool normalize;

        // Maximum number of voxels generated during the voxelization stage
        // (default = 100,000, range = 10,000 - 64,000,000)
        [SmartLabel, PropertyRange(Ranges.resolution_MIN, Ranges.resolution_MAX)]
        [HorizontalGroup("B", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int resolution;

        // Controls the granularity of the search for the "best" clipping plane
        // (default = 4, range = 1 - 16)
        [SmartLabel, PropertyRange(Ranges.planeDownsampling_MIN, Ranges.planeDownsampling_MAX)]
        [HorizontalGroup("C", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int planeDownsampling;

        // Controls the precision of the convex - hull generation process during
        // the clipping plane selection stage(default = 4, range = 1 - 16)
        [FormerlySerializedAs("convexhullDownsampling")]
        [SmartLabel, PropertyRange(Ranges.convexHullDownsampling_MIN, Ranges.convexHullDownsampling_MAX)]
        [HorizontalGroup("C", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int convexHullDownsampling;

        // Maximum allowed concavity (default = 0.0025, range = 0.0 - 1.0)
        [SmartLabel, PropertyRange(Ranges.concavity_MIN, Ranges.concavity_MAX)]
        [HorizontalGroup("D", .33f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public double concavity;

        // Controls the bias toward clipping along symmetry planes
        // (default = 0.05, range = 0.0 - 1.0)
        [SmartLabel, PropertyRange(Ranges.alpha_MIN, Ranges.alpha_MAX)]
        [HorizontalGroup("D", .33f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public double alpha;

        // Controls the bias toward clipping along revolution axes
        // (default = 0.05, range = 0.0 - 1.0)
        [SmartLabel, PropertyRange(Ranges.beta_MIN, Ranges.beta_MAX)]
        [HorizontalGroup("D", .33f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public double beta;

        // Controls the maximum number of triangles per convex hull
        // (default = 64, range = 4 - 1024)
        [FormerlySerializedAs("maxVerticesPerHull")]
        [FormerlySerializedAs("maxNumVerticesPerCH")]
        [SmartLabel, PropertyRange(Ranges.maximumVerticesPerHull_MIN, Ranges.maximumVerticesPerHull_MAX)]
        [HorizontalGroup("E", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int maximumVerticesPerHull;

        // Controls the adaptive sampling of the generated convex hulls
        // (default = 0.0001, range = 0.0 - 0.01)
        [FormerlySerializedAs("minVolumePerCH")]
        [SmartLabel, PropertyRange(Ranges.minimumVolumePerHull_MIN, Ranges.minimumVolumePerHull_MAX)]
        [HorizontalGroup("E", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public double minimumVolumePerHull;

        // Enable / disable approximation when computing convex hulls
        // (default = true)
        [SmartLabel(Postfix = true)]
        [HorizontalGroup("F", .5f)]
        [ReadOnly]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public bool convexHullApproximation;

        // Project the output convex hull vertices onto the original source mesh to
        // increase the floating point accuracy of the results (default = true)
        [SmartLabel(Postfix = true)]
        [HorizontalGroup("F", .5f)]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public bool projectHullVertices;

        [NonSerialized]
        public bool dirty;

        private void MarkFieldsModified()
        {
            dirty = true;
        }

        private ValueDropdownList<int> ___openCLPlatforms;

        private ValueDropdownList<int> _openCLPlatforms
        {
            get
            {
                if (___openCLPlatforms == null)
                {
                    ___openCLPlatforms = new ValueDropdownList<int>();

                    var platforms = VHCDAPI.GetPlatforms();

                    for (var i = 0; i < platforms.Count; i++)
                    {
                        ___openCLPlatforms.Add(new ValueDropdownItem<int>(platforms[i], i));
                    }
                }

                return ___openCLPlatforms;
            }
        }

        private ValueDropdownList<int> ___openCLDevices;

        private ValueDropdownList<int> _openCLDevices
        {
            get
            {
                if (___openCLDevices == null)
                {
                    ___openCLDevices = new ValueDropdownList<int>();

                    var devices = VHCDAPI.GetDevices((uint) openClPlatform);

                    for (var i = 0; i < devices.Count; i++)
                    {
                        ___openCLDevices.Add(new ValueDropdownItem<int>(devices[i], i));
                    }
                }

                return ___openCLDevices;
            }
        }

        private bool _canAccelerate => (_openCLPlatforms.Count > 0) && (_openCLDevices.Count > 0);
        private bool _canAccelerateDetail => _canAccelerate && openClAcceleration;

        // Project the output convex hull vertices onto the original source mesh to
        // increase the floating point accuracy of the results (default = true)
        [SmartLabel(Text = "OpenCL Acceleration", Postfix=true)]
        [HorizontalGroup("G", .33f)]
        [EnableIf(nameof(_canAccelerate))]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public bool openClAcceleration;

        // OpenCL platform id (default = 0, range = 0 - # OCL platforms)
        [SmartLabel(Text = "OpenCL Platform")]
        [HorizontalGroup("G", .33f)]
        [ValueDropdown(nameof(_openCLPlatforms))]
        [EnableIf(nameof(_canAccelerateDetail))]
        [OnValueChanged(nameof(ResetOpenCLDevice))]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int openClPlatform;

        private void ResetOpenCLDevice()
        {
            openClDevice = 0;
        }

        // OpenCL device id (default = 0, range = 0 - # OCL devices)
        [SmartLabel(Text = "OpenCL Device")]
        [HorizontalGroup("G", .33f)]
        [ValueDropdown(nameof(_openCLDevices))]
        [EnableIf(nameof(_canAccelerateDetail))]
        [OnValueChanged(nameof(MarkFieldsModified))]
        public int openClDevice;

        public VHACDSession ToSession()
        {
            var session = VHACDSession.Create();

            session.maxConvexHulls = maxConvexHulls;
            session.resolution = resolution;
            session.concavity = concavity;
            session.planeDownsampling = planeDownsampling;
            session.convexhullDownsampling = convexHullDownsampling;
            session.alpha = alpha;
            session.beta = beta;
            session.pca = normalize ? 1 : 0;
            session.mode = (int) mode;
            session.maxNumVerticesPerCH = maximumVerticesPerHull;
            session.minVolumePerCH = minimumVolumePerHull;
            session.convexHullApproximation = convexHullApproximation ? 1 : 0;
            session.projectHullVertices = projectHullVertices ? 1 : 0;
            session.oclAcceleration = _canAccelerate
                ? openClAcceleration
                    ? 1
                    : 0
                : 0;
            session.oclPlatformID = session.oclAcceleration == 1
                ? openClAcceleration
                    ? openClPlatform
                    : 0
                : 0;
            session.oclDeviceID = session.oclAcceleration == 1
                ? openClAcceleration
                    ? openClDevice
                    : 0
                : 0;

            return session;
        }

        public static ConvexMeshSettings Default()
        {
            var x = new ConvexMeshSettings();

            x.DefaultSettings();

            return x;
        }

        [ButtonGroup, LabelText("Voxel")]
        public void Voxel()
        {
            DefaultSettings();
            mode = ConvexMeshDecompositionMode.Voxel;
            dirty = true;
        }

        [ButtonGroup, LabelText("Tetrahedron")]
        public void Tetrahedron()
        {
            DefaultSettings();
            mode = ConvexMeshDecompositionMode.Tetrahedron;
            dirty = true;
        }

        [ButtonGroup, LabelText("Default")]
        public void DefaultSettings()
        {
            maxConvexHulls = 6;
            resolution = 200000;
            concavity = 0;
            planeDownsampling = 4;
            convexHullDownsampling = 16;
            alpha = 0;
            beta = 0;
            normalize = true;
            mode = ConvexMeshDecompositionMode.Voxel;
            maximumVerticesPerHull = 128;
            minimumVolumePerHull = 0;
            convexHullApproximation = true;
            projectHullVertices = false;
            openClAcceleration = _canAccelerate;
            dirty = true;
        }

        [ButtonGroup]
        public void Clamp()
        {
            maxConvexHulls = math.clamp(maxConvexHulls,                 Ranges.maxConvexHulls_MIN,         Ranges.maxConvexHulls_MAX);
            resolution = math.clamp(resolution,                         Ranges.resolution_MIN,             Ranges.resolution_MAX);
            planeDownsampling = math.clamp(planeDownsampling,           Ranges.planeDownsampling_MIN,      Ranges.planeDownsampling_MAX);
            convexHullDownsampling = math.clamp(convexHullDownsampling, Ranges.convexHullDownsampling_MIN, Ranges.convexHullDownsampling_MAX);
            concavity = math.clamp(concavity,                           Ranges.concavity_MIN,              Ranges.concavity_MAX);
            alpha = math.clamp(alpha,                                   Ranges.alpha_MIN,                  Ranges.alpha_MAX);
            beta = math.clamp(beta,                                     Ranges.beta_MIN,                   Ranges.beta_MAX);
            maximumVerticesPerHull = math.clamp(maximumVerticesPerHull, Ranges.maximumVerticesPerHull_MIN, Ranges.maximumVerticesPerHull_MAX);
            minimumVolumePerHull = math.clamp(minimumVolumePerHull,     Ranges.minimumVolumePerHull_MIN,   Ranges.minimumVolumePerHull_MAX);
            dirty = true;
        }

        public int SuggestedPlaneDownsamplingByResolution => (int) ((resolution / (float) Ranges.resolution_MAX) * Ranges.planeDownsampling_MAX);

        public int SuggestedHullsByResolution => (int) ((resolution / (float) Ranges.resolution_MAX) * Ranges.maxConvexHulls_MAX);

        public int SuggestedVerticesPerHull => (int) ((resolution / (float) Ranges.resolution_MAX) * Ranges.maximumVerticesPerHull_MAX);

        public int SuggestedHullDownsamplingByResolution => (int) ((resolution / (float) Ranges.resolution_MAX) * Ranges.convexHullDownsampling_MAX);

#region IEquatable

        [DebuggerStepThrough] public bool Equals(ConvexMeshSettings other)
        {
            return (mode == other.mode) &&
                   (maxConvexHulls == other.maxConvexHulls) &&
                   (normalize == other.normalize) &&
                   (resolution == other.resolution) &&
                   (planeDownsampling == other.planeDownsampling) &&
                   (convexHullDownsampling == other.convexHullDownsampling) &&
                   concavity.Equals(other.concavity) &&
                   alpha.Equals(other.alpha) &&
                   beta.Equals(other.beta) &&
                   (maximumVerticesPerHull == other.maximumVerticesPerHull) &&
                   minimumVolumePerHull.Equals(other.minimumVolumePerHull) &&
                   (convexHullApproximation == other.convexHullApproximation) &&
                   (projectHullVertices == other.projectHullVertices) &&
                   (openClAcceleration == other.openClAcceleration) &&
                   (openClPlatform == other.openClPlatform) &&
                   (openClDevice == other.openClDevice);
        }

        [DebuggerStepThrough] public override bool Equals(object obj)
        {
            return obj is ConvexMeshSettings other && Equals(other);
        }

        [DebuggerStepThrough] public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) mode;
                hashCode = (hashCode * 397) ^ maxConvexHulls;
                hashCode = (hashCode * 397) ^ normalize.GetHashCode();
                hashCode = (hashCode * 397) ^ resolution;
                hashCode = (hashCode * 397) ^ planeDownsampling;
                hashCode = (hashCode * 397) ^ convexHullDownsampling;
                hashCode = (hashCode * 397) ^ concavity.GetHashCode();
                hashCode = (hashCode * 397) ^ alpha.GetHashCode();
                hashCode = (hashCode * 397) ^ beta.GetHashCode();
                hashCode = (hashCode * 397) ^ maximumVerticesPerHull;
                hashCode = (hashCode * 397) ^ minimumVolumePerHull.GetHashCode();
                hashCode = (hashCode * 397) ^ convexHullApproximation.GetHashCode();
                hashCode = (hashCode * 397) ^ projectHullVertices.GetHashCode();
                hashCode = (hashCode * 397) ^ openClAcceleration.GetHashCode();
                hashCode = (hashCode * 397) ^ openClPlatform;
                hashCode = (hashCode * 397) ^ openClDevice;
                return hashCode;
            }
        }

        [DebuggerStepThrough] public static bool operator ==(ConvexMeshSettings left, ConvexMeshSettings right)
        {
            return left.Equals(right);
        }

        [DebuggerStepThrough] public static bool operator !=(ConvexMeshSettings left, ConvexMeshSettings right)
        {
            return !left.Equals(right);
        }

#endregion

#region ToString

        [DebuggerStepThrough] public override string ToString()
        {
            return $"{nameof(mode)}: {mode}, " +
                   $"{nameof(maxConvexHulls)}: {maxConvexHulls}, " +
                   $"{nameof(normalize)}: {normalize}, " +
                   $"{nameof(resolution)}: {resolution}, " +
                   $"{nameof(planeDownsampling)}: {planeDownsampling}, " +
                   $"{nameof(convexHullDownsampling)}: {convexHullDownsampling}, " +
                   $"{nameof(concavity)}: {concavity:F4}, " +
                   $"{nameof(alpha)}: {alpha:F4}, " +
                   $"{nameof(beta)}: {beta:F4}, " +
                   $"{nameof(maximumVerticesPerHull)}: {maximumVerticesPerHull}, " +
                   $"{nameof(minimumVolumePerHull)}: {minimumVolumePerHull:F4}, " +
                   $"{nameof(convexHullApproximation)}: {convexHullApproximation}, " +
                   $"{nameof(projectHullVertices)}: {projectHullVertices}, " +
                   $"{nameof(openClAcceleration)}: {openClAcceleration}, " +
                   $"{nameof(openClPlatform)}: {openClPlatform}, " +
                   $"{nameof(openClDevice)}: {openClDevice}";
        }

#endregion
    }
}
#endif
