using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MenuPatching
{
    class Patcher : InstantiateOnGameSceneLoad
    {
        static MenuPageHandsFactory menuPageHandsFactory;

        [LoadOrder(int.MinValue + 1)]
        public Patcher()
        {
            menuPageHandsFactory = new(GameScene.FindInScene("Rendering", "Canvas/TotalsMenu/Header").gameObject);
        }

        public static void AddPageToMenu(MenuScreen menu, CursorPage page, bool append = true)
        {
            IEnumerable<CursorPage> pages = menu.GetComponentsInChildren<CursorPage>();
            int numPages = pages.Count();

            if (numPages == 1) {
                menuPageHandsFactory.AddToMenu(menu);
            }

            GameObject.DestroyImmediate(page.transform.Find("MenuCursor").gameObject);
            page.cursorImage = menu.transform.Find("MenuCursor").GetComponent<Image>();

            page.transform.SetParent(menu.transform, false);
            page.transform.SetSiblingIndex(
                append
                    ? pages.Last().transform.GetSiblingIndex() + 1
                    : pages.First().transform.GetSiblingIndex()
            );
        }
    }
}
