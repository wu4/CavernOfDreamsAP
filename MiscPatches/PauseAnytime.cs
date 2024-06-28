using HarmonyLib;

namespace CoDArchipelago.MiscPatches
{
    [HarmonyPatch(typeof(GlobalHub), "CanPause")]
    static class PauseAnytime
    {
        static bool Prefix(ref bool __result) {
            __result = true;
            return false;
        }
    }
}
