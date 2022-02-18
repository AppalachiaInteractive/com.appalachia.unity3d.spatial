#if UNITY_EDITOR

#region

using System;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public abstract class MeshBurialManyQueueItem : MeshBurialQueueItem
    {
        protected MeshBurialManyQueueItem(string name, int length, UnityEngine.Object owner) : base(
            name,
            length,
            owner
        )
        {
        }

        /// <inheritdoc />
        protected override void OnInitializeInternal()
        {
        }
    }
}

#endif
