using CoDArchipelago.GlobalGameScene;
using UnityEngine;

namespace CoDArchipelago.MiscPatches
{
    class KerringtonRace : InstantiateOnGameSceneLoad
    {
        static Transform heartGate;

        public KerringtonRace()
        {
            heartGate = GameScene.FindInScene("MONSTER", "Monster/Rotate (Inside Monster)/monster_inside2/HeartGate");
            MoveHeartGate();
            RemoveHeartSafetyTrigger();
            Collecting.MyItem.RegisterTrigger("MONSTER_HEART_GATE_OPEN", OpenHeartGate);
        }

        static void OpenHeartGate(bool randomized)
        {
            if (!randomized || GlobalHub.Instance.GetArea().name == "Monster") {
                // heartGate.GetComponent<Activation>().Activate();
                heartGate.gameObject.SetActive(false);
            }
        }

        static void MoveHeartGate()
        {
            heartGate.localPosition = heartGate.localPosition with {
                x = -26f,
                y = 17.6f
            };
        }

        static void RemoveHeartSafetyTrigger()
        {
            Transform safetyTrigger = GameScene.FindInScene("MONSTER", "Monster/Rotate (Inside Monster)/Objects (Cargo)/HeartSafetyTrigger");
            GameObject.Destroy(safetyTrigger.gameObject);
        }
    }
}
