using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    static class Events
    {
        static class MonsterBoilsPatch
        {
            [HarmonyPatch(typeof(MonsterBoilListener), "RemoveBoil")]
            class Patch
            {
                static bool Prefix(MonsterBoilListener __instance, ref int ___numBoils)
                {
                    ___numBoils--;
                    if (___numBoils == 0) {
                        StockSFX.Instance.jingleGood.Play();
                        GlobalHub.Instance.save.SetFlag("LOCATION_MONSTER_BOILS_REMOVED", true);
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }

                    return false;
                }
            }
        }


        public static readonly Dictionary<string, Cutscene> locationCutscenes = new(){};

        static bool isCollectingCutscene = false;
        static void CollectCutscene(Cutscene cs)
        {
            isCollectingCutscene = true;
            GlobalHub.Instance.SetCutscene(cs);
            isCollectingCutscene = false;
        }

        public static bool TryCollect(string flag)
        {
            if (locationCutscenes.TryGetValue(flag, out Cutscene cutscene)) {
                CollectCutscene(cutscene);
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(GlobalHub), nameof(GlobalHub.SetCutscene))]
        static class CutscenePatch
        {

            static bool Prefix(Cutscene cs)
            {
                string flag;
                if (cs.name == "PALACE_MELTED_ICE") {
                    flag = "PALACE_MELTED_ICE";
                } else {
                    flag = cs.flag;
                }
                if (locationCutscenes.ContainsKey(flag) && !isCollectingCutscene) {
                    GlobalHub.Instance.save.SetFlag("LOCATION_" + flag, true);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Princess), nameof(Princess.Interact))]
        static class PrincessInteractPatch {
            static MethodInfo setItemCutsceneInfo = typeof(Princess).GetMethod("SetItemCutscene", BindingFlags.Instance | BindingFlags.NonPublic);
            static void SetItemCutscene(Princess princess, int n, Cutscene cs)
            {
                setItemCutsceneInfo.Invoke(princess, new object[]{n, cs});
            }

            // lady opal should only unfreeze after you deliver her eggs
            static bool Prefix(Princess __instance)
            {
                Save save = GlobalHub.Instance.GetSave();

                if (save.GetFlag("LOCATION_PALACE_MELTED_ICE").On()) {
                    if (GlobalHub.Instance.save.GetFlag(NPC.FLAG_FINISHED).on)
                        GlobalHub.Instance.SetCutscene(__instance.speakPostgame);
                    else
                        GlobalHub.Instance.SetCutscene(__instance.speakThawed);
                }
                else if (save.GetFlag("ITEM_PRINCESS_1").On())
                    SetItemCutscene(__instance, 1, __instance.speakGiveItem1);
                else if (save.GetFlag("ITEM_PRINCESS_2").On())
                    SetItemCutscene(__instance, 2, __instance.speakGiveItem2);
                else if (save.GetFlag("ITEM_PRINCESS_3").On())
                    SetItemCutscene(__instance, 3, __instance.speakGiveItem3);
                else if (!save.GetFlag("LOCATION_PALACE_MELTED_ICE").On() && (save.GetFlag("ITEM_PRINCESS_1_GIVEN").On() || save.GetFlag("ITEM_PRINCESS_2_GIVEN").On() || save.GetFlag("ITEM_PRINCESS_3_GIVEN").On()))
                    GlobalHub.Instance.SetCutscene(__instance.speakFrozenIncomplete);
                else if (save.GetFlag("PRINCESS_SPEAK_FROZEN").on)
                    GlobalHub.Instance.SetCutscene(__instance.speakFrozenRepeat);
                else
                    GlobalHub.Instance.SetCutscene(__instance.speakFrozen);

                return false;
            }

        }

        static void PatchHeavensPath()
        {
            // shift ending door backwards to enable finishing the race with it closed
            var sanctumFellaGate = GlobalGameScene.FindInScene("PALACE", "Sanctum/seadeep/Fella_Gate");
            sanctumFellaGate.localPosition = sanctumFellaGate.localPosition with {
                z = -18.1423f
            };
        }

        public static void PatchLocations()
        {
            PatchHeavensPath();

            locationCutscenes.Clear();

            var cutscenes = GlobalGameScene.GetComponentsInChildren<Cutscene>(true).ToList();
            var switches = GlobalGameScene.GetComponentsInChildren<TimedSwitch>(true).ToList();
            foreach (var flagName in Data.eventItems.Keys) {
                Debug.Log(flagName);
                int cutsceneInd = cutscenes.FindIndex(
                    x => (flagName == "PALACE_MELTED_ICE" ? x.name : x.flag) == flagName
                );
                if (cutsceneInd >= 0) {
                    locationCutscenes.Add(flagName, cutscenes[cutsceneInd]);
                    continue;
                }

                int switchInd = switches.FindIndex(x => x.flag == flagName);
                if (switchInd >= 0) {
                    TimedSwitch ts = switches[switchInd];
                    ts.flag = "LOCATION_" + ts.flag;
                    ts.SetState();
                    continue;
                }

                Debug.LogError(String.Format("Failed to find flag for {0}", flagName));
            }
        }
    }
}