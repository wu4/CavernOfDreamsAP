using CoDArchipelago.GlobalGameScene;
using UnityEngine;

namespace CoDArchipelago.MiscPatches
{
    class AtelierWarpToEndgame : InstantiateOnGameSceneLoad
    {
        public AtelierWarpToEndgame()
        {
            Entrances.Set(
                "GALLERY/Atelier/Warps/WarpFromAtelierWindowToGalleryLobby",
                "FINALE/Finale/Warps/DestFromCorruptToFinale"
            );
        }
    }

    static class Entrances
    {
        public static void Set(string warpPath, string newDestPath)
        {
            WarpTrigger warp = GameScene.FindInSceneFullPath(warpPath).GetComponent<WarpTrigger>();
            Transform newDest = GameScene.FindInSceneFullPath(newDestPath);
            warp.destination = newDest.GetComponent<Destination>();
            warp.area = GameScene.GetContainingArea(newDest);
        }
    }
}
