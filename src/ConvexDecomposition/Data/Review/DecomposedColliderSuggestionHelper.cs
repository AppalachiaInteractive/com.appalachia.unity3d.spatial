using System;
using Appalachia.CI.Integration.Assets;
using Appalachia.Core.Extensions;
using Appalachia.Utility.Extensions;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;

namespace Appalachia.Spatial.ConvexDecomposition.Data.Review
{
    public static class DecomposedColliderSuggestionHelper
    {
        private const string _PRF_PFX = nameof(DecomposedColliderSuggestionHelper) + ".";
        
        [NonSerialized] private static ValueDropdownList<GameObject> ___assets;

        private static readonly ProfilerMarker _PRF__assets = new ProfilerMarker(_PRF_PFX + nameof(assets));
        internal static ValueDropdownList<GameObject> assets
        {
            get
            {
                using (_PRF__assets.Auto())
                {
                    if (___assets != null)
                    {
                        return ___assets;
                    }

                    ___assets = new ValueDropdownList<GameObject>();

                    var found = AssetDatabaseManager.FindAssets<GameObject>(_assetFilter);

                    for (var i = 0; i < found.Count; i++)
                    {
                        var f = found[i];
                        ___assets.Add(f.name, f);
                    }

                    return ___assets;
                }
            }
        }

        private const string _assetFilter = "t:Model collider";

        private static readonly ProfilerMarker _PRF_RebuildAssetsList = new ProfilerMarker(_PRF_PFX + nameof(RebuildAssetsList));
        public static void RebuildAssetsList()
        {
            using (_PRF_RebuildAssetsList.Auto())
            {
                ___assets = null;
            }
        }

        private static readonly ProfilerMarker _PRF_SuggestExternal = new ProfilerMarker(_PRF_PFX + nameof(SuggestExternal));
        public static GameObject SuggestExternal(string searchTerm)
        {
            using (_PRF_SuggestExternal.Auto())
            {
                var match = assets.FirstOrDefault_NoAlloc(a => a.Text.Contains(searchTerm));

                return match.Value;
            }
        }
        
        public static void UpdateReplacementReview(ref DecomposedColliderReplacementReviewItem replacementReview, GameObject replacementModel)
        {
            if (replacementReview == null)
            {
                replacementReview = new DecomposedColliderReplacementReviewItem();
            }
            
            replacementReview.pieces = 0;
            replacementReview.triangles = 0;
            replacementReview.vertices = 0;

            if (replacementModel == null)
            {
                return;
            }
            
            var modelPath = AssetDatabaseManager.GetAssetPath(replacementModel);

            var subassets = AssetDatabaseManager.LoadAllAssetsAtPath(modelPath);

            for (var i = 0; i < subassets.Length; i++)
            {
                var subasset = subassets[i];

                if (subasset is Mesh m)
                {
                    replacementReview.pieces += 1;

                    replacementReview.triangles += (m.triangles.Length/3);
                    replacementReview.vertices += m.vertexCount;
                }
            }
        }
    }
}
