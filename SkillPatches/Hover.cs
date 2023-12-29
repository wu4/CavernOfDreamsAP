using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static CoDArchipelago.CodeMatchHelpers;

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
                    new(OpCodes.Ldarg_0), CodeInstruction.LoadField(typeof(Player), "carryingParaglider"), new(OpCodes.Brtrue, skipHasHover)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}