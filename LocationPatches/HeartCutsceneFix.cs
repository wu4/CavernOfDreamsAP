using System;

namespace CoDArchipelago.LocationPatches
{
    class HeartCutsceneFix : InstantiateOnGameSceneLoad
    {
        [LoadOrder(Int32.MinValue + 1)]
        public HeartCutsceneFix()
        {
            CutsceneFixes.RegisterRaiseFixer(
                locationFlag: "LOCATION_FELLA_MONSTER7",
                itemFlag: "MONSTER_HEART_ROOM_SUCCESS",
                areaName: "Heart",
                offset: new(0f, 6f, 0f)
            );
        }
    }
}
