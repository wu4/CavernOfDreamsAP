using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    class NoSentryLaserGaps : InstantiateOnGameSceneLoad
    {
        public NoSentryLaserGaps()
        {
            Transform parent = GameScene.FindInScene("PALACE", "Palace/Entities");
            RemoveSentryWallLaserGaps(parent.Find("SentryWall"));
            RemoveSentryWallLaserGaps(parent.Find("SentryWall (1)"));
        }

        static void RemoveSentryWallLaserGaps(Transform sentryWall)
        {
            sentryWall.GetComponentsInChildren<Sentry>().Do(RemoveSentryLaserGap);
        }

        static void RemoveSentryLaserGap(Sentry sentry)
        {
            Debug.Log("Changing Sentry location", sentry);
            Transform laser = sentry.transform.Find("sentry/Head/Laser");
            laser.localPosition = laser.localPosition with {
                z = 1f
            };
        }
    }
}
