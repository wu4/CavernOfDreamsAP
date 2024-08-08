using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;
using System;

namespace CoDArchipelago.LocationPatches
{
    class FellaNestPatches : InstantiateOnGameSceneLoad
    {
        public static readonly List<HatchableFellaInfo> hatchableFellaNests = new() {
            new("Nest FellaHatchable Lake",    "FELLA_LAKE1",    "LOCATION_GRATITUDE1", 40),
            new("Nest FellaHatchable Monster", "FELLA_MONSTER1", "LOCATION_GRATITUDE2", 40 + 60),
            new("Nest FellaHatchable Palace",  "FELLA_PALACE1",  "LOCATION_GRATITUDE3", 40 + 60 + 80),
            new("Nest FellaHatchable Gallery", "FELLA_GALLERY1", "LOCATION_GRATITUDE4", 40 + 60 + 80 + 100),
        };

        public class HatchableFellaInfo
        {
            public readonly string nestName;
            public readonly string hatchedFlag;
            public readonly string gratitudeFlag;
            public readonly int requirement;

            public HatchableFellaInfo(string nestName, string hatchedFlag, string gratitudeFlag, int requirement) {
                this.nestName = nestName;
                this.hatchedFlag = hatchedFlag;
                this.gratitudeFlag = gratitudeFlag;
                this.requirement = requirement;
            }
        }

        public FellaNestPatches()
        {
            var fellas = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Fellas");

            foreach (var nest in hatchableFellaNests) {
                var fellaNest = fellas.Find(nest.nestName);

                // prevent the fella hint stones from disappearing (necessary for some logic)
                Component.DestroyImmediate(fellaNest.Find("HintStone").GetComponent<TwoState>());

                GameObject hatchedObj = fellaNest.Find("FellaHatched").gameObject;
                FellaHatched hatched = hatchedObj.GetComponent<FellaHatched>();
                hatched.notesRequirement = nest.requirement;
                hatched.gratitudeFlag = nest.gratitudeFlag;

                GameObject hatchable = fellaNest.Find("FellaHatchable").gameObject;

                Collecting.MyItem.RegisterTrigger(nest.hatchedFlag, ShowFellaFactory(hatchable, nest.hatchedFlag));
                Collecting.Location.RegisterTrigger(nest.gratitudeFlag, FeedFellaFactory(hatched, nest.hatchedFlag));

                string noMoreNotesDialogPath = hatchedObj.transform.Find("Cutscenes/NoMoreNotesCutscene/NoMoreNotesDialog").GetPath();
                MiscPatches.DialogPatches.RegisterDynamicDialogPatch(noMoreNotesDialogPath, NotEnoughNotesDialogFactory(nest.requirement));
            }
        }

        static Action FeedFellaFactory(FellaHatched hatched, string hatchedFlag)
        {
            return () => {
                // coverage for loading into a seed with the flag already set
                GlobalHub.Instance.save.SetFlag($"{hatchedFlag}_HATCHED", true);
                GlobalHub.Instance.save.SetFlag(hatchedFlag, true);

                try {
                    hatched.StopBeingHungry();
                } catch (Exception e) {
                    Debug.LogError($"error while stopping FellaHatched from being hungry: {e.Message}");
                }
            };
        }

        static Action<bool> ShowFellaFactory(GameObject hatchable, string hatchedFlag)
        {
            return randomized => {
                if (GlobalHub.Instance.save.GetFlag($"{hatchedFlag}_HATCHED").on) {
                    hatchable.SetActive(false);
                    return;
                };
                hatchable.SetActive(true);
            };
        }

        static Func<Dialog, string> NotEnoughNotesDialogFactory(int requirement)
        {
            return dialog => {
                int remaining = requirement - GlobalHub.Instance.save.GetCollectible(Collectible.CollectibleType.NOTE);
                return $"Hmm. It seems this fella needs {remaining} more shrooms!";
            };
        }

        [HarmonyPatch(typeof(FellaHatched), nameof(FellaHatched.Interact))]
        static class InteractPatch
        {
            static bool Prefix(FellaHatched __instance, ref FellaHatched.FellaBehaviour ___behaviour)
            {
                switch (___behaviour)
                {
                case FellaHatched.FellaBehaviour.SLEEP:
                    GlobalHub.Instance.SetCutscene(__instance.interactSleep);
                    break;
                case FellaHatched.FellaBehaviour.CRY:
                    GlobalHub.Instance.SetCutscene(__instance.interactCry);
                    break;
                case FellaHatched.FellaBehaviour.WALK:
                    GlobalHub.Instance.SetCutscene(__instance.interactWalk);
                    break;
                case FellaHatched.FellaBehaviour.HUNGRY:
                    if (GlobalHub.Instance.save.GetCollectible(Collectible.CollectibleType.NOTE) < __instance.notesRequirement) {
                        GlobalHub.Instance.SetCutscene(__instance.noMoreNotesCutscene);
                        break;
                    }

                    // string fedFlag = __instance.feedCutscene.GetComponentInChildren<CutsceneFeedEvent>().fedCutscene.flag;
                    // GlobalHub.Instance.save.SetFlag("LOCATION_" + fedFlag, true);
                    // GlobalHub.Instance.SetCutscene(this.fedCutscene);
                    // GlobalHub.Instance.save.SetFlag(__instance.flag + FellaHatched.fed, true);
                    GlobalHub.Instance.save.SetFlag(__instance.gratitudeFlag, true);
                    // GlobalHub.Instance.Collect(1, Collectible.CollectibleType.GRATITUDE, (GameObject) null, (Cutscene) null);
                    __instance.StopBeingHungry();
                    break;
                case FellaHatched.FellaBehaviour.SIT:
                    GlobalHub.Instance.SetCutscene(__instance.interactSit);
                    break;
                default:
                    Debug.LogWarning("Unknown behaviour " + ___behaviour, __instance);
                    GlobalHub.Instance.SetCutscene(__instance.interactSit);
                    break;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(FellaHatchable), "HandleWhack")]
        class SkipHatchCutscenePatch
        {
            static bool Prefix(FellaHatchable __instance, ref bool __result)
            {
                StockSFX.Instance.jingleGood.Play();
                __instance.transform.Find("fella_egg").gameObject.SetActive(false);

                Transform hatchSFX = __instance.transform.Find("Cutscenes/FellaHatch/PlayHatchSFX");
                hatchSFX.parent = null;
                hatchSFX.GetComponent<AudioSource>().Play();

                Transform breakParticles = __instance.transform.Find("BreakFellaEggParticles");
                breakParticles.parent = null;
                breakParticles.gameObject.SetActive(true);

                __instance.fh.gameObject.SetActive(true);
                __instance.fh.Hatch();

                GlobalHub.Instance.save.SetFlag(__instance.hatchCutscene.flag, true);

                GameObject.Destroy(__instance.gameObject);

                __result = true;
                return false;
            }
        }
    }
}
