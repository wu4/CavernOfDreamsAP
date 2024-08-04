using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace CoDArchipelago.Messaging
{
    class TextLogInputField : InputField
    {
        public static readonly Font textFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        public static TextLogInputField Create(Transform parent)
        {
            GameObject container = new("Input Field Container");
            container.transform.SetParent(parent, false);

            RectTransform rectTransform = Helpers.CreatePaddedTransform(
                container,
                anchorMax: new(0.5f, 0),
                anchorMin: new(0, -0.1f),
                topLeftPadding: new(0, -15)
            );

            GameObject inputViewport = new("Input Viewport");
            inputViewport.transform.SetParent(container.transform, false);
            GameObject inputPlaceholderContainer = new("Input Placeholder");
            inputPlaceholderContainer.transform.SetParent(inputViewport.transform, false);
            GameObject inputTextContainer = new("Input Text");
            inputTextContainer.transform.SetParent(inputViewport.transform, false);

            // var image = gameObject.AddComponent<Image>();
            // image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            // image.type = Image.Type.Sliced;
            // image.color = Color.blue;

            TextLogInputField inputField = container.AddComponent<TextLogInputField>();
            // var callbackHandler = container.AddComponent<CallbackEventHandler>();
            // inputField.event.RegisterE<NavigationMoveEvent>((evt) => evt.StopPropagation(), TrickleDown.TrickleDown);

            var colors = inputField.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);

            inputViewport.AddComponent<RectMask2D>();

            Helpers.CreatePaddedTransform(inputViewport);

            Text inputText = inputTextContainer.AddComponent<Text>();
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

            var outline = inputTextContainer.AddComponent<Outline>();
            outline.effectDistance = new(0.5f, -0.5f);
            outline.effectColor = new(0, 0, 0, 1);
            outline.useGraphicAlpha = true;

            Text inputPlaceholder = inputPlaceholderContainer.AddComponent<Text>();
            inputPlaceholder.text = "Send a message...";
            inputPlaceholder.fontSize = 14;
            inputPlaceholder.fontStyle = FontStyle.Italic;
            inputPlaceholder.font = textFont;
            // inputPlaceholderUGUI.enableWordWrapping = false;
            // inputPlaceholderUGUI.extraPadding = true;

            var placeholderOutline = inputPlaceholderContainer.AddComponent<Outline>();
            placeholderOutline.effectDistance = new(0.5f, -0.5f);
            placeholderOutline.effectColor = new(0, 0, 0, 1);
            placeholderOutline.useGraphicAlpha = true;

            inputPlaceholder.color = inputText.color with {
                a = inputText.color.a * 0.5f
            };

            Helpers.CreatePaddedTransform(inputTextContainer);

            Helpers.CreatePaddedTransform(inputPlaceholderContainer);

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
