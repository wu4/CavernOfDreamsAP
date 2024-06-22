using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.Cutscenes
{
    static class TutorialSkipPatch
    {
        [HarmonyPatch(typeof(TutorialHandler), nameof(TutorialHandler.StartTutorial))]
        static class StartPatch { static bool Prefix() => false; }

        [HarmonyPatch(typeof(TutorialHandler), nameof(TutorialHandler.CompleteTutorial))]
        static class CompletePatch { static bool Prefix() => false; }
        [HarmonyPatch(typeof(TutorialHandler), nameof(TutorialHandler.EndTutorial))]
        static class EndPatch { static bool Prefix() => false; }
    }

    static class Patching
    {
        public static void PatchCutscene(Cutscene cutscene, WLOptions options, params string[] whitelist)
        {
            new WhitelistEntry(options, whitelist).PatchCutscene(cutscene);
        }

        public static void PatchCutscene(string rootName, string cutscenePath, WLOptions options, params string[] whitelist)
        {
            new WhitelistEntry(options, whitelist).PatchCutscene(GameScene.FindInScene(rootName, cutscenePath).GetComponent<Cutscene>());
        }

        public static void PatchCutsceneList(string worldName, Dictionary<string, WhitelistEntry> whitelist) {
            GameObject root = GameScene.GetRootObjectByName(worldName);
            foreach ((string path, WhitelistEntry entry) in whitelist) {
                // Debug.Log(path);
                entry.PatchCutscene(root.transform.Find(path).GetComponent<Cutscene>());
            }
        }

        class PatchCutscenesOnGameLoad : InstantiateOnGameSceneLoad
        {
            public PatchCutscenesOnGameLoad()
            {
                foreach ((string worldName, var whitelist) in cutsceneWhitelists) {
                    PatchCutsceneList(worldName, whitelist);
                }
            }

            static readonly Dictionary<string, Dictionary<string, WhitelistEntry>> cutsceneWhitelists = new() {
                {"CAVE", new() {
                    {"Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Nurikabe Fall Cutscene", new(
                        WLOptions.None, 
                        "NurikabeFallOver",
                        "CreateDustFX",
                        "PlayLandFX",
                        "ScreenShake"
                    )},
                    {"Sun Cavern (Main)/Objects/GratitudeDoor/SuccessCutscene", new(
                        WLOptions.None,
                        "PlayBreakSFX"
                    )},

                    {"Monster Lobby/Cutscenes/PipesRisen", new(
                        WLOptions.None,
                        "Raise Pipe",
                        "Raise Pipe (1)",
                        "Raise Pipe (2)",
                        "Raise Pipe (3)",
                        "Raise Pipe (4)"
                    )},
                    {"Monster Lobby/Cutscenes/Steam", new(
                        WLOptions.None,
                        "AwakenSteam"
                    )},

                    {"Palace Lobby/Cutscenes/FaucetCutscene", new(
                        WLOptions.MakeFast,
                        "EnableFaucetSFX",
                        "TurnOnFaucetStream",
                        "TurnOnFaucetStream (1)",
                        "TurnOnFaucetStream (2)",
                        "TurnOnFaucetStream (3)",
                        "GrowMushroom",
                        "PlayGrowSFX",
                        "GrowMushroom (1)"
                    )},

                    {"Gallery Lobby/Cutscenes/OpenGalleryCutscene", new(
                        WLOptions.None,
                        "OpenHedgeMazeInnerGate",
                        "OpenGateGallery"
                    )},
                    {"Gallery Lobby/Cutscenes/OpenHedgeMazePuzzle", new(
                        WLOptions.None,
                        "LowerHedgeMazeGate"
                    )},
                    {"Gallery Lobby/Cutscenes/OpenHedgeMazeExploit", new(
                        WLOptions.None,
                        "LowerHedgeMazeGate"
                    )},
                    {"Gallery Lobby/HedgeMaze/Objects/DeadRose/GrowDeadRoseCutscene", new(
                        WLOptions.None,
                        "ShrinkRoseWilted",
                        "GrowRoseWatered",
                        "GrowSFX"
                    )},
                }},

                {"LAKE", new() {
                    {"Lake (Main)/Cutscenes/RaiseSwingsCutscene", new(
                        WLOptions.None,
                        "RaiseSwing",
                        "RaiseSwing (1)",
                        "RaiseSwing (2)",
                        "RaiseSwing (3)"
                    )},
                    {"Lake (Main)/Cutscenes/OpenChurchGateCutscene", new(
                        WLOptions.None,
                        "OpenChurchGate"
                    )},
                    {"Lake (Main)/Cutscenes/OpenCryptGateCutscene", new(
                        WLOptions.None,
                        "OpenCryptGate",
                        "PlayCryptGateSFX"
                    )},
                    {"Lake (Main)/Cutscenes/OpenBedroomDoorCutscene", new(
                        WLOptions.None,
                        "OpenTreehouseDoor",
                        "PlayCreakHingeSFX"
                    )},
                    {"Lake (Main)/Cutscenes/KappaTalkProblem", new(
                        WLOptions.None,
                        "KappaTalk1"
                    )},
                    {"Lake (Main)/Cutscenes/KappaTalkProblemRepeat", new(
                        WLOptions.None,
                        "KappaTalk"
                    )},
                    {"Lake (Main)/Cutscenes/KappaTalkSuccess", new(
                        WLOptions.MakeFast,
                        "OpenDeepWoodsGate",
                        "OpenDeepWoodsGateSFX",
                        "OldKoiDisappear",
                        "NewKoiAppear",
                        "KappaFeed"
                    )},

                    {"Crypt/Cutscenes/PlatformRaiseCutscene", new(
                        WLOptions.MakeFast,
                        "RaisePlatform",
                        "RaisePlatformSFX"
                    )},

                    {"Church/Cutscenes/ChurchSuccess", new(
                        WLOptions.None,
                        "SlideTile"
                    )},
                }},

                // {"LAKE/Church/Cutscenes/ChurchSuccess", (false, true, new int[2]{0, 5})},

                {"PALACE", new() {
                    // {"Valley (Main)/Cutscenes/PALACE_MELTED_ICE", new(WLOptions.MakeFast)},
                    {"Valley (Main)/Cutscenes/PALACE_MORAY_AWAKE", new(WLOptions.MakeFast)},
                    {"Valley (Main)/Cutscenes/PALACE_LAKE_GATE_OPEN", new(
                        WLOptions.None,
                        "LakeFenceOpen"
                    )},
                    {"Valley (Main)/Cutscenes/PALACE_GATE_RISEN", new(
                        WLOptions.None,
                        "OpenPalaceGate"
                    )},
                    {"Valley (Main)/Cutscenes/PALACE_ABYSS_HOOP_SUCCESS", new(
                        WLOptions.None,
                        "RaiseBasementDoor"
                    )},
                    {"Valley (Main)/Cutscenes/PALACE_SNOW_CASTLE_GATE_OPEN", new(
                        WLOptions.None,
                        "SnowCastleGateOpen",
                        "SnowCastleGateOpenSFX"
                    )},
                    {"Valley (Main)/Cutscenes/PALACE_OBSERVATORY_SHORTCUT", new(
                        WLOptions.None,
                        "RaiseDoor1",
                        "RaiseDoor2"
                    )},

                    {"Palace/Cutscenes/PALACE_SENTRIES_DISABLED", new(
                        WLOptions.MakeFast,
                        "DisableSentry",
                        "DisableSentry (1)",
                        "DisableSentry (2)",
                        "DisableSentry (3)",
                        "DisableSentry (4)",
                        "DisableSentry (5)",
                        "DisableSentry (6)",
                        "DisableSentry (7)",
                        "DisableSentry (8)",
                        "DisableSentry (9)"
                    )},
                    // TODO: Detangle Seestars from tiles
                    {"Palace/Cutscenes/TilesSuccessCutscene", new(
                        WLOptions.None,
                        // "FadeOutSeeStarsClosed",
                        // "AwakenSeeStarsOpen",
                        "RaiseDoor"
                    )},

                    {"Dining Room/Cutscenes/ThroneCutscene", new(
                        WLOptions.None,
                        "ScreenShake",
                        "RaiseThroneTrick"
                    )},

                    {"Sanctum/Cutscenes/RaiseSanctumStopperCutscene", new(
                        WLOptions.None,
                        "ScreenShake",
                        "RaiseStopper"
                    )},
                    {"Sanctum/Cutscenes/StartSanctumRaceCutscene", new(
                        WLOptions.MakeFast,
                        "CloseGate",
                        "OpenGate"
                    )},
                    {"Sanctum/Cutscenes/StopSanctumRaceCutscene", new(
                        WLOptions.MakeFast,
                        "CloseFellaGate",
                        "OpenGate (1)",
                        "OpenGate (2)"
                    )},
                }},
                // {"PALACE/Valley (Main)/Cutscenes/PALACE_MELTED_ICE", (false, true, new int[7]{8, 11, 12, 13, 14, 15, 16})},

                {"GALLERY", new() {
                    {"Earth Lobby/Cutscenes/OpenMonsterShortcut", new(
                        WLOptions.None,
                        "RaiseMonsterBars",
                        "PlayRaiseSFX"
                    )},
                    {"Earth Lobby/Cutscenes/StatuePuzzleSuccess", new(
                        WLOptions.None,
                        "RaiseCastleGate"
                    )},
                    {"Earth Lobby/Cutscenes/KappaPaintingSuccess", new(
                        WLOptions.None,
                        "FadeOutArtworkUnfinished",
                        "DestroyDragonSkullGem"
                    )},

                    {"Foyer (Main)/Cutscenes/SagePaintingSuccess", new(
                        WLOptions.None,
                        "OpenEarthLobby",
                        "OpenFireLobby"
                    )},

                    {"Fire Lobby/Cutscenes/MonsterPaintingSuccessCutscene", new(
                        WLOptions.None,
                        "FryingPansExtend",
                        "RaiseSFX"
                    )},

                    {"Water Lobby/Cutscenes/FinishPrincessPainting", new(
                        WLOptions.None,
                        "FadeOutArtworkUnfinished",
                        "OpenChest"
                    )},

                    {"Water Lobby/Cutscenes/AngelStatueShadowsSuccess", new(
                        WLOptions.None,
                        "OpenChest"
                    )},

                    {"Water Lobby/Cutscenes/HealGiant", new(
                        WLOptions.None,
                        "HealGiantManual",
                        "DestroyGiantBlockade"
                    )},
                }},
            };
        }
    }
}
