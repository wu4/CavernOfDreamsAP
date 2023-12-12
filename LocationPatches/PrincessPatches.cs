using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    [HasInitMethod]
    static class PrincessPatches
    {
        static void PatchMeltedIceFlags(CodeInstructionParser parser)
        {
            foreach (var code in parser.byIndex) {
                if (code.Item.opcode != OpCodes.Ldstr) continue;
                if ((string)code.Item.operand != "PALACE_MELTED_ICE") continue;
                code.Item.operand = "LOCATION_PALACE_MELTED_ICE";
            }
        }

        static IEnumerable<CodeInstruction> PatchMeltedIceTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var parser = new CodeInstructionParser(instructions, generator);

            PatchMeltedIceFlags(parser);

            return parser.codes.AsEnumerable();
        }

        static MethodInfo[] patchTargets = new MethodInfo[] {
            AccessTools.Method(typeof(Princess), nameof(Princess.SetDefaultAnimation)),
            AccessTools.Method(typeof(Princess), nameof(Princess.Interact)),
            AccessTools.Method(typeof(Princess), "HandleWhack"),
        };

        public static void Patch(Harmony harmony)
        {
            var transpiler = new HarmonyMethod(AccessTools.Method(typeof(PrincessPatches), nameof(PatchMeltedIceTranspiler)));
            foreach(MethodInfo method in patchTargets) {
                harmony.Patch(method, transpiler: transpiler);
            }
        }

        public static void Init()
        {
            var root = GlobalGameScene.FindInScene("PALACE", "Valley (Main)/Cutscenes");
            foreach (int i in Enumerable.Range(1, 3)) {
                root.Find("PrincessGiveItem" + i + "/ItemToToss").GetComponent<TwoStateExists>().flag = "ITEM_PRINCESS_" + i + "_GIVEN";
            }
        }
    }
}