using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class AirSwim
    {
        [HarmonyPatch(typeof(Player), "CanPaddle")]
        static class Patch
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                if (FlagCache.CachedSkillFlags.airSwim) return true;

                if (__instance.IsUnderwater()) {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}