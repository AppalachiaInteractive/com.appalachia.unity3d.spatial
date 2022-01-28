#if UNITY_EDITOR

#region

using Appalachia.CI.Integration.Assets;
using Appalachia.Core.Attributes;
using Appalachia.Core.Objects.Availability;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [CallStaticConstructorInEditor]
    public static class MeshBurialSharedStateManager
    {
        static MeshBurialSharedStateManager()
        {
            RegisterInstanceCallbacks.WithoutSorting()
                                     .When.Object<MeshBurialSharedStateDictionary>()
                                     .IsAvailableThen(i => _meshBurialSharedStateDictionary = i);
        }

        #region Static Fields and Autoproperties

        private static readonly ProfilerMarker _PRF_Get = new(_PRF_PFX + nameof(Get));

        private static readonly ProfilerMarker _PRF_GetByPrefab = new(_PRF_PFX + nameof(GetByPrefab));

        private static readonly ProfilerMarker _PRF_GetByGameObject = new(_PRF_PFX + nameof(GetByGameObject));

        private static readonly ProfilerMarker _PRF_GetByHashCode = new(_PRF_PFX + nameof(GetByHashCode));

        private static MeshBurialSharedStateDictionary _meshBurialSharedStateDictionary;

        #endregion

        public static MeshBurialSharedState Get(GameObject go)
        {
            using (_PRF_Get.Auto())
            {
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(go) ||
                    UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(go))
                {
                    AssetDatabaseManager.TryGetGUIDAndLocalFileIdentifier(go, out var guid, out var _);

                    var hashCode = guid.GetHashCode();

                    return GetByHashCode(hashCode, go);
                }

                return GetByHashCode(go.GetHashCode(), go);
            }
        }

        public static MeshBurialSharedState GetByGameObject(GameObject gameObject)
        {
            using (_PRF_GetByGameObject.Auto())
            {
                return GetByHashCode(gameObject.GetHashCode(), gameObject);
            }
        }

        public static MeshBurialSharedState GetByHashCode(int hashCode, GameObject model)
        {
            using (_PRF_GetByHashCode.Auto())
            {
                var state = _meshBurialSharedStateDictionary;

                if (state.State.ContainsKey(hashCode))
                {
                    return state.State.Get(hashCode);
                }

                var meshObj = new MeshBurialSharedState(
                    model,
                    MeshBurialDictionaryManager.optimizationParameters
                );

                state.State.AddOrUpdate(hashCode, meshObj);

                return meshObj;
            }
        }

        public static MeshBurialSharedState GetByPrefab(GameObject prefab)
        {
            using (_PRF_GetByPrefab.Auto())
            {
                AssetDatabaseManager.TryGetGUIDAndLocalFileIdentifier(prefab, out var guid, out var _);

                var hashCode = guid.GetHashCode();

                var state = GetByHashCode(hashCode, prefab);

                return state;
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(MeshBurialSharedStateManager) + ".";

        #endregion
    }
}

#endif
