using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Reflection;
using UnityEngine.SceneManagement;
using CoDArchipelago.Messaging;

namespace CoDArchipelago.APClient
{
    class PatchAPMenu : InstantiateOnMenuLoad
    {
        static void DestroyAllChildren(Transform parent)
        {
            for(int i = parent.childCount -1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                child.SetParent(null);
                GameObject.Destroy(child.gameObject);
            }
        }

        ///<summary>
        ///This is better because I like it more.
        ///(More specifically, it allows you to modify its contents immediately
        ///before `Open` without any hitches.)
        ///</summary>
        class BetterCursorPage : CursorPage
        {
            static readonly Access.Action<CursorPage> SetCursorImagePosition = new("SetCursorImagePosition");


            static readonly Access.Field<CursorPage, Vector2> cursorImagePositionPrev = new("cursorImagePositionPrev");

            static readonly Access.Field<CursorPage, Vector2> cursorImagePositionCurrent = new("cursorImagePositionCurrent");

            public void SetMenuOptions(MenuOption[] menuOptions)
            {
                this.menuOptions = menuOptions;
            }

            public override void Open()
            {
                SetCursorImagePosition.Invoke(this);
                cursorImagePositionPrev.Set(this, cursorImagePositionCurrent.Get(this));
                cursorImage.rectTransform.anchoredPosition = cursorImagePositionCurrent.Get(this);
            }
        }

        static GameObject GetRootGameObject(string name)
        {
            var scene = SceneManager.GetActiveScene();
            return scene.GetRootGameObjects().First(obj => name == obj.name);
        }

        static GameObject FindObject(string fullPath)
        {
            var firstPath = fullPath.Split('/').First();
            var restPath = String.Join("/", fullPath.Split('/').Skip(1));
            return GetRootGameObject(firstPath).transform.Find(restPath).gameObject;
        }

        static FieldInfo pages = AccessTools.Field(typeof(GenericMenu), "pages");

        static T CreateMenu<T>(Transform parent, PlayerInput input, GenericMenu escapeMenu = null)
            where T: GenericMenu
        {
            GameObject menu = new("Menu");
            menu.transform.SetParent(parent, false);

            menu.transform.SetSiblingIndex(menu.transform.GetSiblingIndex()-1);
            var menuCmp = menu.AddComponent<T>();
            menuCmp.playerInput = input;
            if (escapeMenu == null) {
                menuCmp.escapeMenu = GameObject.Find("Canvas/MainMenu").GetComponent<GenericMenu>();
            } else {
                menuCmp.escapeMenu = escapeMenu;
            }

            menu.AddComponent<RectTransform>();
            menu.SetActive(false);

            return menuCmp;
        }

        static void SetPage(GenericMenu menu, Page page)
        {
            pages.SetValue(menu, new Page[] {page});
        }

        class AddAPMenu : GenericMenu
        {
            bool canInput = true;

            public override void MenuUpdate()
            {
                if (!canInput) return;

                base.MenuUpdate();
            }

            TextLogInputField CreateInputField(Transform parent, string placeholder)
            {
                GameObject input = new(placeholder);
                input.transform.SetParent(parent, false);

                GameObject placeholderObj = new("placeholder");
                placeholderObj.transform.SetParent(input.transform, false);
                var placeholderText = placeholderObj.AddComponent<Text>();
                placeholderText.text = placeholder;
                placeholderText.font = TextLogInputField.textFont;
                placeholderText.fontStyle = FontStyle.Italic;
                placeholderText.color = new(1,1,1,0.5f);

                GameObject textObj = new("text");
                textObj.transform.SetParent(input.transform, false);
                var text = textObj.AddComponent<Text>();
                text.font = TextLogInputField.textFont;

                var field = input.AddComponent<TextLogInputField>();
                field.textComponent = text;
                field.placeholder = placeholderText;
                field.caretColor = new(1,1,1);
                field.OnSelected += DisableInput;
                field.OnDeselected += EnableInput;
                return field;
            }

            public override void Open()
            {
                Page page = GetPage();

                DestroyAllChildren(page.transform);

                int y = 40;

                inputFields.Clear();
                foreach (var fieldName in fields) {
                    var inputField = CreateInputField(page.transform, fieldName);
                    inputField.transform.localPosition = new(0, y, 0);
                    if (fieldName == "Port")
                        inputField.characterValidation = InputField.CharacterValidation.Integer;
                    inputFields.Add(fieldName, inputField);
                    y -= 20;
                }

                MO_CONFIRM_ADD_AP.Create(page.transform);

                inputFields["Player Name"].ActivateInputField();

                InitActions();
                Page[] pgs = (Page[])pages.GetValue(this);
                pgs[0].Open();
                // base.Open();
            }

            void DisableInput(BaseEventData eventData)
            {
                canInput = false;
            }

            void EnableInput(BaseEventData eventData)
            {
                canInput = true;
            }


