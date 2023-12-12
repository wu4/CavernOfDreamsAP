using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace CoDArchipelago
{
    public class MapPatches
    {
        /// <summary>
        /// Change the Winky Tree's target behavior, making it always active.
        /// Instead, the target winks after the check has been cleared
        /// </summary>
        [HasInitMethod]
        static void PatchWinkyTreeTarget()
        {
            Transform parent = GlobalGameScene.FindInScene("LAKE", "Lake (Main)/lake2");

            {
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

            {
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
        }

        /// <summary>
        /// Shift Heaven's Path finish door backwards to enable finishing the
        /// race with it closed
        /// </summary>
        static void PatchHeavensPathRace()
        {
            var sanctumFellaGate = GlobalGameScene.FindInScene("PALACE", "Sanctum/seadeep/Fella_Gate");
            sanctumFellaGate.localPosition = sanctumFellaGate.localPosition with {
                z = -17.1423f
            };
        }


        static readonly string[] valleyWaterPaths = new string[2] {
            // collision
            "Valley (Main)/palace2/WaterMesh",
            // visual
            "Valley (Main)/palace2/Water"
        };
        /// <summary>
        /// Very slightly lower the water in Prismic Palace such that you don't
        /// drown on entry from Lostleaf
        /// </summary>
        static void PatchPalaceWaterHeight()
        {
            foreach (string path in valleyWaterPaths) {
                var transform = GlobalGameScene.FindInScene("PALACE", path);
                transform.localPosition = transform.localPosition with {
                    y = transform.localPosition.y - 0.1f
                };
            }
        }

        /// <summary>
        /// Adds some new platforms in the Sand Castle area to enable traversal
        /// without mobility items when the ice is melted.
        /// </summary>
        static void PatchPalaceNewPlatformsAtSandCastle()
        {
            Transform parent = GlobalGameScene.FindInScene("PALACE", "Valley (Main)/palace2");
            GameObject prefab = parent.Find("SnowCastleSwitchLedge").gameObject;

            {
                GameObject newPlatform = GameObject.Instantiate(prefab);
                newPlatform.transform.parent = parent;
                newPlatform.name = "AP Platform";
                newPlatform.transform.localPosition = new Vector3() {x = -53f, y = -2f, z = -17f};
                newPlatform.transform.localScale = new Vector3() {x = 1f, y = 1.9f, z = 1.5f};
                newPlatform.transform.localRotation = Quaternion.Euler(270f, 45f, 0f);
            }

            {
                GameObject newPlatform = GameObject.Instantiate(prefab);
                newPlatform.transform.parent = parent;
                newPlatform.name = "AP Platform 2";
                newPlatform.transform.localPosition = new Vector3() {x = -56.5f, y = 0.5f, z = -8f};
                newPlatform.transform.localScale = new Vector3() {x = 2.2f, y = 0.7f, z = 0.5f};
                newPlatform.transform.localRotation = Quaternion.Euler(270f, 50f, 0f);
            }
        }

        /// <summary>
        /// Changes the Observatory door's requirement such that it remains open
        /// after the first time melting the ice.
        /// </summary>
        static void PatchObservatoryDoor()
        {
            GlobalGameScene.FindInScene("PALACE", "Valley (Main)/palace2/Observatory_Door").GetComponent<TwoState>().flag = "HAS_PALACE_MELTED_ICE";
        }

        /// <summary>
        /// Adds a bridge to the Prismic Palace -> Lostleaf Lake connector such
        /// that you no longer drown on entry or risk death when trying to
        /// traverse it without the ability to swim.
        /// </summary>
        static void PatchPalaceLakeConnectorEntry()
        {
            Transform parent = GlobalGameScene.FindInScene("PALACE", "Valley (Main)/palace2");
            GameObject bridge = GameObject.Instantiate(parent.Find("Bridge").gameObject, parent, false);
            bridge.transform.localPosition = new Vector3(-44f, -1f, -30f);
            bridge.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            bridge.transform.localScale = new Vector3(3f, 1.125f, 0.5f);
        }

        /// <summary>
        /// Slightly moves the location you start in when you enter Prismic
        /// Palace from the secret Lostleaf Lake connector to prevent instantly
        /// drowning.
        /// </summary>
        static void PatchLakePalaceConnectorDestination()
        {
            Transform warp = GlobalGameScene.FindInScene("PALACE", "Valley (Main)/Warps/DestFromLakeToValley");
            warp.localPosition = warp.localPosition with {
                y = warp.localPosition.y + 0.1f,
                z = warp.localPosition.z + 1f
            };
        }

        public static void Init()
        {
            PatchWinkyTreeTarget();
            PatchHeavensPathRace();
            // PatchPalaceWaterHeight();
            PatchPalaceLakeConnectorEntry();
            PatchObservatoryDoor();
            // PatchPalaceNewPlatformsAtSandCastle();
        }
    }
}