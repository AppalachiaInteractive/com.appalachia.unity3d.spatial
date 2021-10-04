#region

using Appalachia.Editing.Attributes;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [EditorOnlyInitializeOnLoad]
    public static class MeshBurialSharedStateManager
    {
        private const string _PRF_PFX = nameof(MeshBurialSharedStateManager) + ".";

        private static readonly ProfilerMarker _PRF_Get = new ProfilerMarker(_PRF_PFX + nameof(Get));
        public static MeshBurialSharedState Get(GameObject go)
        {
            using (_PRF_Get.Auto())
            {
                if (PrefabUtility.IsPartOfPrefabAsset(go) || PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(go, out var guid, out long _);

                    var hashCode = guid.GetHashCode();

                    return GetByHashCode(hashCode, go);
                }

                return GetByHashCode(go.GetHashCode(), go);
            }
        }

        private static readonly ProfilerMarker _PRF_GetByPrefab = new ProfilerMarker(_PRF_PFX + nameof(GetByPrefab));

        public static MeshBurialSharedState GetByPrefab(GameObject prefab)
        {
            using (_PRF_GetByPrefab.Auto())
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out var guid, out long _);

                var hashCode = guid.GetHashCode();

                var state = GetByHashCode(hashCode, prefab);

                return state;
            }
        }

        private static readonly ProfilerMarker _PRF_GetByGameObject = new ProfilerMarker(_PRF_PFX + nameof(GetByGameObject));
        public static MeshBurialSharedState GetByGameObject(GameObject gameObject)
        {
            using (_PRF_GetByGameObject.Auto())
            {
                return GetByHashCode(gameObject.GetHashCode(), gameObject);
            }
        }

        private static readonly ProfilerMarker _PRF_GetByHashCode = new ProfilerMarker(_PRF_PFX + nameof(GetByHashCode));
        public static MeshBurialSharedState GetByHashCode(int hashCode, GameObject model)
        {
            using (_PRF_GetByHashCode.Auto())
            {
                var state = MeshBurialSharedStateDictionary.instance;

                if (state.State.ContainsKey(hashCode))
                {
                    return state.State.Get(hashCode);
                                 }

                var meshObj = new MeshBurialSharedState(model, MeshBurialDictionaryManager.optimizationParameters);

                state.State.AddOrUpdate(hashCode, meshObj);

                return meshObj;
            }
        }
    }
}
