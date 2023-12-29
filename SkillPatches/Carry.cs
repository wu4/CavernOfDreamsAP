using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Carry
    {
        [HarmonyPatch(typeof(Player), "CanPickUpObject")]
        static class Patch
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                if (FlagCache.CachedSkillFlags.carry) return true;

                __result = false;
                return false;
            }
        }
    }
}