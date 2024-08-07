using TMPro;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Cinemachine;
using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
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

        static string startLocation = "StartLocation(Sun Cavern)";

        class StartLocations : InstantiateOnGameSceneLoad
        {
            public StartLocations()
            {
                startLocations = new() {
                    {"StartLocation(Sun Cavern)", new(
                        "CAVE", "Sun Cavern (Main)",
                        "Sage",
                        new(0f, -1.5f, 0f), Quaternion.Euler(270f, 90f, 0f)
                    )},
                    {"StartLocation(Moon Cavern)", new(
                        "CAVE", "Moon Cavern",
                        "Moon Cavern",
                        new(-4.23f, -2f, -0.375f), Quaternion.Euler(270f, 320f, 0f)
                    )},

                    {"StartLocation(Lostleaf Lobby)", new(
                        "CAVE", "Lake Lobby",
                        "Lostleaf Lobby",
                        new(3.8f, -0.5f, 8.2f), Quaternion.Euler(270f, 215f, 0f)
                    )},
                    {"StartLocation(Armada Lobby)", new(
                        "CAVE", "Monster Lobby",
                        "Armada Lobby",
                        new(2.145f, -1.04f, 3.0429f), Quaternion.Euler(270f, 220f, 0f)
                    )},
                    {"StartLocation(Prismic Lobby)", new(
                        "CAVE", "Palace Lobby",
                        "Prismic Lobby",
                        new(1.1098f, -0.04f, 4.0945f), Quaternion.Euler(270f, 230f, 0f)
                    )},
                    {"StartLocation(Gallery Lobby)", new(
                        "CAVE", "Gallery Lobby",
                        "Gallery Lobby",
                        new(35.8975f, 9.96f, 2.4454f), Quaternion.Euler(270f, 270f, 0f)
                    )},

                    {"StartLocation(Lostleaf Lake)", new(
                        "LAKE", "Lake (Main)",
                        "Lostleaf Lake",
                        new(7.5977f, -0.915f, -10.6188f), Quaternion.Euler(270f, 140f, 0f)
                    )},
                    {"StartLocation(Lostleaf Church)", new(
                        "LAKE", "Church",
                        "Church",
                        new(21.9583f, -0.04f, -0.0301f), Quaternion.Euler(270f, 270f, 0f)
                    )},
                    {"StartLocation(Lostleaf Treehouse)", new(
                        "LAKE", "Bedroom",
                        "Treehouse",
                        new(-0.0658f, 0.0851f, -0.0489f), Quaternion.Euler(270f, 0f, 0f)
                    )},
                    {"StartLocation(Lostleaf Crypt)", new(
                        "LAKE", "Crypt",
                        "Crypt",
                        new(10.4072f, 0.585f, 0.1865f), Quaternion.Euler(270f, 90f, 0f)
                    )},

                    {"StartLocation(Armada Outside)", new(
                        "MONSTER", "Sky (Main)",
                        "Airborne Armada",
                        new(31.7403f, -4.04f, 45.6413f), Quaternion.Euler(270f, 220f, 0f)
                    )},
                    {"StartLocation(Armada Inside)", new(
                        "MONSTER", "Monster",
                        "Kerrington",
                        new(0.023f, 0.96f, 51.1915f), Quaternion.Euler(270f, 180f, 0f)
                    )},
                    {"StartLocation(Armada Earth Drone)", new(
                        "MONSTER", "DroneEarth",
                        "Earth Drone",
                        new(1.9985f, 2.9414f, -11.0252f), Quaternion.Euler(270f, 0f, 0f)
                    )},
                    {"StartLocation(Armada Fire Drone)", new(
                        "MONSTER", "DroneFire",
                        "Fire Drone",
                        new(9.0833f, -0.04f, 2.4716f), Quaternion.Euler(270f, 240f, 0f)
                    )},
                    {"StartLocation(Armada Water Drone)", new(
                        "MONSTER", "DroneWater",
                        "Water Drone",
                        new(17.2807f, -0.04f, 0.0607f), Quaternion.Euler(270f, 270f, 0f)
                    )},

                    {"StartLocation(Prismic Valley)", new(
                        "PALACE", "Valley (Main)",
                        "Valley",
                        new(-0.0203f, 2.46f, -11.7598f), Quaternion.Euler(270f, 180f, 0f)
                    )},
                    {"StartLocation(Prismic Palace)", new(
                        "PALACE", "Palace",
                        "Prismic Palace",
                        new(-0.0504f, -0.04f, -5.4154f), Quaternion.Euler(270f, 180f, 0f)
                    )},
                    {"StartLocation(Heavens Gate)", new(
                        "PALACE", "Sanctum",
                        "Heaven's Gate",
                        new(-11.1413f, -1.04f, -2.1963f), Quaternion.Euler(270f, 80f, 0f)
                    )},
                    {"StartLocation(Observatory)", new(
                        "PALACE", "Observatory",
                        "Observatory",
                        new(0.358f, 0.11f, 0.1266f), Quaternion.Euler(270f, 110f, 0f)
                    )},

                    {"StartLocation(Gallery Foyer)", new(
                        "GALLERY", "Foyer (Main)",
                        "Foyer",
                        new(-16.4714f, 5.96f, -0.1199f), Quaternion.Euler(270f, 90f, 0f)
                    )},
                    {"StartLocation(Gallery Earth)", new(
                        "GALLERY", "Earth Lobby",
                        "Rattles",
                        new(0.8906f, -0.04f, -3.8734f), Quaternion.Euler(270f, 300f, 0f)
                    )},
                    {"StartLocation(Gallery Fire)", new(
                        "GALLERY", "Fire Lobby",
                        "Gallery Fire Lobby",
                        new(-3.3196f, -0.04f, -0.0787f), Quaternion.Euler(270f, 270f, 0f)
                    )},
                    {"StartLocation(Gallery Water)", new(
                        "GALLERY", "Water Lobby",
                        "Gallery Sewers",
                        new(-19.7272f, -2.04f, 0.1615f), Quaternion.Euler(270f, 130f, 0f)
                    )},

                    {"StartLocation(Wastes Of Eternity)", new(
                        "UNDEAD", "Undead (Main)",
                        "Wastes of Eternity",
                        new(-16.1147f, 8.5998f, 12.3986f), Quaternion.Euler(270f, 186f, 0f)
                    )},
                    {"StartLocation(Coils Of Agony)", new(
                        "CHALICE", "Chalice (Main)",
                        "Coils of Agony",
                        new(-0.5528f, -0.04f, 1.8209f), Quaternion.Euler(270f, 180f, 0f)
                    )},
                    {"StartLocation(Pits Of Despair)", new(
                        "DROWN", "Drown (Main)",
                        "Pits of Despair",
                        new(0.1167f, 121.9601f, -7.1557f), Quaternion.Euler(270f, 180f, 0f)
                    )},
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
            startLocation = startLocationName;
        }

        public static void SetStartLocation(string worldName, string areaName, string warpName)
        {
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
                StartLocations.Get(startLocation).SetAsStart();

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
