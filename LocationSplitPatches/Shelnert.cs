using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    static class Shelnert
    {
        class FixShelnertKoi : InstantiateOnGameSceneLoad
        {
            public FixShelnertKoi()
            {
                TwoStateExists koi = GameScene.FindInScene("LAKE", "Lake (Main)/Entities/Koi 1").GetComponent<TwoStateExists>();
                TwoStateExists koiSuccess = GameScene.FindInScene("LAKE", "Lake (Main)/Entities/Koi 1 Moved").GetComponent<TwoStateExists>();

                koi.flag = SHELNERT_FLAG;
                koiSuccess.flag = SHELNERT_FLAG;

                Collecting.Location.RegisterTrigger(SHELNERT_FLAG, () => {
                    koi.SetState();
                    koiSuccess.SetState();
                });
            }
        }

        class RandomShelnertDialogue : InstantiateOnGameSceneLoad
        {
            static string[] successRandomResponses = new string[] {
                "Oh, hooray! Um, please don't sit next to me. My whackable bubble jitters around a lot and it's kind of embarrassing.",
                "Goodness, I just <i>love</i> feeding this one particular fish. I couldn't possibly fathom feeding another! Well, aside from my little buddy.",
                "Have you played Ocarina of Time, Fynn? They let you fish in that game.",
            };

            static string ShelnertSuccess(Dialog dialog)
            {
                List<string> randomResponses = new();
                randomResponses.AddRange(successRandomResponses);

                // APClient.Client client = APClient.Client.Instance;
                // TODO: add more dialogue based on player options

                return randomResponses[Random.Range(0, randomResponses.Count)];
            }

            public RandomShelnertDialogue()
            {
                MiscPatches.DialogPatches.RegisterDynamicDialogPatch(
                    "/LAKE/Lake (Main)/Cutscenes/KappaTalkSuccessRepeat/KappaTalk",
                    ShelnertSuccess
                );
            }
        }

        static readonly string SHELNERT_FLAG = "LOCATION_LAKE_KAPPA_SUCCESS";

        [HarmonyPatch(typeof(Priest), nameof(Priest.Interact))]
        static class ShelnertFlagsFix
        {
            public static bool Prefix(Priest __instance)
            {
                Save save = GlobalHub.Instance.save;
                if (save.GetFlag(SHELNERT_FLAG).on) {
                    GlobalHub.Instance.SetCutscene(__instance.talkSuccessRepeat);
                    return false;
                }

                if (save.GetFlag("ITEM_FISH_FOOD").on)
                {
                    save.SetFlag(SHELNERT_FLAG, true);
                    return false;
                }

                GlobalHub.Instance.SetCutscene(__instance.talk1Repeat);
                return false;
            }
        }

        [HarmonyPatch(typeof(Priest), nameof(Priest.SetDefaultAnimation))]
        static class ShelnertAnimationFix
        {
            public static bool Prefix(Priest __instance)
            {
                string stateName = (GlobalHub.Instance.save.GetFlag(SHELNERT_FLAG).on ? "Feeding" : "Morose");
                __instance.GetComponentInChildren<Animator>().CrossFade(stateName, 5f * Time.deltaTime);
                return false;
            }
        }
    }
}
