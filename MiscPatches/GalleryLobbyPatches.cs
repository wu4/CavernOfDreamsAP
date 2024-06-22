using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using System.Collections.Generic;

namespace CoDArchipelago.MiscPatches
{
    class GalleryLobbyPatches : InstantiateOnGameSceneLoad
    {
        public GalleryLobbyPatches()
        {
            MakeRoseDewaterable();
        }

        class Dewaterable : Whackable
        {
            protected override bool HandleWhack(Whackable.WhackType type, GameObject whacker)
            {
                Transform roseWatered = transform.GetParent();
                roseWatered.GetComponent<GrowFromNothingActivation>().activated = false;
                roseWatered.GetParent().Find("RoseWilted").GetComponent<GrowFromNothingActivation>().activated = false;

                return true;
            }
        }

        /// <summary>
        /// Makes it so you can un-water the rose in the Hedge Maze, making
        /// tech surrounding it no longer an Indiana Jones ordeal
        /// </summary>
        static void MakeRoseDewaterable()
        {
            Transform rose = GameScene.FindInScene("CAVE", "Gallery Lobby/HedgeMaze/Objects/DeadRose");

            Cutscene growDeadRoseCutscene = rose.Find("GrowDeadRoseCutscene").GetComponent<Cutscene>();
            growDeadRoseCutscene.flag = "";

            Transform roseWilted = rose.Find("RoseWilted");

            roseWilted.Find("hedge_maze_plant").GetComponent<WhackForCutscene>().canRepeat = true;

            Component.DestroyImmediate(roseWilted.GetComponent<TwoStateExists>());
            roseWilted.GetComponent<GrowFromNothingActivation>().flag = "";

            Transform roseWatered = rose.Find("RoseWatered");
            roseWatered.GetComponent<GrowFromNothingActivation>().flag = "";

            Dewaterable dewater = roseWatered.Find("hedge_maze_plant").gameObject.AddComponent<Dewaterable>();
            dewater.projectileWorks = true;

            dewater.diveWorks = false;
            dewater.rollWorks = false;
            dewater.throwWorks = false;
            dewater.attackWorks = false;
            dewater.specialWorks = false;
            dewater.cutsceneWorks = false;
        }

        [HarmonyPatch(typeof(Area), "Activate")]
        static class UnwaterRose
        {
            static Access.Field<Cutscene, List<Event>> cutsceneEvents = new("events");

            static bool Prefix(ref bool __runOriginal)
            {
                if (!__runOriginal) return false;

                Transform rose = GameScene.FindInScene("CAVE", "Gallery Lobby/HedgeMaze/Objects/DeadRose");

                Cutscene growDeadRoseCutscene = rose.Find("GrowDeadRoseCutscene").GetComponent<Cutscene>();
                var events = cutsceneEvents.Get(growDeadRoseCutscene);
                if (events != null) {
                    // the arguments do absolutely nothing in the source
                    growDeadRoseCutscene.Reset(start: false, init: false);
                }

                Transform roseWatered = rose.Find("RoseWatered");
                // roseWatered.gameObject.SetActive(true);
                roseWatered.GetComponent<GrowFromNothingActivation>().activated = false;

                Transform roseWilted = rose.Find("RoseWilted");
                // roseWilted.gameObject.SetActive(true);
                roseWilted.GetComponent<GrowFromNothingActivation>().activated = false;

                return true;
            }
        }
    }
}
