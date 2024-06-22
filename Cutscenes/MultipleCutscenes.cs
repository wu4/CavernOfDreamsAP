using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using UnityEngine.InputSystem;
using System.Reflection.Emit;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago.Cutscenes
{
    static class MultipleCutscenes
    {
        static readonly List<Cutscene> activeCutscenes = new();

        public static void Add(Cutscene cutscene)
        {
            cutscene.SetFlag();
            Debug.Log("Add cutscene: " + cutscene.gameObject.name, cutscene.gameObject);

            if (GlobalHub.Instance.GetArea().ContainsComponentInChildren(cutscene)) {
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

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    false,
                    new(OpCodes.Ldarg_0),
                    Calls<GlobalHub>("CutsceneActive")
                );

                matcher.Insert(
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(UpdatePatch), nameof(UpdatePatch.UpdateCutscenes))
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
