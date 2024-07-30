using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago.MiscPatches
{
    static class DialogPatches
    {
        static readonly Dictionary<string, string> staticDialogPatches = new() {
            {"/LAKE/Lake (Main)/Cutscenes/KappaTalkProblemRepeat/KappaTalk", "O-oh dear, Fynn... I was so excited when you gave me the fish food that I lost it. Could you please get more?"},
        };

        class ResetDialogPatches : InstantiateOnGameSceneLoad
        {
            [LoadOrder(int.MinValue)]
            public ResetDialogPatches()
            {
                dynamicDialogPatches.Clear();
            }
        }

        static readonly Dictionary<string, Func<Dialog, string>> dynamicDialogPatches = new();
        public static void RegisterDynamicDialogPatch(string path, Func<Dialog, string> func) =>
            dynamicDialogPatches.Add(path, func);

        public static string TryGetPatchText(Dialog dialog) {
            string path = dialog.transform.GetPath();
            if (staticDialogPatches.TryGetValue(path, out string newDialog)) {
                return newDialog;
            } else if (dynamicDialogPatches.TryGetValue(path, out var func)) {
                return func(dialog);
            }

            Debug.Log("=============================");
            Debug.Log("Dialog path: " + path);

            return null;
        }

        [HarmonyPatch(typeof(UIController), nameof(UIController.StartDialog))]
        static class DynamicDialogPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    false,
                    new(OpCodes.Ldarg_1),
                    Calls<Dialog>(nameof(Dialog.GetText)),
                    new(OpCodes.Stloc_0)
                );
                matcher.ThrowIfInvalid("Dynamic dialog, GetText");

                matcher.CreateLabelAt(matcher.Pos + 2, out Label store);

                matcher.Insert(
                    new(OpCodes.Ldarg_1), CodeInstruction.Call(typeof(DialogPatches), nameof(DialogPatches.TryGetPatchText)), new(OpCodes.Dup), new(OpCodes.Brtrue, store),
                    new(OpCodes.Pop)
                );

                return matcher.InstructionEnumeration();
            }
        }

        // static void Init()
        // {
        //     foreach (var kv in staticDialogPatches) {
        //         var root = GameScene.GetRootObjectByName(kv.Key);
        //         foreach (var kv2 in kv.Value) {
        //             root.transform.Find(kv2.Key).GetComponent<Dialog>().text.PatchText(kv2.Value);
        //         }
        //     }
        // }
    }
}
