using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Climb
    {
        [HarmonyPatch(typeof(Player), "CanClimb")]
        static class Patch
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                if (FlagCache.CachedSkillFlags.climb) return true;

                __result = false;
                return false;
            }
        }
    }
}