
using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    /// <summary>
    /// Checkpoints can potentially mess with accessibility in weird and
    /// unpredictable ways, and could also lead to some nasty softlocks.
    /// So, uh. I'm going nuclear now.
    /// </summary>
    class RemoveCheckpoints : InstantiateOnGameSceneLoad
    {
        public RemoveCheckpoints()
        {
            GameScene.GetComponentsInChildren<Checkpoint>(true).Do(GameObject.Destroy);
        }
    }
}