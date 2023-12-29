using HarmonyLib;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    class APTextLog : InstantiateOnGameSceneLoad
    {
        public static APTextLogInstance Instance;

        [LoadOrder(-100)]
        public APTextLog()
        {
            Instance = new APTextLogInstance();
        }

        [HarmonyPatch(typeof(GlobalHub), "Update")]
        static class ChatInputPatch
        {
            static void Postfix()
            {
                Instance.Update();

                bool show_cursor = GlobalHub.Instance.IsPaused() || Instance.IsOpen;
                Cursor.lockState = show_cursor ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = show_cursor;
            }
        }
    }

    class APTextLogInstance
    {
        static readonly int textLogViewCount = 15;

        public bool IsOpen {
            get => isOpen;
        }
        bool isOpen = false;

        readonly CinemachineInputProvider cameraControl;

        readonly InputAction sendChatAction;
        readonly InputAction closeChatAction;

        readonly InputActionReference storedCameraInputActionReference;

        // ScrollView scrollView;
        // TextMeshProUGUI textInterface;

        bool atBottom = true;
        int scrollPos = 0;

        readonly List<System.DateTime> textLogTimes = new();
        readonly GameObject[] textLogObjects = new GameObject[textLogViewCount];
        readonly List<string> textLog = new();

        public void AddLine(string text)
        {
            textLogTimes.Add(System.DateTime.Now);
            textLog.Add(text);
            if (atBottom) {
                scrollPos += 1;
                RefreshText();
            }
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
            string lastText = "";
            foreach (GameObject textLogObject in textLogObjects.Reverse()) {
                var textComponent = textLogObject.GetComponent<TextMeshProUGUI>();
                (textComponent.text, lastText) = (lastText, textComponent.text);
            }
        }

        private void SendButtonPressed()
        {
            if (!isOpen) {
                Open();
            } else {
                Close();
            }
        }

        private void Open()
        {
            MenuHandler.Instance.menuOpen.Play();
            isOpen = true;
            cameraControl.XYAxis = null;
        }

        private void Close()
        {
            MenuHandler.Instance.menuClose.Play();
            isOpen = false;
            cameraControl.XYAxis = storedCameraInputActionReference;
        }

        public void Update()
        {
            if (sendChatAction.WasPressedThisFrame()) {
                SendButtonPressed();
            }

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

        public APTextLogInstance()
        {
            sendChatAction = new(binding: "<Keyboard>/enter");
            sendChatAction.Enable();
            closeChatAction = new(binding: "<Keyboard>/escape");
            closeChatAction.Enable();
            InputAction ac = new(binding: "<Keyboard>/#(b)");
            ac.performed += (c) => {
                AddLine("wew lad! " + GlobalHub.Instance.player.transform.position.x.ToString());
            };
            ac.Enable();

            cameraControl = GameScene.FindInScene("Cameras", "CM Standard").GetComponent<CinemachineInputProvider>();
            storedCameraInputActionReference = cameraControl.XYAxis;

            Transform parent = GameScene.FindInScene("Rendering", "Canvas");

            GameObject container = new("AP Log");
            container.transform.parent = parent;
            container.transform.localPosition = new Vector3{x = 0, y = 0, z = 0};
            container.transform.localScale = new Vector3{x = 1, y = 1, z = 1};
            container.transform.localRotation = new Quaternion();

            var tr = container.AddComponent<RectTransform>();
            tr.pivot = new Vector2{x = 0, y = 0};
            // tr.offsetMin = new Vector2{x = -310, y = -175};
            // tr.offsetMax = new Vector2{x = -100, y = -50};
            tr.anchoredPosition = new Vector2{x = -310, y = -175};

            // var scrollRect = container.AddComponent<ScrollRect>();
            // scrollRect.horizontal = false;

            // GameObject scrollbar = new("Scrollbar");
            // scrollbar.transform.SetParent(container.transform, false);

            // Scrollbar scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
            // scrollbarComponent.SetDirection(Scrollbar.Direction.BottomToTop, false);
            // scrollRect.verticalScrollbar = scrollbarComponent;

            GameObject viewport = new("Viewport");
            viewport.transform.SetParent(container.transform, false);
            // viewport.transform.localPosition = new Vector2{x = 0, y = 0};

            /*
            chatViewportTransform = viewport.AddComponent<RectTransform>();
            chatViewportTransform.pivot = new Vector2{x = 0, y = 0};
            chatViewportTransform.offsetMin = new Vector2{x = 0, y = -125};
            chatViewportTransform.offsetMax = new Vector2{x = 210, y = 0};
            chatViewportTransform.anchoredPosition = new Vector2{x = 0, y = 0};
            viewport.transform.localPosition = new Vector2{x = 0, y = 0};

            var maskImage = viewport.AddComponent<RawImage>();
            maskImage.isMaskingGraphic = true;

            var mask = viewport.AddComponent<Mask>();
            mask.enabled = true;
            mask.showMaskGraphic = false;
            */

            GameObject content = new("Content");
            content.transform.SetParent(viewport.transform, false);

            content.transform.localPosition = new Vector2{x = 0, y = 0};
            // scrollRect.content = content.AddComponent<RectTransform>();

            GameObject textContainer = new("Text");
            textContainer.transform.SetParent(content.transform, false);

            var t_tr = textContainer.AddComponent<RectTransform>();
            t_tr.offsetMax = new Vector2{x = 200, y = 500};
            t_tr.offsetMin = new Vector2{x = 0, y = 0};
            t_tr.anchorMax = new Vector2{x = 1, y = 0};
            t_tr.anchorMin = new Vector2{x = 0, y = 0};
            t_tr.pivot = new Vector2{x = 0, y = 0};
            t_tr.anchoredPosition = new Vector2{x = 0, y = 0};

            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childAlignment = TextAnchor.LowerLeft;

            foreach (int i in Enumerable.Range(1, textLogViewCount)) {
                GameObject obj = new("Line " + i.ToString());
                obj.transform.SetParent(textContainer.transform, false);

                // var o_tr = obj.AddComponent<RectTransform>();
                // o_tr.anchorMax = new Vector2{x = 1, y = 0};
                // o_tr.anchorMin = new Vector2{x = 0, y = 0};
                // o_tr.pivot = new Vector2{x = 0, y = 0};
                // o_tr.anchoredPosition = new Vector2{x = 0, y = i * 15};

                var text = obj.AddComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.BottomLeft;
                text.fontSizeMin = 14;
                text.fontSize = 14;
                text.autoSizeTextContainer = true;

                textLogObjects[i-1] = obj;
            }

            foreach (int a in Enumerable.Range(1, 5)) {
                AddLine("test " + a.ToString());
            }
        }
    }
}