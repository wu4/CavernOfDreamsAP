using UnityEngine;
using System;
using TMPro;

namespace CoDArchipelago.SkillMenuPatches
{
    class MO_JESTERBOOTS : MenuOptionToggle
    {
        static readonly Color lockedColor = new(0.5f, 0.5f, 0.5f);

        TextMeshProUGUI label;

        protected override void Toggle()
        {
            throw new NotImplementedException();
        }

        protected override bool IsOn()
            => GlobalHub.Instance.player.WearingHoverBoots();

        public static MO_JESTERBOOTS Replace(MO_FLAG flag)
        {
            GameObject obj = flag.gameObject;

            MO_JESTERBOOTS toggle = obj.AddComponent<MO_JESTERBOOTS>();
            toggle.text = flag.text;
            toggle.BG = flag.GetBG();
            toggle.selectColor = flag.selectColor;
            toggle.regularColor = flag.regularColor;
            toggle.onStr = flag.onStr;
            toggle.offStr = flag.offStr;

            Component.DestroyImmediate(flag);

            return toggle;
        }

        bool CanTakeOffHoverBoots() => GlobalHub.Instance.player.WearingHoverBoots();

        void SetTogglableAppearance()
        {
            if (!CanTakeOffHoverBoots()) {
                label.alpha = 0.1f;
                BG.color = lockedColor;
            } else {
                label.alpha = 1f;
            }
        }

        public override void Open()
        {
            label = transform.Find("Label").GetComponent<TextMeshProUGUI>();
            base.Open();
            SetTogglableAppearance();
        }

        bool LockableToggle()
        {
            if (!CanTakeOffHoverBoots()) return false;

            Player player = GlobalHub.Instance.player;
            player.EquipHoverBoots(false, true);

            return true;
        }

        public override bool OnSelect()
        {
            bool ret = LockableToggle();
            if (!ret) {
                GlobalHub.Instance.player.whimperSFX.Play();
            }

            base.SetAppearance();
            SetTogglableAppearance();
            return ret;
        }
    }
}
