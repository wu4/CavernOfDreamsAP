using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using System.Collections.Generic;
using HarmonyLib;

namespace CoDArchipelago.MiscPatches
{
    [HarmonyPatch(typeof(Area), "Activate")]
    static class ErrorOutAreaActivation
    {
        static Area testArea;
        static bool isFoyer = false;

        class GetTestArea : InstantiateOnGameSceneLoad
        {
            public GetTestArea()
            {
                if (isFoyer) {
                    testArea = GameScene.FindInScene("GALLERY", "Foyer (Main)").GetComponent<Area>();
                } else {
                    testArea = new Area();
                }
            }
        }

        public static void Prefix(Area __instance)
        {
            if (__instance == testArea) {
                throw new System.Exception("oh no! what a terrible exception!");
            }
        }
    }

    // disabled, it isnt necessary anymore
    class FindAtelierPortals // : InstantiateOnGameSceneLoad
    {
        public FindAtelierPortals()
        {
            Dictionary<string, Area> target_areas = new() {
                {"atelier", GameScene.FindInScene("GALLERY", "Atelier").GetComponent<Area>()},
                {"finale",  GameScene.FindInScene("FINALE", "Finale").GetComponent<Area>()},
                {"corrupt", GameScene.FindInScene("CAVE", "Corrupt").GetComponent<Area>()},
            };
            foreach (var wt in GameScene.GetComponentsInChildren<WarpTrigger>(includeInactive: true)) {
                foreach (var (name, area) in target_areas) {
                    if (wt.area != area) continue;
                    Debug.Log((name, wt.transform.GetPath()));
                    break;
                }
            }
        }
    }
}
