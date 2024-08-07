using System;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.MiscPatches
{
    static class DeathPatches
    {
        enum WaterTeleportDeath {
            WATER = 0xdead
        };

        public static void WaterTeleport(Player player)
        {
            player.Die((Kill.KillType)WaterTeleportDeath.WATER);
        }

        static bool isDying = false;
        static bool waterVoid = false;

        [HarmonyPatch(typeof(Collectible), "Collect", new Type[] {})]
        static class CollectPatch
        {
            [HarmonyPriority(Priority.VeryHigh)]
            static bool Prefix() {
                return !isDying;
            }
        }

        // [HarmonyPatch(typeof(GlobalHub), "PlayerCanAct")]
        // static class DiePatch2
        // {
        //     static bool Prefix(ref bool __result) {
        //         if (isDying) {
        //             __result = false;
        //             return false;
        //         }
        //         return true;
        //     }
        // }

        static readonly Access.Field<Player, GameObject> model = new("model");
        static readonly Access.Field<Player, CharacterController> cc = new("cc");

        static SkinnedMeshRenderer GetPlayerRenderer(Player player) =>
            model.Get(player).transform.Find("dragon/Main").GetComponent<SkinnedMeshRenderer>();

        static void HideFynn(Player player)
        {
            model.Get(player).SetActive(false);
            // player.gameObject.SetActive(false);
            // model.Get(player).transform.localPosition = new(0, -1000, 0);
            // GetPlayerRenderer(player).enabled = false;
            // cc.Get(player).enabled = false;
        }

        static void ShowFynn(Player player)
        {
            model.Get(player).SetActive(true);
            // player.gameObject.SetActive(true);
            // player.enabled = true;
            // GetPlayerRenderer(player).enabled = true;
            // cc.Get(player).enabled = true;
        }

        public static bool shouldSendDeathLink = true;

        [HarmonyPatch(typeof(Player), "Die")]
        static class DiePatch
        {
            static readonly Access.Field<GlobalHub, bool> warpIsDeath = new("warpIsDeath");
            static readonly Access.Field<GlobalHub, Timer> warpTimer = new("warpTimer");

            static bool Prefix(Player __instance, Kill.KillType killType) {
                if (warpIsDeath.Get(GlobalHub.Instance) || isDying) return false;
                if (shouldSendDeathLink) {
                    APClient.Client.SendDeathLink(killType);
                }

                isDying = true;
                if (killType == (Kill.KillType)WaterTeleportDeath.WATER) {
                    HideFynn(__instance);
                    waterVoid = true;
                    GlobalHub.Instance.Die();
                    warpTimer.Get(GlobalHub.Instance).Reset(60f);

                    return false;
                }
                return true;
            }
        }

        static class SinkOnTouchFix
        {
            static SinkOnTouch touch = null;

            [HarmonyPatch(typeof(SinkOnTouch), "Touch")]
            static class AddNewTouch
            {
                static bool Prefix(SinkOnTouch __instance)
                {
                    if (isDying) return false;
                    touch = __instance;
                    return true;
                }
            }

            [HarmonyPatch(typeof(SinkOnTouch), "Leave")]
            static class RemoveTouch
            {
                static bool Prefix(SinkOnTouch __instance)
                {
                    if (isDying) return false;
                    touch = null;
                    return true;
                }
            }

            public static void Fix()
            {
                touch?.Leave();
            }
        }

        [HarmonyPatch(typeof(GlobalHub), "WarpHelper")]
        static class DeathWarpPatch
        {
            static void Postfix() {
                if (waterVoid) {
                    ShowFynn(GlobalHub.Instance.player);
                    waterVoid = false;
                }
                isDying = false;
                SinkOnTouchFix.Fix();
            }
        }
    }
}
