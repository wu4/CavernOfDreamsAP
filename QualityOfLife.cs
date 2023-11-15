using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace CoDArchipelago
{
    static class QualityOfLife
    {
        static class CutsceneTriggerPatch
        {
            [HarmonyPatch(typeof(CutsceneTrigger), "OnTrigger")]
            class Patch
            {
                private static readonly string[] trigger_blacklist = {
                    "Nurikabe CT",
                    "Challenge Nurikabe CT"
                };

                static bool Prefix(CutsceneTrigger __instance) =>
                    !trigger_blacklist.Contains(__instance.gameObject.name);
            }
        }

        static class TutorialSkipPatch
        {
            [HarmonyPatch(typeof(TutorialHandler), nameof(TutorialHandler.StartTutorial))]
            static class StartPatch { static bool Prefix() => false; }

            [HarmonyPatch(typeof(TutorialHandler), nameof(TutorialHandler.CompleteTutorial))]
            static class CompletePatch { static bool Prefix() => false; }
            [HarmonyPatch(typeof(TutorialHandler), nameof(TutorialHandler.EndTutorial))]
            static class EndPatch { static bool Prefix() => false; }
        }

        static class SavePatch
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

                    "GALLERY_TRAPDOOR_ACTIVE",

                    "UNDEAD_FIRST_ENTRY",
                    "CHALICE_FIRST_ENTRY",
                    "DROWN_FIRST_ENTRY"
                };

                static void Postfix(Save __instance)
                {
                    foreach (string flag_name in set_flags) {
                        __instance.SetFlag(flag_name, true);
                    }
                    // foreach (string name in Enum.GetNames(typeof(TutorialNode.TutorialType))) {
                    //     __instance.SetFlag("TUTORIAL_" + name + "_COMPLETE", true);
                    // }
                }
            }
        }

        static readonly Dictionary<string, Dictionary<string, (bool Interrupt, bool MakeFast, int[] IndexWhitelist)>> cutsceneWhitelists = new() {
            {"CAVE", new() {
                {"Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Nurikabe Fall Cutscene", (false, false, new int[4]{2, 4, 5, 6})},
                {"Gallery Lobby/Cutscenes/OpenGalleryCutscene", (false, true, new int[3]{0, 3, 4})},
            }},

            {"LAKE", new() {
                {"Lake (Main)/Cutscenes/RaiseSwingsCutscene", (false, false, new int[6]{0, 1, 5, 6, 7, 8})},
                {"Lake (Main)/Cutscenes/OpenChurchGateCutscene", (false, false, new int[2]{0, 1})},
                {"Lake (Main)/Cutscenes/OpenCryptGateCutscene", (false, false, new int[3]{0, 3, 4})},
                {"Lake (Main)/Cutscenes/KappaTalkProblem", (false, false, new int[1]{0})},
                {"Lake (Main)/Cutscenes/KappaTalkProblemRepeat", (false, false, new int[2]{0, 1})},
                {"Lake (Main)/Cutscenes/KappaTalkSuccess", (false, true, new int[5]{11, 12, 14, 15, 16})},

                {"Crypt/Cutscenes/PlatformRaiseCutscene", (false, true, new int[2]{1, 2})},

                {"Church/Cutscenes/ChurchSuccess", (false, true, new int[2]{0, 5})},
            }},

            {"MONSTER", new() {
                {"Monster/Rotate (Inside Monster)/Cutscenes/RemoveAllBoilsCutscene", (false, true, new int[3]{0, 5, 6})},
            }},

            {"PALACE", new() {
                {"Valley (Main)/Cutscenes/PALACE_MELTED_ICE", (false, true, new int[9]{4, 8, 9, 11, 12, 13, 14, 15, 16})},
                {"Valley (Main)/Cutscenes/PALACE_MORAY_AWAKE", (false, false, new int[0]{})},

                {"Sanctum/Cutscenes/RaiseSanctumStopperCutscene", (false, true, new int[2]{1, 2})},
                {"Sanctum/Cutscenes/StartSanctumRaceCutscene", (false, true, new int[2]{1, 3})},
                {"Sanctum/Cutscenes/StopSanctumRaceCutscene", (false, true, new int[2]{1, 2})},
            }},
        };

        public static void Init()
        {
            foreach (var rootKVPair in cutsceneWhitelists) {
                Transform rootTransform = GlobalGameScene.GetRootObjectByName(rootKVPair.Key).transform;
                foreach (var innerKVPair in rootKVPair.Value) {
                    Debug.Log(innerKVPair.Key);
                    Transform transform = rootTransform.Find(innerKVPair.Key);
                    Cutscene cutscene = transform.GetComponent<Cutscene>();
                    var (interrupt, makeFast, indexWhitelist) = innerKVPair.Value;
                    cutscene.interrupt = interrupt;
                    if (makeFast) {
                        cutscene.durationAfterFinal = 1;
                        foreach (Event ev in cutscene.gameObject.GetComponentsInChildren<Event>(true)) {
                            ev.start = 0;
                            if (ev is CutsceneActivationEvent cutsceneEv) {
                                if (cutsceneEv.activation is Raise raise) {
                                    raise.raiseTime = 1;
                                }
                            }
                        }
                    }

                    int ids_len = indexWhitelist.Length;
                    int whitelistIndex = 0;
                    int cursor = 0;
                    int realIndex = 0;
                    Debug.Log("culling " + transform.gameObject.name);
                    while (transform.childCount > ids_len) {
                        if (whitelistIndex >= indexWhitelist.Length || indexWhitelist[whitelistIndex] != realIndex) {
                            Transform child = transform.GetChild(cursor);
                            Debug.Log(child.gameObject.name);
                            child.parent = null;
                            GameObject.DestroyImmediate(child.gameObject);
                        } else {
                            whitelistIndex++;
                            cursor++;
                        }
                        realIndex++;
                    }
                }
            }
        }
    }
}