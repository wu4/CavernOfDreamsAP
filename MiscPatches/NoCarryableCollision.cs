using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    class NoCarryableCollision : InstantiateOnGameSceneLoad
    {
        public NoCarryableCollision()
        {
            RemoveAllTreeComponents();
        }

        ///<summary>
        ///These only serve to drop the item they're holding.
        ///Fynn can grab carryables in the air now, so there's no need.
        ///(Also saves the headache of physics on all items)
        ///</summary>
        static void RemoveAllTreeComponents()
        {
            foreach (Tree tree in GameScene.GetComponentsInChildren<Tree>(true)) {
                Component.DestroyImmediate(tree);
            }
        }
    }
}
