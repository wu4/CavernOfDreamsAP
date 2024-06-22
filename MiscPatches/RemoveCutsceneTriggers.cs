using System.Collections.Generic;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    class RemoveCutsceneTriggers : InstantiateOnGameSceneLoad
    {
        public RemoveCutsceneTriggers()
        {
            foreach ((string rootName, string[] blacklist) in triggerBlacklist) {
                GameObject root = GameScene.GetRootObjectByName(rootName);

                foreach (string path in blacklist) {
                    GameObject.Destroy(root.transform.Find(path).gameObject);
                }
            }
        }

        static readonly Dictionary<string, string[]> triggerBlacklist = new() {
            {"CAVE", new[] {
                "Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Nurikabe CT",
                "Sun Cavern (Main)/Cutscenes/Misc Cutscenes/Challenge Nurikabe CT",
                "Gallery Lobby/Cutscenes/OpenHedgeMazeExploitTrigger"
            }},

            {"LAKE", new[] {
                "Lake (Main)/Cutscenes/GroveHelloTrigger",
                "Lake (Main)/Cutscenes/ReactivateGroveHelloTrigger",
                "Lake (Main)/Cutscenes/KappaGoodbyeTrigger",
                "Lake (Main)/Cutscenes/EnableGoodbyeTrigger",
            }},

            {"PALACE", new[] {
                "Valley (Main)/Cutscenes/HelloTrigger",
                "Valley (Main)/Cutscenes/EnableHelloTrigger",
                "Valley (Main)/Cutscenes/GoodbyeTrigger",
                "Valley (Main)/Cutscenes/EnableGoodbyeTrigger",
            }}
        };
    }
}
