using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Roll
    {
        [HarmonyPatch(typeof(Player), "CanStartRoll")]
        static class Patch
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                if (FlagCache.CachedSkillFlags.roll) return true;

                __result = false;
                return false;
            }
        }
    }
}