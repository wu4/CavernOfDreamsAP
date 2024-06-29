using HarmonyLib;
using UnityEngine.InputSystem;
using UnityEngine;

namespace CoDArchipelago.MiscPatches
{
    static class SaveStates
    {
        static State state;

        class State
        {
            readonly Vector3 position;
            readonly Quaternion rotation;

            readonly Vector3 cameraPosition;
            readonly Quaternion cameraRotation;

            static Access.Field<Player, CharacterController> cc = new("cc");

            public State()
            {
                var player = GlobalHub.Instance.player;
                position = player.transform.localPosition;
                rotation = player.GetModel().transform.localRotation;

                var camera = GlobalHub.Instance.cameraHandler.standardCam;
                cameraPosition = camera.State.RawPosition;
                cameraRotation = camera.State.RawOrientation;
            }

            public void Load()
            {
                var player = GlobalHub.Instance.player;
                var c = cc.Get(player);
                c.enabled = false;
                player.transform.localPosition = position;
                player.GetModel().transform.localRotation = rotation;

                var camera = GlobalHub.Instance.cameraHandler.standardCam;
                camera.ForceCameraPosition(cameraPosition, cameraRotation);
                c.enabled = true;
            }
        }

        static void SaveState()
        {
            state = new();
        }

        static void LoadState()
        {
            state.Load();
        }

        [HarmonyPatch(typeof(GlobalHub), "Update")]
        static class UpdatePatch
        {
            static Access.Field<GlobalHub, InputAction> selectAction = new("selectAction");
            static Access.Field<GlobalHub, InputAction> inputAction = new("inputAction");

            static void Postfix(GlobalHub __instance)
            {
                if (MenuHandler.Instance.IsMenuActive()) {
                    if (inputAction.Get(__instance).WasPressedThisFrame()) {
                        SaveState();
                    }
                } else {
                    if (selectAction.Get(__instance).WasPressedThisFrame()) {
                        LoadState();
                    }
                }
            }
        }
    }
}
