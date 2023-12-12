using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace CoDArchipelago
{
    static partial class Skills
    {
        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class HighJumpPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var parser = new CodeInstructionParser(instructions, generator);

                int instructionIndex =
                    parser.FirstInstanceCodeIndex(o =>
                        parser[o.Index + 4].LoadsField(PlayerFields.highJumpAfterLandingDuration)
                    );

                Label failure = (Label)
                    parser
                    .AssertGetCode(instructionIndex + 6,
                                    (code) => code.opcode == OpCodes.Bgt_Un, "High Jump Patch")
                    .operand;

                var codesToInject = new CodeInstruction[] {
                    new(OpCodes.Ldstr, "HIGHJUMP"), CodeInstruction.Call(typeof(Skills), "HasSkill"), new(OpCodes.Brfalse_S, failure),
                };

                parser.codes.InsertRange(instructionIndex + 7, codesToInject);

                return parser.codes.AsEnumerable();
            }
        }
    }
}