#if UNITY_EDITOR

#region

using System.Collections.Generic;
using Appalachia.Core.Attributes;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [CallStaticConstructorInEditor]
    public static class MeshBurialDictionaryManager
    {
        static MeshBurialDictionaryManager()
        {
            MeshBurialOptimizationParameters.InstanceAvailable += i => _meshBurialOptimizationParameters = i;
        }

        #region Static Fields and Autoproperties

        public static MeshBurialDictionary gameObjects = new();

        private static Dictionary<string, MeshBurialDictionary> _extras = new();

        private static MeshBurialOptimizationParameters _meshBurialOptimizationParameters;

        #endregion

        public static MeshBurialOptimizationParameters optimizationParameters =>
            _meshBurialOptimizationParameters;

        public static MeshBurialDictionary GetNamed(string name)
        {
            if (_extras == null)
            {
                _extras = new Dictionary<string, MeshBurialDictionary>();
            }

            if (_extras.ContainsKey(name))
            {
                return _extras[name];
            }

            var newDict = new MeshBurialDictionary();

            _extras.Add(name, newDict);

            return newDict;
        }
    }
}

#endif
