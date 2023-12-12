using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.InputSystem.EnhancedTouch;

namespace CoDArchipelago
{
    [HasInitMethod]
    static class QualityOfLife
    {
        static class CutsceneTriggerBlacklist
        {
            private static readonly Dictionary<string, string[]> triggerBlacklist = new() {
                {"CAVE", new[] {
                    "Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Nurikabe CT",
                    "Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Challenge Nurikabe CT",
                    "Gallery Lobby/Cutscenes/OpenHedgeMazeExploitTrigger"
                }},

                {"LAKE", new[] {
                    "Lake (Main)/Cutscenes/GroveHelloTrigger",
                    "Lake (Main)/Cutscenes/ReactivateGroveHelloTrigger",
                    "Lake (Main)/Cutscenes/KappaGoodbyeTrigger",
                    "Lake (Main)/Cutscenes/EnableGoodbyeTrigger",
                }},

                {"MONSTER", new[] {
                    "Sky (Main)/Cutscenes/GoodbyeTrigger",
                    "Sky (Main)/Cutscenes/EnableGoodbyeTrigger",
                    "Sky (Main)/Cutscenes/HelloTrigger",
                    "Sky (Main)/Cutscenes/EnableHelloTrigger",
                }},

                {"PALACE", new[] {
                    "Valley (Main)/Cutscenes/HelloTrigger",
                    "Valley (Main)/Cutscenes/EnableHelloTrigger",
                    "Valley (Main)/Cutscenes/GoodbyeTrigger",
                    "Valley (Main)/Cutscenes/EnableGoodbyeTrigger",
                }}
            };

            public static void DeleteTriggers()
            {
                foreach (var kv in triggerBlacklist) {
                    // GameObject.Find("/" + kv.Key);
                    GameObject root = GlobalGameScene.GetRootObjectByName(kv.Key);

                    foreach (string path in kv.Value) {
                        GameObject.Destroy(root.transform.Find(path).gameObject);
                    }
                }
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
                    "PRINCESS_GOODBYE",

                    "GALLERY_TRAPDOOR_ACTIVE",

                    "UNDEAD_FIRST_ENTRY",
                    "CHALICE_FIRST_ENTRY",
                    "DROWN_FIRST_ENTRY"
                };

                static void Postfix(Save __instance) =>
                    set_flags.Do(flag_name => __instance.SetFlag(flag_name, true));
            }
        }

