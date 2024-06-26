using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago.SkillPatches
{
    static class SuperBounce
    {
        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    false,
                    new(OpCodes.Ldarg_0), LoadsField<Player>("rollJumpTimer"), Calls<Timer>(nameof(Timer.Active)), new(OpCodes.Brtrue),
                    new(OpCodes.Ldarg_0),      Calls<Player>("IsGrounded"), new(OpCodes.Brfalse),
                    new(OpCodes.Ldarg_0), LoadsField<Player>("groundedLastFrame")
                );

                matcher.MatchForward(
                    true,
                    new(OpCodes.Ldarg_0), Calls<Player>("IsAttacking"), new(OpCodes.Brfalse)
                );

                Label leave = (Label)matcher.Instruction.operand;

                matcher.MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldloc_S)
                );

                var loadFacingDirectionLoc = matcher.Instruction.operand;

                matcher.CreateLabel(out Label performFastRoll);

                matcher.Insert(
                    // new(OpCodes.Ldstr, "SUPERBOUNCE"),            CodeInstruction.Call(typeof(Skills), nameof(HasSkill)), new(OpCodes.Brtrue_S, performFastRoll),
                    LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.superBounce)), new(OpCodes.Brtrue_S, performFastRoll),
                    new(OpCodes.Ldarg_0),                         Call<Player>("IsRollHeld"),      new(OpCodes.Brtrue_S, performFastRoll),
                    new(OpCodes.Ldloc_S, loadFacingDirectionLoc), LoadField<Vector3>("y"),         new(OpCodes.Ldc_R4, 0.05f), new(OpCodes.Blt_S, performFastRoll),

                    new(OpCodes.Ldarg_0), LoadField<Player>("attackTimer"), Call<Timer>(nameof(Timer.End)),
                    new(OpCodes.Ldarg_0), LoadField<Player>("whimperSFX"),  Call<AudioGroup>(nameof(AudioGroup.Play)),
                    new(OpCodes.Pop),
                    new(OpCodes.Br_S, leave)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
