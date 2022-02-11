/*namespace Appalachia.Core.Spatial.MeshBurial.State
{
    /*public static class MeshBurialStateExtensions
    {
        public static Matrix4x4 Bury(this MeshBurialState state, bool updateinitial.error)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                var movement = state.CalculateBurialAdjustment(state.localToWorld);

                state.localToWorld *= Matrix4x4.Translate(movement);

                if (updateinitial.error)
                {
                    state.Calculateinitial.error();
                }

                return state.localToWorld;
            }
        }

        public static Matrix4x4 AdjustNormal(this MeshBurialState state, bool updateinitial.error)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                var terrainNormal = TerrainJobHelper.GetTerrainNormal(
                    state.localToWorld.GetPositionFromMatrix(),
                    state.Terrain.terrainPosition,
                    state.Terrain.heights,
                    state.Terrain.resolution,
                    state.Terrain.scale
                );

                var target = state.localToWorld.inverse.MultiplyVector(terrainNormal);

                var adjustment = Quaternion.FromToRotation(state.shared.meshObject.BorderNormal, target);

                state.localToWorld *= Matrix4x4.Rotate(adjustment);

                if (updateinitial.error)
                {
                    state.Calculateinitial.error();
                }

                return state.localToWorld;
            }
        }


        public static void PrepareAdjustmentParameters(
            this MeshBurialState state,
            bool updateinitial.error,
            bool minimalRotation,
            int permissiveness = 1)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                var startTime = CoreClock.Instance.RealtimeSinceStartup;

                if (updateinitial.error)
                {
                    state.Calculateinitial.error();
                }

                IOptimizer optimizer;
                IParameterSpec[] parameters;
                var seed = UnityEngine.Random.Range(1, 100);

                var ops = state.shared.optimizationParams;

                var degreeAdjustment = permissiveness *
                    (minimalRotation ? ops.xzDegreeAdjustmentMinimal : ops.xzDegreeAdjustment);

                if (ops.optimizerType != OptimizerType.GridSearch)
                {
                    parameters = new IParameterSpec[]
                    {
                        new MinMaxParameterSpec(-degreeAdjustment, degreeAdjustment), // x axis rotation,
                        new MinMaxParameterSpec(-degreeAdjustment, degreeAdjustment), // z axis rotation,
                    };
                }
                else
                {
                    var iterations = ops.gridSearchParameters.iterations;

                    var paramDepth = (int) (Mathf.Sqrt(iterations));
                    var array = new double[paramDepth];

                    for (var i = 0; i < paramDepth; i++)
                    {
                        array[i] = Mathf.Lerp(-degreeAdjustment, degreeAdjustment, i / (float) paramDepth);
                    }

                    parameters = new IParameterSpec[]
                    {
                        new GridParameterSpec(array), // x axis rotation,
                        new GridParameterSpec(array), // z axis rotation,
                    };
                }

                switch (ops.optimizerType)
                {
                    case OptimizerType.GridSearch:
                    {
                        optimizer = new GridSearchOptimizer(parameters, true);
                    }
                        break;
                    case OptimizerType.Bayesian:
                    {
                        var iterations = ops.bayesianParameters.iterations;
                        var startingPositions = ops.bayesianParameters.randomStartingPointsCount;

                        if (ops.overrideIterations)
                        {
                            iterations = ops.iterationCountMulti;
                            startingPositions = ops.subIterationCount;
                        }

                        optimizer = new BayesianOptimizer(
                            parameters,
                            iterations,
                            startingPositions,
                            ops.bayesianParameters.functionEvaluationsPerIterationCount,
                            ops.bayesianParameters.randomSearchPointCount,
                            runParallel: true,
                            seed: seed
                        );
                    }
                        break;
                    case OptimizerType.ParticleSwarm:
                    {
                        var iterations = ops.particleSwarmParameters.maxIterations;
                        var numberOfParticles = ops.particleSwarmParameters.numberOfParticles;

                        if (ops.overrideIterations)
                        {
                            iterations = ops.iterationCountMulti;
                            numberOfParticles = ops.subIterationCount;
                        }

                        optimizer = new ParticleSwarmOptimizer(
                            parameters,
                            iterations,
                            numberOfParticles,
                            ops.particleSwarmParameters.localBestWeight,
                            ops.particleSwarmParameters.globalBestWeight,
                            seed
                        );
                    }
                        break;
                    case OptimizerType.RandomSearch:
                    {

                        var iterations = ops.randomSearchParameters.iterations;

                        if (ops.overrideIterations)
                        {
                            iterations = ops.iterationCountMulti;
                        }

                        optimizer = new RandomSearchOptimizer(parameters, iterations, runParallel: true, seed: seed);
                    }
                        break;
                    case OptimizerType.Smac:
                    {

                        var iterations = ops.smacParameters.maxIterations;
                        var randomStartingPointCount = ops.smacParameters.randomStartingPointCount;

                        if (ops.overrideIterations)
                        {
                            iterations = ops.iterationCountMulti;
                            randomStartingPointCount = ops.subIterationCount;
                        }

                        optimizer = new SmacOptimizer(
                            parameters,
                            iterations,
                            randomStartingPointCount: randomStartingPointCount,
                            randomSearchPointCount: ops.smacParameters.randomSearchPointCount,
                            localSearchPointCount: ops.smacParameters.localSearchPointCount,
                            functionEvaluationsPerIterationCount: ops.smacParameters
                                .functionEvaluationsPerIterationCount
                        );
                    }
                        break;
                    case OptimizerType.NelderMead:
                    {

                        var maxRestarts = ops.nelderMeadParameters.maxRestarts;
                        var maxIterationsWithoutImprovement = ops.nelderMeadParameters.maxIterationsWithoutImprovement;

                        if (ops.overrideIterations)
                        {
                            maxRestarts = ops.iterationCountMulti;
                            maxIterationsWithoutImprovement = ops.subIterationCount;
                        }

                        optimizer = new GlobalizedBoundedNelderMeadOptimizer(
                            parameters,
                            maxRestarts,
                            seed: seed,
                            maxIterationsWithoutImprovement: maxIterationsWithoutImprovement,
                            maxFunctionEvaluations: ops.nelderMeadParameters.maxFunctionEvaluations,
                            maxIterationsPrRestart: ops.nelderMeadParameters.maxIterationsPrRestart,
                            noImprovementThreshold: ops.nelderMeadParameters.noImprovementThreshold
                        );
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                switch (ops.optimizerType)
                {
                    case OptimizerType.Bayesian:
                        break;
                    case OptimizerType.GridSearch:
                        break;
                    case OptimizerType.ParticleSwarm:
                        break;
                    case OptimizerType.RandomSearch:
                        break;
                    case OptimizerType.Smac:
                        break;
                    case OptimizerType.NelderMead:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Func<double[], OptimizerResult> minimize = p =>
                {
                    var error = state.CalculateError(p, out _);

                    return new OptimizerResult(p, error);
                };

                var result = optimizer.OptimizeBest(minimize);

                state.optimized.bestError = result.Error;
                var bestParameters = result.ParameterSet;


                var rotation = Quaternion.Euler((float) bestParameters[0], 0.0f, (float) bestParameters[1]);

                var shift = Matrix4x4.Rotate(rotation);
                var proposed = state.localToWorld * shift;

                proposed *= Matrix4x4.Translate(state.CalculateBurialAdjustment(proposed));

                state.optimized.bestMatrix = proposed;
                var endTime = CoreClock.Instance.RealtimeSinceStartup;

                state.optimized.executionTime = endTime - startTime;
            }
        }

        public static Matrix4x4 ApplyAdjustmentParameters(this MeshBurialState state)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                if (state.optimized.initial.error < state.optimized.bestError)
                {
                    state.localToWorld = state.optimized.initialMatrix;
                }
                else
                {
                    state.localToWorld = state.optimized.bestMatrix;
                }

                return state.localToWorld;
            }
        }
        
        public static Vector3 CalculateBurialAdjustment(this MeshBurialState state, Matrix4x4 ltw)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                var maxOffset = -1024.0f;
                var minOffset = 1024.0f;

                for (var i = 0; i < state.shared.meshObject.vertices.Length; i++)
                {
                    var vertex = state.shared.meshObject.vertices[i];

                    if (!state.shared.meshObject.borderVertices.Contains(vertex))
                    {
                        continue;
                    }

                    var pos = ltw.MultiplyPoint(vertex.position);

                    var diff = TerrainJobHelper.CalculateHeightDifference(
                        pos,
                        state.Terrain.terrainPosition,
                        state.Terrain.heights,
                        state.Terrain.resolution,
                        state.Terrain.scale
                    );

                    if (diff > maxOffset) maxOffset = diff;
                    if (diff < minOffset) minOffset = diff;
                }

                var buffer = .01f;

                var up = ltw.inverse.MultiplyVector(Vector3.up);

                var movement = up * (-maxOffset - buffer);

                return movement;
            }
        }

        public static void Calculateinitial.error(this MeshBurialState state)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                state.optimized.initial.error = state.CalculateError(new[] {0.0, 0.0}, out var ltw);

                state.optimized.initialMatrix = ltw;
            }
        }

        public static double CalculateError(
            this MeshBurialState state,
            double[] parameters,
            out Matrix4x4 ltw)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                var r = Quaternion.Euler((float) parameters[0], 0.0f, (float) parameters[1]);

                ltw = state.localToWorld;

                ltw *= Matrix4x4.Rotate(r);

                var movement = state.CalculateBurialAdjustment(ltw);

                ltw *= Matrix4x4.Translate(movement);

                int borderError = 0;
                int internalError = 0;

                var vertices = state.shared.meshObject.vertices;
                var length = vertices.Length;
                var borderVertices = state.shared.meshObject.borderVertices;
                
                var borderTests = borderVertices.Count;
                var internalTests = length - borderTests;
                
                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex = vertices[i];
                    var worldPosition = ltw.MultiplyPoint(vertex.position);

                    var diff = TerrainJobHelper.CalculateHeightDifference(
                        worldPosition,
                        state.Terrain.terrainPosition,
                        state.Terrain.heights,
                        state.Terrain.resolution,
                        state.Terrain.scale
                    );

                    if (borderVertices.Contains(vertex))
                    {
                        borderTests += 1;

                        if (diff > 0)
                        {
                            borderError += 1;
                        }
                    }
                    else
                    {
                        internalTests += 1;

                        if (diff < 0)
                        {
                            internalError += 1;

                        }
                    }
                }

                if (borderError > 0.0f)
                {
                    return 1.0f;
                }

                if (internalError == internalTests)
                {
                    return 1.0f;
                }

                var internalScore = (internalError / (float)internalTests);


                return math.clamp(internalScore, 0f, 1f);
            }
        }

    }#1#
}*/


