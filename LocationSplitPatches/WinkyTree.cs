using System.Linq;
using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationSplitPatches
{
    /// <summary>
    /// Change the Winky Tree's target behavior, making it always active.
    /// Instead, the target winks after the check has been cleared
    /// </summary>
    class WinkyTree : InstantiateOnGameSceneLoad
    {
        public WinkyTree()
        {
            MakeTargetUnbreakable();
            PatchSmile();

            Collecting.Location.RegisterTrigger("LOCATION_LAKE_SWINGS_RAISED", ShowSmile);
        }

        /// <summary>
        /// Adjusts the position of the smile on the Winky Tree such that it
        /// sits in front of the target, as well as adding a TwoState
        /// component to it
        /// </summary>
        static void PatchSmile()
        {
            Transform parent = GameScene.FindInScene("LAKE", "Lake (Main)/lake2");

            Transform smile = parent.Find("TargetSmile");

            // remove collision from smile just in case
            Component.Destroy(smile.GetComponent<MeshCollider>());

            smile.transform.localPosition = smile.transform.localPosition with {
                z = smile.transform.localPosition.z + 0.5f
            };
            smile.transform.localRotation = Quaternion.Euler(275f, 0f, 0f);

            TwoStateExists smile_tse = smile.gameObject.AddComponent<TwoStateExists>();
            smile_tse.flag = "LOCATION_LAKE_SWINGS_RAISED";
            smile_tse.flagOnExists = true;
        }

        static void MakeTargetUnbreakable()
        {
            Transform parent = GameScene.FindInScene("LAKE", "Lake (Main)/lake2");

            Transform target = parent.Find("Tree_Target");
            Component.DestroyImmediate(target.GetComponent<TwoStateExists>());

            Breakable br = target.GetComponent<Breakable>();
            WhackForCutscene wh = target.gameObject.AddComponent<WhackForCutscene>();

            typeof(Whackable).GetProperties()
                .Where(prop => prop.CanWrite && prop.CanRead)
                .Do(prop => prop.SetValue(wh, prop.GetValue(br)));

            wh.cutscene = br.cutsceneOnBreak;
            wh.canRepeat = false;

            Component.DestroyImmediate(br);
        }

        static void ShowSmile()
        {
            Transform smile = GameScene.FindInScene("LAKE", "Lake (Main)/lake2/TargetSmile");
            smile.gameObject.SetActive(true);
        }
    }
}
