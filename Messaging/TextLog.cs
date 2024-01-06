using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;
using UnityEngine.UIElements;

namespace CoDArchipelago.Messaging
{
    partial class TextLog
    {
        public readonly GameObject gameObject;
        static readonly int textLogViewCount = 25;

        readonly ScrollRect scrollRect;

        bool atBottom = true;
        int scrollPos = 0;

        readonly List<System.DateTime> textLogTimes = new();
        readonly GameObject[] textLogObjects = new GameObject[textLogViewCount];
        readonly List<string> textLog = new();

        public void AddLine(string text)
        {
            textLogTimes.Add(System.DateTime.Now);
            textLog.Add(text);
            scrollPos += 1;
            RefreshText();
            /*
            if (atBottom) {
                ScrollDown();
                // scrollPos += 1;
                // RefreshText();
            }
            */
        }

        void RefreshText()
        {
            foreach (int i in Enumerable.Range(1, textLogViewCount)) {
                var textComponent = textLogObjects[textLogViewCount-i].GetComponent<TextMeshProUGUI>();
                textComponent.text = textLog.ElementAtOrDefault(scrollPos - i);
            }
        }

        void ScrollDown()
        {
            scrollPos += 1;
            string lastText = textLog.ElementAtOrDefault(scrollPos - 1);
            foreach (GameObject textLogObject in textLogObjects.Reverse()) {
                var textComponent = textLogObject.GetComponent<TextMeshProUGUI>();
                (textComponent.text, lastText) = (lastText, textComponent.text);
            }
        }

        void ScrollUp()
        {
            scrollPos -= 1;
            string firstText = textLog.ElementAtOrDefault(scrollPos - textLogViewCount);
            foreach (GameObject textLogObject in textLogObjects) {
                var textComponent = textLogObject.GetComponent<TextMeshProUGUI>();
                (textComponent.text, firstText) = (firstText, textComponent.text);
            }
        }
        
        public void Update(bool isOpen)
        {
            var now = System.DateTime.Now;

            foreach (int i in Enumerable.Range(1, textLogViewCount)) {
                var textComponent = textLogObjects[textLogViewCount-i].GetComponent<TextMeshProUGUI>();
                if (isOpen) {
                    textComponent.alpha = 1;
                } else {
                    var timespan = now - textLogTimes.ElementAtOrDefault(scrollPos - i);
                    if (timespan.TotalSeconds > 5) {
                        textComponent.alpha = 0;
                    } else if (timespan.TotalSeconds < 4) {
                        textComponent.alpha = 1;
                    } else {
                        textComponent.alpha = 1f - ((float)(timespan.TotalMilliseconds - 4000) / 1000f);
                    }
                }
            }
        }

        public TextLog()
        {
            Transform parent = GameScene.FindInScene("Rendering", "Canvas");

            gameObject = new("AP Log");
            gameObject.transform.SetParent(parent, false);
            // container.transform.SetSiblingIndex(parent.GetComponentInChildren<Menu>(true).transform.GetSiblingIndex());

            var tr = gameObject.AddComponent<RectTransform>();
            tr.pivot = new(0, 0);
            tr.anchorMin = new(0, 0);
            tr.anchorMax = new(1, 1);

            GameObject viewport = new("Viewport");
            viewport.transform.SetParent(gameObject.transform, false);
            scrollRect = viewport.AddComponent<ScrollRect>();

            GameObject content = new("Content");
            content.transform.SetParent(viewport.transform, false);

            content.transform.localPosition = new(0, 0);

            GameObject textContainer = new("Text");
            textContainer.transform.SetParent(content.transform, false);

            var t_tr = textContainer.AddComponent<RectTransform>();
            t_tr.anchorMax = new(1, 0);
            t_tr.anchorMin = new(0, 0);
            t_tr.anchoredPosition = new(0, 15f);
            t_tr.pivot = new(0, 0);
            t_tr.sizeDelta = new(200, 500);

            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childAlignment = TextAnchor.LowerLeft;

            foreach (int i in Enumerable.Range(1, textLogViewCount)) {
                GameObject obj = new("Line " + i.ToString());
                obj.transform.SetParent(textContainer.transform, false);

                var text = obj.AddComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.BottomLeft;
                text.fontSizeMin = 14;
                text.fontSize = 14;
                text.autoSizeTextContainer = true;

                textLogObjects[i-1] = obj;
            }
        }
    }
}