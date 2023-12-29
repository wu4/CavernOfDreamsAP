using System.Collections.Generic;
using CoDArchipelago;
using CoDArchipelago.GlobalGameScene;
using HarmonyLib;
using UnityEngine;
using CoDArchipelago.Cutscenes;

namespace CoDArchipelago
{
    [HarmonyPatch(typeof(MonsterBoilListener), "RemoveBoil")]
    static class MonsterBoilsPatch
    {
        static bool Prefix(MonsterBoilListener __instance, ref int ___numBoils)
        {
            ___numBoils--;
            if (___numBoils == 0) {
                StockSFX.Instance.jingleGood.Play();
                GlobalHub.Instance.save.SetFlag("LOCATION_MONSTER_BOILS_REMOVED", true);
                GameObject.Destroy(__instance.gameObject);
            }

            return false;
        }
    }
    
    class MonsterPatches : InstantiateOnGameSceneLoad
    {
        static readonly string[] cutsceneTriggerPaths = new[] {
            "Sky (Main)/Cutscenes/GoodbyeTrigger",
            "Sky (Main)/Cutscenes/EnableGoodbyeTrigger",
            "Sky (Main)/Cutscenes/HelloTrigger",
            "Sky (Main)/Cutscenes/EnableHelloTrigger",
            "Sky (Main)/Cutscenes/MonsterEyeOpenCSTRigger", // not a typo
        };
        
        static readonly Dictionary<string, WhitelistEntry> whitelists = new() {
            {"Monster/Rotate (Inside Monster)/Cutscenes/RemoveAllBoilsCutscene", new(
                WLOptions.MakeFast,
                "OpenControlGrate",
                "PlayCreakHingeSlowSFX"
            )},
            {"Monster/Rotate (Inside Monster)/Cutscenes/OpenTailGateCutscene", new(
                WLOptions.None,
                "OpenTailGate",
                "PlayCreakHingeSFX"
            )},
            {"Monster/Rotate (Inside Monster)/Cutscenes/HeartGateOpenCutscene", new(
                WLOptions.MakeFast,
                "OpenHeartGate"
            )},
            {"Monster/Rotate (Inside Monster)/Cutscenes/HeartGateCloseCutscene", new(
                WLOptions.MakeFast,
                "PlayFailureSFX",
                "CloseHeartGate"
            )},
            {"Monster/Rotate (Inside Monster)/Cutscenes/OpenTestChubesCutscene", new(
                WLOptions.None,
                "OpenTestChubeDumpling",
                "DumplingChangeTint",
                "OpenTestChube",
                "OpenTestChube (1)",
                "OpenTestChube (3)", // not a typo, (2) does not exist
                "RemoveMonster",
                "RemoveMonster (1)",
                "AddMonsterFreed",
                "AddMonsterFreed (1)",
                "AddMonsterFreed (2)",
                "RemoveDumpling"
            )},
            {"Monster/NoRotate (Inside Monster)/Cutscenes/RotateMonsterLeft", new(
                WLOptions.None,
                "MoveChair",
                "MoveCrate",
                "ChangeCrateColor"
            )},
            {"Monster/NoRotate (Inside Monster)/Cutscenes/RotateMonsterMiddle", new(WLOptions.None)},
            {"Monster/NoRotate (Inside Monster)/Cutscenes/RotateMonsterRight", new(
                WLOptions.None,
                "MoveChair",
                "MoveCrate",
                "ChangeCrateColor"
            )},
            
            {"Heart/Cutscenes/HeartRoomSuccessCutscene", new(
                WLOptions.None,
                "StartLightning",
                "StartLightning (1)",
                "StartLightning (2)",
                "StartLightning (3)",
                "ChangeHeartColor",
                "TossFella7",
                "TossFella7Event",
                "PlayFallSFX",
                "ScreenShake (1)",
                "ResetFog"
            )}
        };

        public MonsterPatches()
        {
            RemoveCutsceneTriggers();
            RemoveMonsterEyeFlag();
            Patching.PatchCutsceneList("MONSTER", whitelists);
        }
        
        static void RemoveCutsceneTriggers()
        {
            Transform root = GameScene.GetRootObjectByName("MONSTER").transform;
            foreach (string triggerPath in cutsceneTriggerPaths) {
                GameObject.Destroy(root.Find(triggerPath).gameObject);
            }
        }
        
        /// <summary>
        /// Sets Mr. Kerrington's eye flag to something that is always set,
        /// effectively making it always open
        /// </summary>
        static void RemoveMonsterEyeFlag()
        {
            GameScene.FindInScene("MONSTER", "Sky (Main)/Monster/Objects/MonsterEye").GetComponent<MonsterEye>().flag =
                "CAVE_FIRST_ENTRY";
        }
    }
}