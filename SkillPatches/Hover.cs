using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago.SkillPatches
{
    static class Hover
    {
        [HarmonyPatch(typeof(Player), "HasHover")]
        static class Patch
        {
            static bool Prefix(ref bool __result)
            {
                __result = FlagCache.CachedSkillFlags.hover;
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "CanHover")]
        static class HoverWithHangGliderPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    false,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("HasHover")
                );

                matcher.CreateLabelAt(matcher.Pos + 3, out Label skipHasHover);

                matcher.Insert(
                    new(OpCodes.Ldarg_0), LoadField<Player>("carryingParaglider"), new(OpCodes.Brtrue, skipHasHover)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
