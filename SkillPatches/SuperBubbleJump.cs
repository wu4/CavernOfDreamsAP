using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago.SkillPatches
{
    static class SuperBubbleJump
    {
        [HarmonyPatch(typeof(Player), "Shoot")]
        static class Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    true,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("InFirstPersonMode"),
                    new(OpCodes.Brtrue)
                );

                matcher.Insert(
                    CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.superBubbleJump)),
                    new(OpCodes.Not),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsRolling"),
                    new(OpCodes.And),
                    new(OpCodes.Or)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
