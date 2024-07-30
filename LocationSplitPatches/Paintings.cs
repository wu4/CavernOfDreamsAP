using System;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class Paintings : InstantiateOnGameSceneLoad
    {
        static void PatchPainting(string fullPath)
        {
            var ts = GameScene.FindInSceneFullPath(fullPath).GetComponent<TwoStateExists>();
            ts.flag = $"LOCATION_{ts.flag}";
        }

        public Paintings()
        {
            PatchPainting("GALLERY/Earth Lobby/Objects (Castle)/PaintingWarpUndead/DryPainting");

            PatchPainting("GALLERY/Earth Lobby/Objects (Entrance)/PaintingUnfinishedKappa/KappaArtworkUnfinished");
            PatchPainting("GALLERY/Fire Lobby/Objects/PaintingUnfinishedMONSTER/ArtworkUnfinished");
            PatchPainting("GALLERY/Water Lobby/Objects Storage/PaintingUnfinishedPrincess/PrincessArtworkUnfinished");
            PatchPainting("GALLERY/Foyer (Main)/Paintings/PaintingUnfinishedSage/SageArtworkUnfinished");
        }
    }
}
