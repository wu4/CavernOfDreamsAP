using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.Cutscenes
{
    /// <summary>
    /// Prevents whack cutscenes from re-triggering if the location has already
    /// been found
    /// </summary>
    [HarmonyPatch(typeof(WhackForCutscene), "HandleWhack")]
    static class WhackForCutscenePatch
    {
        static bool Prefix(
            WhackForCutscene __instance,
            Whackable.WhackType type,
            GameObject source,
            
            ref bool ___whacked,
            
            ref bool __result
        ) {
            if (!Data.eventItems.ContainsKey(__instance.cutscene.flag)) return true;
            
            if (GlobalHub.Instance.save.GetFlag("LOCATION_" + __instance.cutscene.flag).On()) {
                __result = false;
                return false;
            }

            ___whacked = true;
            GlobalHub.Instance.SetCutscene(__instance.cutscene);

            __result = true;
            return false;
        }
    }
}