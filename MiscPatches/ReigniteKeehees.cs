using System.Collections.Generic;
using HarmonyLib;

namespace CoDArchipelago
{
    /// <summary>
    /// Reignites all doused Keehees upon leaving the area
    /// (I crave their suffering)
    /// </summary>
    static class ReigniteKeeheesOnAreaEntry
    {
        static readonly List<ShootingStar> dousedKeehees = new();

        [HarmonyPatch(typeof(ShootingStar), "HandleWhack")]
        static class OnKeeheeDoused
        {
            static void Prefix(ShootingStar __instance, bool ___activated)
            {
                if (!___activated)
                    dousedKeehees.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(Area), "Activate")]
        static class ReigniteKeehees
        {
            static readonly Access.Field<ShootingStar, bool> activatedField = new("activated");

            static void Reignite(ShootingStar ss) {
                if (ss.holder)
                    ss.holder.speed *= 2f;

                ss.act1.activated = false;
                ss.act2.activated = false;

                activatedField.Set(ss, false);

                ss.fireParticles.SetActive(true);
                ss.iceParticles.SetActive(false);
                ss.hurtObject.SetActive(true);
                ss.crackleSFX.SetActive(true);
            }

            static bool Prefix(ref bool __runOriginal)
            {
                if (!__runOriginal) return false;

                dousedKeehees.Do(Reignite);

                dousedKeehees.Clear();

                return true;
            }
        }
    }
}