using UnityEngine;
using HarmonyLib;

namespace CoDArchipelago.MiscPatches
{
    static class NoJesterBootsCarry
    {
        [HarmonyPatch(typeof(HoverBoots), nameof(HoverBoots.Collect))]
        static class StopBootsIfCarrying
        {
            static bool Prefix(HoverBoots __instance)
            {
                Player player = GlobalHub.Instance.player;

                if (player.IsCarrying()) {
                    player.whimperSFX.Play();
                    GameObject.Destroy(__instance.gameObject);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "CanPickUpObject")]
        static class StopCarryingIfBoots
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                if (__instance.HoverBootsActive()) {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}
