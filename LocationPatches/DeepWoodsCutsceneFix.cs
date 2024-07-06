using System;

namespace CoDArchipelago.LocationPatches
{
    class DeepWoodsCutsceneFix : InstantiateOnGameSceneLoad
    {
        [LoadOrder(Int32.MinValue + 1)]
        public DeepWoodsCutsceneFix()
        {
            CutsceneFixes.RegisterRaiseFixer(
                locationFlag: "LOCATION_FELLA_LAKE6",
                itemFlag: "LAKE_GROVE_HELPED",
                areaName: "Lake (Main)",
                offset: new(0f, -32f, 0f)
            );
        }
    }
}
