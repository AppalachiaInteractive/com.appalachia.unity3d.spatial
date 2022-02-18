#if UNITY_EDITOR

#region

using Appalachia.Core.Collections.Interfaces;
using Appalachia.Core.Objects.Root;
using Appalachia.Spatial.MeshBurial.Collections;
using Appalachia.Utility.Async;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.MeshBurial.State
{
    public class MeshBurialSharedStateDictionary : SingletonAppalachiaObject<MeshBurialSharedStateDictionary>
    {
        #region Fields and Autoproperties

        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            NumberOfItemsPerPage = 1
        )]
        private MeshBurialSharedStateLookup _state;

        #endregion

        public IAppaLookup<int, MeshBurialSharedState, AppaList_MeshBurialSharedState> State
        {
            get
            {
                if (_state == null)
                {
                    _state = new MeshBurialSharedStateLookup();
                    MarkAsModified();

                    _state.SetSerializationOwner(this);
                }

                return _state;
            }
        }

        /// <inheritdoc />
        protected override async AppaTask WhenEnabled()
        {
            await base.WhenEnabled();

            using (_PRF_WhenEnabled.Auto())
            {
                if (_state == null)
                {
                    _state = new MeshBurialSharedStateLookup();
                    MarkAsModified();
                }

                _state.SetSerializationOwner(this);
            }
        }
    }
}

#endif
