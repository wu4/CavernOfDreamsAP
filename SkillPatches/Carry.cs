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
                __result = FlagCache.CachedSkillFlags.carry;
                return false;
            }
        }
    }
}
