using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    class EntranceRando : InstantiateOnGameSceneLoad
    {
        static List<List<string>> entranceMap;
        static readonly Dictionary<string, bool> camInFrontPaths = new() {};

        public static void SetEntranceMap(List<List<string>> entranceMap)
        {
            EntranceRando.entranceMap = entranceMap;
        }

        static Destination GetOwnDestination(WarpTrigger bilinearWarp)
        {
            var warpPath = bilinearWarp.transform.GetPath().Substring(1);

            var destPath = Data.entrancePaths.Values
                                             .First(paths => paths.warpPath == warpPath)
                                             .destPath;

            return GameScene.FindInSceneFullPath(destPath).GetComponent<Destination>();
        }

        [HarmonyPatch(typeof(WarpTrigger), "Warp")]
        static class WarpPatch
        {
            static readonly Access.Field<GlobalHub, Timer> warpTimer = new("warpTimer");

            static void WarpToSelf(WarpTrigger warp, Quaternion walkDirection)
            {
                var cancelDestination = GetOwnDestination(warp);
                var cancelArea = GameScene.GetContainingArea(cancelDestination.transform);

                var cancelDestinationPath = cancelDestination.transform.GetPath().Substring(1);
                bool camInFront = camInFrontPaths[cancelDestinationPath];

                GlobalHub.Instance.StartWarp(
                    cancelArea,
                    cancelDestination.transform,
                    warp.warpWalk,
                    walkDirection,
                    camInFront,
                    warp.warpWalkSpeed
                );

                switch (warp.warpSound)
                {
                case WarpTrigger.WarpSound.WORLD:
                    StockSFX.Instance.warpWorld.Play();
                    break;
                case WarpTrigger.WarpSound.PIT:
                    StockSFX.Instance.warpPit.Play();
                    break;
                case WarpTrigger.WarpSound.GO_INSIDE:
                    StockSFX.Instance.warpGoInside.Play();
                    break;
                case WarpTrigger.WarpSound.GO_OUTSIDE:
                    StockSFX.Instance.warpGoOutside.Play();
                    break;
                case WarpTrigger.WarpSound.PAINTING:
                    StockSFX.Instance.warpPainting.Play();
                    break;
                }
            }

            static bool Prefix(WarpTrigger __instance, ref GameObject ___walkDirection)
            {
                // early-out copied from GlobalHub.StartWarp
                if (warpTimer.Get(GlobalHub.Instance).Active()) return false;

                // this small hunk copied from WarpTrigger.Warp
                if (__instance.transform.childCount > 1) {
                    ___walkDirection = __instance.transform.GetChild(1).gameObject;
                } else {
                    ___walkDirection = __instance.gameObject;
                }

                bool hasSwim = GlobalHub.Instance.save.GetFlag("SKILL_SWIM").on;
                if (hasSwim) return true;

                bool isDestinationUnderwater = Data.underwaterDestinationPaths.Contains(__instance.destination.transform.GetPath().Substring(1));
                if (!isDestinationUnderwater) return true;

                Messaging.TextLogManager.AddLine("<color=#9999ff>The pathway leaks with water...</color>");
                WarpToSelf(__instance, ___walkDirection.transform.rotation);

                return false;
            }
        }

        [LoadOrder(Int32.MaxValue)]
        public EntranceRando()
        {
            camInFrontPaths.Clear();
            foreach (var warp in GameScene.GetComponentsInChildren<WarpTrigger>(true)) {
                string destPath = warp.destination.transform.GetPath().Substring(1);
                if (camInFrontPaths.ContainsKey(destPath)) continue;
                camInFrontPaths.Add(
                    destPath,
                    warp.camInFront
                );
            }

            if (entranceMap.Count == 0) return;

            foreach (var map in entranceMap) {
                var fromPaths = Data.entrancePaths[map[0]];
                var toPaths = Data.entrancePaths[map[1]];
                Debug.Log($"from {map[0]} to {map[1]}");
                Debug.Log($"{fromPaths.warpPath}");
                Debug.Log($"{toPaths.destPath}");

                if (fromPaths.warpPath == null) continue;

                var warp = GameScene.FindInSceneFullPath(fromPaths.warpPath).GetComponent<WarpTrigger>();
                Debug.Log($"found warp: {warp}");
                var dest = GameScene.FindInSceneFullPath(toPaths.destPath).GetComponent<Destination>();
                Debug.Log($"found destination: {dest}");

                warp.camInFront = camInFrontPaths[toPaths.destPath];

                warp.destination = dest;
                warp.area = GameScene.GetContainingArea(dest.transform);
            }
        }
    }
}
