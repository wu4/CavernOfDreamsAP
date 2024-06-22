using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoDArchipelago.SkillMenuPatches
{
    class SkillPageFactory
    {
        static readonly Dictionary<string, (string prettyName, Func<bool> check)> skillFlags = new() {
            {"SKILL_GROUNDATTACK", ("Grounded Tail", null)},
            {"SKILL_AIRATTACK", ("Aerial Tail", null)},
            {"SKILL_DIVE", ("Horn", null)},
            {"SKILL_PROJECTILE", ("Bubble", null)},
            {"SKILL_SUPERBOUNCE", ("Super Bounce", null)},
            {"SKILL_SUPERBUBBLEJUMP", ("Super Bubble Jump", null)},

            {"SKILL_HOVER", ("Wings", null)},
            {"SKILL_DOUBLEJUMP", ("Double Jump", null)},
            {"SKILL_HIGHJUMP", ("High Jump", null)},
            {"SKILL_ROLL", ("Roll", null)},
            {"SKILL_AIRSWIM", ("Air Swim", null)},

            {"SKILL_SPRINT", ("Sprint", null)},
            {"SKILL_SWIM", ("Swim", () => {
                Player player = GlobalHub.Instance.player;
                return !player.IsUnderwater() && !player.IsPaddling();
            })},
            {"SKILL_CLIMB", ("Climb", null)},
            {"SKILL_CARRY", ("Carry", null)},
            {"PALACE_MELTED_ICE", ("Melted Ice", () => {
                Area area = GlobalHub.Instance.GetArea();
                if (area.name != "Valley (Main)") return true;

                // only need to check Fynn's position if ice is already melted
                if (!area.transform
                    .Find("palace2/Ice_Melt")
                    .GetComponent<GrowFromNothingActivation>()
                    .activated) return true;

                // ensure Fynn is not occupying the space where the unmelted ice will be
                Vector3 pos = GlobalHub.Instance.player.transform.position;
                return pos.y >= -1.05f || pos.y <= -2.93f ||
                    (pos.x < -40f && pos.z < -29.3f);
            })},
        };
        
        static readonly Dictionary<string, (string, string)> carryables = new() {
            {"Apple", ("LAKE", "Lake (Main)/Objects/Fruit Tree/Replacer")},
            {"Medicine", ("MONSTER", "Monster/Rotate (Inside Monster)/Objects (Meeting)/Monster Throwable Replacer Variant")},
            {"Bubble Conch", ("PALACE", "Sanctum/Objects/Torpedo Replacer")},
            {"Sage's Gloves", ("GALLERY", "Water Lobby/Objects Center/ReplacerPaintingItemSage")},
            {"Lady Opal's Head", ("GALLERY", "Water Lobby/Objects Storage/ReplacerPaintingItemPrincess")},
            {"Shelnert's Fish", ("GALLERY", "Fire Lobby/Objects/ReplacerPaintingItemKappa")},
            {"Mr. Kerrington's Wings", ("GALLERY", "Earth Lobby/Objects (Castle)/PaintingItemMonsterReplacer")},
        };

        readonly GameObject skillPageBase;
        readonly MenuFlagFactory menuFlagFactory;
        
        static readonly Access.Field<MenuOption, MaskableGraphic> menuOptionBG = new("BG");

        class MenuFlagFactory
        {
            readonly GameObject sampleFlagObject;

            public MenuFlagFactory(GameObject fromSkillPage, Transform parent = null)
            {
                sampleFlagObject = CreateSampleFlagObject(fromSkillPage.GetComponentInChildren<MO_FLAG>().gameObject, parent);
            }
            
            GameObject CreateSampleFlagObject(GameObject from, Transform parent = null)
            {
                GameObject sample = GameObject.Instantiate(from, parent, false);

                sample.transform.Find("Text").gameObject.SetActive(false);

                {
                    TextMeshProUGUI tm = sample.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                    tm.margin = tm.margin with {y = 10};
                    tm.fontSize = 14;

                    var tf = tm.GetComponent<RectTransform>();
                    tf.offsetMin = tf.offsetMin with {x = 10};
                    tf.offsetMax = tf.offsetMax with {y = 20};
                }

                {
                    var bg = sample.transform.Find("BG").GetComponent<Image>();
                    menuOptionBG.Set(sample.GetComponent<MO_FLAG>(), bg);

                    var tf = bg.GetComponent<RectTransform>();
                    tf.offsetMin = new Vector2() {x = -6, y = -6};
                    tf.offsetMax = new Vector2() {x = 6, y = 6};
                }
                
                sample.SetActive(false);

                return sample;
            }
            
            public GameObject Create(string flagName, string prettyName, Transform parent = null)
            {
                GameObject flagObj = GameObject.Instantiate(sampleFlagObject, parent, false);

                flagObj.name = flagName;

                MO_FLAG flag = flagObj.GetComponent<MO_FLAG>();
                flag.text = flagObj.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                menuOptionBG.Set(flag, flagObj.transform.Find("BG").GetComponent<Image>());

                flagObj.transform.Find("Label").GetComponent<TextMeshProUGUI>().SetText(prettyName);

                flag.flag = flagName;
                flagObj.SetActive(true);
                
                return flagObj;
            }
        }
        
        public SkillPageFactory(GameObject skillPageBase)
        {
            GameObject container = new("SkillPageBase");
            container.SetActive(false);
            this.skillPageBase = GameObject.Instantiate(skillPageBase, container.transform);

            this.menuFlagFactory = new(this.skillPageBase, container.transform);

            this.skillPageBase.GetComponent<CursorPage>().optionRowWidth = ROW_WIDTH;
            this.skillPageBase.GetComponentsInChildren<MO_FLAG>()
                .Select(flag => flag.gameObject)
                .Do(GameObject.DestroyImmediate);
        }

        static readonly int ROW_WIDTH = 3;
        static readonly int[] COL_HEIGHTS = new int[] {6, 5, 5};
        static readonly int START_X = -200;
        static readonly int START_Y = 65;
        static readonly int STEP_X = 150;
        static readonly int STEP_Y = 20;

        public CursorPage Create(Transform parent = null, bool isDebug = false)
        {
            GameObject newSkillPage = GameObject.Instantiate(skillPageBase.gameObject, parent, false);
            int i = 0; // used after the loop as well
            while (i < skillFlags.Count) {
                (string flagName, var data) = skillFlags.ElementAt(COL_HEIGHTS.Take(i % ROW_WIDTH).Sum() + (i / ROW_WIDTH));
                GameObject flagObj = menuFlagFactory.Create(flagName, data.prettyName, newSkillPage.transform);
                flagObj.transform.localPosition = new Vector3(){x = START_X + (STEP_X * (i % ROW_WIDTH)), y = START_Y - STEP_Y * Mathf.Floor(i / ROW_WIDTH), z = 0};

                i++;
            }

            if (!isDebug) return newSkillPage.GetComponent<CursorPage>();

            GameObject jbFlagObj = menuFlagFactory.Create("", "Jester Boots", newSkillPage.transform);
            MO_JESTERBOOTS.Replace(jbFlagObj.GetComponent<MO_FLAG>());
            jbFlagObj.transform.localPosition = new Vector3(){x = START_X + (STEP_X * (i % ROW_WIDTH)), y = START_Y - STEP_Y * Mathf.Floor(i / ROW_WIDTH), z = 0};
            i++;

            foreach ((string name, (string root, string path)) in carryables) {
                var f = GlobalGameScene.GameScene.FindInScene(root, path);
                Debug.Log(f);
                var e = f.GetComponent<ReplaceEmitter>();
                Debug.Log(e);
                GameObject copy = GameObject.Instantiate(e.prefab);
                copy.SetActive(false);
                GameObject flagObj = menuFlagFactory.Create("", name, newSkillPage.transform);
                MO_CARRYABLE flag = MO_CARRYABLE.Replace(flagObj.GetComponent<MO_FLAG>());
                flag.carryable = copy.GetComponent<Carryable>();
                flagObj.transform.localPosition = new Vector3(){x = START_X + (STEP_X * (i % ROW_WIDTH)), y = START_Y - STEP_Y * Mathf.Floor(i / ROW_WIDTH), z = 0};

                i++;
            }

            return newSkillPage.GetComponent<CursorPage>();
        }

        public CursorPage CreateTogglable(Transform parent = null)
        {
            CursorPage newSkillPage = Create(parent);

            newSkillPage.GetComponentsInChildren<MO_FLAG>().Do(flag => {
                MO_FLAG_LOCKABLE replacement = MO_FLAG_LOCKABLE.Replace(flag, skillFlags[flag.name].check);
            });

            Transform subheader = newSkillPage.transform.Find("Subheader");
            subheader.localPosition = subheader.localPosition with {
                y = subheader.localPosition.y + 60f
            };

            TextMeshProUGUI tmp = subheader.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 40;
            tmp.SetText("TOGGLE ABILITIES");

            return newSkillPage;
        }
    }
}
