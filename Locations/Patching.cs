using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CoDArchipelago
{
    static class LocationPatching
    {
        public static void Collect(Collectible.CollectibleType type, Collectible collectible) {
            switch (type)
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
                if (collectible != null) {
                    APCollectible.CollectiblePatch.DoPlayGotItemCutscene(collectible);
                }
                GlobalHub.Instance.player.findSFX.Play();
                StockSFX.Instance.jingleCollectLarge.Play();
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
            case (Collectible.CollectibleType)APCollectible.APCollectibleType.Minor:
                APResources.apJingleSmall.Play();
                break;
            case (Collectible.CollectibleType)APCollectible.APCollectibleType.Major:
                if (collectible != null) {
                    APCollectible.CollectiblePatch.DoPlayGotItemCutscene(collectible);
                }
                GlobalHub.Instance.player.findSFX.Play();
                APResources.apJingleLarge.Play();
                break;
            case (Collectible.CollectibleType)APCollectible.APCollectibleType.Event:
                StockSFX.Instance.jingleGood.Play();
                break;
            default:
                Debug.LogWarning((object) ("Collectible type " + (object) type + " not supported"), collectible);
                break;
            }
            SaveHandler.SaveFile(GlobalHub.Instance.save, GlobalHub.numSaveFile);
        }


        public static void ReplaceWithMinorAPItem(GameObject gameObject)
        {
            GameObject newItem = UnityEngine.Object.Instantiate(APResources.apMinorObject, new Transform());
            newItem.GetComponent<TwoState>().flag = gameObject.GetComponent<TwoState>().flag;
            newItem.name = gameObject.name;
            newItem.transform.parent   = gameObject.transform.parent;
            newItem.transform.position = gameObject.transform.position;
            newItem.transform.rotation = gameObject.transform.rotation;

            UnityEngine.Object.Destroy(gameObject);
            newItem.GetComponent<TwoState>().SetState();
        }

        public static void ReplaceWithEgg(GameObject gameObject, string flagName)
        {
            GameObject newEgg = UnityEngine.Object.Instantiate(APResources.eggObject, new Transform());
            newEgg.GetComponent<TwoState>().flag = gameObject.GetComponent<TwoState>().flag;
            newEgg.name = gameObject.name;
            newEgg.transform.parent   = gameObject.transform.parent;
            newEgg.transform.position = gameObject.transform.position;
            newEgg.transform.rotation = gameObject.transform.rotation;

            newEgg.GetComponent<Fella>().texture = APResources.eggTextures[flagName];

            UnityEngine.Object.Destroy(gameObject);
            newEgg.GetComponent<TwoState>().SetState();
        }

        static readonly Dictionary<string, string> flagWorldMap = new(){
            {"ITEM_FISH_FOOD", "LAKE"},

            {"ITEM_PRINCESS_1", "PALACE"},
            {"ITEM_PRINCESS_2", "PALACE"},
            {"ITEM_PRINCESS_3", "PALACE"},

            {"GALLERY_TRAPDOOR_ACTIVE", "MONSTER"},
            {"FELLA_UNDEAD1", "MONSTER"},
            {"FELLA_DROWN1", "MONSTER"},
            {"FELLA_CHALICE1", "MONSTER"},
        };
        static readonly Regex _world_regex = new("^(?:LOCATION_)?([A-Z]+)_([A-Z]+)");

        public static void ReplaceWithShroom(GameObject gameObject, string flagName)
        {
            GameObject newShroom = UnityEngine.Object.Instantiate(APResources.shroomObject, new Transform());
            newShroom.GetComponent<TwoState>().flag = gameObject.GetComponent<TwoState>().flag;
            newShroom.name = gameObject.name;
            newShroom.transform.parent   = gameObject.transform.parent;
            newShroom.transform.position = gameObject.transform.position;
            newShroom.transform.rotation = gameObject.transform.rotation;

            SpriteRenderer renderer = newShroom.GetComponentInChildren<SpriteRenderer>();

            var m = _world_regex.Match(flagName);
            if (!m.Success) {
                if (!flagWorldMap.TryGetValue(flagName, out string worldName))
                    throw new System.Exception("failed to identify world for flag " + flagName);

                renderer.sprite = APResources.shroomSprites[worldName];
            } else {
                string worldName = m.Groups[2].Value;

                // gallery has no shroom sprite
                if (worldName == "GALLERY") worldName = "MONSTER";

                if (APResources.shroomSprites.TryGetValue(worldName, out Sprite sprite)) {
                    renderer.sprite = sprite;
                } else {
                    throw new System.Exception("unknown world for flag " + flagName);
                }
            }

            UnityEngine.Object.Destroy(gameObject);
            newShroom.GetComponent<TwoState>().SetState();
        }

        public static void ReplaceWithEvent(GameObject gameObject, string flagName)
        {
            GameObject newShroom = UnityEngine.Object.Instantiate(APResources.shroomObject, new Transform());
            newShroom.GetComponent<TwoState>().flag = gameObject.GetComponent<TwoState>().flag;
            newShroom.GetComponent<Collectible>().type = (Collectible.CollectibleType)APCollectible.APCollectibleType.Event;
            newShroom.name = gameObject.name;
            newShroom.transform.parent   = gameObject.transform.parent;
            newShroom.transform.position = gameObject.transform.position;
            newShroom.transform.rotation = gameObject.transform.rotation;

            SpriteRenderer renderer = newShroom.GetComponentInChildren<SpriteRenderer>();

            var m = _world_regex.Match(flagName);
            if (!m.Success) {
                if (!flagWorldMap.TryGetValue(flagName, out string worldName))
                    throw new System.Exception("failed to identify world for flag " + flagName);

                renderer.sprite = APResources.eventSprites[worldName];
            } else {
                string worldName = m.Groups[1].Value;
                Debug.Log(worldName);
                if (APResources.eventSprites.TryGetValue(worldName, out Sprite sprite)) {
                    renderer.sprite = sprite;
                } else {
                    throw new System.Exception("unknown world for flag " + flagName);
                }
            }

            UnityEngine.Object.Destroy(gameObject);
            newShroom.GetComponent<TwoState>().SetState();
        }
    }
}