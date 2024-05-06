
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using CoDArchipelago.VisualPatches;

namespace CoDArchipelago.Collecting
{
    static class Location
    {
        public static Dictionary<string, Item> checks = new(){
            // {"LOCATION_NOTE_CAVE2",                  new ClientItem("SKILL_CLIMB")},
            {"LOCATION_LAKE_BELL_RANG",              new MyItem("FELLA_LAKE1")},
            {"LOCATION_NOTE_CAVE46",                 new MyItem("TELEPORT_LAKE")},
            // {"LOCATION_NOTE_CAVE2",                  new ClientItem("FELLA_LAKE1")},
            {"LOCATION_SKILL_ATTACK",                new MyItem("SKILL_GROUNDATTACK")},
            {"LOCATION_NOTE_CAVE3",                  new MyItem("GRATITUDE2")},
            {"LOCATION_NOTE_CAVE4",                  new MyItem("GRATITUDE3")},
            {"LOCATION_NOTE_CAVE5",                  new MyItem("GRATITUDE4")},
            {"LOCATION_NOTE_CAVE6",                  new MyItem("SKILL_AIRATTACK")},
            {"LOCATION_NOTE_CAVE7",                  new MyItem("SKILL_CARRY")},
            {"LOCATION_NOTE_CAVE8",                  new MyItem("SKILL_SWIM")},
            {"LOCATION_FELLA_UNDEAD1",               new MyItem("SKILL_GROUNDATTACK")},
            // {"LOCATION_LAKE_SWINGS_RAISED",          new ClientItem("ITEM_FISH_FOOD")},
            // {"LOCATION_NOTE_LAKE41",                 new ClientItem("ITEM_FISH_FOOD")},
            // {"LOCATION_NOTE_LAKE42",                 new ClientItem("SKILL_AIRATTACK")},
            // {"LOCATION_LAKE_KAPPA_SUCCESS",          new ClientItem("SKILL_ROLL")},
            // {"LOCATION_ITEM_FISH_FOOD",              new ClientItem("NOTE_CAVE64")},
            // {"LOCATION_NOTE_CAVE2",                  new ClientItem("PALACE_LOBBY_FAUCET_ON")},
            // {"LOCATION_NOTE_CAVE3",                  new OtherClientItem(0, "lad", false)},
            // {"LOCATION_NOTE_LAKE38",                 new OtherClientItem(0, "lad", false)},
            // {"LOCATION_NOTE_LAKE39",                 new OtherClientItem(0, "lad", false)},
            // {"LOCATION_NOTE_LAKE40",                 new OtherClientItem(0, "lad", true)},
            // {"LOCATION_NOTE_CAVE62",                 new ClientItem("PALACE_LOBBY_WHIRLPOOL_ON")},
            // {"LOCATION_NOTE_CAVE50",                 new ClientItem("MONSTER_LOBBY_PIPES_RISEN")},
            // {"LOCATION_NOTE_CAVE51",                 new ClientItem("MONSTER_LOBBY_STEAM")},
            // {"LOCATION_NOTE_CAVE52",                 new ClientItem("PALACE_SANCTUM_RACE_FINISHED")},
            // {"LOCATION_NOTE_LAKE31",                 new ClientItem("LAKE_KAPPA_SUCCESS")},
            // {"LOCATION_NOTE_CAVE17",                 new ClientItem("CAVE_NURIKABE_FALLEN")},
            // {"LOCATION_ITEM_PRINCESS_1",             new OtherClientItem(0, "lad", true)},
            // {"LOCATION_ITEM_PRINCESS_2",             new OtherClientItem(0, "lad", false)},
            // {"LOCATION_ITEM_PRINCESS_3",             new OtherClientItem(0, "lad", false)},
            // {"LOCATION_NOTE_PALACE6",                new ClientItem("ITEM_PRINCESS_1")},
            // {"LOCATION_NOTE_PALACE7",                new ClientItem("ITEM_PRINCESS_2")},
            // {"LOCATION_NOTE_PALACE8",                new ClientItem("ITEM_PRINCESS_3")},
            // {"LOCATION_NOTE_PALACE9",                new ClientItem("PALACE_MELTED_ICE")},
            // {"LOCATION_NOTE_PALACE13",               new ClientItem("PALACE_SNOW_CASTLE_GATE_OPEN")},
            // {"LOCATION_CARD_PALACE_PALM_TREE_FLOAT", new ClientItem("PALACE_LAKE_GATE_OPEN")},
            // {"LOCATION_PALACE_LOBBY_WHIRLPOOL_ON",   new ClientItem("NOTE_CAVE62")},
            // {"LOCATION_PALACE_LOBBY_FAUCET_ON",      new ClientItem("NOTE_CAVE63")},
        };
        
        // [HarmonyPatch(typeof(Save), nameof(Save.GetFlag))]
        // static class GetFlagPatch
        // {
        //     static void Postfix(string name)
        //     {
        //         Debug.Log(DateTime.Now + ": GET FLAG " + name);
        //     }
        // }
        // 

        class ResetTriggersOnLoad : InstantiateOnGameSceneLoad
        {
            [LoadOrder(int.MinValue)]
            public ResetTriggersOnLoad() => locationTriggers.Clear();
        }

        static readonly Dictionary<string, Action> locationTriggers = new();
        public static void RegisterTrigger(string locationFlag, Action action) =>
            locationTriggers.Add(locationFlag, action);

        [HarmonyPatch(typeof(Save), nameof(Save.SetFlag))]
        static class SetFlagPatch
        {
            static void ToggleIce(bool enable)
            {
                Area area = GlobalHub.Instance.GetArea();
                if (area.name != "Valley (Main)") return;

                // resets music
                area.Activate(false, true);

                area.transform.Find("palace2/Ice_Melt").GetComponent<GrowFromNothingActivation>().activated = !enable;

                area.transform.Find("SFX/BlizzardSound").gameObject.SetActive(enable);

                area.transform.Find("FX/Blizzard").gameObject.SetActive(enable);
                area.transform.Find("FX/SnowPalace").gameObject.SetActive(!enable);
            }

            static void Postfix(Save __instance, string name, bool b)
            {
                if (name.StartsWith("HAS_")) return;

                if (name == "PALACE_MELTED_ICE") {
                    ToggleIce(!b);
                }

                if (!b) return;

                if (name.StartsWith("SKILL") || name == "PALACE_MELTED_ICE") {
                    __instance.SetFlag("HAS_" + name, true);
                }

                if (!name.StartsWith("LOCATION")) return;

                if (locationTriggers.TryGetValue(name, out Action locationTrigger)) {
                    locationTrigger();
                }

                if (checks.TryGetValue(name, out Item item)) {
                    Messaging.TextLogManager.AddLine("Collected <color=purple>" + Data.allLocations[name] + "</color>");
                } else {
                    item = new MyItem(name.Substring(9), false);
                }
                item.Collect();
                VisualPatches.VisualPatches.CollectJingle(item);
            }
        }
    }
}