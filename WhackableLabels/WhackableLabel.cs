using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using HarmonyLib;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.WhackableLabels
{
    class WhackableLabel : MonoBehaviour {
        GameObject innerContainer;
        TextMeshProUGUI textComponent;
        RectTransform innerRectTransform;
        VerticalLayoutGroup innerLayoutGroup;
        
        Image bgComponent;
        readonly Timer timer = new(10f);
        
        Renderer whackableRenderer;
        GameObject whackableObject;

        bool isValid = false;

        public void Init(Whackable whackable) {
            whackableObject = whackable.gameObject;
            whackableRenderer = whackable.GetComponent<Renderer>() ?? whackable.GetComponentInChildren<Renderer>();

            isValid = true;

            SetLabelText(WhackableTypes.ValidWhackableMethods(whackable));
        }

        void SetLabelText(string text)
        {
            textComponent.SetText(text);

            innerLayoutGroup.padding = new(12, 12, (int)(textComponent.preferredHeight / 3), (int)(textComponent.preferredHeight / 3));
        }

        public void Awake()
        {
            innerContainer = new("Container");
            {
                innerRectTransform = innerContainer.AddComponent<RectTransform>();
                innerRectTransform.SetParent(transform, false);
                innerRectTransform.pivot = new(0.5f, 0.5f);

                bgComponent = innerContainer.AddComponent<Image>();
                bgComponent.sprite = OnLoad.cloudSprite;

                innerLayoutGroup = innerContainer.AddComponent<VerticalLayoutGroup>();
                innerLayoutGroup.childControlHeight = false;
                innerLayoutGroup.childControlWidth = false;
                innerLayoutGroup.childForceExpandHeight = false;
                innerLayoutGroup.childForceExpandWidth = false;
                innerLayoutGroup.childScaleHeight = false;
                innerLayoutGroup.childScaleWidth = false;
                
                ContentSizeFitter innerCsf = innerContainer.AddComponent<ContentSizeFitter>();
                innerCsf.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                innerCsf.verticalFit = ContentSizeFitter.FitMode.MinSize;
            }

            GameObject labelObject = new("Label");
            {
                RectTransform rt = labelObject.AddComponent<RectTransform>();
                rt.SetParent(innerContainer.transform, false);
                rt.anchorMin = new(0.0f, 0.0f);
                rt.anchorMax = new(1.0f, 1.0f);

                textComponent = labelObject.AddComponent<TextMeshProUGUI>();
                textComponent.autoSizeTextContainer = true;
                textComponent.alignment = TextAlignmentOptions.Center;
                textComponent.fontSize = 12.0f;
                textComponent.font = OnLoad.tmpFont;
                textComponent.fontMaterial = OnLoad.tmpFontMaterial;
                textComponent.color = new(0.4f, 0f, 0.8f);

                ContentSizeFitter labelCsf = labelObject.AddComponent<ContentSizeFitter>();
                labelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                labelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            
            UpdateLabel();
        }
        
        bool ShouldShow(out Vector3 screenPositionNormal)
        {
            screenPositionNormal = new();
            if (!isValid || whackableObject == null || !whackableObject.activeInHierarchy) return false;
            
            var whackablePosition = whackableRenderer?.bounds.center ?? whackableObject.transform.position;
            if ((whackablePosition - GlobalHub.Instance.player.transform.position).sqrMagnitude > 100) return false;

            screenPositionNormal = GlobalHub.Instance.gCam.WorldToViewportPoint(whackablePosition);
            if (screenPositionNormal.z <= 0) return false;
            
            return true;
        }

        void UpdateLabel()
        {
            if (isValid && whackableObject == null) {
                innerContainer.SetActive(false);
                enabled = false;
                return;
            }

            if (!ShouldShow(out Vector3 screenPositionNormal)) {
                innerContainer.SetActive(false);
                return;
            }

            if (!innerContainer.activeSelf) {
                innerContainer.SetActive(true);
                timer.Reset();
            }

            timer.Update();

            float alpha = timer.GetPercent();
            textComponent.alpha = alpha;
            bgComponent.color = bgComponent.color with {a = alpha};

            innerRectTransform.localPosition = screenPositionNormal * OnLoad.canvasRectTransform.sizeDelta;
        }

        public void OnEnable() {
            timer.Reset();
            UpdateLabel();
        }
        public void Update() => UpdateLabel();
    }
}