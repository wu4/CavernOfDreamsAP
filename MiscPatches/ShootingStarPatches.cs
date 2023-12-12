using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    /// <summary>
    /// Reignites all doused Keehees upon leaving the area
    /// (I crave their suffering)
    /// </summary>
    static class ShootingStarPatches
    {
        static readonly List<ShootingStar> dousedShootingStars = new();

        [HarmonyPatch(typeof(ShootingStar), "HandleWhack")]
        static class OnShootingStarDoused
        {
            static void Prefix(ShootingStar __instance, bool ___activated)
            {
                if (!___activated)
                    dousedShootingStars.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(Area), "Activate")]
        static class ResetShootingStars
        {
            static readonly AccessTools.FieldRef<ShootingStar, bool> activatedRef = AccessTools.FieldRefAccess<ShootingStar, bool>("activated");

            static void Reignite(ShootingStar ss) {
                if (ss.holder)
                    ss.holder.speed *= 2f;

                ss.act1.activated = false;
                ss.act2.activated = false;

                activatedRef(ss) = false;

                ss.fireParticles.SetActive(true);
                ss.iceParticles.SetActive(false);
                ss.hurtObject.SetActive(true);
                ss.crackleSFX.SetActive(true);
            }

            static bool Prefix(ref bool __runOriginal)
            {
                if (!__runOriginal) return false;

                dousedShootingStars.Do(Reignite);

                dousedShootingStars.Clear();

                return true;
            }
        }
    }
}