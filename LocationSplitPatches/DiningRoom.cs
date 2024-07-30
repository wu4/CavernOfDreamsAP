using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class DiningRoom : InstantiateOnGameSceneLoad
    {
        public DiningRoom()
        {
            PatchThroneCutscene();

            Collecting.Location.RegisterTrigger("LOCATION_PALACE_DINING_ROOM_RISEN", PressButton);
        }

        /// <summary>
        /// Removes the button press animation from the throne-raise cutscene
        /// </summary>
        static void PatchThroneCutscene()
        {
            Cutscenes.Patching.PatchCutscene(
                "PALACE", "Dining Room/Cutscenes/ThroneCutscene",
                Cutscenes.WLOptions.None,
                "ScreenShake",
                "RaiseThroneTrick"
            );
        }

        /// <summary>
        /// Presses the button in the Dining Room
        /// </summary>
        public static void PressButton()
        {
            var preston = GameScene.FindInScene("PALACE", "Dining Room/dining_room2/Throne_Switch");
            StockSFX.Instance.click.Play();
            preston.GetComponentInChildren<Raise>().Activate();
        }
    }
}
