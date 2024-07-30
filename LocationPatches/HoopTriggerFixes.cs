using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationPatches
{
    class HoopTriggerFixes : InstantiateOnGameSceneLoad
    {
        public HoopTriggerFixes()
        {
            GameScene.FindInScene("PALACE", "Valley (Main)/Objects/AbyssHoops/AbyssHoop").GetComponent<TintChange>().flag = "LOCATION_PALACE_ABYSS_HOOP_SUCCESS";

            foreach (
                var hoopHandler
                in GameScene.GetComponentsInChildren<HoopHandler>(true)
            ) {
                if (hoopHandler.flag.StartsWith("LOCATION_")) {
                    Debug.LogWarning($"{hoopHandler.flag} has already been fixed");
                    continue;
                }
                hoopHandler.flag = $"LOCATION_{hoopHandler.flag}";
            }
        }
    }
}
