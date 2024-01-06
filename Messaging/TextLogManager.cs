using Cinemachine;
using CoDArchipelago.GlobalGameScene;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace CoDArchipelago.Messaging
{
    class TextLogManager : InstantiateOnGameSceneLoad
    {
        static TextLog textLog;
        static TextLogInputField textLogInputField;

        static InputAction sendChatAction;
        static InputAction closeChatAction;


        [LoadOrder(int.MinValue + 1)]
        public TextLogManager()
        {
            PlayerInputController.Init();
            isOpen = false;

            sendChatAction = new(binding: "<Keyboard>/enter");
            sendChatAction.Enable();
            closeChatAction = new(binding: "<Keyboard>/escape");
            closeChatAction.Enable();

            textLog = new();
            textLogInputField = TextLogInputField.Create(textLog.gameObject.transform);

            textLogInputField.OnSelected += (eventData) => {
                if (!isOpen) {
                    PlayOpenSound();
                    Open();
                }
            };

            textLogInputField.OnDeselected += (eventData) => {
                if (isOpen) {
                    PlayCloseSound();
                    Close();
                }
            };
        }
        
        public static void AddLine(string line) =>
            textLog.AddLine(line);

        static class PlayerInputController
        {
            static CinemachineInputProvider cameraControl;
            static InputActionReference storedCameraInputActionReference;
            
            public static void Init()
            {
                cameraControl = GameScene.FindInScene("Cameras", "CM Standard").GetComponent<CinemachineInputProvider>();
                storedCameraInputActionReference = cameraControl.XYAxis;
            }

            public static void Disable()
            {
                GlobalHub.Instance.playerInput.DeactivateInput();
                // cameraControl.enabled = false;
                cameraControl.XYAxis = null;
            }

            public static void Enable()
            {
                GlobalHub.Instance.playerInput.ActivateInput();
                // cameraControl.enabled = true;
                cameraControl.XYAxis = storedCameraInputActionReference;
            }
        }
        
        static bool isOpen;
        
        static void PlaySendSound() =>
            MenuHandler.Instance.menuSelect.Play();
        
        static void PlayOpenSound() =>
            MenuHandler.Instance.menuOpen.Play();
        
        static void PlayCloseSound() =>
            MenuHandler.Instance.menuClose.Play();
        
        static void Open()
        {
            PlayerInputController.Disable();
            textLogInputField.gameObject.SetActive(true);
            textLogInputField.ActivateInputField();
            textLogInputField.Select();
            isOpen = true;
        }

        static void Close()
        {
            PlayerInputController.Enable();
            textLogInputField.DeactivateInputField();
            textLogInputField.gameObject.SetActive(false);
            isOpen = false;
        }

        static void SendButtonPressed()
        {
            if (!isOpen) {
                PlayOpenSound();
                Open();
                // inputField.ActivateInputField();
            } else {
                if (textLogInputField.text != "") {
                    PlaySendSound();
                    AddLine(textLogInputField.text);
                    textLogInputField.text = "";
                } else {
                    MenuHandler.Instance.menuClose.Play();
                }
                Close();
                // inputField.DeactivateInputField();
            }
        }
        
        static void CloseButtonPressed()
        {
            if (isOpen) {
                PlayCloseSound();
                Close();
                // inputField.DeactivateInputField();
            }
        }
        
        static void Update()
        {
            bool show_cursor = GlobalHub.Instance.IsPaused() || isOpen;
            Cursor.lockState = show_cursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = show_cursor;

            if (GlobalHub.Instance.IsPaused()) {
                if (isOpen) {
                    PlayCloseSound();
                    Close();
                }
            } else {
                if (closeChatAction.WasPressedThisFrame()) {
                    CloseButtonPressed();
                }
                if (sendChatAction.WasPressedThisFrame()) {
                    SendButtonPressed();
                }
            }
            
            textLog.Update(isOpen);
        }

        [HarmonyPatch(typeof(GlobalHub), "Update")]
        static class ChatInputPatch
        {
            static void Postfix() => Update();
        }
    }
}