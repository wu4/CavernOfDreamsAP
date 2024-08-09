using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace CoDArchipelago.MiscPatches
{
    static class NoJesterBootsCarry
    {
        public static bool allowFun = false;

        [HarmonyPatch(typeof(HoverBoots), nameof(HoverBoots.Collect))]
        static class NoBootsIfCarrying
        {
            static bool Prefix(HoverBoots __instance)
            {
                if (allowFun) return true;

                Player player = GlobalHub.Instance.player;

                if (player.IsCarrying()) {
                    player.whimperSFX.Play();
                    GameObject.Destroy(__instance.gameObject);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "PickUpObject")]
        static class NoCarryIfBoots
        {
            static bool Prefix(Player __instance, Carryable carryable)
            {
                if (allowFun) return true;

                if (__instance.WearingHoverBoots()) {
                    BootsDialogPatch.WarnCarry(carryable);
                    return false;
                }
                return true;
            }
        }

        class BootsDialogPatch : InstantiateOnGameSceneLoad
        {
            static bool nextDialogueWarnsCarry = false;
            static string nextDialogue = "";

            static readonly Dictionary<Whackable.WhackType, string> warningDialogs = new() {
                {Whackable.WhackType.POTION,
                    "Whoa, whoa, whoa! You think us a synthetic leather type? <color=#000000>Take us off</color> if you want that thing."},
                {Whackable.WhackType.PAINTING_ITEM_KAPPA,
                    "Ain't that Shelnert's? We got some history. <color=#000000>Take us off</color> if you want that thing."},
                {Whackable.WhackType.PAINTING_ITEM_PRINCESS,
                    "Whoa there, buddy! We ain't playin' Mr. Fix It! <color=#000000>Take us off</color> if you want that thing."},
                {Whackable.WhackType.PAINTING_ITEM_SAGE,
                    "We ain't doin' a magic show! <color=#000000>Take us off</color> if you want that thing."},
            };

            static string GetWarningDialog(Carryable carryable)
            {
                if (carryable is Seed seed)
                    return "We ain't helpin' you out with any gardening! <color=#000000>Take us off</color> if you want that thing.";
                if (carryable is HangGlider hangGlider)
                    return "Why bother with wings when we’ve got the whole floatin' thing covered, huh? <color=#000000>Take us off</color> if you want that thing.";
                if (carryable.TryGetComponent<Torpedo>(out var _))
                    return "We ain't too keen on gettin' soaked at mach speed! <color=#000000>Take us off</color> if you want that thing.";

                return warningDialogs[carryable.type];
            }

            public static void WarnCarry(Carryable carryable)
            {
                nextDialogueWarnsCarry = true;
                nextDialogue = GetWarningDialog(carryable);
                GlobalHub.Instance.SetCutscene(GlobalHub.Instance.bootsWarnCutscene);
            }

            [LoadOrder(Int32.MaxValue)]
            public BootsDialogPatch()
            {
                DialogPatches.RegisterDynamicDialogPatch(
                    "/Global Objects/Cutscenes/BootsWarnCutscene/HoverBootsTutorialDialog",
                    BootsWarningDialog
                );
            }

            static string BootsWarningDialog(Dialog dialog)
            {
                if (nextDialogueWarnsCarry) {
                    nextDialogueWarnsCarry = false;
                    return nextDialogue;
                }
                return dialog.GetText();
            }
        }
    }
}
