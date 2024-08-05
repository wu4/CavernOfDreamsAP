using HarmonyLib;

namespace CoDArchipelago.SkillPatches
{
    static class Attack
    {
        class GiveBothTailsIfUnsplit : InstantiateOnGameSceneLoad
        {
            public GiveBothTailsIfUnsplit()
            {
                Collecting.MyItem.RegisterTrigger(
                    "SKILL_ATTACK",
                    randomized => {
                        GlobalHub.Instance.save.SetFlag("SKILL_GROUNDATTACK", true);
                        GlobalHub.Instance.save.SetFlag("SKILL_AIRATTACK", true);
                    }
                );
            }
        }

        [HarmonyPatch(typeof(Player), "HasAttack")]
        static class Patch
        {
            static bool Prefix(Player __instance, ref bool __result)
            {
                __result = __instance.IsGrounded()
                         ? FlagCache.CachedSkillFlags.groundAttack
                         : FlagCache.CachedSkillFlags.airAttack;

                return false;
            }
        }
    }
}
