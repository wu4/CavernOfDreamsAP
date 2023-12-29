using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;
using System.Collections.Generic;
using System.Linq;

namespace CoDArchipelago.VisualPatches
{
    class Gratitude : StaticObjectPatcher
    {
        public static new Replaces replaces = new(CollectibleItem.CollectibleType.GRATITUDE);

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleCollectLarge.Play();
        }

        public Gratitude()
        {
            Transform caveCollectibles = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles");

            staticReplacement = GameObject.Instantiate(caveCollectibles.Find("Fella 2 (Waterfall)").gameObject, Container);
            staticReplacement.name = "Gratitude";

            // Fellas cry. Major AP pickups don't
            Fella fellaComponent = staticReplacement.GetComponent<Fella>();
            GameObject.DestroyImmediate(fellaComponent.crySFX.gameObject);
            Component.DestroyImmediate(fellaComponent);

            Collectible gratitudeCollectible = staticReplacement.AddComponent<Collectible>();
            gratitudeCollectible.type = Collectible.CollectibleType.FELLA;
            gratitudeCollectible.model = staticReplacement.transform.Find("FellaHolder").gameObject;
            gratitudeCollectible.cutscene = FellaCutscene;

            GameObject originalEgg = staticReplacement.transform.Find("FellaHolder/fella_egg").gameObject;
            while (originalEgg.transform.childCount > 0) {
                GameObject.DestroyImmediate(originalEgg.transform.GetChild(0).gameObject);
            }

            GameObject gratitudeCollectibleModel = GameScene.FindInScene("Rendering", "3D Canvas/3D Objects/Gratitude 3DRender").gameObject;

            GameObject gratitudeMesh = GameObject.Instantiate(gratitudeCollectibleModel);
            Transform heart = gratitudeMesh.transform.Find("heartstone");
            Component.DestroyImmediate(heart.GetComponent<Rotate>());
            heart.SetParent(originalEgg.transform, false);
            GameObject.Destroy(gratitudeMesh);
        }
    }
}