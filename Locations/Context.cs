using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace CoDArchipelago
{
    static class ArchipelagoContext
    {
        static readonly MethodInfo warpHelperInfo = typeof(GlobalHub).GetMethod("WarpHelper", BindingFlags.NonPublic | BindingFlags.Instance);
        static void WarpTo(Area area, GameObject warpTargetObj)
        {
            warpHelperInfo.Invoke(GlobalHub.Instance, new object[]{area, warpTargetObj.transform});
        }

        static (Area Area, GameObject StartLocation) CreateNewStart(string worldName, string areaName, string warpName)
        {
            Area area = GlobalGameScene.FindInScene(worldName, areaName).GetComponent<Area>();
            GameObject warpDestObj = area.transform.Find("Warps/" + warpName + "/warp_destination").gameObject;

            return (area, warpDestObj);
        }

        static class InitPatches
        {
            static bool initFinished = false;

            // block Areas from activating before init is finished
            // this makes for a smaller patch in total than fully overriding GlobalHub.Awake
            [HarmonyPatch(typeof(Area), "Activate")]
            static class AreaInitPatch
            {
                [HarmonyPriority(Priority.VeryHigh)]
                static bool Prefix() => initFinished;
            }
            
            static GlobalHub ghInstance;
            [HarmonyPatch(typeof(GlobalHub), "Awake")]
            static class InitPatch
            {
                // Area areaStartNormal
                // GameObject positionStartNormal
                // GameObject positionStart
                static bool Prefix() {
                    initFinished = false;
                    return true;
                }
                static void Postfix(GlobalHub __instance, ref Area ___areaCurrent, ref Area ___areaStartNormal, ref GameObject ___positionStart) {
                    ghInstance = __instance;

                    GlobalGameScene.Init();

                    APResources.Init();

                    Skills.PatchDebugMenu();

                    QualityOfLife.Init();

                    Events.PatchLocations();

                    APTextLog.Init();

                    /*
                    IEnumerable<Collectible> cs =
                        gameScene.GetRootGameObjects()
                        .SelectMany(x => x.transform.GetComponentsInChildren<Collectible>(true));

                    foreach (GameObject root in gameScene.GetRootGameObjects()) {
                        foreach (Transform area in root.transform) {
                            foreach (Collectible c in area.GetComponentsInChildren<Collectible>(true)) {
                                GameObject obj = c.gameObject;
                                Debug.Log(root.name + "/" + area.name + "/" + obj.GetComponent<TwoState>().flag);
                            }
                        }
                    }
                    */

                    foreach (Collectible c in GlobalGameScene.GetComponentsInChildren<Collectible>(true)) {
                        TwoState ts = c.GetComponent<TwoState>();
                        // ts.flag = "LOCATION_" + ts.flag;

                        GameObject obj = c.gameObject;
                        // ReplaceWithMinorAPItem(obj);
                        // ReplaceWithEgg(obj);
                    }

                    initFinished = true;

                    ___areaStartNormal.gameObject.SetActive(false);

                    var a = CreateNewStart("CAVE", "Palace Lobby", "DestFromDepthsToPalaceLobby");

                    ___areaCurrent = a.Area;
                    ___positionStart = a.StartLocation;

                    a.Area.gameObject.SetActive(true);
                    a.Area.Activate();

                    // GlobalHub.Instance.player.Warp(GlobalHub.Instance.positionStartNormal.transform);

                    foreach (GameObject root in GlobalGameScene.gameScene.GetRootGameObjects()) {
                        foreach (Transform t in root.transform) {
                            var count = t.GetComponentsInChildren<Collectible>().Where(x => x.type == Collectible.CollectibleType.NOTE).Count();
                            if (count > 0) {
                                Debug.Log(t.gameObject.name);
                                Debug.Log(count);
                            }
                        }
                    }

                    LocationPatching.ReplaceWithEvent(GlobalGameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles/Notes/NotePathLake/NoteCave (1)").gameObject, "CAVE_NURIKABE_FALLEN");

                    // WarpTo(GlobalHub.Instance.areaStartNormal, GlobalHub.Instance.positionStartNormal);
                }
            }
        }
    }
}