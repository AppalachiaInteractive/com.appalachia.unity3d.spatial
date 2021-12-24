#if UNITY_EDITOR

#region

using System;
using System.Diagnostics;
using Appalachia.Core.Objects.Root;
using Appalachia.Utility.Strings;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    [Serializable]
    public class MeshBurialAdjustmentEntryWrapper : AppalachiaSimpleBase
    {
        [InlineProperty]
        [HideLabel]
        [LabelWidth(0)]
        public MeshBurialAdjustmentEntry entry;

        [DebuggerStepThrough] public override string ToString()
        {
            return ZString.Format("Error {0}: {1}", entry.error, entry.adjustment);
        }
    }
}

#endif