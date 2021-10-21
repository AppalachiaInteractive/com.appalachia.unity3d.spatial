using Appalachia.Core.Scriptables;
using UnityEditor;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition
{
    public class ConvexTest : SelfSavingSingletonScriptableObject<ConvexTest>
    {
        [MenuItem("Assets/Create/Internal/ConvexDecomposition/Test")]
        public static void Create()
        {
            Debug.Log(instance.name);
        }
        public ConvexMeshSettings settings;
    }
}