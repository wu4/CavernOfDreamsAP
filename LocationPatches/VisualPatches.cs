using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using HarmonyLib;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using JetBrains.Annotations;
using System.Reflection;

namespace CoDArchipelago
{
    static class VisualPatches
    {
        static void PatchCollectible(Collectible col)
        {
            TwoState ts = col.GetComponent<TwoState>();
            GameObject obj = col.gameObject;

            Collecting.Item item;

            if (!Collecting.checks.TryGetValue(ts.flag, out item))
                item = new Collecting.ClientItem(ts.flag.Substring(9), false);

            if (item.type is Collecting.VanillaType vanillaType) {
                switch (vanillaType.type)
                {
                case Collectible.CollectibleType.FELLA:
                    var egg = ReplaceObject(obj, APResources.eggObject);
                    egg.GetComponent<Fella>().texture = APResources.eggTextures[item.GetFlag()];
                    break;
                case Collectible.CollectibleType.ITEM:
                    ReplaceWithItem(obj, item.GetFlag());
                    break;
                case Collectible.CollectibleType.NOTE:
                    ReplaceWithShroom(obj, item.GetFlag());
                    break;
                case Collectible.CollectibleType.CARD:
                    ReplaceObject(obj, APResources.cardObject);
                    break;
                default:
                    throw new Exception("unknown local item type " + vanillaType.type);
                }
            } else if (item.type is Collecting.APType apType) {
                switch (apType.type)
                {
                case Collecting.APCollectibleType.Major:
                    ReplaceObject(obj, APResources.apMajorObject);
                    break;
                case Collecting.APCollectibleType.Minor:
                    ReplaceObject(obj, APResources.apMinorObject);
                    break;
                case Collecting.APCollectibleType.Event:
                    ReplaceWithEvent(obj, item.GetFlag());
                    break;
                case Collecting.APCollectibleType.Ability:
                    ReplaceObject(obj, APResources.orbObject);
                    break;
                default:
                    throw new Exception("unknown AP item type " + apType.type);
                }
            }
        }

        [HarmonyPatch(typeof(Area), "Activate")]
        static class PatchVisualsOnAreaLoad
        {
            static bool Prefix(Area __instance, ref bool __runOriginal)
            {
                if (!__runOriginal) return false;

                if (__instance.transform.Find("AlreadyPatched") != null) return true;

                // checkpoints could potentially mess with accessibility and may create softlocks
                foreach (Checkpoint cp in __instance.GetComponentsInChildren<Checkpoint>()) {
                    GameObject.Destroy(cp.gameObject);
                }
            
                var cols = __instance.GetComponentsInChildren<Collectible>(true);

                foreach (Collectible col in cols) {
                    TwoState ts = col.GetComponent<TwoState>();

                    // Gallery lobby contains a fake egg. Its associated
                    // cutscene is skipped, so we never see the egg anyways
                    if (ts.flag == "GALLERY_TRAPDOOR_ACTIVE") {
                        GameObject.Destroy(col.gameObject);
                        continue;
                    }

                    ts.flag = "LOCATION_" + ts.flag;

                    PatchCollectible(col);
                }

                GameObject g = new();
                g.name = "AlreadyPatched";
                g.transform.parent = __instance.transform;

                return true;
            }
        }

        static GameObject ReplaceObject(GameObject gameObject, GameObject with)
        {
            GameObject newObject = UnityEngine.Object.Instantiate(with, new Transform());
            newObject.GetComponent<TwoState>().flag = gameObject.GetComponent<TwoState>().flag;
            newObject.name = gameObject.name;
            newObject.transform.parent   = gameObject.transform.parent;
            newObject.transform.position = gameObject.transform.position;
            newObject.transform.rotation = gameObject.transform.rotation;

            GameObject.Destroy(gameObject);

            return newObject;
        }

        static void ReplaceWithSprite(GameObject gameObject, Sprite sprite) =>
            ReplaceObject(gameObject, APResources.shroomObject)
                .GetComponentInChildren<SpriteRenderer>().sprite = sprite;

        /*
        static readonly Dictionary<string, string> shroomWorldMap = new(){
            {"ITEM_FISH_FOOD", "LAKE"},

            {"ITEM_PRINCESS_1", "PALACE"},
            {"ITEM_PRINCESS_2", "PALACE"},
            {"ITEM_PRINCESS_3", "PALACE"},

            {"GALLERY_TRAPDOOR_ACTIVE", "MONSTER"},
            {"FELLA_UNDEAD1", "MONSTER"},
            {"FELLA_DROWN1", "MONSTER"},
            {"FELLA_CHALICE1", "MONSTER"},
        };
        */

        public static void ReplaceWithItem(GameObject gameObject, string flagName)
        {
            if (flagName == "ITEM_FISH_FOOD") ReplaceObject(gameObject, APResources.fishFoodObject);
            else if (flagName == "ITEM_PRINCESS_1") ReplaceObject(gameObject, APResources.ladyOpalEggObjects[0]);
            else if (flagName == "ITEM_PRINCESS_2") ReplaceObject(gameObject, APResources.ladyOpalEggObjects[1]);
            else if (flagName == "ITEM_PRINCESS_3") ReplaceObject(gameObject, APResources.ladyOpalEggObjects[2]);
        }

        static readonly Dictionary<string, string> eventWorldMap = new(){
            {"MONSTER_LOBBY_PIPES_RISEN", "CAVE"},
            {"MONSTER_LOBBY_STEAM", "CAVE"},
            {"PALACE_LOBBY_FAUCET_ON", "CAVE"},
            {"PALACE_LOBBY_WHIRLPOOL_ON", "CAVE"},

            {"GRATITUDE_DOOR1", "CAVE"},
        };

        static readonly Regex eventWorldRegex = new("^([A-Z]+)_");
        static readonly Regex shroomWorldRegex = new("^NOTE_([A-Z]+)[0-9]+$");

        static string GetWorldFromFlag(string flagName, Regex regex)
        {
            var m = regex.Match(flagName);
            if (m.Success) {
                return m.Groups[1].Value;
            } else {
                return null;
            }
        }

        public static void ReplaceWithShroom(GameObject gameObject, string flagName)
        {
            string worldName = GetWorldFromFlag(flagName, shroomWorldRegex)
                ?? throw new Exception("failed to identify world for shroom " + flagName);

            ReplaceWithSprite(gameObject, APResources.shroomSprites[worldName]);
        }

        public static void ReplaceWithEvent(GameObject gameObject, string flagName)
        {
            if (!eventWorldMap.TryGetValue(flagName, out string worldName))
                worldName = GetWorldFromFlag(flagName, eventWorldRegex)
                    ?? throw new Exception("failed to identify world for event " + flagName);

            ReplaceWithSprite(gameObject, APResources.eventSprites[worldName]);
        }
    }
}