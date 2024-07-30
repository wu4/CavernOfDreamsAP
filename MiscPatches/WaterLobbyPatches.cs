using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    class WaterLobbyPatches : InstantiateOnGameSceneLoad
    {
        GameObject barsContainer;
        GameObject barsBaseObj;

        void CreateBarsAt(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            GameObject newObj = GameObject.Instantiate(barsBaseObj, barsContainer.transform);
            Transform t = newObj.transform;
            t.localPosition = position;
            t.localRotation = Quaternion.Euler(rotation);
            t.localScale = scale;
        }

        void CreateBarPlatform(
            Vector3 position,
            Vector3? scale = null
        ) {
            Vector3 scale_ = scale ?? Vector3.one;
            scale_.Scale(new(0.5f, 0.5f, 0.4075f));

            CreateBarsAt(
                position,
                rotation: new Vector3(90f, 90f, 0f),
                scale: scale_
            );
        }

        void AddSewerSafetyBars() {
            Transform sewerParent = GameScene.FindInScene("GALLERY", "Water Lobby/water_lobby2/Sewer");
            barsBaseObj = sewerParent.Find("Bars_001").gameObject;
            barsContainer = new("AP Safety Bars");
            barsContainer.transform.SetParent(sewerParent, false);
            barsContainer.transform.localPosition = Vector3.zero;
            barsContainer.transform.localScale = Vector3.one;
            barsContainer.transform.localRotation = Quaternion.Euler(Vector3.zero);

            // hide them once Swim is acquired
            TwoStateExists ts = barsContainer.AddComponent<TwoStateExists>();
            ts.flag = "HAS_SKILL_SWIM";
            ts.flagOnExists = false;

            // entrance in the center
            CreateBarPlatform(new(0f, 28f, -0.025f));

            // entrance next to the Sewer
            CreateBarPlatform(new(0f, -8f, -0.025f));
            CreateBarPlatform(new(0f, 0f, -0.025f));

            // entrance with the egg
            CreateBarPlatform(new(-24f, 28f, -0.025f), new(1f, 0.5f, 1f));

            // entrance with the giant
            CreateBarPlatform(new(32f, 28f, 3.975f));
            CreateBarPlatform(new(24f, 28f, 3.975f));
            CreateBarPlatform(new(16f, 28f, 3.975f));
        }

        public WaterLobbyPatches()
        {
            AddSewerSafetyBars();
        }
    }
}
