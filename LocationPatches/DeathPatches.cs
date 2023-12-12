using System;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
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

        [HarmonyPatch(typeof(GlobalHub), "CanPause")]
        static class PausePatch
        {
            static bool Prefix(ref bool __result) {
                __result = true;
                return false;
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
        
        static readonly AccessTools.FieldRef<Player, GameObject> model = AccessTools.FieldRefAccess<Player, GameObject>("model");

        [HarmonyPatch(typeof(Player), "Die")]
        static class DiePatch
        {
            static readonly AccessTools.FieldRef<GlobalHub, bool> warpIsDeath = AccessTools.FieldRefAccess<GlobalHub, bool>("warpIsDeath");
            static readonly AccessTools.FieldRef<GlobalHub, Timer> warpTimer = AccessTools.FieldRefAccess<GlobalHub, Timer>("warpTimer");

            static bool Prefix(Player __instance, Kill.KillType killType) {
                if (warpIsDeath(GlobalHub.Instance) || isDying) return false;
                isDying = true;
                if (killType == (Kill.KillType)WaterTeleportDeath.WATER) {
                    model(__instance).SetActive(false);
                    waterVoid = true;
                    GlobalHub.Instance.Die();
                    warpTimer(GlobalHub.Instance).Reset(60f);

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GlobalHub), "WarpHelper")]
        static class DeathWarpPatch
        {
            static void Postfix() {
                if (waterVoid) {
                    model(GlobalHub.Instance.player).SetActive(true);
                    waterVoid = false;
                }
                isDying = false;
            }
        }
    }
}