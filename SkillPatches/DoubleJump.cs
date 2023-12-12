using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    static partial class Skills
    {
        static class DoubleJumpPatch
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
                    static bool ShouldShowDoubleJumpAnim(Player p)
                    {
                        return !p.IsGrounded() && doubleJumpStateTimer.Active();
                    }

                    static bool ShouldShowDoubleJumpFlipAnim(Player p)
                    {
                        return !p.IsGrounded() && doubleJumpFlipTimer.Active();
                    }

                    static void PatchSlowFlapAnimation(CodeInstructionParser parser)
                    {
                        int instructionIndex = parser.FirstInstanceCodeIndex(
                            o => parser[o.Index + 1].Calls(PlayerMethods.HasFlight)
                        );

                        var hasSkillCodes = new CodeInstruction[] {
                            new(OpCodes.Ldstr, "DOUBLEJUMP"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)),
                            new(OpCodes.Or)
                        };

                        parser.codes.InsertRange(instructionIndex + 2, hasSkillCodes);
                    }

                    static void PatchDoubleJumpAnimations(CodeInstructionParser parser)
                    {
                        Label onFalse = parser.generator.DefineLabel();

                        Label onFalse2 = parser.generator.DefineLabel();
                        parser.AddLabel(0, onFalse2);

                        Label onTrue = (Label)parser.FirstCode(
                            o => o.Item.opcode == OpCodes.Br
                        ).Item.operand;

                        var codesToInject = new CodeInstruction[] {
                            new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(AnimationPatch), nameof(ShouldShowDoubleJumpFlipAnim)), new(OpCodes.Brfalse, onFalse),
                            new(OpCodes.Ldstr, "JumpSomersault"), new(OpCodes.Stloc_0),
                            new(OpCodes.Br, onTrue),
                            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(onFalse), CodeInstruction.Call(typeof(AnimationPatch), nameof(ShouldShowDoubleJumpAnim)), new(OpCodes.Brfalse, onFalse2),
                            new(OpCodes.Ldstr, "FlyUpWeakLoop"), new(OpCodes.Stloc_0),
                            new(OpCodes.Br, onTrue)
                        };

                        parser.codes.InsertRange(0, codesToInject);
                    }

                    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                    {
                        var parser = new CodeInstructionParser(instructions, generator);

                        PatchSlowFlapAnimation(parser);
                        PatchDoubleJumpAnimations(parser);

                        return parser.codes.AsEnumerable();
                    }
                }

                [HarmonyPatch(typeof(Player), "CleanUp")]
                static class ModelVisibilityPatch
                {
                    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                    {
                        var parser = new CodeInstructionParser(instructions, generator);

                        var instructionIndex =
                            parser.FirstInstanceCodeIndex(o =>
                                parser[o.Index + 2].Calls(PlayerMethods.HasFlight)
                            );

                        var codesToInject = new CodeInstruction[] {
                            new(OpCodes.Ldstr, "DOUBLEJUMP"), CodeInstruction.Call(typeof(Skills), "HasSkill"),
                            CodeInstruction.LoadField(typeof(DoubleJumpPatch), nameof(doubleJumpStateTimer)), CodeInstruction.Call(typeof(Timer), "Active"),
                            new(OpCodes.Ldstr, "HOVER"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)),
                            new(OpCodes.Or),
                            new(OpCodes.And),

                            new(OpCodes.Or)
                        };

                        parser.codes.InsertRange(instructionIndex + 3, codesToInject);

                        return parser.codes.AsEnumerable();
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

                    if (!(bool)PlayerMethods.IsJumpHeld.Invoke(p, new object[] {})) break;

                    if (HasSkill("HOVER")) {
                        PlayerFields.flying.SetValue(p, true);
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

            static bool CanDoubleJump(Player p)
            {
                return
                    HasSkill("DOUBLEJUMP")
                    && !p.IsGrounded()
                    && !(bool)PlayerMethods.CanJump.Invoke(p, new object[] {})
                    && !p.IsPaddling()
                    && !p.IsUnderwater()
                    && (canDoubleJump || (bool)PlayerFields.bouncing.GetValue(p));
            }

            static Vector3 DoDoubleJump(Player p, Vector3 newVelocity)
            {
                doubleJumpStateTimer.Reset();
                canDoubleJump = false;
                doubleJumpState = 0;

                PlayerFields.bouncing.SetValue(p, false);
                // p.bouncing = false;

                newVelocity.y = p.jumpStr * 0.75f;
                ((Timer)PlayerFields.jumpTimer.GetValue(p)).Reset();

                p.hoverSFX.Play();
                p.gruntSFX.Play();

                if (!HasSkill("HOVER")) {
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
                    var parser = new CodeInstructionParser(instructions, generator);

                    int blockIndex =
                        parser.FirstInstanceCodeIndex(
                            o => parser.codes[o.Index + 1].Calls(PlayerMethods.IsJumpPressed)
                        );

                    int ifChainIndex =
                        parser.FirstInstanceCodeIndex(
                            o => o.Index > blockIndex
                            && parser[o.Index + 1].Calls(PlayerMethods.IsClimbing)
                        );

                    var newVelocityArg =
                        parser.FirstCode(
                            o => o.Index > blockIndex && o.Item.opcode == OpCodes.Starg_S
                        ).Item.operand;

                    Label start = parser.generator.DefineLabel();

                    parser.byIndex
                        .Where(o =>
                            o.Index > blockIndex
                            && o.Index < ifChainIndex
                            && (
                                o.Item.opcode == OpCodes.Brtrue
                                || o.Item.opcode == OpCodes.Brfalse
                            )
                        )
                        .Skip(2)
                        .Do(o => {
                            o.Item.operand = start;
                        });

                    Label onFalse = parser.generator.DefineLabel();
                    parser.AddLabel(ifChainIndex, onFalse);

                    Label onTrue =
                        (Label)parser.FirstCode(
                            o => o.Index > ifChainIndex && o.Item.opcode == OpCodes.Br
                        ).Item.operand;

                    var codesToInject = new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldarg_0).WithLabels(start), CodeInstruction.Call(typeof(DoubleJumpPatch), nameof(CanDoubleJump)), new(OpCodes.Brfalse, onFalse),
                        new(OpCodes.Ldarg_0), new(OpCodes.Ldarg_S, newVelocityArg), CodeInstruction.Call(typeof(DoubleJumpPatch), nameof(DoDoubleJump)), new(OpCodes.Starg_S, newVelocityArg),
                        new(OpCodes.Br, onTrue)
                    };

                    parser.codes.InsertRange(ifChainIndex, codesToInject);

                    var otherCodes = new CodeInstruction[] {
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(DoubleJumpPatch), nameof(UpdateDoubleJump)),
                    };

                    parser.codes.InsertRange(0, otherCodes);

                    return parser.codes.AsEnumerable();
                }
            }
        }
    }
}