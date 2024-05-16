using System;
using BepInEx;
using HarmonyLib;
using Archipelago.MultiClient.Net;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Packets;
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
        }

        private void OnDestroy()
        {
            Patcher.Unpatch();
        }
    }

    public static class ArchipelagoConnection {
        private static bool _connected = false;

        private static async Task<bool> Connect() {
            if (_connected) return false;

            ArchipelagoSession sesh = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

            RoomInfoPacket packet = null;

            while (packet == null) {
                try {
                    packet = await sesh.ConnectAsync();
                }
                catch (Exception) {
                }
            }
            return true;
        }
    }

    public static class Patcher
    {
        // make sure DoPatching() is called at start either by
        // the mod loader or by your injector
        static Harmony harmony;

        public static void Patch()
        {
            harmony = new Harmony("cavernofdreams.mod.archipelago");
            // PrincessPatches.Patch(harmony);
            harmony.PatchAll();
        }

        public static void Unpatch()
        {
            harmony.UnpatchSelf();
        }
    }

    /*
    static class DialoguePatches
    {
        private static readonly Dictionary<string, string[]> replace_dialogue = new Dictionary<string, string[]>(){
            {"Sage Post Intro", new[]{"lad u cannot b srs"}},
            {"Sage Unlock Attack Cutscene", new[]{
                "Alright, bro. It's time for me to give you a check. I hope" +
                " you're ready to groan at the 37th Power Bomb and continue" +
                " BKing your friends.",

                "Holy shit",
                "God damn",

                "Yeah, well, you're welcome, idiot.",
                "There's your epicness.",
            }}
        };

        [HarmonyPatch(typeof(Dialog), nameof(Dialog.GetText))]
        class DialogPatch
        {
            static bool Prefix(Dialog __instance, ref string __result)
            {
                GameObject parent = __instance.gameObject.transform.parent.gameObject;
                if (replace_dialogue.TryGetValue(parent.name, out string[] dialogues)) {
                    int ind = Array.IndexOf(parent.GetComponentsInChildren<Dialog>(), __instance);
                    __result = dialogues[ind];
                    return false;
                }

                return true;
            }
        }
    }
    */
}

