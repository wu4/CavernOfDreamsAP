
using TMPro;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Cinemachine;
using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    class ChangeStartLocation : InstantiateOnGameSceneLoad
    {
        public ChangeStartLocation()
        {
            GameObject injectionTarget = 
                GameScene
                    .FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1/Controls Button")
                    .gameObject;

            // injectionTarget.name = "StartWarp Button";

            injectionTarget.GetComponentInChildren<TextMeshProUGUI>().text = "WARP TO START";

            Component.Destroy(injectionTarget.GetComponent<MO_CHANGE_MENU>());

            startWarpMenuOption = injectionTarget.AddComponent<MO_DEBUG_WARP>();
        }
        
        class StartLocation
        {
            readonly Area area;
            readonly GameObject location;
            readonly bool startCamInFront;
            
            public StartLocation(
                string worldName, string areaName, string warpName,
                Vector3 position, Quaternion rotation,
                bool startCamInFront = false
            ) {
                this.startCamInFront = startCamInFront;
                area = GameScene.FindInScene(worldName, areaName).GetComponent<Area>();
                var warpsContainer = area.transform.Find("Warps");
                location = new(warpName);
                location.transform.SetParent(warpsContainer, false);
                location.transform.position = position;
                location.transform.rotation = rotation;
                location.AddComponent<Destination>();
            }
            
            public void SetAsStart()
            {
                SetStartLocation(area, location, startCamInFront);
            }
        }
        
        class StartLocations : InstantiateOnGameSceneLoad
        {
            public StartLocations()
            {
                startLocations = new() {
                    {"Cavern of Dreams - Sage", new(
                        "CAVE", "Sun Cavern (Main)", "Sage",
                        new(0f, -1.5f, 0f), Quaternion.Euler(270f, 90f, 0f)
                    )}
                };
            }

            static Dictionary<string, StartLocation> startLocations;
            
            public static StartLocation Get(string locationName) =>
                startLocations[locationName];
        }

        static MO_DEBUG_WARP startWarpMenuOption;
        static bool isStartCamInFront = false;
        
        static class GlobalHubFields
        {
            public static readonly AccessTools.FieldRef<GlobalHub, Area> areaCurrent = AccessTools.FieldRefAccess<GlobalHub, Area>("areaCurrent");
            public static readonly AccessTools.FieldRef<GlobalHub, GameObject> positionStart = AccessTools.FieldRefAccess<GlobalHub, GameObject>("positionStart");
            public static readonly AccessTools.FieldRef<GlobalHub, Transform> lastDest = AccessTools.FieldRefAccess<GlobalHub, Transform>("lastDest");
            public static readonly AccessTools.FieldRef<GlobalHub, World> world = AccessTools.FieldRefAccess<GlobalHub, World>("world");
        }
        
        public static void SetStartLocation(string startLocationName)
        {
            StartLocations.Get(startLocationName).SetAsStart();
        }

        public static void SetStartLocation(string worldName, string areaName, string warpName)
        {
            // var a = CreateNewStart("CAVE", "Palace Lobby", "DestFromDepthsToPalaceLobby");
            // var a = CreateNewStart("PALACE", "Valley (Main)", "DestFromPalaceLobbyToValley");
            // var a = CreateNewStart("CAVE", "Sun Cavern (Main)", "DestFromMonsterLobbyToCave");
            
            Area area = GameScene.FindInScene(worldName, areaName).GetComponent<Area>();
            Destination dest = area.transform.Find(warpName).GetComponent<Destination>();
            GameObject startLocationObj = area.transform.Find(warpName + "/warp_destination").gameObject;
            
            try {
                WarpTrigger entryTrigger =
                    GameScene
                    .GetComponentsInChildren<WarpTrigger>(true)
                    .First(t => t.destination == dest);
                
                isStartCamInFront = entryTrigger.camInFront;
            } catch (System.InvalidOperationException) {
                isStartCamInFront = false;
            }
            
            SetStartLocation(area, startLocationObj, isStartCamInFront);
        }
        
        static void SetStartLocation(Area area, GameObject startLocationObj, bool camInFront)
        {
            isStartCamInFront = camInFront;

            GlobalHubFields.areaCurrent(GlobalHub.Instance) = area;
            GlobalHubFields.positionStart(GlobalHub.Instance) = startLocationObj;
            GlobalHubFields.lastDest(GlobalHub.Instance) = startLocationObj.transform;
            GlobalHubFields.world(GlobalHub.Instance) = area.GetComponentInParent<World>();

            area.gameObject.SetActive(true);
            area.Activate();

            startWarpMenuOption.area = area;
            startWarpMenuOption.origin = startLocationObj.transform;
        }
        
        
        [HarmonyPatch(typeof(MO_DEBUG_WARP), "OnSelect")]
        static class Patch
        {
            static readonly MethodInfo WarpHelper = AccessTools.Method(typeof(GlobalHub), "WarpHelper");
            static readonly AccessTools.FieldRef<GlobalHub, Checkpoint> checkpoint = AccessTools.FieldRefAccess<GlobalHub, Checkpoint>("checkpoint");
            static readonly AccessTools.FieldRef<GlobalHub, Timer> warpTimer = AccessTools.FieldRefAccess<GlobalHub, Timer>("warpTimer");
            static readonly AccessTools.FieldRef<GlobalHub, bool> camInFront = AccessTools.FieldRefAccess<GlobalHub, bool>("camInFront");
            static readonly AccessTools.FieldRef<Player, Carryable> carryableObject = AccessTools.FieldRefAccess<Player, Carryable>("carryableObject");
            static readonly MethodInfo PlayerReleaseCarryable = AccessTools.Method(typeof(Player), "ReleaseCarryable");
            static readonly MethodInfo CarryableResetMe = AccessTools.Method(typeof(Carryable), "ResetMe");

            static bool Prefix(MO_DEBUG_WARP __instance)
            {
                if (__instance == startWarpMenuOption) {
                    camInFront(GlobalHub.Instance) = isStartCamInFront;
                    Player player = GlobalHub.Instance.player;
                    player.EquipHoverBoots(false, false);
                    if (player.IsCarrying()) {
                        Vector3 forward = player.GetModel().transform.forward;

                        Carryable carryable = carryableObject(player);
                        carryable.Drop(forward);

                        PlayerReleaseCarryable.Invoke(player, new object[] {forward});

                        // CarryableResetMe.Invoke(carryable, new object[] {});
                        GameObject.Destroy(carryable.gameObject);
                    }
                }
                checkpoint(GlobalHub.Instance) = null;
                warpTimer(GlobalHub.Instance).EndAndDeactivate();
                WarpHelper.Invoke(GlobalHub.Instance, new object[] {__instance.area, __instance.origin});

                MenuHandler.Instance.CloseMenu();
                return false;
            }
        }

        [HarmonyPatch(typeof(GlobalHub), "Start")]
        static class GameStartCameraFixPatch
        {
            static void Postfix()
            {
                Transform t = ((GameObject)AccessTools.Field(typeof(GlobalHub), "positionStart").GetValue(GlobalHub.Instance)).transform;
                
                var rotationToSet = t.rotation;

                rotationToSet *= Quaternion.Euler(90 * Vector3.right);
                if (isStartCamInFront) {
                    rotationToSet *= Quaternion.Euler(180 * Vector3.up);
                }

                rotationToSet *= Quaternion.Euler(30 * Vector3.right);
                var posToSet = t.position;
                var b = ((Vector3.up * 0.2f) - (rotationToSet * Vector3.forward)) * 6f;
                posToSet += b;

                foreach (CinemachineVirtualCameraBase camera in new CinemachineVirtualCameraBase[] {
                    GlobalHub.Instance.cameraHandler.standardCam,
                    GlobalHub.Instance.cameraHandler.cutsceneCam,
                }) {
                    camera.ForceCameraPosition(posToSet, rotationToSet);
                }
            }
        }
    }
}