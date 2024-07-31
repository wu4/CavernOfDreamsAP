using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    class Drown : InstantiateOnGameSceneLoad
    {
        readonly Transform area;
        static readonly string LOCATION_FLAG = "LOCATION_FELLA_DROWN1";

        public Drown()
        {
            area = GameScene.FindInScene("DROWN", "Drown (Main)");

            fogBottom = area.Find("drown3/FogBottom").gameObject;
            fogTop = area.Find("drown3/FogTop").gameObject;
            debris = area.Find("FX/Debris").gameObject;

            divingHelmetLid = area.Find("drown3/DivingHelmetLid").GetComponent<Activation>();
            drownWater = area.Find("Objects/DrownWaterHandler").GetComponent<DrownWater>();
            bubbleUp = area.Find("Objects/BubbleUp").gameObject;
            propellerRotate = area.Find("drown3/Propeller").GetComponent<Rotate>();
            propellerSFX = area.Find("SFX/PropellerSound").gameObject;

            area.GetComponent<Area>().skyboxFlag = LOCATION_FLAG;
            fogBottom.GetComponent<TwoState>().flag = LOCATION_FLAG;
            fogTop.GetComponent<TwoState>().flag = LOCATION_FLAG;
            debris.GetComponent<TwoState>().flag = LOCATION_FLAG;
            divingHelmetLid.flag = LOCATION_FLAG;
            drownWater.flag = LOCATION_FLAG;
            bubbleUp.GetComponent<TwoState>().flag = LOCATION_FLAG;
            propellerRotate.GetComponent<TwoStateComponent>().flag = LOCATION_FLAG;
            propellerSFX.GetComponent<TwoState>().flag = LOCATION_FLAG;

            area.Find("FogHandlers/WaterFogHandler").GetComponent<FogHandlerDrown>().flag = LOCATION_FLAG;

            Collecting.Location.RegisterTrigger(LOCATION_FLAG, MakePeaceful);
        }

        readonly GameObject fogBottom;
        readonly GameObject fogTop;
        readonly GameObject debris;

        readonly Activation divingHelmetLid;
        readonly DrownWater drownWater;
        readonly GameObject bubbleUp;
        readonly Rotate propellerRotate;
        readonly GameObject propellerSFX;

        void MakePeaceful()
        {
            fogBottom.SetActive(false);
            fogTop.SetActive(false);
            debris.SetActive(false);

            divingHelmetLid.Activate();
            drownWater.Activate();
            bubbleUp.SetActive(true);
            propellerRotate.enabled = true;
            propellerSFX.SetActive(true);

            GlobalHub.Instance.GetArea().Activate(false, true);
        }
    }
}
