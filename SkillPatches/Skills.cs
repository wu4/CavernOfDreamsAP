using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.InputSystem;

namespace CoDArchipelago
{
    static partial class Skills {
        public static bool HasSkill(string skill_name) => GlobalHub.Instance.HasSkill(skill_name);

        static class PlayerMethods
        {
            static MethodInfo Pm(string name) => AccessTools.Method(typeof(Player), name);
            public static readonly MethodInfo CanHover = Pm("CanHover");
            public static readonly MethodInfo ReleaseCarryable = Pm("ReleaseCarryable");
            public static readonly MethodInfo CanJump = Pm("CanJump");
            public static readonly MethodInfo HasFlight = Pm("HasFlight");
            public static readonly MethodInfo IsActionHeld = Pm("IsActionHeld");
            public static readonly MethodInfo IsAttacking = Pm("IsAttacking");
            public static readonly MethodInfo IsClimbing = Pm("IsClimbing");
            public static readonly MethodInfo IsCoyoteTime = Pm("IsCoyoteTime");
            public static readonly MethodInfo IsGrounded = Pm("IsGrounded");
            public static readonly MethodInfo IsJumpHeld = Pm("IsJumpHeld");
            public static readonly MethodInfo IsJumpPressed = Pm("IsJumpPressed");
            public static readonly MethodInfo IsMucky = Pm("IsMucky");
            public static readonly MethodInfo IsRollGrounded = Pm("IsRollGrounded");
            public static readonly MethodInfo IsRollHeld = Pm("IsRollHeld");
            public static readonly MethodInfo IsRolling = Pm("IsRolling");
            public static readonly MethodInfo IsSitting = Pm("IsSitting");
            public static readonly MethodInfo IsWingTypeState = Pm("IsWingTypeState");
            public static readonly MethodInfo MakeSeperateParticleSystem = Pm("MakeSeperateParticleSystem");
            public static readonly MethodInfo SetParticles = Pm("SetParticles");
        }

        static class PlayerFields
        {
            static FieldInfo Pf(string name) => AccessTools.Field(typeof(Player), name);
            public static readonly FieldInfo animator = Pf("animator");
            public static readonly FieldInfo attackTimer = Pf("attackTimer");
            public static readonly FieldInfo bouncing = Pf("bouncing");
            public static readonly FieldInfo flying = Pf("flying");
            public static readonly FieldInfo groundedLastFrame = Pf("groundedLastFrame");
            public static readonly FieldInfo highJumpAfterLandingDuration = Pf("highJumpAfterLandingDuration");
            public static readonly FieldInfo isMoveInput = Pf("isMoveInput");
            public static readonly FieldInfo jumpTimer = Pf("jumpTimer");
            public static readonly FieldInfo momentum = Pf("momentum");
            public static readonly FieldInfo rollJumpTimer = Pf("rollJumpTimer");
        }

        static class MiscMethods
        {
            public static readonly MethodInfo timerEnd = AccessTools.Method(typeof(Timer), "End");
            public static readonly MethodInfo audioGroupPlay = AccessTools.Method(typeof(AudioGroup), "Play", new Type[] {});
        }

        static class PlayerSwim
        {
            [HarmonyPatch(typeof(Player), "OnTriggerEnter")]
            static class Patch
            {
                static void Postfix(Player __instance, Collider collision, Carryable ___carryableObject, GameObject ___model)
                {
                    if (HasSkill("SWIM")) return;

                    if (collision.gameObject.tag == "Water") {
                        __instance.SetVelocity(new Vector3());

                        if (__instance.IsCarrying())
                        {
                            ___carryableObject.Drop(___model.transform.forward);
                            PlayerMethods.ReleaseCarryable.Invoke(__instance, new object[] {___model.transform.forward});
                        }

                        DeathPatches.WaterTeleport(__instance);
                    }
                }
            }
        }

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

        static class PlayerSplitAttack
        {
            [HarmonyPatch(typeof(Player), "HasAttack")]
            class Patch
            {
                static bool Prefix(Player __instance, ref bool __result)
                {
                    if (HasSkill("ATTACK")) return true;

                    __result = __instance.IsGrounded()
                        ? HasSkill("GROUNDATTACK")
                        : HasSkill("AIRATTACK");

                    return false;
                }
            }
        }
    }
}