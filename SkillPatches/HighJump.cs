using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace CoDArchipelago.SkillPatches
{
    static class HighJump
    {
        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);
                
                matcher.MatchForward(
                    true,
                    new(OpCodes.Ldarg_0),
                    CodeMatchHelpers.LoadsField<Player>("landTimer"),
                    CodeMatchHelpers.Calls<Timer>("GetCounter"),
                    new(OpCodes.Ldarg_0),
                    CodeMatchHelpers.LoadsField<Player>("highJumpAfterLandingDuration"),
                    new(OpCodes.Conv_R4),
                    new(OpCodes.Bgt_Un)
                );
                
                Label failure = (Label)matcher.Instruction.operand;

                matcher.Advance(1);

                matcher.Insert(
                    //new(OpCodes.Ldstr, "HIGHJUMP"), CodeInstruction.Call(typeof(Skills), "HasSkill"), new(OpCodes.Brfalse_S, failure),
                    CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.highJump)), new(OpCodes.Brfalse_S, failure)
                );
                
                return matcher.InstructionEnumeration();
                /*
                var parser = new CodeInstructionParser(instructions, generator);
                
                var a = typeof(HighJumpSkill);

                int instructionIndex =
                    parser.FirstCodeIndex(
                        o => o.opcode == OpCodes.Ldarg_0,
                        o => o.LoadsField<Player>("landTimer"),
                        o => o.Calls<Timer>("GetCounter"),
                        o => o.opcode == OpCodes.Ldarg_0,
                        o => o.LoadsField<Player>("highJumpAfterLandingDuration"),
                        o => o.opcode == OpCodes.Conv_R4,
                        o => o.opcode == OpCodes.Bgt_Un
                    );

                Label failure = (Label) parser[instructionIndex + 6].operand;

                var codesToInject = new CodeInstruction[] {
                    //new(OpCodes.Ldstr, "HIGHJUMP"), CodeInstruction.Call(typeof(Skills), "HasSkill"), new(OpCodes.Brfalse_S, failure),
                    CodeInstruction.LoadField(typeof(FlagCache.Skills), nameof(FlagCache.Skills.highJump)), new(OpCodes.Brfalse_S, failure),
                };

                parser.codes.InsertRange(instructionIndex + 7, codesToInject);

                return parser.codes.AsEnumerable();
                */
            }
        }
    }
}