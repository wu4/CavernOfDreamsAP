using System.Linq;
using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    class PrincessPatches : InstantiateOnGameSceneLoad
    {
        public PrincessPatches()
        {
            PatchEggVisibility();
            PatchPrincessTint();

            Collecting.Location.RegisterTrigger("LOCATION_PALACE_MELTED_ICE", UnfreezePrincess);
        }

        static void UnfreezePrincess()
        {
            Transform princess = GameScene.FindInScene("PALACE", "Valley (Main)/NPCs/Princess");

            princess.Find("princess2/Prison").gameObject.SetActive(false);
            princess.Find("princess2/Body").GetComponent<TintChange>().Activate();

            princess.GetComponent<Princess>().Whack(Whackable.WhackType.ATTACK, null);
        }

        /// <summary>
        /// Update the tint component's flag
        /// </summary>
        static void PatchPrincessTint()
        {
            var area = GameScene.FindInScene("PALACE", "Valley (Main)");

            var princessBody = area.Find("NPCs/Princess/princess2/Body");
            princessBody.GetComponent<TintChange>().flag = "LOCATION_PALACE_MELTED_ICE";
        }


        /// <summary>
        /// Add the correct flags to the eggs such that they appear at the
        /// correct time
        /// </summary>
        static void PatchEggVisibility()
        {
            var area = GameScene.FindInScene("PALACE", "Valley (Main)");

            var cutscenes = area.Find("Cutscenes");
            foreach (int i in Enumerable.Range(1, 3)) {
                cutscenes.Find("PrincessGiveItem" + i + "/ItemToToss").GetComponent<TwoStateExists>().flag = "ITEM_PRINCESS_" + i + "_GIVEN";
            }
        }

        [HarmonyPatch(typeof(Princess), nameof(Princess.SetDefaultAnimation))]
        static class SetDefaultAnimationPatch
        {
            static bool Prefix(Princess __instance)
            {
                var animator = __instance.GetComponentInChildren<Animator>();
                animator.Play(FlagCache.CachedOtherFlags.locationPalaceMeltedIce ? "Idle" : "Frozen");

                return false;
            }
        }

        [HarmonyPatch(typeof(Princess), "HandleWhack")]
        static class HandleWhackPatch
        {
            static bool Prefix(Princess __instance, ref bool ___hit, ref bool __result)
            {
                if (___hit) {
                    __result = false;
                    return false;
                }
                var animator = __instance.GetComponentInChildren<Animator>();
                animator.Play(FlagCache.CachedOtherFlags.locationPalaceMeltedIce ? "Hit" : "FrozenHit");
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Princess), nameof(Princess.Interact))]
        static class InteractPatch
        {
            static bool Prefix(Princess __instance)
            {
                Save save = GlobalHub.Instance.save;
                if (FlagCache.CachedOtherFlags.locationPalaceMeltedIce) {
                    GlobalHub.Instance.SetCutscene(__instance.speakThawed);
                    return false;
                }

                bool gaveItem = false;
                foreach (int i in Enumerable.Range(1, 3)) {
                    if (save.GetFlag("ITEM_PRINCESS_" + i).On()) {
                        save.SetFlag("ITEM_PRINCESS_" + i, false);
                        save.SetFlag("ITEM_PRINCESS_" + i + "_GIVEN", true);
                        GameScene.FindInScene("PALACE", "Valley (Main)/Cutscenes/PrincessGiveItem" + i + "/ItemToToss").gameObject.SetActive(true);
                        gaveItem = true;
                    }
                }
                if (gaveItem) {
                    StockSFX.Instance.whistleUp.Play();
                    StockSFX.Instance.jingleGoodShort.Play();
                    if (Enumerable.Range(1, 3).All(i => save.GetFlag("ITEM_PRINCESS_" + i + "_GIVEN").On())) {
                        save.SetFlag("LOCATION_PALACE_MELTED_ICE", true);
                    }
                    return false;
                }

                if (Enumerable.Range(1, 3).Any(i => save.GetFlag("ITEM_PRINCESS_" + i + "_GIVEN").On())) {
                    GlobalHub.Instance.SetCutscene(__instance.speakFrozenIncomplete);
                    return false;
                }

                GlobalHub.Instance.SetCutscene(__instance.speakFrozenRepeat);

                return false;
            }

        }
    }
}
