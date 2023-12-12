using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace CoDArchipelago
{
    class HasInitMethod : Attribute
    {
        public readonly System.Type[] dependencies;
        
        public HasInitMethod(params System.Type[] dependencies)
        {
            this.dependencies = dependencies;
        }
    }
    
    static class ArchipelagoContext
    {
        static void wew()
        {
            var patchers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Select(type => new {Type = type, RuntimePatch = type.GetCustomAttribute<HasInitMethod>()})
                .Where(a => a.RuntimePatch != null)
                .ToList();

            var a = new TopologicalSorter(patchers.Count);
            
            Dictionary<string, int> indexes = new();
            
            for (int i = 0; i < patchers.Count; i++) {
                indexes[patchers[i].GetType().Name] = a.AddVertex(i);
            }
            for (int i = 0; i < patchers.Count; i++) {
                foreach (var t in patchers[i].RuntimePatch.dependencies) {
                    a.AddEdge(i, indexes[t.Name]);
                }
            }
            
            var r = a.Sort();
            foreach (int i in r) {
                AccessTools.Method(patchers[i].Type, "RuntimePatch").Invoke(null, new object[] {});
            }
        }

        static readonly MethodInfo warpHelperInfo = typeof(GlobalHub).GetMethod("WarpHelper", BindingFlags.NonPublic | BindingFlags.Instance);
        static void WarpTo(Area area, GameObject warpTargetObj)
        {
            warpHelperInfo.Invoke(GlobalHub.Instance, new object[]{area, warpTargetObj.transform});
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

            [HarmonyPatch(typeof(GlobalHub), "Start")]
            static class StartPatch
            {
                static void Postfix()
                {
                    GlobalHub.Instance.cameraScript.Reset();
                }
            }
            
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

                static void Postfix(GlobalHub __instance, ref Area ___areaCurrent, ref Area ___areaStartNormal, ref GameObject ___positionStart, ref Transform ___lastDest, ref World ___world)
                {
                    GlobalGameScene.Init();
                    wew();
                    // APResources.Init();
                    // SkillMenuPatches.Init();
                    // MapPatches.Init();
                    // QualityOfLife.Init();
                    // Cutscenes.Init();
                    // APTextLog.Init();
                    // PrincessPatches.RuntimePatchPrincess();
                    // var warpComponent = WarpToStart.PatchScene();

                    initFinished = true;

                    ___areaStartNormal.gameObject.SetActive(false);

                    // var a = CreateNewStart("CAVE", "Palace Lobby", "DestFromDepthsToPalaceLobby");
                    // var a = CreateNewStart("PALACE", "Valley (Main)", "DestFromPalaceLobbyToValley");
                    var a = new WarpToStart.StartLocation("PALACE", "Valley (Main)", "DestFromLakeToValley");
                    // var a = CreateNewStart("CAVE", "Sun Cavern (Main)", "DestFromMonsterLobbyToCave");

                    ___areaCurrent = a.area;
                    ___positionStart = a.startLocation;
                    ___lastDest = a.startLocation.transform;
                    ___world = a.area.GetComponentInParent<World>();

                    a.area.gameObject.SetActive(true);
                    a.area.Activate();

                    WarpToStart.SetStartLocation(a);
                }
            }
        }
    }
}