using UnityEngine;
using static CoDArchipelago.SkillPatches.Sprint;

namespace CoDArchipelago.WhackableLabels
{
    class HideWhenSitting : MonoBehaviour {
        public GameObject objectToHide;

        public void Update()
        {
            Player player = GlobalHub.Instance.player;
            objectToHide.SetActive(!player.IsRolling() && player.IsRollHeld() && !IsSprintInput(player) && !GlobalHub.Instance.IsPaused());
        }
    }
}