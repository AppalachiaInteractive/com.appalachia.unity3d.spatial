#region

using System;

#endregion

namespace Appalachia.Spatial.MeshBurial.Processing.QueueItems
{
    [Serializable]
    public abstract class MeshBurialManyQueueItem : MeshBurialQueueItem
    {
        protected MeshBurialManyQueueItem(string name, int length) : base(name, length)
        {
        }

        protected override void OnInitializeInternal()
        {
        }
    }
}
