using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;
using System;

namespace CoDArchipelago.Messaging
{
    class TextLog
    {
        static TextLog Instance;

        public readonly GameObject masterContainer;
        readonly HideableObject textHideable;
        readonly HideableObject timedTextHideable;
        readonly TextContainer textContainer;
        readonly TimedTextContainer timedTextContainer;
        static Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        class TextLogEntry
        {
            protected readonly GameObject gameObject;
            protected readonly Text textCmp;
            protected readonly Outline outlineCmp;

            public TextLogEntry(string text, Transform parent)
            {
                gameObject = CreateLine(text, parent);
                textCmp = gameObject.GetComponent<Text>();
                outlineCmp = gameObject.GetComponent<Outline>();
            }
        }

        class TimedTextLogEntry : TextLogEntry
        {
            readonly DateTime time;

            public TimedTextLogEntry(string text, Transform parent) : base(text, parent)
            {
                time = DateTime.Now;
            }

            public bool Update(DateTime now)
            {
                var timespan = now - time;

                if (timespan.TotalSeconds < 4) return true;

                // textCmp.CrossFadeAlpha(0, 0.1f, false);
                textCmp.color = textCmp.color with {
                    a = 1f - ((float)(timespan.TotalMilliseconds - 4000) / 1000f)
                };
                // float newAlpha = 1f - ((float)(timespan.TotalMilliseconds - 4000) / 1000f);
                // textCmp.color = new(1, 1, 1, newAlpha);
                // outlineCmp.effectColor = new(0, 0, 0, newAlpha);

                if (timespan.TotalSeconds < 5) return true;

                GameObject.Destroy(gameObject);
                return false;
            }
        }

        readonly List<TextLogEntry> textLogEntries = new();
        readonly List<TimedTextLogEntry> timedTextLogEntries = new();

        static GameObject CreateLine(string text, Transform parent)
        {
            GameObject obj = new(); //GameObject.Instantiate(referenceObject);
            obj.transform.SetParent(parent, false);

            var textCmp = obj.AddComponent<Text>();
            textCmp.supportRichText = true;
            textCmp.alignment = TextAnchor.LowerLeft;
            textCmp.fontSize = 14;
            textCmp.text = text;
            textCmp.font = defaultFont;

            var outline = obj.AddComponent<Outline>();
            outline.effectDistance = new(0.5f, -0.5f);
            outline.effectColor = new(0, 0, 0, 1);
            outline.useGraphicAlpha = true;

            return obj;
        }

        public void AddLine(string text)
        {
            textLogEntries.Add(new(text, TextLog.Instance.textContainer.gameObject.transform));
            timedTextLogEntries.Add(new(text, TextLog.Instance.timedTextContainer.gameObject.transform));
        }

        bool storedIsOpen = false;

        public void Update(bool isOpen)
        {
            if (storedIsOpen != isOpen) {
                storedIsOpen = isOpen;

                textHideable.SetVisible(isOpen);
                timedTextHideable.SetVisible(!isOpen);
            }

            var now = System.DateTime.Now;
            timedTextLogEntries.RemoveAll(t => !t.Update(now));
        }


        class TimedTextContainer
        {
            public readonly GameObject gameObject;

            public TimedTextContainer(Transform parent)
            {
                gameObject = new("Timed Text");
                gameObject.transform.SetParent(parent, false);

                var ttc_tr = Helpers.CreatePaddedTransform(
                    gameObject,
                    bottomRightPadding: new(0, 30f),
                    anchorMax: new(0.5f, 1),
                    pivot: new(0, 0)
                );

                var csf = gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childAlignment = TextAnchor.LowerLeft;
            }
        }

        class TextContainer
        {
            readonly GameObject viewport;
            public readonly GameObject gameObject;
            readonly ScrollRect scrollRect;

            public TextContainer(Transform parent)
            {
                viewport = new("Viewport");
                viewport.transform.SetParent(parent, false);
                viewport.AddComponent<RectMask2D>();

                var viewport_rt = Helpers.CreatePaddedTransform(
                    viewport,
                    bottomRightPadding: new(0, 15f),
                    anchorMax: new(0.5f, 1)
                );

                gameObject = new("Text");
                gameObject.transform.SetParent(viewport.transform, false);

                var t_tr = Helpers.CreatePaddedTransform(gameObject, pivot: Vector2.zero);

                var csf = gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childAlignment = TextAnchor.LowerLeft;

                scrollRect = viewport.AddComponent<ScrollRect>();
                scrollRect.viewport = viewport_rt;
                scrollRect.scrollSensitivity = 5;
                scrollRect.content = t_tr;
            }
        }

        ///<summary>
        /// Let me tell you a story about TextMeshProUGUIs. They are stubborn,
        /// obnoxious, and ridiculous in how they initialize. If you add a
        /// SINGLE TMPUGUI into an active GameObject whose parent is inactive,
        /// that's it. You're fucked. The size of the rendered text will
        /// forever be ruined. So you can't ever do that. So, what are you
        /// going to do? Set the transparency of the container to max? Dude.
        /// This is Unity. That's not a feature that exists. Nothing fucking
        /// works. It's not software designed for people. It's software
        /// designed for fucking shareholders. So, as a result of these trying
        /// circumstances, I am going to create a bullshit container class that
        /// cartoonishly positions itself offscreen so as to sidestep all of
        /// the bullshit created by this godawful library in this equally
        /// godawful engine.
        ///</summary>
        class HideableObject
        {
            public readonly GameObject gameObject;
            public readonly RectTransform transform;

            static Vector3 off = new(-10000, 0, 0);
            static Vector3 on = new(0, 0, 0);

            public void SetVisible(bool visible)
            {
                transform.localPosition = visible ? on : off;
            }

            public HideableObject(Transform parent)
            {
                gameObject = new("Hideable");
                transform = Helpers.CreatePaddedTransform(gameObject);
                transform.SetParent(parent, false);
            }
        }

        public TextLog()
        {
            Instance = this;

            Transform parent = GameScene.FindInScene("Rendering", "Canvas");

            masterContainer = new("AP Log");
            masterContainer.transform.SetParent(parent, false);
            var masterTransform = Helpers.CreatePaddedTransform(masterContainer);

            textHideable = new(masterTransform);
            textContainer = new TextContainer(textHideable.transform);

            timedTextHideable = new(masterTransform);
            timedTextContainer = new TimedTextContainer(timedTextHideable.transform);

            textHideable.SetVisible(false);
        }
    }
}
