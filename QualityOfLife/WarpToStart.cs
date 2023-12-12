
using TMPro;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace CoDArchipelago
{
    [HasInitMethod]
    static class WarpToStart
    {
        public class StartLocation
        {
            public Area area;
            public GameObject startLocation;

            public StartLocation(string worldName, string areaName, string warpName)
            {
                area = GlobalGameScene.FindInScene(worldName, areaName).GetComponent<Area>();
                startLocation = area.transform.Find("Warps/" + warpName + "/warp_destination").gameObject;
            }
        }

        static MO_DEBUG_WARP startWarpComponent;
        
        public static void SetStartLocation(StartLocation to)
        {
            startWarpComponent.area = to.area;
            startWarpComponent.origin = to.startLocation.transform;
        }

        public static void Init()
        {
            GameObject injectionTarget = 
                GlobalGameScene
                    .FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1/Controls Button")
                    .gameObject;

            injectionTarget.name = "StartWarp Button";

            injectionTarget.GetComponentInChildren<TextMeshProUGUI>().text = "WARP TO START";

            Component.Destroy(injectionTarget.GetComponent<MO_CHANGE_MENU>());

            startWarpComponent = injectionTarget.AddComponent<MO_DEBUG_WARP>();
        }
        
        static readonly MethodInfo WarpHelper = AccessTools.Method(typeof(GlobalHub), "WarpHelper");
        static readonly AccessTools.FieldRef<GlobalHub, Checkpoint> checkpoint = AccessTools.FieldRefAccess<GlobalHub, Checkpoint>("checkpoint");
        static readonly AccessTools.FieldRef<GlobalHub, Timer> warpTimer = AccessTools.FieldRefAccess<GlobalHub, Timer>("warpTimer");
        
        [HarmonyPatch(typeof(MO_DEBUG_WARP), "OnSelect")]
        static class Patch
        {
            static bool Prefix(MO_DEBUG_WARP __instance)
            {
                checkpoint(GlobalHub.Instance) = null;
                warpTimer(GlobalHub.Instance).EndAndDeactivate();
                WarpHelper.Invoke(GlobalHub.Instance, new object[] {__instance.area, __instance.origin});

                MenuHandler.Instance.CloseMenu();
                return false;
            }
        }

    }
}