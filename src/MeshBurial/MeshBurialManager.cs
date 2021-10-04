#region

#endregion

using Appalachia.Editing.Attributes;

namespace Appalachia.Spatial.MeshBurial
{
    [EditorOnlyInitializeOnLoad]
    public static class MeshBurialManager
    {
        /*
        public static MeshBurialState GetInitializedState(
            Matrix4x4 ltw,
            GameObject go,
            ref int goHashCode,
            ref Terrain terrain,
            ref int terrainHashCode)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                if (go == null)
                {
                    return null;
                }

                if (goHashCode == 0)
                {
                    goHashCode = go.GetHashCode();
                }

                if (terrain == null)
                {
                    terrain = Terrain.activeTerrains.GetTerrainAtPosition(ltw.GetPositionFromMatrix());
                }

                if (terrainHashCode == 0)
                {
                    terrainHashCode = terrain.GetHashCode();
                }

                var state = MeshBurialDictionaryManager.gameObjects.GetOrCreate(goHashCode, ltw, go, terrainHashCode);

                state.localToWorld = ltw;

                return state;
            }
        }

        public static Matrix4x4 PrepareAndApplyAdjustmentParameters(
            this MeshBurialState state,
            bool adoptTerrainNormal,
            bool buryBeforeAdjustment,
            out double error)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                return state.PrepareAndApplyAdjustmentParameters(
                    state.localToWorld,
                    adoptTerrainNormal,
                    buryBeforeAdjustment,
                    out error
                );
            }
        }

        public static Matrix4x4 PrepareAndApplyAdjustmentParameters(
            this MeshBurialState state,
            Matrix4x4 ltw,
            bool adoptTerrainNormal,
            bool buryBeforeAdjustment,
            out double error)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                if (state == null)
                {
                    error = 1.0;
                    return Matrix4x4.identity;
                }

                state.localToWorld = ltw;

                if (adoptTerrainNormal)
                {
                    state.localToWorld = state.AdjustNormal(false);
                }

                if (buryBeforeAdjustment)
                {
                    state.localToWorld = state.Bury(false);
                }

                state.Calculateinitial.error();

                state.PrepareAdjustmentParameters(true, !adoptTerrainNormal);
                state.localToWorld = state.ApplyAdjustmentParameters();

                error = state.optimized.bestError;

                return state.localToWorld;
            }
        }
        
        

        public static Matrix4x4 ImproveAdjustment(
            this MeshBurialState state,
            bool adoptTerrainNormal,
            bool buryBeforeAdjustment,
            out double error)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                return state.ImproveAdjustment(state.localToWorld, adoptTerrainNormal, buryBeforeAdjustment, out error);
            }
        }

        public static Matrix4x4 ImproveAdjustment(
            this MeshBurialState state,
            Matrix4x4 ltw,
            bool adoptTerrainNormal,
            bool buryBeforeAdjustment,
            out double error)
        {
           
using (ASPECT.Many(ASPECT.Profile(), ASPECT.Trace()))
            {
                if (state == null)
                {
                    error = 1.0;
                    return Matrix4x4.identity;
                }

                state.localToWorld = ltw;

                if (adoptTerrainNormal)
                {
                    state.localToWorld = state.AdjustNormal(false);
                }

                if (buryBeforeAdjustment)
                {
                    state.localToWorld = state.Bury(false);
                }

                state.Calculateinitial.error();

                state.optimized.permissiveness += 1;

                state.PrepareAdjustmentParameters(true, !adoptTerrainNormal, state.optimized.permissiveness);
                state.localToWorld = state.ApplyAdjustmentParameters();

                error = state.optimized.bestError;

                return state.localToWorld;
            }
        }
        */
    }
}
