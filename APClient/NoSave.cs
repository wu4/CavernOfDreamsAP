using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.APClient
{
    [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.LoadFile))]
    static class NoLoad
    {
        static bool Prefix(Save save, int numSaveFile)
        {
            Debug.Log("not actually loading");
            return false;
        }
    }

    [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.SaveFile))]
    static class NoSave
    {
        static bool Prefix(Save save, int numSaveFile)
        {
            Debug.Log("not actually saving");
            return false;
        }
    }
}
