using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    static partial class Skills
    {
        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class SuperBouncePatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var parser = new CodeInstructionParser(instructions, generator);

                int instructionIndex =
                    parser.FirstInstanceCodeIndex(o =>
                        parser[o.Index + 1].LoadsField(PlayerFields.rollJumpTimer)
                        && parser[o.Index + 5].Calls(PlayerMethods.IsGrounded)
                        && parser[o.Index + 8].LoadsField(PlayerFields.groundedLastFrame)
                    );

                int innerBlockIndex =
                    parser.FirstInstanceCodeIndex(o =>
                        o.Index > instructionIndex
                        && parser[o.Index + 1].Calls(PlayerMethods.IsAttacking)
                    );

                Label leave = (Label)
                    parser
                    .AssertGetCode(innerBlockIndex + 2,
                                    (code) => code.opcode == OpCodes.Brfalse, "Super Bounce Patch")
                    .operand;
                
                int injectionIndex =
                    parser.FirstCodeIndex(
                        o => o.Index > innerBlockIndex
                        && o.Item.opcode == OpCodes.Ldloc_S
                    );

                var loadFacingDirectionLoc = parser[injectionIndex].operand;

                Label performFastRoll = parser.generator.DefineLabel();
                parser.AddLabel(injectionIndex, performFastRoll);

                var checkSuperBounceCodes = new CodeInstruction[] {
                    new(OpCodes.Ldstr, "SUPERBOUNCE"),            CodeInstruction.Call(typeof(Skills), nameof(HasSkill)), new(OpCodes.Brtrue_S, performFastRoll),
                    new(OpCodes.Ldarg_0),                         CodeInstruction.Call(typeof(Player), "IsRollHeld"),     new(OpCodes.Brtrue_S, performFastRoll),
                    new(OpCodes.Ldloc_S, loadFacingDirectionLoc), CodeInstruction.LoadField(typeof(Vector3), "y"),        new(OpCodes.Ldc_R4, 0.05f), new(OpCodes.Blt_S, performFastRoll),

                    new(OpCodes.Ldarg_0), CodeInstruction.LoadField(typeof(Player), "attackTimer"), new(OpCodes.Callvirt, MiscMethods.timerEnd),
                    new(OpCodes.Ldarg_0), CodeInstruction.LoadField(typeof(Player), "whimperSFX"), new(OpCodes.Callvirt, MiscMethods.audioGroupPlay),
                    new(OpCodes.Pop),
                    new(OpCodes.Br_S, leave)
                };

                parser.codes.InsertRange(injectionIndex, checkSuperBounceCodes);

                return parser.codes.AsEnumerable();
            }
        }
    }
}