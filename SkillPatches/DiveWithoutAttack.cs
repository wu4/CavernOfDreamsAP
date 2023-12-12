using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CoDArchipelago
{
    static partial class Skills
    {
        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class DiveWithoutAttackPatch
        {
            static readonly MethodInfo tutorialHandlerGetInstance = AccessTools.Method(typeof(TutorialHandler), "get_Instance");

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var parser = new CodeInstructionParser(instructions, generator);

                int instructionIndex =
                    parser.FirstInstanceCodeIndex(o =>
                        parser[o.Index + 1].Calls(PlayerMethods.IsActionHeld)
                        && parser[o.Index + 4].LoadsField(PlayerFields.attackTimer)
                    );

                Label forceDiveWithoutAttack = parser.generator.DefineLabel();
                parser.AssertAddLabel(instructionIndex + 20, forceDiveWithoutAttack,
                                        (code) => code.Calls(tutorialHandlerGetInstance), "Dive Patch 1");

                Label vanillaChecks = parser.generator.DefineLabel();
                parser.AssertAddLabel(instructionIndex + 3, vanillaChecks,
                                        (code) => code.opcode == OpCodes.Ldarg_0, "Dive Patch 2");

                var checkDiveWithoutAttackCodes = new CodeInstruction[] {
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "CanAttack"),       new(OpCodes.Brtrue_S,  vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsPaddling"),      new(OpCodes.Brtrue_S,  vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsActionPressed"), new(OpCodes.Brfalse_S, vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsCarrying"),      new(OpCodes.Brtrue_S,  vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "CanDive"),         new(OpCodes.Brtrue_S,  forceDiveWithoutAttack),
                };

                parser.codes.InsertRange(instructionIndex + 3, checkDiveWithoutAttackCodes);

                return parser.codes.AsEnumerable();
            }
        }
    }
}