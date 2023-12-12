
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    static class Collecting
    {
        public enum APCollectibleType {
            Minor,
            Major,
            Event,
            Ability
        }

        public abstract class ItemType {}

        public class VanillaType : ItemType {
            public Collectible.CollectibleType type;
            public VanillaType(Collectible.CollectibleType type) {
                this.type = type;
            }
        }

        public class APType : ItemType {
            public APCollectibleType type;
            public APType(APCollectibleType type) {
                this.type = type;
            }
        }

        public abstract class Item
        {
            public ItemType type;

            public abstract string GetFlag();

            public abstract void Collect();
        }

        public class ClientItem : Item
        {
            readonly bool isLocal;

            static ItemType GetFlagCollectibleType(string flag)
            {
                if (Data.pickupItems.ContainsKey(flag)) return new VanillaType(Collectible.CollectibleType.ITEM);
                if (Data.abilityItems.ContainsKey(flag) ||
                    Data.nonVanillaAbilityItems.ContainsKey(flag)) return new APType(APCollectibleType.Ability);
                if (Data.cardItems.ContainsKey(flag))   return new VanillaType(Collectible.CollectibleType.CARD);
                if (Data.eggItems.ContainsKey(flag))    return new VanillaType(Collectible.CollectibleType.FELLA);
                if (Data.eventItems.ContainsKey(flag))  return new APType     (APCollectibleType.Event);
                if (Data.shroomItems.ContainsKey(flag)) return new VanillaType(Collectible.CollectibleType.NOTE);

                throw new System.Exception("Unknown type for collectible " + flag);
            }

            readonly string flag;
            public override string GetFlag() => flag;

            public readonly bool randomized;

            static readonly Dictionary<string, Action<bool>> itemTriggers = new() {
                {"PALACE_SANCTUM_RACE_FINISHED", (randomized) => {
                    var area = GlobalGameScene.GetCurrentArea();
                    if (area.name != "Sanctum") return;

                    var gate = area.transform.Find("seadeep/Fella_Gate");
                    var raise = gate.GetComponent<Raise>();

                    if (!randomized) {
                        raise.activated = true;
                        return;
                    }

                    int raiseTime = 60;
                    raise.raiseTime = raiseTime;
                    raise.GetTimer().SetMax(raiseTime);

                    raise.Activate();
                }},
            };

            public ClientItem(string flag, bool randomized = true, bool isLocal = true)
            {
                this.flag = flag;
                this.randomized = randomized;
                this.isLocal = isLocal;
                type = GetFlagCollectibleType(flag);
            }

            public override void Collect()
            {
                CollectJingle(type);

                if (randomized) {
                    APTextLog.instance.AddLine("containing " + Data.allItems[flag]);
                }

                if (!isLocal) {
                    // TODO: collect for other players
                    throw new NotImplementedException();
                }

                if (itemTriggers.TryGetValue(flag, out Action<bool> itemTrigger)) {
                    itemTrigger(randomized);
                }

                bool collectedAsCutscene = Cutscenes.TryCollect(flag);

                if (!collectedAsCutscene) {
                    GlobalHub.Instance.save.SetFlag(flag, true);
                }

                if (type is VanillaType vanillaType && vanillaType.type != Collectible.CollectibleType.ITEM) {
                    GlobalHub.Instance.save.AddCollectible(vanillaType.type, 1);
                    UIController.Instance.SetModelVisible(vanillaType.type);
                    UIController.Instance.collectibleCounter.text = GlobalHub.Instance.save.GetCollectible(vanillaType.type).ToString();
                }
                // SaveHandler.SaveFile(GlobalHub.Instance.save, GlobalHub.numSaveFile);
            }
        }

        public class OtherClientItem : Item
        {
            public readonly int playerId;
            public readonly string itemName;

            public override string GetFlag() {
                if (Data.allItemsByName.TryGetValue(itemName, out string ret)) return ret;

                throw new Exception("invalid use of GetFlag: flag for " + itemName + " does not exist");
            }

            public OtherClientItem(int playerId_, string itemName_, bool isMajor)
            {
                playerId = playerId_;
                itemName = itemName_;

                type = new APType(isMajor ? APCollectibleType.Major : APCollectibleType.Minor);
            }

            public override void Collect()
            {
                CollectJingle(type);
                // SaveHandler.SaveFile(GlobalHub.Instance.save, GlobalHub.numSaveFile);
            }
        }

        static void CollectJingle(ItemType type) {
            if (type is APType apType) {
                switch (apType.type)
                {
                case APCollectibleType.Minor:
                    APResources.apJingleSmall.Play();
                    break;
                case APCollectibleType.Major:
                    // APResources.apJingleLarge.Play();
                    break;
                case APCollectibleType.Event:
                    StockSFX.Instance.jingleGood.Play();
                    break;
                case APCollectibleType.Ability:
                    APResources.grabAbilityJingle.Play();
                    GlobalHub.Instance.player.curiousSFX.Play();
                    break;
                default:
                    Debug.LogWarning("AP Collectible type " + type + " not supported");
                    break;
                }
            } else if (type is VanillaType vanillaType) {
                switch (vanillaType.type)
                {
                case Collectible.CollectibleType.GRATITUDE:
                    StockSFX.Instance.jingleCollectLarge.Play();
                    break;
                case Collectible.CollectibleType.NOTE:
                    StockSFX.Instance.jingleCollectTiny.Play();
                    break;
                case Collectible.CollectibleType.CARD:
                    StockSFX.Instance.jingleCollectSmall.Play();
                    break;
                case Collectible.CollectibleType.FELLA:
                    StockSFX.Instance.jingleCollectLarge.Play();
                    GlobalHub.Instance.player.findSFX.Play();
                    /*
                    int collectible = GlobalHub.Instance.save.GetCollectible(Collectible.CollectibleType.FELLA);
                    if (collectible == Sage.FLIGHT_REQ)
                    {
                        GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockFlightCutscene);
                    // GlobalHub.Instance.SetAchievement(this.ACHIEVEMENT_ALL_FELLAS);
                    }
                    else if (world.CheckAndSetAllFellasObtained())
                        GlobalHub.Instance.QueueCutscene(UnityEngine.Object.Instantiate<Cutscene>(world.painting ? GlobalHub.Instance.paintingFellaMessage : GlobalHub.Instance.allFellasMessage));
                    if (collectible == Sage.HOVER_REQ)
                    GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockHoverCutscene);
                    else if (collectible == Sage.DIVE_REQ)
                    GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockDiveCutscene);
                    else if (collectible == Sage.PROJECTILE_REQ)
                    GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockProjectileCutscene);
                    */
                    break;
                case Collectible.CollectibleType.ITEM:
                    StockSFX.Instance.jingleGoodShort.Play();
                    GlobalHub.Instance.player.curiousSFX.Play();
                    break;
                default:
                    Debug.LogWarning("Collectible type " + type + " not supported");
                    break;
                }
            }
        }

        public static Dictionary<string, Item> checks = new(){
            // {"LOCATION_LAKE_SWINGS_RAISED",          new ClientItem("ITEM_FISH_FOOD")},
            {"LOCATION_NOTE_LAKE41",                 new ClientItem("ITEM_FISH_FOOD")},
            {"LOCATION_NOTE_LAKE42",                 new ClientItem("SKILL_AIRATTACK")},
            {"LOCATION_LAKE_KAPPA_SUCCESS",          new ClientItem("SKILL_ROLL")},
            {"LOCATION_ITEM_FISH_FOOD",              new ClientItem("NOTE_CAVE64")},
            {"LOCATION_NOTE_CAVE2",                  new ClientItem("PALACE_LOBBY_FAUCET_ON")},
            {"LOCATION_NOTE_CAVE3",                  new OtherClientItem(0, "lad", false)},
            {"LOCATION_NOTE_LAKE38",                 new OtherClientItem(0, "lad", false)},
            {"LOCATION_NOTE_LAKE39",                 new OtherClientItem(0, "lad", false)},
            {"LOCATION_NOTE_LAKE40",                 new OtherClientItem(0, "lad", true)},
            {"LOCATION_NOTE_CAVE62",                 new ClientItem("PALACE_LOBBY_WHIRLPOOL_ON")},
            {"LOCATION_NOTE_CAVE50",                 new ClientItem("MONSTER_LOBBY_PIPES_RISEN")},
            {"LOCATION_NOTE_CAVE51",                 new ClientItem("MONSTER_LOBBY_STEAM")},
            {"LOCATION_NOTE_CAVE52",                 new ClientItem("PALACE_SANCTUM_RACE_FINISHED")},
            {"LOCATION_NOTE_LAKE31",                 new ClientItem("LAKE_KAPPA_SUCCESS")},
            {"LOCATION_NOTE_CAVE17",                 new ClientItem("CAVE_NURIKABE_FALLEN")},
            {"LOCATION_ITEM_PRINCESS_1",             new OtherClientItem(0, "lad", true)},
            {"LOCATION_ITEM_PRINCESS_2",             new OtherClientItem(0, "lad", false)},
            {"LOCATION_ITEM_PRINCESS_3",             new OtherClientItem(0, "lad", false)},
            {"LOCATION_NOTE_PALACE6",                new ClientItem("ITEM_PRINCESS_1")},
            {"LOCATION_NOTE_PALACE7",                new ClientItem("ITEM_PRINCESS_2")},
            {"LOCATION_NOTE_PALACE8",                new ClientItem("ITEM_PRINCESS_3")},
            {"LOCATION_NOTE_PALACE9",                new ClientItem("PALACE_MELTED_ICE")},
            {"LOCATION_NOTE_PALACE13",               new ClientItem("PALACE_SNOW_CASTLE_GATE_OPEN")},
            {"LOCATION_CARD_PALACE_PALM_TREE_FLOAT", new ClientItem("PALACE_LAKE_GATE_OPEN")},
            {"LOCATION_PALACE_LOBBY_WHIRLPOOL_ON",   new ClientItem("NOTE_CAVE62")},
            {"LOCATION_PALACE_LOBBY_FAUCET_ON",      new ClientItem("NOTE_CAVE63")},
        };

        [HarmonyPatch(typeof(Save), "SetFlag")]
        static class SetFlagPatch
        {

            static readonly Dictionary<string, Action> locationTriggers = new() {
                {"LOCATION_LAKE_SWINGS_RAISED", () => {
                    var smile = GlobalGameScene.FindInScene("LAKE", "Lake (Main)/lake2/TargetSmile");
                    smile.gameObject.SetActive(true);
                }},

                {"LOCATION_PALACE_MELTED_ICE", () => {
                    var princess = GlobalGameScene.FindInScene("PALACE", "Valley (Main)/NPCs/Princess");
                    princess.Find("princess2/Prison").gameObject.SetActive(false);
                    princess.Find("princess2/Body").GetComponent<TintChange>().Activate();
                    princess.GetComponent<Princess>().Whack(Whackable.WhackType.ATTACK, null);
                }},

                {"LOCATION_PALACE_DINING_ROOM_RISEN", () => {
                    var preston = GlobalGameScene.FindInScene("PALACE", "Dining Room/dining_room2/Throne_Switch");
                    StockSFX.Instance.click.Play();
                    preston.GetComponentInChildren<Raise>().Activate();
                }},
            };

            static void ToggleIce(bool enable)
            {
                Area area = GlobalGameScene.GetCurrentArea();
                if (area.name != "Valley (Main)") return;

                // resets music
                area.Activate(false, true);

                area.transform.Find("palace2/Ice_Melt").GetComponent<GrowFromNothingActivation>().activated = !enable;

                area.transform.Find("SFX/BlizzardSound").gameObject.SetActive(enable);

                area.transform.Find("FX/Blizzard").gameObject.SetActive(enable);
                area.transform.Find("FX/SnowPalace").gameObject.SetActive(!enable);
            }
            
            /*
            static bool Prefix(Save __instance, string name)
            {
                if (name.StartsWith("LOCATION") && __instance.GetFlag(name).on) return false;
                
                return true;
            }
            */

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
                    APTextLog.instance.AddLine("Collected <color=purple>" + Data.allLocations[name] + "</color>");
                    item.Collect();
                } else {
                    new ClientItem(name.Substring(9), false).Collect();
                }
            }
        }
    }
}