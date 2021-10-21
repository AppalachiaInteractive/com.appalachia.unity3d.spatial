using System;
using Appalachia.Core.Behaviours;
using Appalachia.Core.Debugging;
using Appalachia.Core.Extensions;
using Appalachia.Editing.Debugging;
using Appalachia.Editing.Debugging.Handle;
using AwesomeTechnologies.VegetationSystem;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Appalachia.Spatial.Terrains
{
    public class TerrainSuitabilityAnalyzer : AppalachiaMonoBehaviour
    {
        private static readonly string[] _directions =
        {
            "N",
            "N by E",
            "N NE",
            "NE by N",
            "NE",
            "NE by E",
            "E NE",
            "E by N",
            "E",
            "E by S",
            "E SE",
            "SE by E",
            "SE",
            "SE by S",
            "S SE",
            "S by E",
            "S",
            "S by W",
            "S SW",
            "SW by S",
            "SW",
            "SW by W",
            "W SW",
            "W by S",
            "W",
            "W by N",
            "W NW",
            "NW by W",
            "NW",
            "NW by N",
            "N NW",
            "N by W"
        };

        private static GUIStyle label;

        private static GUIStyle miniLbel;
        /*public enum SuitabilityType
        {
            None,
            Concavity,
            Depression,
            Aspect
        }*/

        [BoxGroup("HUD")]
        [PropertyRange(0.01f, 2.0f)]
        public float hudSize = 1.0f;

        [BoxGroup("HUD")]
        [PropertyRange(0.001f, 0.1f)]
        public float hudSphereSize = 0.1f;

        [BoxGroup("HUD")] public Vector3 hudOffset = new(0.2f, 0.2f, 0.5f);

        [BoxGroup("HUD")] public Vector3 labelOffset = new(0.1f, 0.1f, 0.0f);

        [BoxGroup("HUD")] public float indicatorSphereSize = 2.0f;

        [BoxGroup("Setup")] public Terrain terrain;

        [BoxGroup("Setup")] public SuitabilityType suitabilityType;

        [BoxGroup("Setup")] public LocationSamplingMode SamplingMode;

        [BoxGroup("Setup")]
        [PropertyRange(1, 4)]
        public int SampleResolution;

        [BoxGroup("Setup")]
        [PropertyRange(1, 4)]
        public int SampleLevels;

        [BoxGroup("Setup")]
        [PropertyRange(1.0f, 30.0f)]
        public float MinDistance;

        [BoxGroup("Setup")]
        [PropertyRange(nameof(minMaxDistance), 100.0f)]
        public float MaxDistance;

        [BoxGroup("Setup")]
        [PropertyRange(0.1f, 1.0f)]
        public float GizmoSize = 0.2f;

        [ShowIfGroup("Show Concavity", Condition = nameof(showConcavity))]
        [BoxGroup("Show Concavity/Concavity")]
        public bool Convex;
        /*
        [BoxGroup("Show Concavity/Concavity")]
        public bool ConsiderBounds = true;*/

        [BoxGroup("Show Concavity/Concavity")]
        [PropertyRange(0.01f, 2.0f)]
        public float ConcavityThreshold;

        [BoxGroup("Show Concavity/Concavity")]
        public bool ConcaveInverse;

        [ShowIfGroup("Show Depression", Condition = nameof(showDepression))]
        [BoxGroup("Show Depression/Depression")]
        public bool DepressionHill;

        [FormerlySerializedAs("DepressionMinHeightDifference")]
        [BoxGroup("Show Depression/Depression")]
        [PropertyRange(0.01f, 20.0f)]
        public float DepressionHeightDifference;

        [BoxGroup("Show Depression/Depression")]
        public bool DepressionInverse;

        [ShowIfGroup("Show Aspect", Condition = nameof(showAspect))]
        [BoxGroup("Show Aspect/Aspect")]
        [PropertyRange(0.0f, 359.9f)]
        [SuffixLabel("$" + nameof(AspectReferenceDirectionXZ_Label))]
        public float AspectReferenceDirection;

        [BoxGroup("Show Aspect/Aspect")]
        [PropertyRange(0.01f, 2.0f)]
        public float AspectTolerance;

        [BoxGroup("Show Aspect/Aspect")]
        public bool AspectInverse;

        private float minMaxDistance => MinDistance + 1;

        private bool showConcavity => suitabilityType == SuitabilityType.Concavity;

        private bool showDepression => suitabilityType == SuitabilityType.Depression;

        private bool showAspect => suitabilityType == SuitabilityType.Aspect;

        private float2 up => new(0.0f, 1.0f);
        private float2 right => new(1.0f, 0.0f);
        private float2 down => new(0.0f, -1.0f);
        private float2 left => new(-1.0f, 0.0f);

        public float AspectReferenceDirectionTime =>
            AspectReferenceDirection < 90.0f
                ? AspectReferenceDirection / 90.0f
                : AspectReferenceDirection < 180.0f
                    ? (AspectReferenceDirection - 90.0f) / 90.0f
                    : AspectReferenceDirection < 270.0f
                        ? (AspectReferenceDirection - 180.0f) / 90.0f
                        : (AspectReferenceDirection - 270.0f) / 90.0f;

        public float2 AspectReferenceDirectionXZ =>
            AspectReferenceDirection < 90.0f
                ? math.lerp(up, right, AspectReferenceDirectionTime)
                : AspectReferenceDirection < 180.0f
                    ? math.lerp(right, down, AspectReferenceDirectionTime)
                    : AspectReferenceDirection < 270.0f
                        ? math.lerp(down, left, AspectReferenceDirectionTime)
                        : math.lerp(left, up,   AspectReferenceDirectionTime);

        public string AspectReferenceDirectionXZ_Label =>
            $"{GetDirectionalString(AspectReferenceDirection)}";

        public static GUIStyle Label
        {
            get
            {
                if (label == null)
                {
                    label = new GUIStyle(EditorStyles.label);

                    label.normal.textColor = Color.black;
                    label.normal.background = Texture2D.whiteTexture;
                    label.border = new RectOffset(1, 1, 1, 1);

                    label.fontSize = 12;
                }

                return label;
            }
        }

        public static GUIStyle MiniLabel
        {
            get
            {
                if (miniLbel == null)
                {
                    miniLbel = new GUIStyle(Label) {fontSize = 9};
                }

                return miniLbel;
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (!GizmoCameraChecker.ShouldRenderGizmos())
            {
                return;
            }

            var cam = SceneView.GetAllSceneCameras()[0];

            var terrainData = terrain.terrainData;
            float3 terrainPosition_WS = terrain.transform.position;

            float3 rootPosition_WS = cam.transform.position;
            var rootPosition_TS = rootPosition_WS - terrainPosition_WS;

            var rootPosition_XZ_TS_N = new float2(
                rootPosition_TS.x / terrainData.size.x,
                rootPosition_TS.z / terrainData.size.z
            );

            var rootHeight = terrainData.GetInterpolatedHeight(
                rootPosition_XZ_TS_N.x,
                rootPosition_XZ_TS_N.y
            );

            var rootNormal = terrainData.GetInterpolatedNormal(
                rootPosition_XZ_TS_N.x,
                rootPosition_XZ_TS_N.y
            );

            var rootNormal_XZ = math.normalizesafe(rootNormal.xz());

            var sampleCounters = suitabilityType switch
            {
                SuitabilityType.Aspect    => InitializeAspectSample(rootNormal_XZ, AspectReferenceDirectionXZ),
                SuitabilityType.Concavity => InitializeConcavitySample(),
                _ => InitializeDepressionSample()
            };

            var sampleCount = (int) sampleCounters.x;
            var sampleSum = sampleCounters.y;
            var sampleMin = sampleCounters.z;
            var sampleMax = sampleCounters.w;

            var maxPosition = rootPosition_WS;
            var minPosition = rootPosition_WS;

            var samplePointCount = SampleResolution * 4;
            var points = SampleLevels * samplePointCount;

            var samplePositions = new float3[points + 1];
            var sampleColors = new Color[points + 1];

            for (var ringIndex = 0; ringIndex < SampleLevels; ringIndex++)
            {
                var ringTime = math.clamp(ringIndex / (float) SampleLevels, 0.0f, 1.0f);

                var radius = SampleLevels == 1
                    ? MinDistance
                    : math.lerp(MinDistance, MaxDistance, ringTime);

                for (var pointIndex = 0; pointIndex < samplePointCount; pointIndex++)
                {
                    var aggPointIndex = (ringIndex * samplePointCount) + pointIndex;

                    var pointTime = 2.0f * math.PI * (pointIndex / (float) samplePointCount);

                    var samplePositionX_TS = rootPosition_TS.x + (math.cos(pointTime) * radius);
                    var samplePositionZ_TS = rootPosition_TS.z + (math.sin(pointTime) * radius);

                    var samplePosition_N = new float2(
                        samplePositionX_TS / terrainData.size.x,
                        samplePositionZ_TS / terrainData.size.z
                    );

                    var sampleHeight = terrainData.GetInterpolatedHeight(
                        samplePosition_N.x,
                        samplePosition_N.y
                    );

                    var sampleNormal = math.normalizesafe(
                        terrainData.GetInterpolatedNormal(samplePosition_N.x, samplePosition_N.y)
                    );

                    var sampleNormal_XZ = math.normalizesafe(sampleNormal.xz);

                    var samplePosition_TS = new float3(
                        samplePositionX_TS,
                        sampleHeight,
                        samplePositionZ_TS
                    );
                    var samplePosition_WS = samplePosition_TS + terrainPosition_WS;

                    var sampleValue = suitabilityType switch
                    {
                        SuitabilityType.Aspect => GetAspectSampleValue(sampleNormal_XZ, AspectReferenceDirectionXZ),
                        SuitabilityType.Concavity => GetConcavitySampleValue(
                            samplePosition_TS,
                            rootPosition_TS,
                            sampleNormal
                        ),
                        _ => sampleHeight - rootHeight
                    };

                    Handles.Label(samplePosition_WS, $"{sampleValue:F3}", Label);

                    sampleSum += sampleValue;
                    sampleCount += 1;

                    if (sampleValue > sampleMax)
                    {
                        sampleMax = sampleValue;
                        maxPosition = samplePosition_WS;
                    }
                    else if (sampleValue < sampleMin)
                    {
                        sampleMin = sampleValue;
                        minPosition = samplePosition_WS;
                    }

                    var colorRemove = suitabilityType switch
                    {
                        SuitabilityType.Aspect    => CheckAspectRemoval(sampleValue),
                        SuitabilityType.Concavity => CheckConcavityRemoval(sampleValue),
                        _                         => CheckDepressionRemoval(sampleValue)
                    };

                    sampleColors[aggPointIndex] = colorRemove ? Color.red : Color.green;
                    samplePositions[aggPointIndex] = samplePosition_WS;
                }
            }

            var testValue = GetTestValue(sampleMin, sampleMax, sampleSum, sampleCount);

            var remove = suitabilityType switch
            {
                SuitabilityType.Aspect    => CheckAspectRemoval(testValue),
                SuitabilityType.Concavity => CheckConcavityRemoval(testValue),
                _                         => CheckDepressionRemoval(testValue)
            };

            DrawSpheres(samplePositions, sampleColors, minPosition, maxPosition, GizmoSize);

            var bounds = new Bounds();
            foreach (var samplePosition in samplePositions)
            {
                bounds.Encapsulate(samplePosition);
            }

            var cameraTransform = cam.transform;

            var hudPositionOffset = cameraTransform.position +
                                    (cameraTransform.right * hudOffset.x) +
                                    (cameraTransform.up * hudOffset.y) +
                                    (cameraTransform.forward * hudOffset.z);

            var hudScaleFactor = hudSize / bounds.size.magnitude;

            samplePositions[points] = rootPosition_WS;
            sampleColors[points] = remove ? Color.red : Color.green;

            DrawSpheres(
                samplePositions,
                sampleColors,
                minPosition,
                maxPosition,
                hudScaleFactor,
                rootPosition_WS,
                hudPositionOffset,
                hudSphereSize,
                testValue,
                labelOffset,
                indicatorSphereSize
            );
        }

        private string GetDirectionalString(float direction)
        {
            var segment = (int) (direction / 11.25f);

            if (segment == 1)
            {
                segment = 0;
            }

            segment = Mathf.Clamp(segment, 0, 31);

            return _directions[segment];
        }

        private float4 InitializeConcavitySample()
        {
            var sampleCount = 0;
            var sampleSum = 0.0f;
            var sampleMin = 1.0f;
            var sampleMax = -1.0f;
            return new float4(sampleCount, sampleSum, sampleMin, sampleMax);
        }

        private float4 InitializeDepressionSample()
        {
            var sampleCount = 0;
            var sampleSum = 0.0f;
            var sampleMin = 5000.0f;
            var sampleMax = -5000.0f;
            return new float4(sampleCount, sampleSum, sampleMin, sampleMax);
        }

        private float4 InitializeAspectSample(float2 centerNormal, float2 referenceDirection)
        {
            var aspectDot = math.dot(centerNormal, referenceDirection);

            var sampleCount = 1;
            var sampleSum = aspectDot;
            var sampleMax = aspectDot;
            var sampleMin = aspectDot;

            return new float4(sampleCount, sampleSum, sampleMin, sampleMax);
        }

        private float GetAspectSampleValue(float2 sampleNormal, float2 referenceDirection)
        {
            return math.dot(sampleNormal, referenceDirection);
        }

        private float GetConcavitySampleValue(
            float3 samplePosition,
            float3 rootPosition,
            float3 sampleNormal)
        {
            var outward = math.normalize(samplePosition - rootPosition);

            var result = math.dot(outward, sampleNormal);

            return result;
        }

        private float GetTestValue(
            float sampleMin,
            float sampleMax,
            float sampleSum,
            int sampleCount)
        {
            float testValue;

            switch (SamplingMode)
            {
                case LocationSamplingMode.UseMinimum:
                    testValue = sampleMin;
                    break;
                case LocationSamplingMode.UseAverage:
                    testValue = sampleSum / sampleCount;
                    break;
                case LocationSamplingMode.UseMaximum:
                    testValue = sampleMax;
                    break;
                case LocationSamplingMode.UsageAverageExclude1Outlier:
                    testValue = (sampleSum - sampleMax - sampleMin) / (sampleCount - 2);
                    break;
                case LocationSamplingMode.UseAverageExcludeMax:
                    testValue = (sampleSum - sampleMax) / (sampleCount - 1);
                    break;
                case LocationSamplingMode.UseAverageExcludeMin:
                    testValue = (sampleSum - sampleMin) / (sampleCount - 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return testValue;
        }

        private bool CheckAspectRemoval(float testValue)
        {
            var keep = testValue > (1.0 - AspectTolerance);

            if (AspectInverse)
            {
                keep = !keep;
            }

            return !keep;
        }

        private bool CheckConcavityRemoval(float testValue)
        {
            bool keep;

            if (Convex)
            {
                keep = testValue > (1.0 - ConcavityThreshold);
            }
            else
            {
                keep = testValue < (-1.0f + ConcavityThreshold);
            }

            if (ConcaveInverse)
            {
                keep = !keep;
            }

            return !keep;
        }

        private bool CheckDepressionRemoval(float testValue)
        {
            bool keep;

            if (DepressionHill)
            {
                keep = testValue < DepressionHeightDifference;
            }
            else
            {
                keep = testValue > DepressionHeightDifference;
            }

            if (DepressionInverse)
            {
                keep = !keep;
            }

            return !keep;
        }

        private void DrawSpheres(
            float3[] samplePositions,
            Color[] sampleColors,
            float3 minPosition,
            float3 maxPosition,
            float sphereSize)
        {
            Color color;

            for (var index = 0; index < (samplePositions.Length - 1); index++)
            {
                var position = samplePositions[index];
                color = sampleColors[index];

                var matchesMax = position == maxPosition;
                var matchesMin = position == minPosition;

                if (matchesMax.x && matchesMax.y && matchesMax.z)
                {
                    color = Color.cyan;

                    if ((SamplingMode == LocationSamplingMode.UseAverageExcludeMax) ||
                        (SamplingMode == LocationSamplingMode.UsageAverageExclude1Outlier))
                    {
                        color = Color.white;
                    }
                }
                else if (matchesMin.x && matchesMin.y && matchesMin.z)
                {
                    color = Color.yellow;

                    if ((SamplingMode == LocationSamplingMode.UseAverageExcludeMin) ||
                        (SamplingMode == LocationSamplingMode.UsageAverageExclude1Outlier))
                    {
                        color = Color.gray;
                    }
                }

                SmartHandles.DrawWireSphere(position, sphereSize, color);
            }
        }

        private void DrawSpheres(
            float3[] samplePositions,
            Color[] sampleColors,
            float3 minPosition,
            float3 maxPosition,
            float3 scale,
            float3 center,
            float3 offset,
            float sphereSize,
            float testValue,
            float3 lblOffset,
            float indicSphereSize)
        {
            Color color;

            for (var index = 0; index < samplePositions.Length; index++)
            {
                var position = samplePositions[index];
                color = sampleColors[index];

                if (index < (samplePositions.Length - 1))
                {
                    var matchesMax = position == maxPosition;
                    var matchesMin = position == minPosition;

                    if (matchesMax.x && matchesMax.y && matchesMax.z)
                    {
                        color = Color.cyan;

                        if ((SamplingMode == LocationSamplingMode.UseAverageExcludeMax) ||
                            (SamplingMode == LocationSamplingMode.UsageAverageExclude1Outlier))
                        {
                            color = Color.white;
                        }
                    }
                    else if (matchesMin.x && matchesMin.y && matchesMin.z)
                    {
                        color = Color.yellow;

                        if ((SamplingMode == LocationSamplingMode.UseAverageExcludeMin) ||
                            (SamplingMode == LocationSamplingMode.UsageAverageExclude1Outlier))
                        {
                            color = Color.gray;
                        }
                    }
                }

                var drawPoint = ((position - center) * scale) + offset;

                if (index == (samplePositions.Length - 1))
                {
                    SmartHandles.DrawWireSphere(drawPoint, sphereSize * indicSphereSize, color);
                }
                else
                {
                    SmartHandles.DrawWireSphere(drawPoint, sphereSize, color);
                }
            }

            Handles.Label(offset + lblOffset, $"{testValue:F3}", MiniLabel);
        }
    }
}