        static readonly Dictionary<string, (bool Interrupt, bool MakeFast, int[] IndexWhitelist)> cutsceneWhitelists = new() {
            // {"CAVE/Sun Cavern (Main)/Cutscenes/Sage Cutscenes/Sage Unlock Attack Cutscene", (true, false, new int[3]{1, 3, 4})},

            {"CAVE/Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Nurikabe Fall Cutscene", (false, false, new int[4]{2, 4, 5, 6})},

            {"CAVE/Monster Lobby/Cutscenes/PipesRisen", (false, false, new int[5]{3, 5, 6, 7, 8})},
            {"CAVE/Monster Lobby/Cutscenes/Steam", (false, false, new int[1]{3})},

            {"CAVE/Gallery Lobby/Cutscenes/OpenGalleryCutscene", (false, true, new int[3]{0, 3, 4})},

            {"CAVE/Palace Lobby/Cutscenes/FaucetCutscene", (false, true, new int[8]{2, 3, 4, 5, 6, 7, 8, 9})},

            {"LAKE/Lake (Main)/Cutscenes/RaiseSwingsCutscene", (false, false, new int[4]{5, 6, 7, 8})},
            {"LAKE/Lake (Main)/Cutscenes/OpenChurchGateCutscene", (false, false, new int[2]{0, 1})},
            {"LAKE/Lake (Main)/Cutscenes/OpenCryptGateCutscene", (false, false, new int[3]{0, 3, 4})},
            {"LAKE/Lake (Main)/Cutscenes/OpenBedroomDoorCutscene", (false, false, new int[2]{2, 3})},
            {"LAKE/Lake (Main)/Cutscenes/KappaTalkProblem", (false, false, new int[1]{0})},
            {"LAKE/Lake (Main)/Cutscenes/KappaTalkProblemRepeat", (false, false, new int[2]{0, 1})},
            {"LAKE/Lake (Main)/Cutscenes/KappaTalkSuccess", (false, true, new int[5]{11, 12, 14, 15, 16})},

            {"LAKE/Crypt/Cutscenes/PlatformRaiseCutscene", (false, true, new int[2]{1, 2})},

            {"LAKE/Church/Cutscenes/ChurchSuccess", (false, true, new int[2]{0, 5})},

            {"MONSTER/Monster/Rotate (Inside Monster)/Cutscenes/RemoveAllBoilsCutscene", (false, true, new int[3]{0, 5, 6})},

            // {"PALACE/Valley (Main)/Cutscenes/PALACE_MELTED_ICE", (false, true, new int[9]{4, 8, 9, 11, 12, 13, 14, 15, 16})},
            {"PALACE/Valley (Main)/Cutscenes/PALACE_MELTED_ICE", (false, true, new int[7]{8, 11, 12, 13, 14, 15, 16})},
            {"PALACE/Valley (Main)/Cutscenes/PALACE_MORAY_AWAKE", (false, false, new int[0]{})},
            {"PALACE/Valley (Main)/Cutscenes/PALACE_LAKE_GATE_OPEN", (false, false, new int[1]{2})},
            {"PALACE/Valley (Main)/Cutscenes/PALACE_SNOW_CASTLE_GATE_OPEN", (false, false, new int[2]{2, 3})},
            {"PALACE/Valley (Main)/Cutscenes/PALACE_OBSERVATORY_SHORTCUT", (false, false, new int[2]{1, 4})},

            {"PALACE/Dining Room/Cutscenes/ThroneCutscene", (false, false, new int[2]{3, 4})},

            {"PALACE/Sanctum/Cutscenes/RaiseSanctumStopperCutscene", (false, true, new int[2]{1, 2})},
            {"PALACE/Sanctum/Cutscenes/StartSanctumRaceCutscene", (false, true, new int[2]{1, 3})},
            {"PALACE/Sanctum/Cutscenes/StopSanctumRaceCutscene", (false, true, new int[2]{1, 2})},
        };

        static string GetPath(Transform transform) {
            List<string> accum = new();
            var seek = transform;
            while (seek != null) {
                accum.Add(seek.name);
                seek = seek.parent;
            }
            accum.Reverse();
            return String.Join("/", accum);
        }

        static void MakeEventFast(Event ev)
        {
            ev.start = 0;

            if (ev is CutsceneActivationEvent activationEvent) {
                Activation activation = activationEvent.activation;

                if (activation is Raise raise) {
                    raise.raiseTime = 1;
                    raise.GetTimer().SetMax(1);
                } else if (activation is GrowFromNothingActivation grow) {
                    grow.duration = 1;
                    grow.GetTimer().SetMax(1);
                }
            }
        }
        
        public static void PatchCutscene(Cutscene cutscene, bool interrupt, bool makeFast, int[] indexWhitelist)
        {
            cutscene.interrupt = interrupt;

            if (makeFast) {
                cutscene.durationAfterFinal = 1;

                cutscene.GetComponentsInChildren<Event>(true).Do(MakeEventFast);
            }

            void deleteChild(int index) => GameObject.DestroyImmediate(cutscene.transform.GetChild(index).gameObject);
                // Transform child = __instance.transform.GetChild(index);
                // child.parent = null;
                // GameObject.DestroyImmediate(child.gameObject);

            int whitelistCount = indexWhitelist.Length;
            int deletedCount = 0;
            for (int childIndex = 0; cutscene.transform.childCount > whitelistCount; childIndex++) {
                int cursor = childIndex - deletedCount;
                if (cursor == whitelistCount || childIndex != indexWhitelist[cursor]) {
                    deleteChild(cursor);
                    deletedCount++;
                }
            }
            
        }

        [HarmonyPatch(typeof(Cutscene), "Awake")]
        static class Patch {
            static bool Prefix(Cutscene __instance)
            {
                string p = GetPath(__instance.transform);
                if (!cutsceneWhitelists.TryGetValue(p, out var val)) return true;
                PatchCutscene(__instance, val.Interrupt, val.MakeFast, val.IndexWhitelist);

                return true;
            }
        }

        public static void Init()
        {
            CutsceneTriggerBlacklist.DeleteTriggers();
        }
    }
}