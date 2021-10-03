#region

using System;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialAdjustmentEntryWrapper
    {
        [InlineProperty, HideLabel, LabelWidth(0)]
        public MeshBurialAdjustmentEntry entry;

        public override string ToString()
        {
            return $"Error {entry.error}: {entry.adjustment}";
        }
    }
}
