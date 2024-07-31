using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class Rattles : InstantiateOnGameSceneLoad
    {
        readonly Transform area;
        static readonly string LOCATION_FLAG = "LOCATION_GALLERY_STATUE_PUZZLE_SUCCESS";

        public Rattles()
        {
            NPCGeneric rattles = GameScene.FindInScene("GALLERY", "Earth Lobby/NPCs/Rattles").GetComponent<NPCGeneric>();
            rattles.flagSpoken = LOCATION_FLAG;

            Collecting.Location.RegisterTrigger(LOCATION_FLAG, () => {
                rattles.SetDefaultAnimation();
            });
        }
    }
}
