using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CoDArchipelago
{
    class MO_CARRYABLE : MenuOption
    {
        public Carryable carryable;
        
        Access.Action<Player> pickUp = new("PickUpObject");

        public override bool OnSelect()
        {
            Player player = GlobalHub.Instance.player;
            if (player.IsCarrying()) return false;

            GameObject copy = GameObject.Instantiate(carryable.gameObject);
            copy.SetActive(true);
            pickUp.Invoke(player, copy.GetComponent<Carryable>());
            return true;
        }

        public static MO_CARRYABLE Replace(MO_FLAG flag)
        {
            GameObject obj = flag.gameObject;

            MO_CARRYABLE toggle = obj.AddComponent<MO_CARRYABLE>();
            toggle.text = flag.text;
            toggle.BG = flag.GetBG();
            toggle.selectColor = flag.selectColor;
            toggle.regularColor = flag.regularColor;

            Component.DestroyImmediate(flag);

            return toggle;
        }
    }

}