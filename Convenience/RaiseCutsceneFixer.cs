using UnityEngine;

namespace CoDArchipelago
{
    static class CutsceneFixes
    {
        public static void RegisterRaiseFixer(string locationFlag, string itemFlag, string areaName, Vector3 offset)
        {
            new RaiseCutsceneFixer(locationFlag, itemFlag, areaName, offset);
        }

        class RaiseCutsceneFixer
        {
            Raise raise;
            string itemFlag;
            string areaName;
            Vector3 offset;

            public RaiseCutsceneFixer(string locationFlag, string itemFlag, string areaName, Vector3 offset)
            {
                this.itemFlag = itemFlag;
                this.areaName = areaName;
                this.offset = offset;
                VisualPatches.VisualPatches.RegisterTrigger(locationFlag, AddRaise);
            }

            void AddRaise(GameObject collectible)
            {
                raise = collectible.AddComponent<Raise>();
                raise.flag = itemFlag;
                // raise.raise = new(0, -32f, 0);
                raise.raise = offset;
                raise.raiseTime = 180;

                CoDArchipelago.Collecting.MyItem.RegisterTrigger(itemFlag, LowerCollectibleIfVisible);
            }

            void LowerCollectibleIfVisible(bool randomized)
            {
                if (!randomized || GlobalHub.Instance.GetArea().name == areaName) {
                    raise.Activate();
                }
            }
        }
    }
}
