using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class Paintings : InstantiateOnGameSceneLoad
    {
        static void PatchPainting(string path)
        {
            var painting = GameScene.FindInScene("GALLERY", path);
            var twoState = painting.GetComponent<TwoStateExists>();
            var modelFadeIn = painting.GetComponent<ModelFadeIn>();

            var flag = twoState.flag;
            var locationFlag = $"LOCATION_{flag}";
            twoState.flag = locationFlag;
            modelFadeIn.flag = locationFlag;

            Collecting.Location.RegisterTrigger(locationFlag, () => {
                modelFadeIn.Activate();
            });
        }

        public Paintings()
        {
            PatchPainting("Earth Lobby/Objects (Castle)/PaintingWarpUndead/DryPainting");

            PatchPainting("Earth Lobby/Objects (Entrance)/PaintingUnfinishedKappa/KappaArtworkUnfinished");
            PatchPainting("Fire Lobby/Objects/PaintingUnfinishedMONSTER/ArtworkUnfinished");
            PatchPainting("Water Lobby/Objects Storage/PaintingUnfinishedPrincess/PrincessArtworkUnfinished");
            PatchPainting("Foyer (Main)/Paintings/PaintingUnfinishedSage/SageArtworkUnfinished");
        }
    }
}
