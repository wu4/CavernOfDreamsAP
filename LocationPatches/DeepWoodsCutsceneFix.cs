using System;

namespace CoDArchipelago.Cutscenes
{
    class DeepWoodsCutsceneFix : InstantiateOnGameSceneLoad
    {
        [LoadOrder(Int32.MinValue + 1)]
        public DeepWoodsCutsceneFix()
        {
            VisualPatches.VisualPatches.RegisterTrigger(
                "LOCATION_FELLA_LAKE6",
                (collectible) => {
                    Raise raise = collectible.AddComponent<Raise>();
                    raise.flag = "LAKE_GROVE_HELPED";
                    raise.raise = new(0, -32f, 0);
                    raise.raiseTime = 180;

                    CoDArchipelago.Collecting.MyItem.RegisterTrigger(
                        "LAKE_GROVE_HELPED",
                        (randomized) => raise.Activate()
                    );
                }
            );
        }
    }
}
