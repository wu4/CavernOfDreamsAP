using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago.SkillPatches
{
    public static class Sprint
    {
        static readonly float maxSprintMomentum = 0.5f;

        // NOTE: hardcoded value taken from Player.AboveMomentumFloor()
        static readonly float minSprintMomentum = 0.10000000149011612f;

        public static bool IsSprintInput(Player player)
        {
            return FlagCache.CachedSkillFlags.sprint
                && !FlagCache.CachedSkillFlags.roll
                && player.IsGrounded()
                && !player.IsSitting()
                && PlayerAccess.Fields.isMoveInput.Get(player)
                && player.IsRollHeld();
        }

        static void UpdateSprint(Player player)
        {
            if (!IsSprintInput(player)) return;

            float momentum = PlayerAccess.Fields.momentum.Get(player);
            if (momentum >= maxSprintMomentum) return;

            PlayerAccess.Fields.momentum.Set(player, Mathf.Clamp(momentum + (Time.deltaTime * 0.3f), minSprintMomentum, maxSprintMomentum));
        }


        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    false,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("IsClimbing")
                );

                matcher.Insert(
                    new(OpCodes.Ldarg_0), Call(typeof(Sprint), nameof(UpdateSprint))
                );

                matcher.MatchForward(
                    false,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("IsMucky")
                );

                CodeInstruction leaveOp = matcher.Clone().MatchForward(
                    false,
                    new CodeMatch(OpCodes.Br)
                ).Instruction;

                matcher.CreateLabel(out Label goNextBlock);

                matcher.Insert(
                    new(OpCodes.Ldarg_0),         Call<Player>("CarryingTorpedoWhileUnderwater"), new(OpCodes.Brtrue_S, goNextBlock),
                    // new(OpCodes.Ldsfld, sprintInfo), new(OpCodes.Brtrue_S, goNextBlock),
                    LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.sprint)), new(OpCodes.Brtrue_S, goNextBlock),
                    // new(OpCodes.Ldstr, "SPRINT"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)), new(OpCodes.Brtrue_S, goNextBlock),

                    new(OpCodes.Ldarg_0), new(OpCodes.Ldc_R4, 0f), StoreField<Player>("momentum"),

                    leaveOp
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
