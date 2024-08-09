using UnityEngine;

namespace CoDArchipelago.SkillMenuPatches
{
    class MO_DEBUG_JESTERBOOTS : MenuOptionToggle
    {
        Access.Field<Player, bool> wearingHoverBoots = new("wearingHoverBoots");
        protected override void Toggle()
        {
            Player player = GlobalHub.Instance.player;
            wearingHoverBoots.Set(player, !wearingHoverBoots.Get(player));
        }

        protected override bool IsOn()
            => GlobalHub.Instance.player.WearingHoverBoots();

        public static MO_DEBUG_JESTERBOOTS Replace(MO_FLAG flag)
        {
            GameObject obj = flag.gameObject;

            MO_DEBUG_JESTERBOOTS toggle = obj.AddComponent<MO_DEBUG_JESTERBOOTS>();
            toggle.text = flag.text;
            toggle.BG = flag.GetBG();
            toggle.selectColor = flag.selectColor;
            toggle.regularColor = flag.regularColor;
            toggle.onStr = flag.onStr;
            toggle.offStr = flag.offStr;

            Component.DestroyImmediate(flag);

            return toggle;
        }
    }
}
