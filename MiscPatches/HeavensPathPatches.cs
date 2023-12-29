using CoDArchipelago.GlobalGameScene;
namespace CoDArchipelago
{
    class HeavensPathPatches : InstantiateOnGameSceneLoad
    {
        public HeavensPathPatches()
        {
            ShiftFinishLineDoorBackwards();
            Collecting.MyItem.RegisterTrigger("PALACE_SANCTUM_RACE_FINISHED", OpenFellaGate);
        }
        
        static void OpenFellaGate(bool randomized)
        {
            var area = GlobalHub.Instance.GetArea();
            if (randomized && area.name != "Sanctum") return;

            var gate = area.transform.Find("seadeep/Fella_Gate");
            var raise = gate.GetComponent<Raise>();

            if (!randomized) {
                raise.activated = true;
                return;
            }

            int raiseTime = 60;
            raise.raiseTime = raiseTime;
            raise.GetTimer().SetMax(raiseTime);

            raise.Activate();
        }
        
        /// <summary>
        /// Shift Heaven's Path finish door backwards to enable finishing the
        /// race with it still closed
        /// </summary>
        static void ShiftFinishLineDoorBackwards()
        {
            var sanctumFellaGate = GameScene.FindInScene("PALACE", "Sanctum/seadeep/Fella_Gate");
            sanctumFellaGate.localPosition = sanctumFellaGate.localPosition with {
                z = -17.1423f
            };
        }
    }
}