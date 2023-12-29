using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Flight
    {
        [HarmonyPatch(typeof(Player), "HasFlight")]
        static class Patch
        {
            static bool Prefix(ref bool __result)
            {
                __result = FlagCache.CachedSkillFlags.flight;
                return false;
            }
        }
    }
}