            public override void Update()
            {
                if (Keyboard.current[Key.Tab].wasPressedThisFrame) {
                    bool focusNext = false;
                    foreach (var field in fields.Append(fields.First())) {
                        var input = inputFields[field];

                        if (input.isFocused) {
                            focusNext = true;
                        } else if (focusNext) {
                            input.ActivateInputField();
                            break;
                        }
                    }
                }
            }

            static string[] fields = new string[] {"Player Name", "Address", "Port"};
            Dictionary<string, TextLogInputField> inputFields = new();

            public CursorPage CreateCursorPage(Transform parent)
            {
                GameObject page = GameObject.Instantiate(GameObject.Find("Canvas/MainMenu/Page1"), parent);

                DestroyAllChildren(page.transform);

                page.GetComponent<CursorPage>().optionRowWidth = 1;

                return page.GetComponent<CursorPage>();
            }

            class MO_CONFIRM_ADD_AP : MO_CHANGE_MENU
            {
                public override bool OnSelect()
                {
                    var inputFields = addAPMenu.inputFields;
                    sessions.AddSession(
                        inputFields["Player Name"].text,
                        inputFields["Address"].text,
                        int.Parse(inputFields["Port"].text)
                    );
                    SaveSavedSessions();
                    return base.OnSelect();
                }

                public static MO_CONFIRM_ADD_AP Create(Transform parent)
                {
                    GameObject obj = GameObject.Instantiate(FindObject("Canvas/MainMenu/Page1/Options"), parent);
                    obj.transform.SetParent(parent, false);

                    Component.DestroyImmediate(obj.GetComponent<MenuOption>());

                    var mo = obj.AddComponent<MO_CONFIRM_ADD_AP>();
                    mo.menuScreen = apMenu;
                    mo.BG = obj.transform.Find("ButtonBG").GetComponent<Image>();
                    mo.BG.rectTransform.sizeDelta = new(200, 56);
                    mo.text = obj.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>();
                    mo.text.rectTransform.sizeDelta = new(400, 50);
                    mo.text.SetText("ADD");

                    return mo;
                }
            }

            public class MO_ADD_AP : MO_CHANGE_MENU
            {
                public static MO_ADD_AP Create(Transform parent)
                {
                    GameObject obj = GameObject.Instantiate(FindObject("Canvas/MainMenu/Page1/Options"), parent);
                    obj.transform.SetParent(parent, false);

                    Component.DestroyImmediate(obj.GetComponent<MenuOption>());

                    var mo = obj.AddComponent<MO_ADD_AP>();
                    mo.menuScreen = addAPMenu;
                    mo.BG = obj.transform.Find("ButtonBG").GetComponent<Image>();
                    mo.BG.rectTransform.sizeDelta = new(300, 56);
                    mo.text = obj.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>();
                    mo.text.rectTransform.sizeDelta = new(400, 50);
                    mo.text.SetText("ADD");

                    return mo;
                }
            }
        }

        static readonly string savedSessionsPath = $"{Application.persistentDataPath}/savedSessions.json";

        [Serializable]
        public class APSavedSessions
        {
            [Serializable]
            public struct APSession
            {
                [SerializeField]
                public string playerName;

                [SerializeField]
                public string address;

                [SerializeField]
                public int port;

                public APSession(string _playerName, string _address, int _port)
                {
                    playerName = _playerName;
                    address = _address;
                    port = _port;
                }
            }

            public List<APSession> sessions = new();

            public void RemoveSession(int index)
            {
                sessions.RemoveAt(index);
            }

            public void AddSession(string playerName, string address, int port = 38281)
            {
                sessions.Add(new(playerName, address, port));
            }
        }

        static APSavedSessions sessions = new();

        static bool LoadSavedSessions()
        {
            if (!File.Exists(savedSessionsPath)) return false;

            try {
                var ret = JsonConvert.DeserializeObject<APSavedSessions>(File.ReadAllLines(savedSessionsPath)[0]);
                if (ret != null) {
                    sessions = ret;
                    Debug.Log("Save loaded successfully");
                    return true;
                } else {
                    Debug.LogError("Failed to deserialize save");
                    return false;
                }
            } catch (Exception ex) {
                Debug.LogError("Error loading save: " + ex);
                return false;
            }
        }

        static void SaveSavedSessions()
        {
            string[] contents = new string[1] {JsonConvert.SerializeObject(sessions)};
            File.WriteAllLines(savedSessionsPath, contents);
        }

        class APMenu : GenericMenu
        {
            public override void Open()
            {
                Transform parent = GetPage().transform;

                DestroyAllChildren(parent);

                AddAPMenu.MO_ADD_AP.Create(parent);
                // temporary
                MO_NOTHING.Create(parent);

                int index = 0;
                foreach (var session in sessions.sessions) {
                    MO_START_AP.Create(parent, session);
                    MO_DELETE_AP.Create(parent, index);
                    index++;
                }

                RealignCursorPage(parent);

                base.Open();
            }

