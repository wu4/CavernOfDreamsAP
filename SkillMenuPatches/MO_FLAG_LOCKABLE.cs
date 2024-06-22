using System;
using TMPro;
using UnityEngine;

namespace CoDArchipelago
{
    /// <summary>
    /// MO_FLAG that can have its flag-toggling functionality disabled
    /// </summary>
    class MO_FLAG_LOCKABLE : MO_FLAG {
        public Func<bool> check;

        static readonly Color inactiveOnColor = new(0.6f, 0.8f, 0.6f);
        static readonly Color inactiveOffColor = new(0.8f, 0.6f, 0.6f);

        static readonly Color lockedColor = new(0.5f, 0.5f, 0.5f);

        TextMeshProUGUI label;

        public static MO_FLAG_LOCKABLE Replace(MO_FLAG flag, Func<bool> check)
        {
            GameObject obj = flag.gameObject;

            MO_FLAG_LOCKABLE toggle = obj.AddComponent<MO_FLAG_LOCKABLE>();
            toggle.text = flag.text;
            toggle.BG = flag.GetBG();
            toggle.selectColor = flag.selectColor;
            toggle.regularColor = flag.regularColor;
            toggle.flag = flag.flag;
            toggle.onStr = flag.onStr;
            toggle.offStr = flag.offStr;
            toggle.check = check;

            Component.DestroyImmediate(flag);

            return toggle;
        }

        bool HasFlag() => GlobalHub.Instance.GetSave().GetFlag("HAS_" + this.flag).On();

        bool CanToggleFlag() => check?.Invoke() ?? true;

        void SetTogglableAppearance()
        {
            if (!HasFlag()) {
                label.alpha = 0.1f;
                BG.color = lockedColor;
            } else if (!CanToggleFlag()) {
                label.alpha = 0.5f;
                BG.color = IsOn() ? inactiveOnColor : inactiveOffColor;
            } else {
                label.alpha = 1f;
            }
        }

        bool LockableToggle()
        {
            if (!(HasFlag() && CanToggleFlag())) return false;
            base.Toggle();
            return true;
        }

        public override void Open()
        {
            label = transform.Find("Label").GetComponent<TextMeshProUGUI>();
            base.Open();
            SetTogglableAppearance();
        }

        public override bool OnSelect()
        {
            bool ret = LockableToggle();
            if (!ret) {
                GlobalHub.Instance.player.whimperSFX.Play();
            }

            base.SetAppearance();
            // bool ret = base.OnSelect();
            SetTogglableAppearance();
            return ret;
        }
    }
}
