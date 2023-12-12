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
        static class SprintPatch
        {
            static readonly float maxSprintMomentum = 0.5f;

            // taken from Player.AboveMomentumFloor()
            static readonly float minSprintMomentum = 0.10000000149011612f;

            static bool IsSprinting(Player player)
            {
                return HasSkill("SPRINT")
                    && !HasSkill("ROLL")
                    && player.IsGrounded()
                    && !(bool)PlayerMethods.IsSitting.Invoke(player, new object[] {})
                    && (bool)PlayerFields.isMoveInput.GetValue(player)
                    && (bool)PlayerMethods.IsRollHeld.Invoke(player, new object[] {});
            }

            static void UpdateSprint(Player player)
            {
                if (!IsSprinting(player)) return;

                float momentum = (float)PlayerFields.momentum.GetValue(player);
                if (momentum >= maxSprintMomentum) return;

                PlayerFields.momentum.SetValue(player, Mathf.Clamp(momentum + (Time.deltaTime * 0.3f), minSprintMomentum, maxSprintMomentum));
            }

            // keep momentum at zero if no sprint
            static void PatchMomentum(CodeInstructionParser parser)
            {
                int instructionIndex =
                    parser.FirstInstanceCodeIndex(o =>
                        parser[o.Index + 1].Calls(PlayerMethods.IsMucky)
                    );

                Label goNextBlock = parser.generator.DefineLabel();
                parser.AddLabel(instructionIndex, goNextBlock);

                CodeInstruction leaveOp =
                    parser.FirstCode(o =>
                        o.Index > instructionIndex
                        && o.Item.opcode == OpCodes.Br
                    )
                    .Item;

                var checkSprintMovementCodes = new CodeInstruction[] {
                    new(OpCodes.Ldarg_0),         CodeInstruction.Call(typeof(Player), "CarryingTorpedoWhileUnderwater"), new(OpCodes.Brtrue_S, goNextBlock),
                    new(OpCodes.Ldstr, "SPRINT"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)), new(OpCodes.Brtrue_S, goNextBlock),

                    new(OpCodes.Ldarg_0), new(OpCodes.Ldc_R4, 0f), CodeInstruction.StoreField(typeof(Player), "momentum"),

                    leaveOp
                };

                parser.codes.InsertRange(instructionIndex, checkSprintMovementCodes);
            }

            static void PatchInjectSprint(CodeInstructionParser parser)
            {
                var instructionIndex =
                    parser.FirstInstanceCodeIndex(o =>
                        parser[o.Index + 1].Calls(PlayerMethods.IsClimbing)
                    );

                var injectDoSprintCodes = new CodeInstruction[] {
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(SprintPatch), nameof(UpdateSprint))
                };

                parser.codes.InsertRange(instructionIndex, injectDoSprintCodes);
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var parser = new CodeInstructionParser(instructions, generator);

                PatchMomentum(parser);
                PatchInjectSprint(parser);

                return parser.codes.AsEnumerable();
            }
        }
    }
}