            class MO_NOTHING : MenuOption
            {
                public static MO_NOTHING Create(Transform parent)
                {
                    GameObject obj = GameObject.Instantiate(FindObject("Canvas/MainMenu/Page1/Options"), parent);
                    obj.transform.SetParent(parent, false);

                    Component.DestroyImmediate(obj.GetComponent<MenuOption>());

                    var ap = obj.AddComponent<MO_NOTHING>();
                    ap.BG = obj.transform.Find("ButtonBG").GetComponent<Image>();
                    ap.BG.rectTransform.sizeDelta = new(100, 56);
                    ap.text = obj.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>();
                    ap.text.rectTransform.sizeDelta = new(100, 50);
                    ap.text.SetText("");

                    return ap;
                }

                public override bool OnSelect()
                {
                    return true;
                }
            }


            class MO_DELETE_AP : MenuOption
            {
                int index;

                public static MO_DELETE_AP Create(Transform parent, int index)
                {
                    GameObject obj = GameObject.Instantiate(FindObject("Canvas/MainMenu/Page1/Options"), parent);
                    obj.transform.SetParent(parent, false);

                    Component.DestroyImmediate(obj.GetComponent<MenuOption>());

                    var ap = obj.AddComponent<MO_DELETE_AP>();
                    ap.index = index;
                    ap.BG = obj.transform.Find("ButtonBG").GetComponent<Image>();
                    ap.BG.rectTransform.sizeDelta = new(100, 56);
                    ap.text = obj.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>();
                    ap.text.rectTransform.sizeDelta = new(100, 50);
                    ap.text.SetText($"DEL");

                    return ap;
                }

                public override bool OnSelect()
                {
                    sessions.RemoveSession(index);

                    apMenu.Open();

                    return true;
                }
            }



            class MO_START_AP : MenuOption
            {
                string playerName;
                string address;
                int port;

                public static MO_START_AP Create(Transform parent, APSavedSessions.APSession session)
                {
                    GameObject obj = GameObject.Instantiate(FindObject("Canvas/MainMenu/Page1/Options"), parent);
                    obj.transform.SetParent(parent, false);

                    Component.DestroyImmediate(obj.GetComponent<MenuOption>());

                    var ap = obj.AddComponent<MO_START_AP>();
                    ap.playerName = session.playerName;
                    ap.address = session.address;
                    ap.port = session.port;
                    ap.BG = obj.transform.Find("ButtonBG").GetComponent<Image>();
                    ap.BG.rectTransform.sizeDelta = new(300, 56);
                    ap.text = obj.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>();
                    ap.text.rectTransform.sizeDelta = new(400, 50);
                    ap.text.SetText($"{session.playerName} {session.address}:{session.port}");

                    return ap;
                }

                public override bool OnSelect()
                {
                    var loadMenuObj = FindObject("Canvas/LoadingMenu");
                    var loadMenu = loadMenuObj.GetComponent<LoadingMenu>();
                    var scene = SceneManager.GetActiveScene();
                    var dragonMainMenu = GetRootGameObject("DRAGON").GetComponent<DragonMainMenu>();

                    APClient.Client.SetConnection(playerName, address, port);

                    MenuHandler.Instance.SetMenu(loadMenu, escape: false);

                    dragonMainMenu.WakeUp();
                    loadMenu.StartCoroutine(GlobalHub.LoadGame(SaveHandler.SAVE_FILE_DEBUG));

                    return true;
                }
            }

            static readonly float xstart = -50;
            static readonly float ystart = 150;

            static void RealignCursorPage(Transform page)
            {
                var childCount = 0;
                foreach (Transform child in page.transform)
                {
                    float x = (childCount % 2) * 200;
                    float y = ((float)Math.Floor(childCount/2f)) * 50;
                    child.localPosition = new(xstart + x, ystart-y, 0);
                    childCount++;
                }
            }

            public static CursorPage CreateCursorPage(Transform parent)
            {
                GameObject page = GameObject.Instantiate(GameObject.Find("Canvas/MainMenu/Page1"), parent);

                DestroyAllChildren(page.transform);

                page.GetComponent<CursorPage>().optionRowWidth = 2;

                return page.GetComponent<CursorPage>();
            }
        }

        static AddAPMenu addAPMenu;
        static APMenu apMenu;

        public PatchAPMenu()
        {
            GameObject canvas = GameObject.Find("Canvas");

            GameObject fileSelectOption = canvas.transform.Find("MainMenu/Page1/FileSelect").gameObject;
            fileSelectOption.transform.Find("ButtonText").GetComponent<TextMeshProUGUI>().SetText("CONNECT");

            var playerInput = canvas.transform.Find("MainMenu").GetComponent<GenericMenu>().playerInput;

            LoadSavedSessions();

            apMenu = CreateMenu<APMenu>(canvas.transform, playerInput);
            addAPMenu = CreateMenu<AddAPMenu>(canvas.transform, playerInput, apMenu);

            fileSelectOption.GetComponent<MO_CHANGE_MENU>().menuScreen = apMenu;

            SetPage(apMenu, APMenu.CreateCursorPage(apMenu.transform));
            SetPage(addAPMenu, addAPMenu.CreateCursorPage(addAPMenu.transform));
        }
    }
}
