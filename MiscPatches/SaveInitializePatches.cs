using HarmonyLib;

namespace CoDArchipelago.MiscPatches
{
    static class SaveInitializePatch
    {
        [HarmonyPatch(typeof(Save), nameof(Save.Initialize))]
        class Patch
        {
            private static readonly string[] set_flags = {
                "CAVE_INTRO",

                "CAVE_SAGE_ENTRY",
                "SAGE_TALK_INTRO",

                "FIRST_NOTE",
                "FIRST_CARD",
                "FIRST_HOVER_BOOTS",

                "LAKE_FIRST_ENTRY",
                "MONSTER_FIRST_ENTRY",
                "PALACE_FIRST_ENTRY",
                "GALLERY_FIRST_ENTRY",

                "CAVE_VILLAIN_LAKE_LOBBY",
                "CAVE_VILLAIN_MONSTER_LOBBY",
                "CAVE_MONSTER_LOBBY_VILLAIN_CUTSCENE",
                "CAVE_VILLAIN_MOON_CAVERN",

                "LAKE_KAPPA_GOODBYE",

                "MONSTER_ENTERED",
                "MONSTER_ONE_BOIL_REMOVED",
                "MONSTER_DRONE_FIRE_INTRO",

                "PALACE_MORAY_AWAKE",
                "PRINCESS_GOODBYE",

                "GALLERY_TRAPDOOR_ACTIVE",

                "UNDEAD_FIRST_ENTRY",
                "CHALICE_FIRST_ENTRY",
                "DROWN_FIRST_ENTRY",



                "AP_SHUFFLE_GRATITUDE"
            };

            static void Postfix(Save __instance) =>
                set_flags.Do(flag_name => __instance.SetFlag(flag_name, true));
        }
    }
}
