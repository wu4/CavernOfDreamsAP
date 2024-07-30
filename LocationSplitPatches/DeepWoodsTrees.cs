using HarmonyLib;

namespace CoDArchipelago.LocationSplitPatches
{
    [HarmonyPatch(typeof(KoiSwitchListener), "Start")]
    static class FixDeepWoodsTreesFlagPatch
    {
        static Access.Field<KoiSwitchListener, bool> success = new("success");

        public static void Postfix(KoiSwitchListener __instance)
        {
            success.Set(__instance, false);
        }
    }
}
