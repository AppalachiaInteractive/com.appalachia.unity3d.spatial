#region

using System;
using Appalachia.Base.Scriptables;
using SharpLearning.Optimization;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.MeshBurial
{
    [Serializable]
    public class MeshBurialOptimizationParameters : SelfSavingSingletonScriptableObject<MeshBurialOptimizationParameters>
    {
        public OptimizerType optimizerType = OptimizerType.RandomSearch;

        /*[TitleGroup("Ranges")]
        public int yDegreeAdjustment = 10;*/
        [TitleGroup("Ranges")] public int xzDegreeAdjustment = 20;

        /*[TitleGroup("Ranges")]
        public int yDegreeAdjustment = 10;*/
        [TitleGroup("Ranges")] public int xzDegreeAdjustmentMinimal = 8;

        public bool overrideIterations;

        internal bool _showNormalIterationOverride =>
            overrideIterations && (optimizerType == OptimizerType.GridSearch || optimizerType == OptimizerType.RandomSearch);

        internal bool _showMultiIterationOverride => overrideIterations && !_showNormalIterationOverride;

        /*
        private bool _showBayesian => optimizerType == OptimizerType.Bayesian;
        [ShowIfGroup(nameof(_showBayesian))]
        [TitleGroup(nameof(_showBayesian) + "/Bayesian"), InlineProperty, HideLabel]
        public BayesianOptimizationParameters bayesianParameters;

        private bool _showSmac => optimizerType == OptimizerType.Smac;
        [ShowIfGroup(nameof(_showSmac))]
        [TitleGroup(nameof(_showSmac) + "/Sequential Model-based Algorithm Configuration"), InlineProperty, HideLabel]
        public SmacOptimizationParameters smacParameters;

        private bool _showGridSearch => optimizerType == OptimizerType.GridSearch;
        [ShowIfGroup(nameof(_showGridSearch))]
        [TitleGroup(nameof(_showGridSearch) + "/Grid Search"), InlineProperty, HideLabel]
        public GridSearchOptimizationParameters gridSearchParameters;

        private bool _showNelderMead => optimizerType == OptimizerType.NelderMead;
        [ShowIfGroup(nameof(_showNelderMead))]
        [TitleGroup(nameof(_showNelderMead) + "/Nelder-Mead Downhill Simplex"), InlineProperty, HideLabel]
        public NelderMeadOptimizationParameters nelderMeadParameters;

        private bool _showParticleSwarm => optimizerType == OptimizerType.ParticleSwarm;
        [ShowIfGroup(nameof(_showParticleSwarm))]
        [TitleGroup(nameof(_showParticleSwarm) + "/Particle Swarm"), InlineProperty, HideLabel]
        public ParticleSwarmOptimizationParameters particleSwarmParameters;

        private bool _showRandomSearch => optimizerType == OptimizerType.RandomSearch;
        [ShowIfGroup(nameof(_showRandomSearch))]
        [TitleGroup(nameof(_showRandomSearch) + "/Random Search"), InlineProperty, HideLabel]
        public RandomSearchOptimizationParameters randomSearchParameters;*/
    }
}
