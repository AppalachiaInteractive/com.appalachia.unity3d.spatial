#region

using Appalachia.Core.Collections.Interfaces;
using Appalachia.Core.Scriptables;
using Appalachia.Spatial.MeshBurial.Collections;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    public class
        MeshBurialSharedStateDictionary : SingletonAppalachiaObject<
            MeshBurialSharedStateDictionary>
    {
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            NumberOfItemsPerPage = 1
        )]
        private MeshBurialSharedStateLookup _state;

        public IAppaLookup<int, MeshBurialSharedState, AppaList_MeshBurialSharedState> State
        {
            get
            {
                if (_state == null)
                {
                    _state = new MeshBurialSharedStateLookup();
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
                _state = new MeshBurialSharedStateLookup();
                SetDirty();
            }

            _state.SetDirtyAction(SetDirty);
        }
    }
}
