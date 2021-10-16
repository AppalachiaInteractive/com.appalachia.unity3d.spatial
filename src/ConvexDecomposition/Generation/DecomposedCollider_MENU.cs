#if UNITY_EDITOR

#region

using Appalachia.Core.Constants;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Generation
{
    public partial class DecomposedCollider
    {
        private const string _PRF_PFX = nameof(DecomposedCollider) + ".";
        
               

        private const string MENU_BASE = "GameObject/Collisions/Convex Decompose/";
        private const string CREATE_BASE = MENU_BASE + "Create/";
        private const string RIGID_BASE = MENU_BASE + "Rigidbodies/";

        [MenuItem(CREATE_BASE + "1x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_1(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1f);
        }

        [MenuItem(CREATE_BASE + "1.1x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_2(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.1f);
        }

        [MenuItem(CREATE_BASE + "1.2x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_3(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.2f);
        }

        [MenuItem(CREATE_BASE + "1.3x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_4(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.3f);
        }

        [MenuItem(CREATE_BASE + "1.4x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_5(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.4f);
        }

        [MenuItem(CREATE_BASE + "1.5x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_75(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 1.5f);
        }

        [MenuItem(CREATE_BASE + "2.0x Volume Tolerance", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void Decompose_10(MenuCommand menuCommand)
        {
            Decompose(menuCommand, 2f);
        }

        private static readonly ProfilerMarker _PRF_Decompose = new ProfilerMarker(_PRF_PFX + nameof(Decompose));
        public static void Decompose(MenuCommand menuCommand, float tolerance)
        {
            using (_PRF_Decompose.Auto())
            {
                var go = menuCommand.context as GameObject;
                if (go == null)
                {
                    return;
                }

                var decomposed = go.GetComponent<DecomposedCollider>();

                if (decomposed == null)
                {
                    decomposed = go.AddComponent<DecomposedCollider>();
                }

                decomposed.data.settings.maxConvexHulls = 64;
                decomposed.data.successThreshold = tolerance;

                decomposed.ExecuteDecomposition(ExecutionStyle.Normal);
            }
        }
        
        [MenuItem(RIGID_BASE + "Add Rigidbody/1 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_1(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 1.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/2 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_2(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 2.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/3 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_3(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 3.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/4 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_4(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 4.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/5 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_5(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 5.0f);
        }

        [MenuItem(RIGID_BASE + "Add Rigidbody/10 kg", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void AddRigidbody_10(MenuCommand menuCommand)
        {
            AddRigidbody(menuCommand, 10.0f);
        }

        public static void AddRigidbody(MenuCommand menuCommand, float mass)
        {
            var go = menuCommand.context as GameObject;
            if (go == null)
            {
                return;
            }

            var decomposedColliders = go.GetComponentsInChildren<DecomposedCollider>();

            for (var i = 0; i < decomposedColliders.Length; i++)
            {
                var decomposed = decomposedColliders[i];

                var rigidbody = decomposed.GetComponent<Rigidbody>();

                if (!rigidbody)
                {
                    rigidbody = decomposed.gameObject.AddComponent<Rigidbody>();
                }

                rigidbody.detectCollisions = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.mass = mass;
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
            }
        }

        [MenuItem(RIGID_BASE + "Remove Rigidbody", priority = APPA_MENU.GAME_OBJ.COLLIDERS.COLLIDER_BAKE)]
        public static void RemoveRigidbody(MenuCommand menuCommand)
        {
            var go = menuCommand.context as GameObject;
            if (go == null)
            {
                return;
            }

            var decomposedColliders = go.GetComponentsInChildren<DecomposedCollider>();

            for (var i = 0; i < decomposedColliders.Length; i++)
            {
                var decomposed = decomposedColliders[i];

                var rigidbody = decomposed.GetComponent<Rigidbody>();

                if (!rigidbody)
                {
                    return;
                }

                if (Application.isPlaying)
                {
                    Destroy(rigidbody);
                }
                else
                {
                    DestroyImmediate(rigidbody);
                }
            }
        }
    }
}

#endif