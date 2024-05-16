using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static CoDArchipelago.CodeMatchHelpers;

namespace CoDArchipelago.SkillPatches
{
    static class DoubleJump
    {
        static int doubleJumpState = 0;
        static bool canDoubleJump = false;
        static readonly Timer doubleJumpStateTimer = new Timer(20f);
        static readonly Timer doubleJumpFlipTimer = new Timer(10f);

        static class VisualPatches
        {
            [HarmonyPatch(typeof(Player), "UpdateAnimation")]
            static class AnimationPatch
            {
                static bool ShouldShowDoubleJumpAnim(Player p) =>
                    !p.IsGrounded() && doubleJumpStateTimer.Active();

                static bool ShouldShowDoubleJumpFlipAnim(Player p) =>
                    !p.IsGrounded() && doubleJumpFlipTimer.Active();

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    var matcher = new CodeMatcher(instructions, generator);

                    matcher.Advance(1);
                    matcher.CreateLabel(out Label onFalse2);

                    Label onTrue = (Label)matcher.Clone().MatchForward(
                        false,
                        new CodeMatch(OpCodes.Br)
                    ).Instruction.operand;

                    Label onFalse = generator.DefineLabel();

                    matcher.InsertAndAdvance(
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(AnimationPatch), nameof(ShouldShowDoubleJumpFlipAnim)), new(OpCodes.Brfalse, onFalse),
                        new(OpCodes.Ldstr, "JumpSomersault"), new(OpCodes.Stloc_0),
                        new(OpCodes.Br, onTrue),
                        new CodeInstruction(OpCodes.Ldarg_0).WithLabels(onFalse), CodeInstruction.Call(typeof(AnimationPatch), nameof(ShouldShowDoubleJumpAnim)), new(OpCodes.Brfalse, onFalse2),
                        new(OpCodes.Ldstr, "FlyUpWeakLoop"), new(OpCodes.Stloc_0),
                        new(OpCodes.Br, onTrue)
                    );

                    matcher.MatchForward(
                        true,
                        new(OpCodes.Ldarg_0),
                        Calls<Player>("HasFlight")
                    );
                    matcher.Advance(1);

                    matcher.Insert(
                        // new(OpCodes.Ldstr, "DOUBLEJUMP"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)),
                        CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof (FlagCache.CachedSkillFlags.doubleJump)),
                        new(OpCodes.Or)
                    );

