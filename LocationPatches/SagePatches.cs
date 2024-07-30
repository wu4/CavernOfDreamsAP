using System.Linq;
using HarmonyLib;
using TMPro;
using CoDArchipelago.GlobalGameScene;
using UnityEngine;
using System.Collections.Generic;

namespace CoDArchipelago
{
    static class SagePatches
    {
        static string[] sageRandomResponses = new string[] {
            "Hey, Fynn! Make sure to take good care of your friends. Fast food is bad for you, after all!",
            "Don't forget to check behind the turtle statue in Moon Cavern!",
            "Did you know you can check the interactability of things by sitting?",
            "Oh, hello, Fynn! I've been trying out Timespinner. It's pretty fun!",
            "Have you played the Oracle games? You really should!",
            "If you haven't yet, you should try out GMOTA!"
        };

        static string SageRandomResponse(Dialog dialog)
        {
            List<string> randomResponses = new();
            randomResponses.AddRange(sageRandomResponses);

            // APClient.Client client = APClient.Client.Instance;
            // TODO: add more dialogue based on player options

            return randomResponses[Random.Range(0, randomResponses.Count)];
        }

        class SageRandomResponses : InstantiateOnGameSceneLoad
        {
            public SageRandomResponses()
            {
                MiscPatches.DialogPatches.RegisterDynamicDialogPatch(
                    "/CAVE/Sun Cavern (Main)/Cutscenes/Sage Cutscenes/Sage Post Intro/SageDialog",
                    SageRandomResponse
                );
            }
        }

        static void ChangeHasSkillPopup()
        {
            GameScene.FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1/SageReminder/ButtonText")
                .GetComponent<TextMeshProUGUI>()
                .SetText("NEW ITEM\nAVAILABLE!");
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
