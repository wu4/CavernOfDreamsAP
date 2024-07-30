using System;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class GiantFixes : InstantiateOnGameSceneLoad
    {
        public GiantFixes()
        {
            Giant giant = GameScene.FindInScene("GALLERY", "Water Lobby/NPCs/Giant")
                                   .GetComponent<Giant>();

            FixGiantFlag(giant);
            PatchGiantCutscene();
            Collecting.Location.RegisterTrigger("LOCATION_GALLERY_GIANT_HEALED", HealGiantFactory(giant));
        }

        static Action HealGiantFactory(Giant giant)
        {
            return () => {
                giant.SetDefaultAnimation();
                //giant.Whack(Whackable.WhackType.CUTSCENE, null);
            };
        }

        /// <summary>
        /// Sniffles no longer gets better from Luna's house opening
        /// </summary>
        static void PatchGiantCutscene()
        {
            Cutscenes.Patching.PatchCutscene(
                "GALLERY", "Water Lobby/Cutscenes/HealGiant",
                Cutscenes.WLOptions.None,
                // "HealGiantManual",
                "DestroyGiantBlockade"
            );
        }

        static void FixGiantFlag(Giant giant)
        {
            giant.flagHealed = "LOCATION_GALLERY_GIANT_HEALED";
        }
    }
}
