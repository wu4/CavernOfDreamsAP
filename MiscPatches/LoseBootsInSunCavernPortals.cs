using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using System.Collections.Generic;
using System.Linq;

namespace CoDArchipelago.MiscPatches
{
    ///<summary>
    ///Removes the Jester Boots removal trigger in Sun Cavern, and instead
    ///removes Jester Boots upon portal entry.
    ///</summary>
    static class LoseBootsInSunCavernPortals
    {
        class RemoveSunCavernBootRemovalTrigger: InstantiateOnGameSceneLoad
        {
            public RemoveSunCavernBootRemovalTrigger()
            {
                GameObject.DestroyImmediate(GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Objects/BootRemovalTrigger").gameObject);
            }
        }

        static string[] sunCavernPortalPaths = new string[] {
            "/CAVE/Sun Cavern (Main)/Fellas/Nest FellaHatchable Lake/Portal",
            "/CAVE/Sun Cavern (Main)/Fellas/Nest FellaHatchable Monster/Portal",
            "/CAVE/Sun Cavern (Main)/Fellas/Nest FellaHatchable Palace/Portal",
            "/CAVE/Sun Cavern (Main)/Fellas/Nest FellaHatchable Gallery/Portal",
            "/CAVE/Lake Lobby/Warps/Portal",
            "/CAVE/Monster Lobby/Warps/Portal",
            "/CAVE/Palace Lobby/Warps/Portal",
            "/CAVE/Gallery Lobby/Warps/Portal",
        };

        static bool IsSunCavernPortal(Transform t)
        {
            return sunCavernPortalPaths.Contains(t.GetPath().Substring(1));
        }

        [HarmonyPatch(typeof(WarpTrigger), "Warp")]
        static class RemoveBootsOnSunCavernPortalWarp
        {
            public static void Postfix(WarpTrigger __instance)
            {
                // that will be responsible for this instead
                if (DropCarryablesOnWarp.shouldDropCarryables) return;

                Player player = GlobalHub.Instance.player;
                if (!player.WearingHoverBoots()) return;

                if (!IsSunCavernPortal(__instance.transform)) return;

                GlobalHub.Instance.player.EquipHoverBoots(false, true);
            }
        }
    }
}
