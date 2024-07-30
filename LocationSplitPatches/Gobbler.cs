using System;
using CoDArchipelago.GlobalGameScene;
using UnityEngine;

namespace CoDArchipelago.LocationSplitPatches
{
    class Gobbler : InstantiateOnGameSceneLoad
    {
        [LoadOrder(Int32.MinValue + 1)]
        public Gobbler()
        {
            DetachSeedTriggerFromGobbler();
            // TODO: make the gobbler eat the apple then return to their previous state
        }

        static void DetachSeedTriggerFromGobbler()
        {
            Transform parent = GameScene.FindInScene("PALACE", "Valley (Main)/NPCs");
            Transform seedTriggerT = parent.Find("Moray Awake/MoraySeedTrigger");
            seedTriggerT.SetParent(parent, true);

            CutsceneTrigger seedTrigger = seedTriggerT.GetComponent<CutsceneTrigger>();
            seedTrigger.inactiveFlag = "LOCATION_PALACE_MORAY_FED";
        }
    }
}
