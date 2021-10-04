#region

using Appalachia.Base.Scriptables;
using Appalachia.Core.Collections.Interfaces;
using Appalachia.Spatial.Terrains.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    public class
        TerrainMetadataDictionary : SelfSavingSingletonScriptableObject<TerrainMetadataDictionary>
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
                    SetDirty();

                    _state.SetDirtyAction(SetDirty);
                }

                return _state;
            }
        }

        protected override void WhenEnabled()
        {
            if (_state == null)
            {
                _state = new TerrainMetadataLookup();
                SetDirty();
            }

            _state.SetDirtyAction(SetDirty);
        }
    }
}
