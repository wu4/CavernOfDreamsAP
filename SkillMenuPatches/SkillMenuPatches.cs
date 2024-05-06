using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    /// <summary>
    /// Adds a menu for toggling acquired abilities, as well as updating the
    /// debug menu ability selection page
    /// </summary>
    class SkillMenuPatches : InstantiateOnGameSceneLoad
    {
        readonly SkillPageFactory skillPageFactory;

        public SkillMenuPatches()
        {
            Transform menusContainer = GameScene.FindInScene("Rendering", "Canvas");
            // menusContainer.Find("DarkenScreenFade").SetSiblingIndex(menusContainer.GetComponentInChildren<Menu>(true).transform.GetSiblingIndex());

            GameObject skillPageBase = menusContainer.Find("DebugMenu/SkillPage").gameObject;

            skillPageFactory = new(skillPageBase);

            OverwriteDebugMenuSkillPage(skillPageBase);
            
            MenuScreen pauseMenu = menusContainer.Find("PauseMenu").GetComponent<MenuScreen>();
            AddSkillPageToPauseMenu(pauseMenu);
        }
        
        void OverwriteDebugMenuSkillPage(GameObject skillPageBase)
        {
            int index = skillPageBase.transform.GetSiblingIndex();
            
            CursorPage newDebugSkillPage = skillPageFactory.Create(skillPageBase.transform.parent, isDebug: true);
            newDebugSkillPage.transform.SetSiblingIndex(index);

            GameObject.DestroyImmediate(skillPageBase);
        }
        
        void AddSkillPageToPauseMenu(MenuScreen pauseMenu)
        {
            pauseMenu.transform.Find("PauseMenuPage1/WorldName").GetComponent<TextMeshProUGUI>().fontSize = 35;
            MenuPatches.AddPageToMenu(pauseMenu, skillPageFactory.CreateTogglable());
        }
    }
}