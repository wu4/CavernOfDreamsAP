using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class DiningRoom : InstantiateOnGameSceneLoad
    {
        static readonly string LOCATION_FLAG = "LOCATION_PALACE_DINING_ROOM_RISEN";

        public DiningRoom()
        {
            PatchThroneCutscene();
            PatchThronePreston();
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
        /// Separates the button from the throne
        /// </summary>
        static void PatchThronePreston()
        {
            var preston = GameScene.FindInScene("PALACE", "Dining Room/dining_room2/Throne_Switch");
            var prestonRaise = preston.GetComponentInChildren<Raise>();
            prestonRaise.flag = LOCATION_FLAG;

            Collecting.Location.RegisterTrigger(LOCATION_FLAG, () => {
                StockSFX.Instance.click.Play();
                prestonRaise.Activate();
            });
        }
    }
}
