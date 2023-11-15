using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CoDArchipelago
{

    [AttributeUsage(AttributeTargets.Class,
                    AllowMultiple = true)]
    class Skill : Attribute
    {
        public string skill_name;
        public Skill(string skill_name)
        {
            this.skill_name = skill_name;
        }
    }

    static class Skills {
        public static bool HasSkill(string skill_name) => GlobalHub.Instance.HasSkill(skill_name);

        public static void PatchDebugMenu()
        {
            GameObject sample_obj = null;
            GameObject parent = GlobalGameScene.FindInScene("Rendering", "Canvas/DebugMenu/SkillPage").gameObject;

            int START_POS = 65;
            int STEP = 28;

            int i = 0;
            foreach (MO_FLAG flag in parent.GetComponentsInChildren<MO_FLAG>()) {
                GameObject obj = flag.gameObject;
                sample_obj ??= obj;
                
                var bg = obj.GetComponentInChildren<Image>();
                bg.transform.localScale = bg.transform.localScale with {y = 0.5f};

                obj.transform.localPosition = new Vector3(){x = -200, y = START_POS - STEP * i, z = 0};
                i++;
            }

            var skill_names = typeof(Skills)
                .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static)
                .SelectMany(x => x.GetCustomAttributes(typeof(Skill), false) as IEnumerable<Skill>)
                .Select(x => x.skill_name);

            i = 0;

            foreach (string skill_name in skill_names) {
                Debug.Log(skill_name);
                GameObject copy = GameObject.Instantiate(sample_obj);
                copy.name = "MO_FLAG_SKILL_" + skill_name;
                copy.GetComponent<MO_FLAG>().flag = "SKILL_" + skill_name;
                copy.GetComponentInChildren<TextMeshProUGUI>().SetText(skill_name);

                copy.transform.SetParent(parent.transform, false);

                copy.transform.localPosition = new Vector3(){x = 50, y = START_POS - STEP * i, z = 0};
                // copy.transform.rotation = new Quaternion();
                // copy.transform.localScale = new Vector3(){x = 1, y = 1, z = 1};

                i++;
            }
        }

        static class PlayerMethods
        {
            public static readonly MethodInfo IsAttacking = AccessTools.Method(typeof(Player), "IsAttacking");
            public static readonly MethodInfo IsRolling = AccessTools.Method(typeof(Player), "IsRolling");
            public static readonly MethodInfo IsClimbing = AccessTools.Method(typeof(Player), "IsClimbing");
            public static readonly MethodInfo IsActionHeld = AccessTools.Method(typeof(Player), "IsActionHeld");
            public static readonly MethodInfo IsRollHeld = AccessTools.Method(typeof(Player), "IsRollHeld");
            public static readonly MethodInfo IsSitting = AccessTools.Method(typeof(Player), "IsSitting");
            public static readonly MethodInfo IsGrounded = AccessTools.Method(typeof(Player), "IsGrounded");
            public static readonly MethodInfo IsRollGrounded = AccessTools.Method(typeof(Player), "IsRollGrounded");
            public static readonly MethodInfo IsMucky = AccessTools.Method(typeof(Player), "IsMucky");
        }

        static class PlayerFields
        {
            public static readonly FieldInfo highJumpAfterLandingDuration = AccessTools.Field(typeof(Player), "highJumpAfterLandingDuration");
            public static readonly FieldInfo attackTimer = AccessTools.Field(typeof(Player), "attackTimer");
            public static readonly FieldInfo rollJumpTimer = AccessTools.Field(typeof(Player), "rollJumpTimer");
            public static readonly FieldInfo groundedLastFrame = AccessTools.Field(typeof(Player), "groundedLastFrame");
            public static readonly FieldInfo isMoveInput = AccessTools.Field(typeof(Player), "isMoveInput");
            public static readonly FieldInfo momentum = AccessTools.Field(typeof(Player), "momentum");
        }

        static class MiscMethods
        {
            public static readonly MethodInfo timerEnd = AccessTools.Method(typeof(Timer), "End");
            public static readonly MethodInfo audioGroupPlay = AccessTools.Method(typeof(AudioGroup), "Play", new Type[] {});
        }


        [Skill("HIGHJUMP")]
        [Skill("SPRINT")]
        [Skill("SUPERBOUNCE")]
        static class PlayerMovementPatches
        {

            public static bool IsSprinting(Player player)
            {
                return HasSkill("SPRINT")
                    && !HasSkill("ROLL")
                    && player.IsGrounded()
                    && !(bool)PlayerMethods.IsSitting.Invoke(player, new object[] {})
                    && (bool)PlayerFields.isMoveInput.GetValue(player)
                    && (bool)PlayerMethods.IsRollHeld.Invoke(player, new object[] {});
            }

            static readonly float maxSprintMomentum = 0.5f;

            // taken from Player.AboveMomentumFloor()
            static readonly float minSprintMomentum = 0.10000000149011612f;

            public static void DoSprint(Player player)
            {
                if (!IsSprinting(player)) return;

                float momentum = (float)PlayerFields.momentum.GetValue(player);
                if (momentum >= maxSprintMomentum) return;

                PlayerFields.momentum.SetValue(player, Mathf.Clamp(momentum + (Time.deltaTime * 0.3f), minSprintMomentum, maxSprintMomentum));
            }


            [HarmonyPatch(typeof(Player), "UpdateInputs")]
            static class Patch
            {
                class CodeByIndex
                {
                    public readonly CodeInstruction Item;
                    public readonly int Index;
                    public CodeByIndex(CodeInstruction item, int index)
                    {
                        Item = item;
                        Index = index;
                    }
                }

                static List<CodeInstruction> codes;
                static IEnumerable<CodeByIndex> codesByIndex;
                static IEnumerable<CodeByIndex> instanceInstructions;
                static ILGenerator ilGenerator;

                static void HighJumpPatch()
                {
                    var instructionIndex =
                        instanceInstructions
                        .First(o => codes[o.Index + 4].LoadsField(PlayerFields.highJumpAfterLandingDuration))
                        .Index;

                    var failure = codes[instructionIndex + 6].operand;

                    var codes_to_inject = new CodeInstruction[] {
                        new(OpCodes.Ldstr, "HIGHJUMP"), CodeInstruction.Call(typeof(Skills), "HasSkill"), new(OpCodes.Brfalse_S, failure),
                    };

                    codes.InsertRange(instructionIndex + 7, codes_to_inject);
                }


                static void DivePatch()
                {
                    var instructionIndex =
                        instanceInstructions
                        .First(o =>
                            codes[o.Index + 1].Calls(PlayerMethods.IsActionHeld)
                            && codes[o.Index + 4].LoadsField(PlayerFields.attackTimer)
                        )
                        .Index;

                    Label forceDive = ilGenerator.DefineLabel();
                    codes[instructionIndex + 20] = codes[instructionIndex + 20].WithLabels(forceDive);

                    Label vanillaChecks = ilGenerator.DefineLabel();
                    codes[instructionIndex + 3] = codes[instructionIndex + 3].WithLabels(vanillaChecks);

                    var codes_to_inject = new CodeInstruction[] {
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "CanAttack"),       new(OpCodes.Brtrue_S,  vanillaChecks),
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsActionPressed"), new(OpCodes.Brfalse_S, vanillaChecks),
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "IsCarrying"),      new(OpCodes.Brtrue_S,  vanillaChecks),
                        new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(Player), "CanDive"),         new(OpCodes.Brtrue_S,  forceDive),
                    };

                    codes.InsertRange(instructionIndex + 3, codes_to_inject);
                }

                static void SprintPatch()
                {
                    // keep momentum at zero if no sprint
                    {
                        var instructionIndex =
                            instanceInstructions
                            .First(o =>
                                codes[o.Index + 1].Calls(PlayerMethods.IsMucky)
                            )
                            .Index;

                        Label goNextBlock = ilGenerator.DefineLabel();
                        codes[instructionIndex] = codes[instructionIndex].WithLabels(goNextBlock);

                        var leaveOp =
                            codesByIndex
                            .First(o =>
                                o.Index > instructionIndex
                                && o.Item.opcode == OpCodes.Br
                            )
                            .Item;

                        var codes_to_inject = new CodeInstruction[] {
                            new(OpCodes.Ldarg_0),         CodeInstruction.Call(typeof(Player), "CarryingTorpedoWhileUnderwater"), new(OpCodes.Brtrue_S, goNextBlock),
                            new(OpCodes.Ldstr, "SPRINT"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)), new(OpCodes.Brtrue_S, goNextBlock),

                            new(OpCodes.Ldarg_0), new(OpCodes.Ldc_R4, 0f), CodeInstruction.StoreField(typeof(Player), "momentum"),

                            leaveOp
                        };

                        codes.InsertRange(instructionIndex, codes_to_inject);
                    }

                    // allow sprint if no roll
                    {
                        var instructionIndex =
                            instanceInstructions
                            .First(o =>
                                codes[o.Index + 1].Calls(PlayerMethods.IsClimbing)
                            )
                            .Index;

                        var codes_to_inject = new CodeInstruction[] {
                            new(OpCodes.Ldarg_0), CodeInstruction.Call(typeof(PlayerMovementPatches), nameof(DoSprint))
                        };

                        codes.InsertRange(instructionIndex, codes_to_inject);
                    }
                }

                static void SuperBouncePatch()
                {
                    var instructionIndex =
                        instanceInstructions
                        .First(o =>
                            codes[o.Index + 1].LoadsField(PlayerFields.rollJumpTimer)
                            && codes[o.Index + 5].Calls(PlayerMethods.IsGrounded)
                            && codes[o.Index + 8].LoadsField(PlayerFields.groundedLastFrame)
                        )
                        .Index;

                    var innerBlockIndex =
                        instanceInstructions
                        .First(o =>
                            o.Index > instructionIndex
                            && codes[o.Index + 1].Calls(PlayerMethods.IsAttacking)
                        )
                        .Index;

                    var leave = codes[innerBlockIndex + 2].operand;
                    
                    var injectionIndex =
                        codesByIndex
                        .First(o =>
                            o.Index > innerBlockIndex
                            && o.Item.opcode == OpCodes.Ldloc_S
                        )
                        .Index;

                    Label performFastRoll = ilGenerator.DefineLabel();
                    codes[injectionIndex] = codes[injectionIndex].WithLabels(performFastRoll);

                    var checkSuperBounceCodes = new CodeInstruction[] {
                        new(OpCodes.Ldstr, "SUPERBOUNCE"), CodeInstruction.Call(typeof(Skills), nameof(HasSkill)), new(OpCodes.Brtrue_S, performFastRoll),
                        new(OpCodes.Ldarg_0),              CodeInstruction.Call(typeof(Player), "IsRollHeld"),     new(OpCodes.Brtrue_S, performFastRoll),
                        new(OpCodes.Ldloc_S, 75),          CodeInstruction.LoadField(typeof(Vector3), "y"),        new(OpCodes.Ldc_R4, 0.05f), new(OpCodes.Blt_S, performFastRoll),

                        new(OpCodes.Ldarg_0), CodeInstruction.LoadField(typeof(Player), "attackTimer"), new(OpCodes.Callvirt, MiscMethods.timerEnd),
                        new(OpCodes.Ldarg_0), CodeInstruction.LoadField(typeof(Player), "whimperSFX"), new(OpCodes.Callvirt, MiscMethods.audioGroupPlay),
                        new(OpCodes.Pop),
                        new(OpCodes.Br_S, leave)
                    };

                    codes.InsertRange(injectionIndex, checkSuperBounceCodes);
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    ilGenerator = generator;

                    codes = new List<CodeInstruction>(instructions);

                    codesByIndex =
                        codes.Select((item, index) => new CodeByIndex(item, index));

                    instanceInstructions = 
                        codesByIndex
                        .Where(o => o.Item.opcode == OpCodes.Ldarg_0);

                    HighJumpPatch();
                    DivePatch();
                    SprintPatch();
                    SuperBouncePatch();

                    return codes.AsEnumerable();
                }
            }
        }

        [Skill("SWIM")]
        static class PlayerSwim
        {
            [HarmonyPatch(typeof(Player), "OnTriggerEnter")]
            static class Patch
            {
                static void Postfix(Player __instance, Collider collision)
                {
                    if (HasSkill("SWIM")) return;

                    if (collision.gameObject.tag == "Water") {
                        __instance.Die(Kill.KillType.DROWN);
                    }
                }
            }
        }

        [Skill("ROLL")]
        static class PlayerRoll
        {
            [HarmonyPatch(typeof(Player), "CanStartRoll")]
            static class Patch
            {
                static bool Prefix(Player __instance, ref bool __result)
                {
                    if (HasSkill("ROLL")) return true;

                    __result = false;
                    return false;
                }
            }
        }

        [Skill("CARRY")]
        static class PlayerCarry
        {
            [HarmonyPatch(typeof(Player), "CanPickUpObject")]
            static class Patch
            {
                static bool Prefix(Player __instance, ref bool __result)
                {
                    if (HasSkill("CARRY")) return true;

                    __result = false;
                    return false;
                }
            }
        }

        [Skill("CLIMB")]
        static class PlayerClimb
        {
            [HarmonyPatch(typeof(Player), "CanClimb")]
            static class Patch
            {
                static bool Prefix(Player __instance, ref bool __result)
                {
                    if (HasSkill("CLIMB")) return true;

                    __result = false;
                    return false;
                }
            }
        }

        [Skill("GROUNDEDATTACK")]
        [Skill("AIRATTACK")]
        static class PlayerSplitAttack
        {
            [HarmonyPatch(typeof(Player), "HasAttack")]
            class Patch
            {
                static bool Prefix(Player __instance, ref bool __result)
                {
                    if (HasSkill("ATTACK")) return true;

                    __result = __instance.IsGrounded()
                        ? HasSkill("GROUNDEDATTACK")
                        : HasSkill("AIRATTACK");

                    return false;
                }
            }
        }
    }
}