using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;
using UnityEngine.UIElements;
using System;

namespace CoDArchipelago.Messaging
{
    partial class TextLog
    {
        static TextLog Instance;

        public readonly GameObject gameObject;
        readonly GameObject textContainer;

        readonly ScrollRect scrollRect;
        
        class TextLogEntry
        {
            public readonly DateTime time;
            public readonly GameObject gameObject;
            public readonly TextMeshProUGUI tmp;
            
            public TextLogEntry(string text)
            {
                time = DateTime.Now;
                gameObject = CreateLine(text, TextLog.Instance.textContainer.transform);
                tmp = gameObject.GetComponent<TextMeshProUGUI>();
            }
        }
        
        readonly List<TextLogEntry> textLogEntries = new();

        static GameObject CreateLine(string text, Transform parent)
        {
            GameObject obj = new("Line");
            obj.transform.SetParent(parent, false);

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.BottomLeft;
            tmp.fontSizeMin = 14;
            tmp.fontSize = 14;
            tmp.autoSizeTextContainer = true;
            tmp.SetText(text);

            return obj;
        }

        public void AddLine(string text)
        {
            textLogEntries.Add(new(text));
        }
        
        public void Update(bool isOpen)
        {
            var now = System.DateTime.Now;

            foreach (var entry in textLogEntries) {
                if (isOpen) {
                    entry.tmp.alpha = 1;
                } else {
                    var timespan = now - entry.time;
                    if (timespan.TotalSeconds > 5) {
                        entry.tmp.alpha = 0;
                    } else if (timespan.TotalSeconds < 4) {
                        entry.tmp.alpha = 1;
                    } else {
                        entry.tmp.alpha = 1f - ((float)(timespan.TotalMilliseconds - 4000) / 1000f);
                    }
                }
            }
        }

        public TextLog()
        {
            Instance = this;

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
            // viewport.AddComponent<RectMask2D>();
            
            RectTransform viewport_rt = viewport.AddComponent<RectTransform>();
            viewport_rt.pivot = new(0, 0);
            viewport_rt.anchorMax = new(1, 1);
            viewport_rt.anchorMin = new(0, 0);

            GameObject content = new("Content");
            content.transform.SetParent(viewport.transform, false);

            content.transform.localPosition = new(0, 0);

            textContainer = new("Text");
            textContainer.transform.SetParent(content.transform, false);

            var t_tr = textContainer.AddComponent<RectTransform>();
            t_tr.anchorMax = new(1, 0);
            t_tr.anchorMin = new(0, 0);
            t_tr.anchoredPosition = new(0, 15f);
            t_tr.pivot = new(0, 0);
            t_tr.sizeDelta = new(200, 500);

            scrollRect = viewport.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport_rt;
            scrollRect.content = t_tr;

            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childAlignment = TextAnchor.LowerLeft;
        }
    }
}