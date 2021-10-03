#region

using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Collections.Implementations.Lookups;
using Appalachia.Core.Collections.Interfaces;
using Appalachia.Core.Scriptables;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Appalachia.Core.Terrains
{
    public class TerrainMetadataDictionary : SelfSavingSingletonScriptableObject<TerrainMetadataDictionary>
    {
        [SerializeField]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 1)]
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
