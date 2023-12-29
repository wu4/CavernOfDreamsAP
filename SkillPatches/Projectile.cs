using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Projectile
    {
    [HarmonyPatch(typeof(Player), "HasShoot")]
        static class Patch
        {
            static bool Prefix(ref bool __result)
            {
                __result = FlagCache.CachedSkillFlags.projectile;
                return false;
            }
        }
    }
}