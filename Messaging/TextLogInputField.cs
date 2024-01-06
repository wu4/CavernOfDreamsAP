using HarmonyLib;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace CoDArchipelago.Messaging
{
    class TextLogInputField : InputField
    {
        static readonly Font textFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        public static TextLogInputField Create(Transform parent)
        {
            GameObject container = new("Input Field Container");
            container.transform.SetParent(parent, false);

            RectTransform rectTransform = container.AddComponent<RectTransform>();
            rectTransform.pivot = new(0, 0);
            rectTransform.anchorMin = new(0, 0);
            rectTransform.anchorMax = new(0, 0);
            rectTransform.sizeDelta = new(300f, 15f);

            /* Create game objects for sub components. */
            GameObject inputViewport = new("Input Viewport");
            inputViewport.transform.SetParent(container.transform, false);
            GameObject inputPlaceholderContainer = new("Input Placeholder");
            inputPlaceholderContainer.transform.SetParent(inputViewport.transform, false);
            GameObject inputTextComponent = new("Input Text");
            inputTextComponent.transform.SetParent(inputViewport.transform, false);

            /*
            var image = gameObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            image.type = Image.Type.Sliced;
            image.color = Color.blue;
            */

            TextLogInputField inputField = container.AddComponent<TextLogInputField>();
            // var callbackHandler = container.AddComponent<CallbackEventHandler>();
            // inputField.event.RegisterE<NavigationMoveEvent>((evt) => evt.StopPropagation(), TrickleDown.TrickleDown);

            var colors = inputField.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);

            inputViewport.AddComponent<RectMask2D>();

            var viewportRectTransform = inputViewport.GetComponent<RectTransform>();
            viewportRectTransform.anchorMin = Vector2.zero;
            viewportRectTransform.anchorMax = Vector2.one;
            viewportRectTransform.sizeDelta = Vector2.zero;
            viewportRectTransform.offsetMin = Vector2.zero;
            viewportRectTransform.offsetMax = Vector2.one;
            //UITools.SetTransformValues(viewportRectTransform, 0, 0, 0, 0);

            Text inputText = inputTextComponent.AddComponent<Text>();
            // inputText.text = "";
            inputText.fontSize = 14;
            inputText.fontStyle = FontStyle.Normal;
            inputText.font = textFont;
            //inputText.font = new("Arial");
            // inputTextUGUI.enableWordWrapping = false;
            // inputTextUGUI.extraPadding = true;
            // inputTextUGUI.richText = false;
            //inputTextUGUI.fontSize = 14;
            //inputTextUGUI.color = Color.white;

            Text inputPlaceholder = inputPlaceholderContainer.AddComponent<Text>();
            inputPlaceholder.text = "Send a message...";
            inputPlaceholder.fontSize = 14;
            inputPlaceholder.fontStyle = FontStyle.Italic;
            inputPlaceholder.font = textFont;
            // inputPlaceholderUGUI.enableWordWrapping = false;
            // inputPlaceholderUGUI.extraPadding = true;

            // Make placeholder color half as opaque as normal text color.
            inputPlaceholder.color = inputText.color with {
                a = inputText.color.a * 0.5f
            };

            var textRectTransform = inputTextComponent.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = Vector2.zero;
            textRectTransform.offsetMax = Vector2.zero;

            var placeholderRectTransform = inputPlaceholderContainer.GetComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = Vector2.zero;
            placeholderRectTransform.offsetMax = Vector2.zero;

            //inputField.textViewport = viewportRectTransform;
            inputField.textComponent = inputText;
            inputField.placeholder = inputPlaceholder;
            // inputField.fontAsset = inputTextUGUI.font;

            container.SetActive(false);
            return inputField;
        }

        static readonly Access.Field<InputField, UnityEngine.Event> processingEvent = new("m_ProcessingEvent");

        /// <summary>
        /// This is a patch to OnUpdateSelected to prevent Escape from
        /// reverting the text of the InputField. Everything else remains
        /// unchanged.
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused) return;
            
            UnityEngine.Event tmpProcessingEvent = processingEvent.Get(this);

            bool flag = false;
            while (UnityEngine.Event.PopEvent(tmpProcessingEvent))
            {
                if (tmpProcessingEvent.rawType == EventType.KeyDown)
                {
                    flag = true;
                    // patch begin
                    if (tmpProcessingEvent.keyCode == KeyCode.Escape) {
                        DeactivateInputField();
                        break;
                    }
                    // patch end

                    if (KeyPressed(tmpProcessingEvent) == EditState.Finish)
                    {
                        DeactivateInputField();
                        break;
                    }
                }

                EventType type = tmpProcessingEvent.type;
                if ((uint)(type - 13) <= 1u)
                {
                    string commandName = tmpProcessingEvent.commandName;
                    if (commandName == "SelectAll")
                    {
                        SelectAll();
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                UpdateLabel();
            }

            eventData.Use();
            processingEvent.Set(this, tmpProcessingEvent);
        }

        public event Action<BaseEventData> OnSelected;
        public event Action<BaseEventData> OnDeselected;

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            OnSelected?.Invoke(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            OnDeselected?.Invoke(eventData);
        }
    }
}