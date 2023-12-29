using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace CoDArchipelago
{
    class MenuPageHandsFactory
    {
        readonly GameObject baseHeader;

        public MenuPageHandsFactory(GameObject baseHeader)
        {
            this.baseHeader = GameObject.Instantiate(baseHeader, null);
            GameObject.DestroyImmediate(this.baseHeader.transform.Find("Header Text").gameObject);
            GameObject.DestroyImmediate(this.baseHeader.transform.GetComponentInChildren<TMP_SubMeshUI>().gameObject);
            this.baseHeader.name = "MenuHands";
            this.baseHeader.SetActive(false);
        }
        
        public void AddToMenu(MenuScreen menu)
        {
            GameObject header = GameObject.Instantiate(baseHeader, menu.transform);
            header.SetActive(true);
        
            menu.flipLeftImage = header.transform.Find("FlipLeftImage").GetComponent<Image>();
            menu.flipRightImage = header.transform.Find("FlipRightImage").GetComponent<Image>();
        }
    }
}