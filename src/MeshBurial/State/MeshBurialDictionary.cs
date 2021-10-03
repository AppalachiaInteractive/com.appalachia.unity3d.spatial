#region

using UnityEngine;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    public class MeshBurialDictionary : GenericCustomDictionary<int, MeshBurialState>
    {
        public MeshBurialState GetOrCreate(int key, Matrix4x4 ltw, GameObject instanceOrPrefab, int terrainHashCode)
        {
            var l = lookup;

            if (l.ContainsKey(key))
            {
                return l[key];
            }

            var sharedState = MeshBurialSharedStateManager.GetByPrefab(instanceOrPrefab);

            var newState = new MeshBurialState(sharedState, ltw, terrainHashCode);

            l.Add(key, newState);

            return newState;
        }

        public MeshBurialState GetOrCreate(int key, Matrix4x4 ltw, MeshBurialSharedState sharedState, int terrainHashCode)
        {
            var l = lookup;

            if (l.ContainsKey(key))
            {
                return l[key];
            }

            var newState = new MeshBurialState(sharedState, ltw, terrainHashCode);

            l.Add(key, newState);

            return newState;
        }
    }
}
