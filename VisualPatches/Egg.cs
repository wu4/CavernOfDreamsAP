using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;
using System.Collections.Generic;
using System.Linq;

namespace CoDArchipelago.VisualPatches
{
    class Egg : ObjectPatcher
    {
        public static readonly new Replaces replaces = new(CollectibleItem.CollectibleType.FELLA);

        public static Dictionary<string, Texture> eggTexturesByFlag;
        static GameObject egg;

        public override GameObject Replace(GameObject toReplace, Item item)
        {
            var eggObj = ReplaceWith(toReplace, Egg.egg);
            eggObj.GetComponent<Fella>().texture = eggTexturesByFlag[item.GetFlag()];
            return eggObj;
        }

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleCollectLarge.Play();
            GlobalHub.Instance.player.findSFX.Play();
        }

        public Egg()
        {
            eggTexturesByFlag = GameScene.GetComponentsInChildren<Fella>(true)
                .GroupBy(o => o.GetComponent<TwoState>().flag)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().texture
                );

            Transform caveCollectibles = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles");

            egg = GameObject.Instantiate(caveCollectibles.Find("Fella 2 (Waterfall)").gameObject, Container);
            egg.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            egg.name = "Egg";
            egg.GetComponent<Collectible>().cutscene = FellaCutscene;
        }
    }
}
