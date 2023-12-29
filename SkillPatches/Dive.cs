using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static CoDArchipelago.CodeMatchHelpers;

namespace CoDArchipelago.SkillPatches
{
    static class Dive
    {
        [HarmonyPatch(typeof(Player), "HasDive")]
        static class Patch
        {
            static bool Prefix(ref bool __result)
            {
                __result = FlagCache.CachedSkillFlags.dive;
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class DiveWithoutAttackPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);
                
                matcher.MatchForward(
                    true,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("IsActionHeld"),
                    new(OpCodes.Brfalse),
                    // injection point
                    new(OpCodes.Ldarg_0)
                );

                matcher.CreateLabel(out Label vanillaChecks);

                matcher.CreateLabelAt(
                    matcher.Clone().MatchForward(
                        false,
                        Calls<TutorialHandler>("get_Instance")
                    ).Pos,
                    out Label forceDiveWithoutAttack
                );

                matcher.Insert(
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "CanAttack"),       new(OpCodes.Brtrue_S,  vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsPaddling"),      new(OpCodes.Brtrue_S,  vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsActionPressed"), new(OpCodes.Brfalse_S, vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsCarrying"),      new(OpCodes.Brtrue_S,  vanillaChecks),
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "CanDive"),         new(OpCodes.Brtrue_S,  forceDiveWithoutAttack)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}