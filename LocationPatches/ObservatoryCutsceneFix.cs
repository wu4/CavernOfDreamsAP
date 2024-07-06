using System;

namespace CoDArchipelago.LocationPatches
{
    class ObservatoryCutsceneFix : InstantiateOnGameSceneLoad
    {
        [LoadOrder(Int32.MinValue + 1)]
        public ObservatoryCutsceneFix()
        {
            CutsceneFixes.RegisterRaiseFixer(
                locationFlag: "LOCATION_FELLA_PALACE3",
                itemFlag: "OBSERVATORY_SUCCESS",
                areaName: "Observatory",
                offset: new(0f, -40f, 0f)
            );
        }
    }
}
