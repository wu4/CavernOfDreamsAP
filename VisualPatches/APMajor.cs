using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class APMajor : StaticObjectPatcher
    {
        public static readonly new Replaces replaces = new(APCollectibleType.Major);

        public override void CollectJingle()
        {
            throw new System.NotImplementedException();
        }

        public APMajor()
        {
            Transform caveCollectibles = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles");

            staticReplacement = GameObject.Instantiate(caveCollectibles.Find("Fella 2 (Waterfall)").gameObject, Container);
            staticReplacement.name = "AP Major";

            // Fellas cry. Major AP pickups don't
            GameObject.DestroyImmediate(staticReplacement.transform.Find("CrySFX").gameObject);
            Component.DestroyImmediate(staticReplacement.GetComponent<Fella>());

            Collectible apMajorCollectible = staticReplacement.AddComponent<Collectible>();
            apMajorCollectible.type = Collectible.CollectibleType.FELLA;
            apMajorCollectible.model = staticReplacement.transform.Find("FellaHolder").gameObject;
            apMajorCollectible.cutscene = FellaCutscene;

            GameObject originalEgg = staticReplacement.transform.Find("FellaHolder/fella_egg").gameObject;
            while (originalEgg.transform.childCount > 0) {
                GameObject.DestroyImmediate(originalEgg.transform.GetChild(0).gameObject);
            }

            GameObject majorMesh = GameObject.Instantiate(APResources.apMajorMesh);
            while (majorMesh.transform.childCount > 0) {
                majorMesh.transform.GetChild(0).SetParent(originalEgg.transform, false);
            }

            GameObject.DestroyImmediate(majorMesh);
        }
    }
}
