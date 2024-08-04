using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;

namespace CoDArchipelago
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            HarmonyFileLog.Enabled = true;

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");

            Patcher.Patch();
            InitPatches.AddMenuInit();
        }

        private void OnDestroy()
        {
            Patcher.Unpatch();
        }
    }

    public static class Patcher
    {
        static Harmony harmony;

        public static void Patch()
        {
            harmony = new Harmony("cavernofdreams.mod.archipelago");
            harmony.PatchAll();
        }

        public static void Unpatch()
        {
            harmony.UnpatchSelf();
        }
    }
}

