using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    class UndeadPatches : InstantiateOnGameSceneLoad
    {
        public UndeadPatches() {
            var area = GameScene.FindInScene("UNDEAD", "Undead (Main)");

            spawner = area.Find("Objects/UndeadMonsterSpawner").GetComponent<UndeadMonsterSpawner>();
            sandstorm = area.Find("Objects/TrackingSandstorm").gameObject;
            vileSky = area.Find("desert2/VileSky").gameObject;
            windFX = area.Find("SFX/DesertWindFX").gameObject;
            natureFX = area.Find("SFX/DesertNatureFX").gameObject;

            area.GetComponent<Area>().skyboxFlag = LOCATION_FLAG;
            spawner.flag = LOCATION_FLAG;
            area.Find("FogHandlers/NormalFogHandler").GetComponent<FogHandlerFlag>().flag = LOCATION_FLAG;
            sandstorm.GetComponent<TwoState>().flag = LOCATION_FLAG;
            vileSky.GetComponent<TwoState>().flag = LOCATION_FLAG;
            windFX.GetComponent<TwoState>().flag = LOCATION_FLAG;
            natureFX.GetComponent<TwoState>().flag = LOCATION_FLAG;
            foreach (Transform child in area.Find("Entities")) {
                child.GetComponent<TwoState>().flag = LOCATION_FLAG;
            }
            
            Collecting.Location.RegisterTrigger(LOCATION_FLAG, MakePeaceful);
        }
        readonly GameObject sandstorm;
        readonly GameObject vileSky;
        readonly GameObject windFX;

        readonly GameObject natureFX;

        readonly UndeadMonsterSpawner spawner;
        
        static readonly string LOCATION_FLAG = "LOCATION_FELLA_UNDEAD1";

        void MakePeaceful()
        {
            spawner.Activate();
            
            sandstorm.SetActive(false);
            vileSky.SetActive(false);
            windFX.SetActive(false);
            
            natureFX.SetActive(true);
            
            GlobalHub.Instance.GetArea().Activate(false, true);
        }
    }
}