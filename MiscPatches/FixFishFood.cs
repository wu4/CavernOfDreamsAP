using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    /// <summary>
    /// The Fish Food in the Bedroom has an unnecessary extra TwoState. This
    /// can be a source of problems. So let's go nuclear.
    /// </summary>
    class FixFishFood : InstantiateOnGameSceneLoad
    {
        [LoadOrder(int.MinValue + 1)]
        public FixFishFood()
        {
            Transform fishFoodHolder = GameScene.FindInScene("LAKE", "Bedroom/Collectibles/FishFoodHolder");
            Component.DestroyImmediate(fishFoodHolder.GetComponent<TwoState>());
        }
    }
}
