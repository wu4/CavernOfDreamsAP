using System.Collections.Generic;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class PickupItem : ObjectPatcher
    {
        public static readonly new Replaces replaces = new(CollectibleItem.CollectibleType.ITEM);

        readonly GameObject fishFoodObject;
        readonly GameObject[] ladyOpalEggObjects;
        readonly Dictionary<string, GameObject> replacementMap;

        public override GameObject Replace(GameObject toReplace, Item item) =>
            ReplaceWith(toReplace, replacementMap[item.GetFlag()]);

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleGoodShort.Play();
            GlobalHub.Instance.player.curiousSFX.Play();
        }

        public PickupItem()
        {
            Cutscene getItemCutscene = GetItemCutscene();

            fishFoodObject = CreateFishFoodObject(getItemCutscene);
            ladyOpalEggObjects = CreateLadyOpalEggObjects(getItemCutscene);

            replacementMap = new() {
                {"ITEM_FISH_FOOD", fishFoodObject},
                {"ITEM_PRINCESS_1", ladyOpalEggObjects[0]},
                {"ITEM_PRINCESS_2", ladyOpalEggObjects[1]},
                {"ITEM_PRINCESS_3", ladyOpalEggObjects[2]},
            };
        }

        Cutscene GetItemCutscene()
        {
            Transform getItemCutsceneObject = GameScene.FindInScene("LAKE", "Bedroom/Cutscenes/GetFishFoodCutscene");
            getItemCutsceneObject.GetComponentInChildren<CutscenePlayerAnimEvent>().start = 1;
            getItemCutsceneObject.name = "GetItemCutscene";
            getItemCutsceneObject.SetParent(Container, false);
            Cutscene getItemCutscene = getItemCutsceneObject.GetComponent<Cutscene>();
            Cutscenes.Patching.PatchCutscene(getItemCutscene, Cutscenes.WLOptions.Interrupt, "PlayerGetItemPose");
            getItemCutscene.durationAfterFinal = 45;
            getItemCutscene.destroyOnFinish = false;

            return getItemCutscene;
        }

        GameObject[] CreateLadyOpalEggObjects(Cutscene getItemCutscene)
        {
            Transform ladyOpalEggsHolder = GameScene.FindInScene("PALACE", "Valley (Main)/Collectibles/PrincessCollectiblesHolder");
            GameObject[] ladyOpalEggObjects = new GameObject[3];
            Component.DestroyImmediate(ladyOpalEggsHolder.GetComponent<TwoState>());
            for (int i = 0; i < 3; i++) {
                ladyOpalEggObjects[i] = GameObject.Instantiate(ladyOpalEggsHolder.GetChild(i).gameObject, Container);
                ladyOpalEggObjects[i].name = "LadyOpalEgg" + (i + 1);
                Collectible col = ladyOpalEggObjects[i].GetComponent<Collectible>();
                col.cutscene = getItemCutscene;
                col.type = Collectible.CollectibleType.ITEM;
            }

            return ladyOpalEggObjects;
        }

        GameObject CreateFishFoodObject(Cutscene getItemCutscene)
        {
            Transform fishFoodHolder = GameScene.FindInScene("LAKE", "Bedroom/Collectibles/FishFoodHolder");
            GameObject fishFoodObject = GameObject.Instantiate(fishFoodHolder.Find("Fish Food").gameObject, Container);
            fishFoodObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            fishFoodObject.name = "FishFood";
            CollectibleItem collectibleItem = fishFoodObject.GetComponent<CollectibleItem>();
            collectibleItem.model = fishFoodObject.transform.Find("FishfoodHolder").gameObject;
            collectibleItem.type = Collectible.CollectibleType.ITEM;
            collectibleItem.cutscene = getItemCutscene;
            return fishFoodObject;
        }
    }
}
