using System.Text.RegularExpressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using CoDArchipelago.Collecting;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.FlagCache;
using System.Threading.Tasks;

namespace CoDArchipelago
{
    static class GratitudeTeleportPatches
    {
        class LinkGratitudeWithTeleports : InstantiateOnGameSceneLoad
        {
            static readonly Dictionary<string, string> gratitudeTeleportFlagMap = new() {
                {"GRATITUDE1", "TELEPORT_LAKE"},
                {"GRATITUDE2", "TELEPORT_MONSTER"},
                {"GRATITUDE3", "TELEPORT_PALACE"},
                {"GRATITUDE4", "TELEPORT_GALLERY"},
            };
            static Action<bool> SetTeleportFlagFactory(string teleportFlag) =>
                gratitudeRandomized => new MyItem(teleportFlag, randomized: gratitudeRandomized || CachedAPFlags.splitGratitudeAndTeleports).Collect();

            static void RegisterGratitudeTeleportLink(string gratitudeFlag, string teleportFlag) =>
                MyItem.RegisterTrigger(gratitudeFlag, SetTeleportFlagFactory(teleportFlag));

            public LinkGratitudeWithTeleports()
            {
                if (!CachedAPFlags.splitGratitudeAndTeleports) {
                    foreach ((string gratitudeFlag, string teleportFlag) in gratitudeTeleportFlagMap) {
                        RegisterGratitudeTeleportLink(gratitudeFlag, teleportFlag);
                    }
                }
            }
        }

        class PatchTeleports : InstantiateOnGameSceneLoad
        {
            public static readonly Dictionary<string, string> fellaNestTeleportMap = new() {
                {"Nest FellaHatchable Lake",    "TELEPORT_LAKE"},
                {"Nest FellaHatchable Monster", "TELEPORT_MONSTER"},
                {"Nest FellaHatchable Palace",  "TELEPORT_PALACE"},
                {"Nest FellaHatchable Gallery", "TELEPORT_GALLERY"},
            };

            static Action<bool> ActivatePortalFactory(Transform nestPortal)
            {
                Transform destinationPortal = nestPortal.GetComponent<WarpTrigger>().destination.transform.parent;

                return randomized => {
                    if (!randomized) {
                        ShowPortal(nestPortal);
                        return;
                    }

                    Area area = GlobalHub.Instance.GetArea();

                    if (area.name == "Sun Cavern (Main)") {
                        ShowPortal(nestPortal);
                    } else if (area.ContainsComponentInChildren(destinationPortal.GetComponent<WarpTrigger>(), includeInactive: true)) {
                        ShowPortal(destinationPortal);
                    }
                };
            }
            
            static void InitializePortal(Transform portal, string flag, bool withDestination = false)
            {
                portal.GetComponent<TwoState>().flag = flag;
                Transform modelHolder = portal.Find("PortalModelHolder");
                modelHolder.GetComponent<TwoState>().flag = flag;
                
                if (withDestination) {
                    InitializePortal(portal.GetComponent<WarpTrigger>().destination.transform.parent, flag, false);
                }
            }
            
            static void ShowPortal(Transform portal)
            {
                portal.gameObject.SetActive(true);
                Transform modelHolder = portal.Find("PortalModelHolder");
                modelHolder.GetComponent<Activation>().Activate();
            }

            public PatchTeleports()
            {
                var fellas = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Fellas");
                foreach ((string nestName, string teleportFlag) in fellaNestTeleportMap) {
                    var nestPortal = fellas.Find(nestName + "/Portal");
                    
                    InitializePortal(nestPortal, teleportFlag, withDestination: true);

                    MyItem.RegisterTrigger(teleportFlag, ActivatePortalFactory(nestPortal));
                }
            }
        }
    }
}