using CoDArchipelago.GlobalGameScene;
using UnityEngine;
namespace CoDArchipelago
{
    class FadeBelowPauseMenu : InstantiateOnGameSceneLoad
    {
        public FadeBelowPauseMenu()
        {
            Transform menusContainer = GameScene.FindInScene("Rendering", "Canvas");
            menusContainer.Find("DarkenScreenFade").SetSiblingIndex(menusContainer.GetComponentInChildren<Menu>(true).transform.GetSiblingIndex());
        }
    }
}