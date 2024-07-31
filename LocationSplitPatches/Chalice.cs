using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class Chalice : InstantiateOnGameSceneLoad
    {
        readonly Transform area;
        static readonly string EGG_LOCATION_FLAG = "LOCATION_FELLA_CHALICE1";
        static readonly string PILLAR_LOCATION_FLAG = "LOCATION_FELLA_CHALICE1";
        readonly ResetFireTraps resetFireTraps;

        public Chalice()
        {
            area = GameScene.FindInScene("CHALICE", "Chalice (Main)");

            AssignEggLocationFlags();
            AssignPillarLocationFlag();

            resetFireTraps = area.Find("Objects/ResetFireTraps").GetComponent<ResetFireTraps>();

            Collecting.Location.RegisterTrigger(EGG_LOCATION_FLAG, MakePeaceful);
        }

        void AssignPillarLocationFlag()
        {
            area.Find("chalice/Pillar_Whackable").GetComponent<TwoState>().flag = PILLAR_LOCATION_FLAG;
        }

        void AssignEggLocationFlags()
        {
            foreach (TwoState ts in area.Find("Objects").GetComponentsInChildren<TwoState>(true)) {
                if (ts.flag != "FELLA_CHALICE1") continue;

                ts.flag = EGG_LOCATION_FLAG;
            }
        }

        void MakePeaceful()
        {
            resetFireTraps.Activate();
            GlobalHub.Instance.GetArea().Activate(false, true);
        }
    }
}
