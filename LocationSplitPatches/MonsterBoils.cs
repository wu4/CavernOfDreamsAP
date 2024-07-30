using System.Collections.Generic;
using CoDArchipelago.GlobalGameScene;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.LocationSplitPatches
{
    [HarmonyPatch(typeof(MonsterBoilListener), "RemoveBoil")]
    static class MonsterBoilListenerPatch
    {
        static bool Prefix(MonsterBoilListener __instance, ref int ___numBoils)
        {
            ___numBoils--;
            if (___numBoils == 0) {
                StockSFX.Instance.jingleGood.Play();
                GlobalHub.Instance.save.SetFlag("LOCATION_MONSTER_BOILS_REMOVED", true);
                GameObject.Destroy(__instance.gameObject);
            }

            return false;
        }
    }

    class FixMonsterBoilFlags : InstantiateOnGameSceneLoad
    {
        public FixMonsterBoilFlags()
        {
            foreach (
                var boil
                in GameScene.FindInScene("MONSTER", "Monster")
                            .GetComponentsInChildren<MonsterBoil>()
            ) {
                boil.flag = "LOCATION_MONSTER_BOILS_REMOVED";
            }
        }
    }
}
