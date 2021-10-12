#region

using System.Collections.Generic;
using Appalachia.Core.Attributes.Editing;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [EditorOnlyInitializeOnLoad]
    public static class MeshBurialDictionaryManager
    {
        private static MeshBurialOptimizationParameters _optimizationParameters;

        public static MeshBurialDictionary gameObjects = new();

        private static Dictionary<string, MeshBurialDictionary> _extras = new();

        public static MeshBurialOptimizationParameters optimizationParameters
        {
            get
            {
                if (_optimizationParameters == null)
                {
                    _optimizationParameters = MeshBurialOptimizationParameters.instance;
                }

                return _optimizationParameters;
            }
        }

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
