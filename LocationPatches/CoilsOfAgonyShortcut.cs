using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationPatches
{
    class CoilsOfAgonyShortcut : InstantiateOnGameSceneLoad
    {
        public CoilsOfAgonyShortcut()
        {
            // They are NOT FRIENDS. I believe in BRUTAL SNAKE TENSION
            // TieSnakesTogether();
        }

        ///<summary>
        ///The lower snake has its own flag for when you first enter. Let's
        ///make them friends with the upper snake, and open when they do.
        ///</summary>
        static void TieSnakesTogether()
        {
            Transform snexitTrigger = GameScene.FindInScene("CHALICE", "Chalice (Main)/Cutscenes/SnexitCutsceneTrigger");
            GameObject.Destroy(snexitTrigger.gameObject);

            Transform upperJaw = GameScene.FindInScene("CHALICE", "Chalice (Main)/chalice/Snake_Upper_Jaw_001");
            RotateActivation activation = upperJaw.GetComponent<RotateActivation>();
            activation.flag = "CHALICE_BRIDGE_ACTIVE";

            Collecting.MyItem.RegisterTrigger(
                "CHALICE_BRIDGE_ACTIVE",
                (randomized) => {
                    activation.Activate();
                }
            );
        }
    }
}
