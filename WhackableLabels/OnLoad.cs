using HarmonyLib;
using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace CoDArchipelago.WhackableLabels
{
    class OnLoad : InstantiateOnGameSceneLoad
    {
        public static GameObject labelsContainer;
        public static RectTransform canvasRectTransform;
        public static TMP_FontAsset tmpFont;
        public static Sprite cloudSprite;
        public static Material tmpFontMaterial;

        static readonly Dictionary<string, Dictionary<string, List<WhackableLabel>>> allWhackableTexts = new();

        GameObject CreateBottomLeftAnchoredContainer(string name, Transform parent)
        {
            GameObject ret = new(name);
            RectTransform rt = ret.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMax = new(1.0f, 1.0f);
            rt.anchorMin = new(0.0f, 0.0f);
            rt.pivot = new(0.0f, 0.0f);

            return ret;
        }

        public OnLoad() {
            var sampleButton = GameScene.FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1/Totals Button");
            var tmp = sampleButton.Find("ButtonText").GetComponent<TextMeshProUGUI>();
            tmpFont = tmp.font;
            tmpFontMaterial = tmp.fontMaterial;
            var bg = sampleButton.Find("ButtonBG").GetComponent<Image>();
            cloudSprite = bg.sprite;
            allWhackableTexts.Clear();
            currentWhackableTexts = null;

            canvasRectTransform = GameScene.FindInScene("Rendering", "Canvas") as RectTransform;

            GameObject labelsHiderContainer = CreateBottomLeftAnchoredContainer("WhackableLabelsContainer", canvasRectTransform);
            HideWhenSitting hider = labelsHiderContainer.AddComponent<HideWhenSitting>();
            labelsContainer = CreateBottomLeftAnchoredContainer("WhackableLabels", labelsHiderContainer.transform);
            hider.objectToHide = labelsContainer;

            GameScene.GetComponentsInChildren<Whackable>(true)
                .Where(WhackableTypes.HasValidWhackableMethods)
                .Do(whackable => {
                    Area area = GameScene.GetContainingArea(whackable.transform);
                    if (area == null) return;
                    string regionName = area.transform.parent.name;
                    string areaName = area.name;

                    GameObject newWhackable = new(whackable.name);
                    newWhackable.transform.SetParent(labelsContainer.transform, false);
                    WhackableLabel whackableLabel = newWhackable.AddComponent<WhackableLabel>();
                    whackableLabel.Init(whackable);
                    newWhackable.SetActive(false);
                    allWhackableTexts.GetOrAdd(regionName).GetOrAdd(areaName).Add(whackableLabel);
                });
        }

        static List<WhackableLabel> currentWhackableTexts;

        [HarmonyPatch(typeof(Area), nameof(Area.Activate))]
        static class OnAreaLoad {
            static void Postfix(Area __instance)
            {
                if (currentWhackableTexts != null) {
                    foreach (var wl in currentWhackableTexts) {
                        wl.gameObject.SetActive(false);
                    }
                }

                string regionName = __instance.transform.parent.name;
                string areaName = __instance.name;
                currentWhackableTexts = allWhackableTexts.GetOrAdd(regionName).GetOrAdd(areaName);

                foreach (var wl in currentWhackableTexts) {
                    wl.gameObject.SetActive(true);
                }
            }
        }
    }
}
