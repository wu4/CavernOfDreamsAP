
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;
using UnityEngine.UI;

namespace CoDArchipelago
{
    static class APResources
    {
        public static GameObject eggObject;
        public static GameObject shroomObject;
        public static GameObject apMinorObject;
        public static Texture apMinorTexture;

        public static readonly Dictionary<string, Texture> eggTextures = new(){};
        public static readonly Dictionary<string, Sprite> shroomSprites = new(){};
        public static readonly Dictionary<string, Sprite> eventSprites = new(){};

        public static AudioSource apJingleSmall;
        public static AudioSource apJingleLarge;

        static void LoadAPAssets()
        {
            GameObject soundsContainer = new GameObject();
            soundsContainer.name = "AP Sounds";

            apJingleSmall = soundsContainer.AddComponent<AudioSource>();
            apJingleSmall.clip = Util.LoadWavClip(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "res", "minor.wav").Replace("\\", "/"));
            apJingleSmall.enabled = true;
            apJingleSmall.transform.parent = soundsContainer.transform;

            apJingleLarge = soundsContainer.AddComponent<AudioSource>();
            apJingleLarge.clip = Util.LoadWavClip(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "res", "major.wav").Replace("\\", "/"));
            apJingleLarge.enabled = true;
            apJingleLarge.transform.parent = soundsContainer.transform;

            apMinorTexture = Util.LoadTexture(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "res", "archipelago_icon.png").Replace("\\", "/"));
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
            GameObject container = new();
            container.name = "AP Container";
            container.SetActive(false);

            Transform caveCollectibles = GlobalGameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles");

            eggObject = UnityEngine.Object.Instantiate(caveCollectibles.Find("Fella 2 (Waterfall)").gameObject, new Transform());
            eggObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            eggObject.transform.parent = container.transform;
            eggObject.name = "Egg";

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

            shroomObject = UnityEngine.Object.Instantiate(caveCollectibles.Find("Notes/NotePathLake/NoteCave").gameObject, new Transform());
            shroomObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            shroomObject.transform.parent = container.transform;
            shroomObject.name = "Shroom";

            apMinorObject = UnityEngine.Object.Instantiate(caveCollectibles.Find("Notes/NotePathLake/NoteCave").gameObject, new Transform());
            apMinorObject.transform.position = new Vector3() {x = 0, y = 0, z = 0};
            apMinorObject.transform.parent = container.transform;
            apMinorObject.name = "AP Minor";
            apMinorObject.GetComponentInChildren<Collectible>().type = (Collectible.CollectibleType)APCollectible.APCollectibleType.Minor;
            apMinorObject.GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(apMinorTexture as Texture2D, new Rect{x = 0, y = 0, width = 64, height = 64}, new Vector2{x = 0.5f, y = 0.5f});
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