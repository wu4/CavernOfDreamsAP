using UnityEngine;
using HarmonyLib;

namespace CoDArchipelago.LocationSplitPatches
{
    [HarmonyPatch(typeof(TelescopeInteract), nameof(TelescopeInteract.Interact))]
    static class FixObservatoryTelescope
    {
        static Access.Method<TelescopeInteract, bool> CheckRotation = new("CheckRotation");
        static Access.Field<TelescopeInteract, Timer> fixTimer = new("fixTimer");

        static bool StarsAligned(TelescopeInteract telescopeInteract, GameObject obj, int rotation)
        {
            return CheckRotation.Invoke(telescopeInteract, new object[] {obj, rotation});
        }

        static readonly string TELESCOPE_FLAG = "LOCATION_OBSERVATORY_SUCCESS";

        public static bool Prefix(TelescopeInteract __instance)
        {
            Debug.LogError("hello. i am telescope");
            Save save = GlobalHub.Instance.save;

            if (save.GetFlag(TELESCOPE_FLAG).on) {
                GlobalHub.Instance.player.happySFX.Play();
                return false;
            }

            if (!(
                StarsAligned(__instance, __instance.fakeStars1, __instance.correctRotation1)
                &&
                StarsAligned(__instance, __instance.fakeStars2, __instance.correctRotation2)
            )) {
                GlobalHub.Instance.player.whimperSFX.Play();
                return false;
            }

            fixTimer.Get(__instance).Reset();
            save.SetFlag(TELESCOPE_FLAG, true);

            return false;
        }
    }
}
