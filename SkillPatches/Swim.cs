using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.SkillPatches
{
    static class Swim
    {
        [HarmonyPatch(typeof(Player), "OnTriggerEnter")]
        static class Patch
        {
            static void Postfix(Player __instance, Collider collision, Carryable ___carryableObject, GameObject ___model)
            {
                if (FlagCache.CachedSkillFlags.swim) return;

                if (collision.gameObject.tag == "Water") {
                    __instance.SetVelocity(new Vector3());

                    if (__instance.IsCarrying())
                    {
                        ___carryableObject.Drop(___model.transform.forward);
                        __instance.ReleaseCarryable(___model.transform.forward);
                    }

                    DeathPatches.WaterTeleport(__instance);
                }
            }
        }
    }
}