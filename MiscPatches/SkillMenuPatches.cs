using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoDArchipelago
{
    /// <summary>
    /// Adds a menu for toggling acquired abilities, as well as updating the
    /// debug menu ability selection page
    /// </summary>
    [HasInitMethod]
    static class SkillMenuPatches
    {
        class MO_FLAG_TOGGLABLE : MO_FLAG {
            public Func<bool> check;

            static Color inactiveOnColor = new(0.6f, 0.8f, 0.6f);
            static Color inactiveOffColor = new(0.8f, 0.6f, 0.6f);

            static Color lockedColor = new(0.5f, 0.5f, 0.5f);

            public static MO_FLAG_TOGGLABLE Replace(MO_FLAG flag, Func<bool> check)
            {
                GameObject obj = flag.gameObject;

                MO_FLAG_TOGGLABLE toggle = obj.AddComponent<MO_FLAG_TOGGLABLE>();
                toggle.text = flag.text;
                toggle.BG = flag.GetBG();
                toggle.selectColor = flag.selectColor;
                toggle.regularColor = flag.regularColor;
                toggle.flag = flag.flag;
                toggle.onStr = flag.onStr;
                toggle.offStr = flag.offStr;
                toggle.check = check;

                Component.DestroyImmediate(flag);

                return toggle;
            }

            protected override void Toggle()
            {
                if (!(HasFlag() && CanToggleFlag())) return;
                base.Toggle();
            }

            TextMeshProUGUI label;

            void SetTogglableAppearance()
            {
                if (!HasFlag()) {
                    label.alpha = 0.1f;
                    BG.color = lockedColor;
                } else if (!CanToggleFlag()) {
                    label.alpha = 0.5f;
                    BG.color = IsOn() ? inactiveOnColor : inactiveOffColor;
                } else {
                    label.alpha = 1f;
                }
            }

            public override void Open()
            {
                label = transform.Find("Label").GetComponent<TextMeshProUGUI>();
                base.Open();
                SetTogglableAppearance();
            }

            public override bool OnSelect()
            {
                bool ret = base.OnSelect();
                SetTogglableAppearance();
                return ret;
            }

            bool HasFlag() => GlobalHub.Instance.GetSave().GetFlag("HAS_" + this.flag).On();

            bool CanToggleFlag() => check?.Invoke() ?? true;

            // protected override bool IsOn() => CanToggleFlag() && base.IsOn();
        }

        static readonly AccessTools.FieldRef<MenuOption, MaskableGraphic> bgRef = AccessTools.FieldRefAccess<MenuOption, MaskableGraphic>("BG");

        static readonly Dictionary<string, (string prettyName, Func<bool> check)> readableNames = new() {
            {"SKILL_GROUNDATTACK", ("Grounded Tail", null)},
            {"SKILL_AIRATTACK", ("Aerial Tail", null)},
            {"SKILL_DIVE", ("Horn", null)},
            {"SKILL_PROJECTILE", ("Bubble", null)},
            {"SKILL_SUPERBOUNCE", ("Super Bounce", null)},

            {"SKILL_HOVER", ("Wings", null)},
            {"SKILL_DOUBLEJUMP", ("Double Jump", null)},
            {"SKILL_HIGHJUMP", ("High Jump", null)},
            {"SKILL_ROLL", ("Roll", null)},
            {"PALACE_MELTED_ICE", ("Melted Ice", () => {
                Area area = GlobalGameScene.GetCurrentArea();
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

            {"SKILL_SPRINT", ("Sprint", null)},
            {"SKILL_SWIM", ("Swim", () => {
                Player player = GlobalHub.Instance.player;
                return !player.IsUnderwater() && !player.IsPaddling();
            })},
            {"SKILL_CLIMB", ("Climb", null)},
            {"SKILL_CARRY", ("Carry", null)},
        };

        static GameObject CreateSkillPageCopy(Transform parent = null)
        {
            int ROW_WIDTH = 3;
            int[] COL_HEIGHTS = new int[] {5, 5, 4};
            int START_X = -200;
            int START_Y = 65;
            int STEP_X = 150;
            int STEP_Y = 20;

            GameObject newSkillPage = GameObject.Instantiate(skillPage.gameObject, parent, false);
            CursorPage newSkillPageComponent = newSkillPage.GetComponent<CursorPage>();

            newSkillPageComponent.optionRowWidth = ROW_WIDTH;
            GameObject sampleObj = GameObject.Instantiate(newSkillPageComponent.GetComponentInChildren<MO_FLAG>().gameObject, null, false);

            sampleObj.transform.Find("Text").gameObject.SetActive(false);

            {
                TextMeshProUGUI tm = sampleObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                tm.margin = tm.margin with {y = 10};
                tm.fontSize = 14;

                var tf = tm.GetComponent<RectTransform>();
                tf.offsetMin = tf.offsetMin with {x = 10};
                tf.offsetMax = tf.offsetMax with {y = 20};
            }

            {
                var bg = sampleObj.transform.Find("BG").GetComponent<Image>();
                bgRef(sampleObj.GetComponent<MO_FLAG>()) = bg;
                var tf = bg.GetComponent<RectTransform>();
                tf.offsetMin = new Vector2() {x = -6, y = -6};
                tf.offsetMax = new Vector2() {x = 6, y = 6};
            }

            newSkillPageComponent.GetComponentsInChildren<MO_FLAG>()
                .Select(flag => flag.gameObject)
                .Do(GameObject.DestroyImmediate);

            List<GameObject> objs = new();

            int i = 0;
            foreach (var kv in readableNames) {
                GameObject flagObj = GameObject.Instantiate(sampleObj, null, false);
                objs.Add(flagObj);

                flagObj.name = kv.Key;

                MO_FLAG flag = flagObj.GetComponent<MO_FLAG>();
                flag.text = flagObj.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                bgRef(flag) = flagObj.transform.Find("BG").GetComponent<Image>();

                flagObj.transform.Find("Label").GetComponent<TextMeshProUGUI>().SetText(kv.Value.prettyName);

                flag.flag = kv.Key;

                i++;
            }

            i = 0;
            while (i < objs.Count) {
                GameObject obj = objs[COL_HEIGHTS.Take(i % ROW_WIDTH).Sum() + (i / ROW_WIDTH)];
                obj.transform.SetParent(newSkillPage.transform, false);
                obj.transform.localPosition = new Vector3(){x = START_X + (STEP_X * (i % ROW_WIDTH)), y = START_Y - STEP_Y * Mathf.Floor(i / ROW_WIDTH), z = 0};
                i++;
            }

            GameObject.DestroyImmediate(sampleObj);

            return newSkillPage;
        }
        
        static GameObject CreatePrettySkillPage(Transform parent, Image menuCursor)
        {
            GameObject newSkillPage = CreateSkillPageCopy(parent);

            newSkillPage.GetComponentsInChildren<MO_FLAG>().Do(flag => {
                MO_FLAG_TOGGLABLE replacement = MO_FLAG_TOGGLABLE.Replace(flag, readableNames[flag.name].check);
            });
            GameObject.DestroyImmediate(newSkillPage.transform.Find("MenuCursor").gameObject);
            newSkillPage.GetComponent<CursorPage>().cursorImage = menuCursor;

            Transform subheader = newSkillPage.transform.Find("Subheader");
            subheader.localPosition = subheader.localPosition with {
                y = subheader.localPosition.y + 60f
            };

            TextMeshProUGUI tmp = subheader.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 40;
            tmp.SetText("TOGGLE ABILITIES");

            return newSkillPage;
        }
        
        static GameObject CreateHeaderFrom(Transform sampleHeader, Transform parent)
        {
            GameObject header = new("Header");
            header.transform.localPosition = new Vector3(0f, 150f, 0f);
            header.transform.SetParent(parent, false);

            GameObject flip_left = GameObject.Instantiate(sampleHeader.Find("FlipLeftImage").gameObject, header.transform, false);
            parent.GetComponent<MenuScreen>().flipLeftImage = flip_left.GetComponent<Image>();

            GameObject flip_right = GameObject.Instantiate(sampleHeader.Find("FlipRightImage").gameObject, header.transform, false);
            parent.GetComponent<MenuScreen>().flipRightImage = flip_right.GetComponent<Image>();
            
            return header;
        }
        
        static GameObject AddSkillPageToMenu(Transform menu, bool append = true)
        {
            IEnumerable<CursorPage> pages = menu.GetComponentsInChildren<CursorPage>();
            int numPages = pages.Count();

            if (numPages == 1) {
                CreateHeaderFrom(menusContainer.Find("TotalsMenu/Header"), parent: menu);
            }
            
            GameObject abilitiesPage = CreatePrettySkillPage(parent: menu, menuCursor: menu.Find("MenuCursor").GetComponent<Image>());
            abilitiesPage.name = "AbilitiesPage";

            abilitiesPage.transform.SetSiblingIndex(
                append
                    ? pages.Last().transform.GetSiblingIndex() + 1
                    : pages.First().transform.GetSiblingIndex()
            );
            
            return abilitiesPage;
        }
        
        static GameObject ReplaceDebugSkillPage()
        {
            GameObject newDebugSkillPage = CreateSkillPageCopy();

            int index = skillPage.GetSiblingIndex();
            GameObject.DestroyImmediate(skillPage.gameObject);

            newDebugSkillPage.transform.SetParent(menusContainer.Find("DebugMenu"), false);
            newDebugSkillPage.transform.SetSiblingIndex(index);
            
            return newDebugSkillPage;
        }
        
        static Transform menusContainer;
        static Transform skillPage;

        public static void Init()
        {
            menusContainer = GlobalGameScene.FindInScene("Rendering", "Canvas");
            menusContainer.Find("DarkenScreenFade").SetSiblingIndex(menusContainer.GetComponentInChildren<Menu>(true).transform.GetSiblingIndex());
            skillPage = menusContainer.Find("DebugMenu/SkillPage");

            Transform pauseMenu = menusContainer.Find("PauseMenu");
            pauseMenu.Find("PauseMenuPage1/WorldName").GetComponent<TextMeshProUGUI>().fontSize = 35;

            AddSkillPageToMenu(pauseMenu);

            ReplaceDebugSkillPage();
        }
    }
}