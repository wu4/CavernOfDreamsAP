using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.Collecting
{
    enum APCollectibleType {
        Minor,
        Major,
        Event,
        Ability
    }
    
    abstract class Item
    {
        public Enum type;

        public abstract string GetFlag();

        public abstract void Collect();
        
        /*
        public static void CollectJingle(Enum type)
        {
            if (type is APCollectibleType apType) {
                switch (apType)
                {
                case APCollectibleType.Minor:
                    APResources.Instance.apJingleSmall.Play();
                    break;
                case APCollectibleType.Major:
                    // APResources.apJingleLarge.Play();
                    break;
                case APCollectibleType.Event:
                    StockSFX.Instance.jingleGood.Play();
                    break;
                case APCollectibleType.Ability:
                    APResources.Instance.grabAbilityJingle.Play();
                    GlobalHub.Instance.player.curiousSFX.Play();
                    break;
                default:
                    Debug.LogWarning("AP Collectible type " + type + " not supported");
                    break;
                }
            } else if (type is Collectible.CollectibleType vanillaType) {
                switch (vanillaType)
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
                    // int collectible = GlobalHub.Instance.save.GetCollectible(Collectible.CollectibleType.FELLA);
                    // if (collectible == Sage.FLIGHT_REQ)
                    // {
                    //     GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockFlightCutscene);
                    // // GlobalHub.Instance.SetAchievement(this.ACHIEVEMENT_ALL_FELLAS);
                    // }
                    // else if (world.CheckAndSetAllFellasObtained())
                    //     GlobalHub.Instance.QueueCutscene(UnityEngine.Object.Instantiate<Cutscene>(world.painting ? GlobalHub.Instance.paintingFellaMessage : GlobalHub.Instance.allFellasMessage));
                    // if (collectible == Sage.HOVER_REQ)
                    // GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockHoverCutscene);
                    // else if (collectible == Sage.DIVE_REQ)
                    // GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockDiveCutscene);
                    // else if (collectible == Sage.PROJECTILE_REQ)
                    // GlobalHub.Instance.QueueCutscene(GlobalHub.Instance.unlockProjectileCutscene);
                    break;
                case Collectible.CollectibleType.ITEM:
                    StockSFX.Instance.jingleGoodShort.Play();
                    GlobalHub.Instance.player.curiousSFX.Play();
                    break;
                default:
                    Debug.LogWarning("Collectible type " + type + " not supported");
                    break;
                }
            } else {
                throw new ArgumentException("Collectible kind " + type + " unknown");
            }
        }
        */
    }
}
