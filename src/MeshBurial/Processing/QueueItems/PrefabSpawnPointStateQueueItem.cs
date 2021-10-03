/*
using System;
using Appalachia.Core.Extensions;
using Appalachia.Core.Spawning.Data;
using UnityEngine;

namespace Appalachia.Core.Spatial
{
    [Serializable]
    public class PrefabSpawnPointStateQueueItem : MeshBurialQueueItem
    {
        public PrefabSpawnPointStateQueueItem(PrefabSpawnPointState state) : base(state.)
        {
            points = state;
        }
        
        public PrefabSpawnPointState points;
        
        public override bool TryGetMatrix(int i, out Matrix4x4 matrix)
        {
            if (current >= points.Count)
            {
                matrix = default;
                points.buried = true;
                return false;
            }

            points.buried = false;

            var go = points.spawnedObjects[i];
            matrix = go.transform.localToWorldMatrix;
            return true;
        }

        public override void SetMatrix(int i, Matrix4x4 m)
        {
            points.spawnedObjects[i].transform.Matrix4x4ToTransform(m);
        }
    }
}
*/


