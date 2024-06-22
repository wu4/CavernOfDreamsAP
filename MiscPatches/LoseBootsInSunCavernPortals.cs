using HarmonyLib;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    ///<summary>
    ///Removes the Jester Boots removal trigger in Sun Cavern, and instead
    ///removes Jester Boots upon portal entry.
    ///</summary>
    static class LoseBootsInSunCavernPortals
    {
        class RemoveSunCavernBootRemovalTrigger: InstantiateOnGameSceneLoad
        {
            public RemoveSunCavernBootRemovalTrigger()
            {
                GameObject.DestroyImmediate(GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Objects/BootRemovalTrigger").gameObject);
            }
        }

        static bool IsSunCavernPortal(Transform t)
        {
            if (t.name != "Portal") return false;
            Transform parent = t.GetParent();
            if (!parent.name.StartsWith("Nest FellaHatchable")) return false;

            return true;
        }

        [HarmonyPatch(typeof(WarpTrigger), "Warp")]
        static class RemoveBootsOnSunCavernPortalWarp
        {
            public static void Postfix(WarpTrigger __instance)
            {
                Debug.Log(("Warping from: ", __instance.name));
                Player player = GlobalHub.Instance.player;
                if (!player.WearingHoverBoots()) return;

                if (IsSunCavernPortal(__instance.transform)) goto RemoveBoots;
                if (IsSunCavernPortal(__instance.destination.transform.GetParent())) goto RemoveBoots;
                return;

                RemoveBoots:
                    GlobalHub.Instance.player.EquipHoverBoots(false, true);
            }
        }
    }
}
