
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CoDArchipelago
{
    [HasInitMethod]
    static class APResources
    {
        public static GameObject cardObject;
        public static GameObject eggObject;
        public static GameObject shroomObject;
        public static GameObject fishFoodObject;
        public static GameObject[] ladyOpalEggObjects = new GameObject[3];
        public static GameObject orbObject;
        public static GameObject apMinorObject;
        public static GameObject apMajorObject;
        public static GameObject gratitudeObject;
        static Texture apMinorTexture;
        static GameObject apMajorMesh;
        static AssetBundle bundle;

        public static readonly Dictionary<string, Texture> eggTextures = new(){};
        public static readonly Dictionary<string, Sprite> shroomSprites = new(){};
        public static readonly Dictionary<string, Sprite> eventSprites = new(){};

        public static AudioSource apJingleSmall;
        static AudioClip apJingleSmallClip;
        
        public static AudioSource grabAbilityJingle;
        // public static AudioSource apJingleLarge;

        static void LoadAPAssets()
        {
            // apJingleLarge = soundsContainer.AddComponent<AudioSource>();
            // apJingleLarge.clip = AssetLoading.LoadWavClip(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "res", "major.wav").Replace("\\", "/"));
            // apJingleLarge.enabled = true;

            if (bundle == null) {
                bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "bundle"));

                apMajorMesh = bundle.LoadAsset<GameObject>("major_ap_model.blend");
                apMinorTexture = bundle.LoadAsset<Texture>("archipelago_icon.png");
                apJingleSmallClip = bundle.LoadAsset<AudioClip>("minor.wav");
            }
            //AssetLoading.LoadTexture(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "res", "archipelago_icon.png").Replace("\\", "/"));
            GameObject soundsContainer = new("AP Sounds");

            apJingleSmall = soundsContainer.AddComponent<AudioSource>();
            apJingleSmall.clip = apJingleSmallClip;
            apJingleSmall.enabled = true;
        }

        static void InitEggTextures()
        {
            eggTextures.Clear();

            IEnumerable<Fella> fs = GlobalGameScene.GetComponentsInChildren<Fella>(true);

            foreach (Fella f in fs) {
                GameObject obj = f.gameObject;
                string flag = obj.GetComponent<TwoState>().flag;
                if (eggTextures.ContainsKey(flag)) continue;

                eggTextures.Add(flag, f.texture);
            }
        }

        static void InitShroomTextures()
        {
            shroomSprites.Clear();

            Dictionary<string, string> sampleShrooms = new() {
                {"CAVE", "Sun Cavern (Main)/Collectibles/Notes/NotePathLake/NoteCave"},
                {"LAKE", "Lake (Main)/Collectibles/Notes/NoteBranches/NoteLake"},
                {"MONSTER", "Sky (Main)/Collectibles/Notes/NoteMonster"},
                {"PALACE", "Valley (Main)/Collectibles/Notes/EntranceNotes/NotePalace"},
            };

            foreach (var item in sampleShrooms) {
                Transform noteT = GlobalGameScene.FindInScene(item.Key, item.Value);
                shroomSprites.Add(item.Key, noteT.GetComponentInChildren<SpriteRenderer>().sprite);
            }
        }
        
        static void InitEventSprites()
        {
            eventSprites.Clear();

            var pauseMenuT = GlobalGameScene.FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1");
            Dictionary<string, string> menuOptions = new() {
                {"CAVE", "Quit Game"},
                {"PALACE", "Totals"},
                {"LAKE", "Options"},
                {"MONSTER", "Encyclopedia"},
                {"GALLERY", "Controls"}
            };

            foreach (var item in menuOptions) {
                var tex = pauseMenuT.Find(item.Value + " Button/Icon").GetComponent<RawImage>().texture as Texture2D;
                var spr = Sprite.Create(tex, new Rect{x = 0, y = 0, width = tex.width, height = tex.height}, new Vector2{x = 0.5f, y = 0.5f});
                eventSprites[item.Key] = spr;
            }
        }

        static void InitAPObjects()
        {
            GameObject container = new("AP Container");
            container.SetActive(false);

            Transform caveCollectibles = GlobalGameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles");
            Transform bedroomContainer = GlobalGameScene.FindInScene("LAKE", "Bedroom");
            Transform valleyCollectibles = GlobalGameScene.FindInScene("PALACE", "Valley (Main)/Collectibles");

            cardObject = GameObject.Instantiate(caveCollectibles.Find("CardPack MUSHROOM").gameObject, container.transform);
            cardObject.transform.position = new Vector3();
            cardObject.name = "Card";
            
            {
                Transform caveCutsceneObjects = GlobalGameScene.FindInScene("CAVE", "Sun Cavern (Main)/Cutscenes/Cutscene Objects");
                GameObject soundsContainer = GlobalGameScene.GetRootObjectByName("AP Sounds");
                grabAbilityJingle = soundsContainer.AddComponent<AudioSource>();
                grabAbilityJingle.clip = caveCutsceneObjects.Find("LearnSkillSound").GetComponent<AudioSource>().clip;
                grabAbilityJingle.enabled = true;
                GameObject orbFX = GameObject.Instantiate(caveCutsceneObjects.Find("LearnSkillEffect").gameObject, container.transform, false);
                orbFX.transform.position = new Vector3();
                orbObject = GameObject.Instantiate(caveCutsceneObjects.Find("Magic Orb Attack").gameObject, container.transform, false);
                orbObject.SetActive(true);
                orbObject.transform.position = new Vector3();
                Transform orbObjectModel = orbObject.transform.Find("magic_orb");
                orbObjectModel.localPosition = new Vector3(0f, 0.5f, 0f);
                orbObjectModel.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                SphereCollider collider = orbObject.AddComponent<SphereCollider>();
                collider.radius = 0.5f;
                collider.center = new Vector3(0f, 0.5f, 0f);
                collider.isTrigger = true;
                collider.tag = "NotPlayer";
                Component.DestroyImmediate(orbObject.GetComponent<MagicOrb>());
                TwoState orbTS = orbObject.AddComponent<TwoStateExists>();
                Collectible orbCollectible = orbObject.AddComponent<Collectible>();
                orbCollectible.type = Collectible.CollectibleType.NOTE;
                orbCollectible.amount = 1;
                orbCollectible.cutscene = null;
                orbCollectible.fx = orbFX.GetComponent<ParticleSystem>();
            }

            fishFoodObject = GameObject.Instantiate(bedroomContainer.Find("Collectibles/FishFoodHolder/Fish Food").gameObject, container.transform);
            fishFoodObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            fishFoodObject.name = "FishFood";
            Transform getItemCutsceneObject = bedroomContainer.Find("Cutscenes/GetFishFoodCutscene");
            getItemCutsceneObject.GetComponentInChildren<CutscenePlayerAnimEvent>().start = 1;
            getItemCutsceneObject.name = "GetItemCutscene";
            getItemCutsceneObject.SetParent(container.transform, false);
            Cutscene getItemCutscene = getItemCutsceneObject.GetComponent<Cutscene>();
            QualityOfLife.PatchCutscene(getItemCutscene, interrupt: true, makeFast: false, indexWhitelist: new int[1]{1});
            getItemCutscene.durationAfterFinal = 45;
            getItemCutscene.destroyOnFinish = false;
            fishFoodObject.GetComponent<CollectibleItem>().model = fishFoodObject.transform.Find("FishfoodHolder").gameObject;
            fishFoodObject.GetComponent<CollectibleItem>().type = Collectible.CollectibleType.ITEM;
            // fishFoodObject.GetComponent<CollectibleItem>().ConvertToCollectible(getItemCutsceneComponent);
            Component.DestroyImmediate(bedroomContainer.Find("Collectibles/FishFoodHolder").GetComponent<TwoState>());

            Transform ladyOpalEggsHolder = valleyCollectibles.Find("PrincessCollectiblesHolder");
            Component.DestroyImmediate(ladyOpalEggsHolder.GetComponent<TwoState>());
            for (int i = 0; i < 3; i++) {
                ladyOpalEggObjects[i] = GameObject.Instantiate(ladyOpalEggsHolder.GetChild(i).gameObject, container.transform);
                ladyOpalEggObjects[i].transform.position = new Vector3();
                ladyOpalEggObjects[i].transform.position = new Vector3();
                ladyOpalEggObjects[i].name = "LadyOpalEgg" + (i + 1);
                Collectible col = ladyOpalEggObjects[i].GetComponent<Collectible>();
                col.cutscene = getItemCutscene;
                col.type = Collectible.CollectibleType.ITEM;
            }

            eggObject = GameObject.Instantiate(caveCollectibles.Find("Fella 2 (Waterfall)").gameObject, container.transform);
            eggObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            eggObject.name = "Egg";
            Transform fellaCutscene = eggObject.transform.Find("GetFellaCutscene");
            Cutscene fellaCutsceneComponent = fellaCutscene.GetComponent<Cutscene>();
            QualityOfLife.PatchCutscene(fellaCutsceneComponent, true, false, new int[1]{1});
            fellaCutscene.GetComponentInChildren<CutscenePlayerAnimEvent>().start = 1;
            fellaCutscene.SetParent(container.transform, false);

            {
                apMajorObject = GameObject.Instantiate(eggObject, new Transform());
                apMajorObject.name = "AP Major";
                apMajorObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
                apMajorObject.transform.parent = container.transform;

                GameObject.DestroyImmediate(apMajorObject.transform.Find("CrySFX").gameObject);

                Fella apMajorFella = apMajorObject.GetComponent<Fella>();
                Component.Destroy(apMajorFella);

                Collectible apMajorCollectible = apMajorObject.AddComponent<Collectible>();
                apMajorCollectible.type = Collectible.CollectibleType.FELLA;
                apMajorCollectible.model = apMajorObject.transform.Find("FellaHolder").gameObject;
                apMajorCollectible.cutscene = fellaCutsceneComponent;

                GameObject originalEgg = apMajorObject.transform.Find("FellaHolder/fella_egg").gameObject;
                while (originalEgg.transform.childCount > 0) {
                    GameObject.DestroyImmediate(originalEgg.transform.GetChild(0).gameObject);
                }

                GameObject majorMesh = GameObject.Instantiate(apMajorMesh);
                while (majorMesh.transform.childCount > 0) {
                    majorMesh.transform.GetChild(0).SetParent(originalEgg.transform, false);
                }

                GameObject.Destroy(majorMesh);
            }

            /*
            // delete all cutscene information except for the get item pose
            int seek = 0;
            Transform cutscene = eggObject.transform.Find("GetFellaCutscene");
            while (cutscene.transform.childCount > 1) {
                Transform child = cutscene.GetChild(seek);
                if (child.name == "PlayerGetItemPose") {
                    seek += 1;
                    GameObject obj = child.gameObject;
                    obj.GetComponent<CutscenePlayerAnimEvent>().start = 2;
                } else {
                    child.parent = null;
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            */

            shroomObject = UnityEngine.Object.Instantiate(caveCollectibles.Find("Notes/NotePathLake/NoteCave").gameObject, new Transform());
            shroomObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            shroomObject.transform.parent = container.transform;
            shroomObject.name = "Shroom";

            apMinorObject = UnityEngine.Object.Instantiate(caveCollectibles.Find("Notes/NotePathLake/NoteCave").gameObject, new Transform());
            apMinorObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            apMinorObject.transform.parent = container.transform;
            apMinorObject.name = "AP Minor";
            apMinorObject.GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(apMinorTexture as Texture2D, new Rect{x = 0, y = 0, width = 64, height = 64}, new Vector2{x = 0.5f, y = 0.5f});
        }

        [HarmonyPatch(typeof(UIController), "Awake")]
        static class GratitudeModel
        {
            static void Postfix(UIController __instance) {
                GameObject container = GlobalGameScene.GetRootObjectByName("AP Container");
                Cutscene fellaCutsceneComponent = container.transform.Find("GetFellaCutscene").GetComponent<Cutscene>();

                gratitudeObject = UnityEngine.Object.Instantiate(eggObject, new Transform());
                gratitudeObject.name = "Gratitude";
                gratitudeObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
                gratitudeObject.transform.parent = container.transform;

                GameObject.DestroyImmediate(gratitudeObject.transform.Find("CrySFX").gameObject);

                Fella gratitudeFella = gratitudeObject.GetComponent<Fella>();
                Component.Destroy(gratitudeFella);

                Collectible gratitudeCollectible = gratitudeObject.AddComponent<Collectible>();
                gratitudeCollectible.type = Collectible.CollectibleType.FELLA;
                gratitudeCollectible.model = gratitudeObject.transform.Find("FellaHolder").gameObject;
                gratitudeCollectible.cutscene = fellaCutsceneComponent;

                GameObject originalEgg = gratitudeObject.transform.Find("FellaHolder/fella_egg").gameObject;
                while (originalEgg.transform.childCount > 0) {
                    GameObject.DestroyImmediate(originalEgg.transform.GetChild(0).gameObject);
                }

                GameObject gratitudeCollectibleModel = __instance.collectibleModels.First(col => col.type == Collectible.CollectibleType.GRATITUDE).model;

                GameObject gratitudeMesh = GameObject.Instantiate(gratitudeCollectibleModel);
                while (gratitudeMesh.transform.childCount > 0) {
                    gratitudeMesh.transform.GetChild(0).SetParent(originalEgg.transform, false);
                }
                GameObject.Destroy(gratitudeMesh);
            }
        }

        public static void Init()
        {
            LoadAPAssets();

            InitEggTextures();

            InitShroomTextures();

            InitEventSprites();

            InitAPObjects();
        }
    }
}