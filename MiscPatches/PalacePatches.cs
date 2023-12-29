using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    class PalacePatches : InstantiateOnGameSceneLoad
    {
        public PalacePatches()
        {
            PatchObservatoryDoorState();
            PatchPalaceLakeConnectorEntry();
        }

        /// <summary>
        /// Changes the Observatory door's requirement such that it remains
        /// open once the ice has been melted, regardless of if you re-freeze
        /// it or not.
        /// </summary>
        static void PatchObservatoryDoorState()
        {
            GameScene.FindInScene("PALACE", "Valley (Main)/palace2/Observatory_Door").GetComponent<TwoState>().flag = "HAS_PALACE_MELTED_ICE";
        }

        /// <summary>
        /// Adds a bridge to the Prismic Palace -> Lostleaf Lake connector such
        /// that you no longer drown on entry or risk death when trying to
        /// traverse it without the ability to swim.
        /// </summary>
        static void PatchPalaceLakeConnectorEntry()
        {
            Transform parent = GameScene.FindInScene("PALACE", "Valley (Main)/palace2");
            GameObject bridge = GameObject.Instantiate(parent.Find("Bridge").gameObject, parent, false);
            bridge.transform.localPosition = new Vector3(-44f, -1f, -30f);
            bridge.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            bridge.transform.localScale = new Vector3(3f, 1.125f, 0.5f);
        }
    }
}