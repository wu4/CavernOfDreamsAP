using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Attack
    {
        [HarmonyPatch(typeof(Player), "HasAttack")]
        static class Patch
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                __result = FlagCache.CachedSkillFlags.attack ||
                    __instance.IsGrounded()
                        ? FlagCache.CachedSkillFlags.groundAttack
                        : FlagCache.CachedSkillFlags.airAttack;

                return false;
            }
        }
    }
}