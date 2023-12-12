using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoDArchipelago
{
    [HasInitMethod]
    static class Cutscenes
    {
        static class MonsterBoilsPatch
        {
            [HarmonyPatch(typeof(MonsterBoilListener), "RemoveBoil")]
            static class Patch
            {
                static bool Prefix(MonsterBoilListener __instance, ref int ___numBoils)
                {
                    ___numBoils--;
                    if (___numBoils == 0) {
                        StockSFX.Instance.jingleGood.Play();
                        GlobalHub.Instance.save.SetFlag("LOCATION_MONSTER_BOILS_REMOVED", true);
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }

                    return false;
                }
            }
        }

        public static class MultipleCutscenes
        {
            static readonly List<Cutscene> activeCutscenes = new();

            public static string GetPath(Transform current) {
                if (current.parent == null)
                    return "/" + current.name;
                return GetPath(current.parent) + "/" + current.name;
            }

            public static void AddCutscene(Cutscene cutscene)
            {
                cutscene.SetFlag();
                Debug.Log("Add cutscene: " + cutscene.gameObject.name, cutscene.gameObject);

                if (GlobalGameScene.GetCurrentArea().ContainsComponent(cutscene)) {
                    activeCutscenes.Add(cutscene);
                    cutscene.Reset(true, false);
                }
                // gh.cutsceneRemoveControlTimer.Reset();
                // if (cs.delayListen <= 0)
                //     return;
                // this.delayListenTimer.Reset((float) cs.delayListen);
            }
            [HarmonyPatch(typeof(GlobalHub), "Awake")]
            static class InitPatch
            {
                static void Postfix()
                {
                    activeCutscenes.Clear();
                }
            }

            [HarmonyPatch(typeof(GlobalHub), "Update")]
            static class UpdatePatch
            {
                static readonly AccessTools.FieldRef<GlobalHub, InputAction> inputActionAcc = AccessTools.FieldRefAccess<GlobalHub, InputAction>("inputAction");

                static void UpdateCutscenes(GlobalHub gh)
                {
                    InputAction inputAction = inputActionAcc(gh);
                    bool actionPressed = inputAction.WasPressedThisFrame();

                    activeCutscenes.RemoveAll(cutscene => {
                        cutscene.UpdateCutscene();

                        if (actionPressed) {
                            cutscene.Input();
                        }

                        if (cutscene.Finished()) {
                            cutscene.Finish();
                            return true;
                        } else {
                            return false;
                        }
                    });
                }

                static class GlobalHubMethods
                {
                    public static readonly MethodInfo CutsceneActive = AccessTools.Method(typeof(GlobalHub), "CutsceneActive");
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    var parser = new CodeInstructionParser(instructions, generator);

                    int instructionIndex =
                        parser.FirstInstanceCodeIndex(o =>
                            parser[o.Index + 1].Calls(GlobalHubMethods.CutsceneActive)
                        );

                    var codesToInject = new CodeInstruction[] {
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(UpdatePatch), "UpdateCutscenes")
                    };

                    parser.codes.InsertRange(instructionIndex, codesToInject);

                    return parser.codes.AsEnumerable();
                }
            }
        }


        public static readonly Dictionary<string, Cutscene> locationCutscenes = new(){};

        static bool isCollectingCutscene = false;
        static void CollectCutscene(Cutscene cs)
        {
            isCollectingCutscene = true;
            GlobalHub.Instance.SetCutscene(cs);
            isCollectingCutscene = false;
        }

        public static bool TryCollect(string flag)
        {
            if (locationCutscenes.TryGetValue(flag, out Cutscene cutscene)) {
                CollectCutscene(cutscene);
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(GlobalHub), nameof(GlobalHub.SetCutscene))]
        static class CutscenePatch
        {
            static bool Prefix(Cutscene cs)
            {
                string flag;
                if (cs.name == "PALACE_MELTED_ICE") {
                    flag = "PALACE_MELTED_ICE";
                } else {
                    flag = cs.flag;
                }

                if (!locationCutscenes.ContainsKey(flag)) return true;

                if (!isCollectingCutscene) {
                    GlobalHub.Instance.save.SetFlag("LOCATION_" + flag, true);
                } else {
                    MultipleCutscenes.AddCutscene(cs);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(WhackForCutscene), "HandleWhack")]
        static class WhackForCutscenePatch
        {
            static bool Prefix(WhackForCutscene __instance, Whackable.WhackType type, GameObject source, ref bool ___whacked, ref bool __result)
            {

                if (Data.eventItems.ContainsKey(__instance.cutscene.flag)) {
                    if (GlobalHub.Instance.save.GetFlag("LOCATION_" + __instance.cutscene.flag).On()) {
                        __result = false;
                        return false;
                    }

                    ___whacked = true;
                    GlobalHub.Instance.SetCutscene(__instance.cutscene);

                    __result = true;
                    return false;
                }

                return true;
            }
        }

        public static void Init()
        {
            locationCutscenes.Clear();

            var cutscenes = GlobalGameScene.GetComponentsInChildren<Cutscene>(true).ToList();
            var switches = GlobalGameScene.GetComponentsInChildren<TimedSwitch>(true).ToList();
            var switches_2 = GlobalGameScene.GetComponentsInChildren<GenericCutsceneSwitch>(true).ToList();
            foreach (var flagName in Data.eventItems.Keys) {
                // Debug.Log(flagName);
                {
                    int switchInd = switches.FindIndex(x => x.flag == flagName);
                    if (switchInd >= 0) {
                        TimedSwitch ts = switches[switchInd];
                        ts.flag = "LOCATION_" + ts.flag;
                        // ts.SetState();
                    }
                }

                {
                    int switchInd = switches_2.FindIndex(x => x.flag == flagName);
                    if (switchInd >= 0) {
                        GenericCutsceneSwitch ts = switches_2[switchInd];
                        ts.flag = "LOCATION_" + ts.flag;
                        locationCutscenes.Add(flagName, ts.cutscene);
                        // ts.SetState();
                        continue;
                    }
                }

                {
                    int cutsceneInd = cutscenes.FindIndex(
                        x => (flagName == "PALACE_MELTED_ICE" ? x.name : x.flag) == flagName
                    );
                    if (cutsceneInd >= 0) {
                        locationCutscenes.Add(flagName, cutscenes[cutsceneInd]);
                        continue;
                    }
                }

                Debug.LogWarning(String.Format("Failed to find flag for {0}", flagName));
            }
        }
    }
}