using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using static CoDArchipelago.CodeMatchHelpers;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.Cutscenes
{
    class Collecting : InstantiateOnGameSceneLoad
    {
        public Collecting()
        {
            locationCutscenes.Clear();

            var cutscenes        = GameScene.GetComponentsInChildren<Cutscene>(true)             .GroupBy(x => x.flag).ToDictionary(group => group.Key, group => group.First());
            var timedSwitches    = GameScene.GetComponentsInChildren<TimedSwitch>(true)          .GroupBy(x => x.flag).ToDictionary(group => group.Key, group => group.First());
            var cutsceneSwitches = GameScene.GetComponentsInChildren<GenericCutsceneSwitch>(true).GroupBy(x => x.flag).ToDictionary(group => group.Key, group => group.First());

            foreach (var flagName in Data.eventItems.Keys) {
                if (flagName == "PALACE_MELTED_ICE") continue;

                if (timedSwitches.TryGetValue(flagName, out TimedSwitch ts)) {
                    ts.flag = "LOCATION_" + ts.flag;
                }

                Cutscene cutscene = null;

                if (cutsceneSwitches.TryGetValue(flagName, out GenericCutsceneSwitch gcs)) {
                    gcs.flag = "LOCATION_" + gcs.flag;
                    cutscene = gcs.cutscene;
                }

                if (cutscene == null && !cutscenes.TryGetValue(flagName, out cutscene)) {
                    Debug.LogError("Failed to find cutscene flag for " + flagName);
                    continue;
                }

                locationCutscenes.Add(flagName, cutscene);
            }
        }

        static readonly Dictionary<string, Cutscene> locationCutscenes = new(){};

        /// <summary>
        /// Attempt to add a cutscene to <see cref="MultipleCutscenes"/> by
        /// looking up its associated flag. Returns <c>false</c> if no
        /// such cutscene exists.
        /// </summary>
        /// <param name="cutsceneFlag">The name of the cutscene's flag.</param>
        /// <returns></returns>
        public static bool TryCollect(string cutsceneFlag)
        {
            if (locationCutscenes.TryGetValue(cutsceneFlag, out Cutscene cutscene)) {
                MultipleCutscenes.Add(cutscene);
                return true;
            }
            return false;
        }

        /// <summary>
        /// All cutscenes associated with locations will be played instead
        /// through <see cref="TryCollect"/>
        /// </summary>
        [HarmonyPatch(typeof(GlobalHub), nameof(GlobalHub.SetCutscene))]
        static class LocationCutscenePatch
        {
            static bool Prefix(Cutscene cs)
            {
                if (!locationCutscenes.ContainsKey(cs.flag)) return true;

                GlobalHub.Instance.save.SetFlag("LOCATION_" + cs.flag, true);

                return false;
            }
        }
    }
}