                    return matcher.InstructionEnumeration();
                }
            }

            [HarmonyPatch(typeof(Player), "CleanUp")]
            static class ModelVisibilityPatch
            {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    var matcher = new CodeMatcher(instructions, generator);

                    matcher.MatchForward(
                        true,
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldarg_0),
                        Calls<Player>("HasHover"),
                        // injection point
                        new(OpCodes.Ldarg_0)
                    );
                    matcher.ThrowIfInvalid("Finding HasHover");

                    matcher.Insert(
                        CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.hover)),
                        new(OpCodes.Not),

                        CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.doubleJump)),
                        CodeInstruction.LoadField(typeof(DoubleJump), nameof(DoubleJump.canDoubleJump)),
                        new(OpCodes.And),

                        new(OpCodes.And),

                        new(OpCodes.Or)
                    );

                    matcher.MatchForward(
                        true,
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldarg_0),
                        Calls<Player>("HasFlight"),
                        // injection point
                        new(OpCodes.Ldarg_0)
                    );
                    matcher.ThrowIfInvalid("Finding HasFlight");

                    matcher.Insert(
                        CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.doubleJump)),
                        new(OpCodes.Or),
                        CodeInstruction.LoadField(typeof(DoubleJump), nameof(DoubleJump.doubleJumpStateTimer)), CodeInstruction.Call(typeof(Timer), "Active"),
                        CodeInstruction.LoadField(typeof(FlagCache.CachedSkillFlags), nameof(FlagCache.CachedSkillFlags.hover)),
                        new(OpCodes.Or),
                        new(OpCodes.And)
                    );

                    return matcher.InstructionEnumeration();
                }
            }
        }

        static void UpdateDoubleJumpState(Player p)
        {
            var counter = Mathf.Floor(doubleJumpStateTimer.GetCounter());

            switch (doubleJumpState)
            {
            case 0:
                if (counter < 19) break;

                doubleJumpState++;

                if (!p.IsJumpHeld()) break;

                if (CanDoubleJumpIntoHover(p)) {
                    PlayerAccess.Fields.flying.Set(p, true);
                    // ((Animator)PlayerFields.animator.GetValue(p)).Play("FlyUpLoop", 0, 0.9f);
                } else {
                    doubleJumpFlipTimer.Reset();
                }

                break;

            default:
                break;
            }
        }

        static void UpdateDoubleJump(Player p)
        {
            doubleJumpStateTimer.Update();
            doubleJumpFlipTimer.Update();

            if (
                p.IsGrounded()
                || p.IsPaddling()
                || p.IsUnderwater()
            ) {
                canDoubleJump = true;
                doubleJumpStateTimer.End();
            }

            if (doubleJumpStateTimer.Active()) {
                UpdateDoubleJumpState(p);
            }
        }

        static bool CanDoubleJump(Player p) =>
            FlagCache.CachedSkillFlags.doubleJump
            && !p.IsGrounded()
            && !p.CanJump()
            && !p.IsPaddling()
            && !p.IsUnderwater()
            && (canDoubleJump || PlayerAccess.Fields.bouncing.Get(p));

        static bool CanDoubleJumpIntoHover(Player p) =>
            FlagCache.CachedSkillFlags.hover || PlayerAccess.Fields.carryingParaglider.Get(p);

        static Vector3 DoDoubleJump(Player p, Vector3 newVelocity)
        {
            doubleJumpStateTimer.Reset();
            canDoubleJump = false;
            doubleJumpState = 0;

            PlayerAccess.Fields.bouncing.Set(p, false);
            // p.bouncing = false;

            newVelocity.y = p.jumpStr * 0.75f;
            PlayerAccess.Fields.jumpTimer.Get(p).Reset();

            p.hoverSFX.Play();
            p.gruntSFX.Play();

            if (!FlagCache.CachedSkillFlags.hover) {
                p.whackFX.transform.position = p.transform.position with {y = p.transform.position.y + 0.3f};
                p.whackFX.Play();
            }

            return newVelocity;
        }

        [HarmonyPatch(typeof(Player), "OnTriggerEnter")]
        static class BounceResetDoubleJumpPatch
        {
            static void Prefix(Collider collision)
            {
                BounceTrigger hasBounce = collision.GetComponent<BounceTrigger>();
                if (!(hasBounce?.CanBounce() ?? false)) return;

                canDoubleJump = true;
            }
        }


        [HarmonyPatch(typeof(Player), "UpdateInputs")]
        static class Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.Advance(1);
                matcher.InsertAndAdvance(
                    new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(DoubleJump), nameof(DoubleJump.UpdateDoubleJump))
                );

                matcher.MatchForward(
                    true,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("IsJumpPressed")
                );

                matcher.MatchForward(
                    true,
                    new(OpCodes.Ldarg_0),
                    Calls<Player>("IsGrounded")
                );

                Label start = generator.DefineLabel();

                CodeInstruction code = matcher.Instruction;
                while (!(code.opcode == OpCodes.Ldarg_0 && matcher.InstructionAt(1).Calls<Player>("IsClimbing"))) {
                    if (code.opcode == OpCodes.Brtrue || code.opcode == OpCodes.Brfalse) {
                        matcher.SetOperandAndAdvance(start);
                    } else {
                        matcher.Advance(1);
                    }
                    code = matcher.Instruction;
                }

                matcher.CreateLabel(out Label onFalse);

                Label onTrue =
                    (Label)matcher.Clone().MatchForward(
                        false,
                        new CodeMatch(OpCodes.Br)
                    ).Instruction.operand;

                var newVelocityArg =
                    matcher.Clone().MatchForward(
                        false,
                        new CodeMatch(OpCodes.Starg_S)
                    ).Instruction.operand;

                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0).WithLabels(start), CodeInstruction.Call(typeof(DoubleJump), nameof(CanDoubleJump)), new(OpCodes.Brfalse, onFalse),
                    new(OpCodes.Ldarg_0), new(OpCodes.Ldarg_S, newVelocityArg), CodeInstruction.Call(typeof(DoubleJump), nameof(DoDoubleJump)), new(OpCodes.Starg_S, newVelocityArg),
                    new(OpCodes.Br, onTrue)
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
