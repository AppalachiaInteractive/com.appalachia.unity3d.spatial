#region

using Appalachia.Core.Collections.Interfaces;
using Appalachia.Core.Scriptables;
using Appalachia.Spatial.Terrains.Collections;
using Appalachia.Utility.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    public class
        TerrainMetadataDictionary : SingletonAppalachiaObject<TerrainMetadataDictionary>
    {
        [SerializeField]
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            NumberOfItemsPerPage = 1
        )]
        private TerrainMetadataLookup _state;

        public IAppaLookup<int, TerrainMetadata, AppaList_TerrainMetadata> Lookup
        {
            get
            {
                if (_state == null)
                {
                    _state = new TerrainMetadataLookup();
#if UNITY_EDITOR
                   this.MarkAsModified();
                    _state.SetMarkModifiedAction(this.MarkAsModified);
#endif
                }

                return _state;
            }
        }

        protected override void WhenEnabled()
        {
            if (_state == null)
            {
                _state = new TerrainMetadataLookup();
#if UNITY_EDITOR
               this.MarkAsModified();
                _state.SetMarkModifiedAction(this.MarkAsModified);
#endif
            }
        }
    }
}
