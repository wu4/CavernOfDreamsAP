using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TMPro;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    static class SagePatches
    {
        class ChangeHasSkillPopup : InstantiateOnGameSceneLoad
        {
            public ChangeHasSkillPopup()
            {
                GameScene.FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1/SageReminder/ButtonText")
                    .GetComponent<TextMeshProUGUI>()
                    .SetText("NEW ITEM\nAVAILABLE!");
            }
        }

        static bool HasFlag(string flag_name) => GlobalHub.Instance.save.GetFlag(flag_name).on;
        static int EggCount() => GlobalHub.Instance.save.GetCollectible(Collectible.CollectibleType.FELLA);
        
        static readonly (int EggCount, string FlagName)[] requirements = new (int, string)[5] {
            (Sage.ATTACK_REQ, "LOCATION_SKILL_ATTACK"),
            (Sage.HOVER_REQ, "LOCATION_SKILL_HOVER"),
            (Sage.DIVE_REQ, "LOCATION_SKILL_DIVE"),
            (Sage.PROJECTILE_REQ, "LOCATION_SKILL_PROJECTILE"),
            (Sage.FLIGHT_REQ, "LOCATION_SKILL_FLIGHT"),
        };
        
        [HarmonyPatch(typeof(Sage), nameof(Sage.HasSkill))]
        static class HasSkillPatch
        {
            static bool Prefix(ref bool __result)
            {
                int eggs = EggCount();
                
                __result = requirements.Any(req => eggs >= req.EggCount && !HasFlag(req.FlagName));

                return false;
            }
        }

        [HarmonyPatch(typeof(Sage), nameof(Sage.Interact))]
        static class InteractPatch
        {
            static bool Prefix(Sage __instance)
            {
                int eggs = EggCount();

                foreach (var requirement in requirements) {
                    if (eggs < requirement.EggCount) break;
                    if (HasFlag(requirement.FlagName)) continue;

                    GlobalHub.Instance.save.SetFlag(requirement.FlagName, true);
                    return false;
                }
                
                GlobalHub.Instance.SetCutscene(
                    eggs >= Sage.FLIGHT_REQ
                        ? __instance.postFlight
                        : __instance.postIntro
                );

                return false;
            }
        }
    }
}