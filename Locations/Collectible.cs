using System.Text.RegularExpressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using BepInEx.Logging;
using UnityEngine.SceneManagement;
using System.Linq;
using Archipelago.MultiClient.Net.Packets;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlTypes;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace CoDArchipelago
{
    static class APCollectible
    {

        public enum APCollectibleType {
            Minor = 100,
            Major = 101,
            Event = 102
        }

        [HarmonyPatch(typeof(Area), "Activate")]
        static class Patch
        {
            static bool Prefix(Area __instance)
            {

                if (__instance.transform.Find("AlreadyPatched") != null) return true;

                var cols = __instance.GetComponentsInChildren<Collectible>();

                foreach (Collectible col in cols) {
                    // col.type = Collectible.CollectibleType.KEY;
                    TwoState ts = col.GetComponent<TwoState>();
                    // paths.Add(ts.flag, col.type);
                    ts.flag = "LOCATION_" + ts.flag;
                }

                GameObject g = new();
                g.name = "AlreadyPatched";
                g.transform.parent = __instance.transform;

                return true;
            }
        }

        [HarmonyPatch(typeof(Save), "SetFlag")]
        static class SetFlagPatch
        {
            static readonly Dictionary<string, string> localChecks = new(){
                {"LOCATION_NOTE_CAVE2", "CAVE_NURIKABE_FALLEN"},
            };

            static void Postfix(Save __instance, string name, bool b)
            {
                if (!b) return;

                if (name.StartsWith("LOCATION")) {
                    if (localChecks.TryGetValue(name, out string extra_flag)) {
                        if (!Events.TryCollect(extra_flag)) {
                            __instance.SetFlag(extra_flag, true);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Collectible), "Collect", new Type[] {})]
        public static class CollectiblePatch
        {
            static readonly MethodInfo collectEffectsInfo = typeof(Collectible).GetMethod("CollectEffects", BindingFlags.NonPublic | BindingFlags.Instance);

            static readonly AccessTools.FieldRef<GlobalHub, GameObject> cutsceneItemModelRef =
                AccessTools.FieldRefAccess<GlobalHub, GameObject>("cutsceneItemModel");

            static readonly AccessTools.FieldRef<Player, Animator> animatorRef =
                AccessTools.FieldRefAccess<Player, Animator>("animator");

            public static void DoPlayGotItemCutscene(Collectible collectible)
            {
                GameObject model = collectible.model;
                Cutscene itemCutscene = collectible.cutscene;
                if (itemCutscene != null) {
                    animatorRef(GlobalHub.Instance.player).Play("Attack", -1, 0);
                    GlobalHub.Instance.SetCutscene(itemCutscene);
                } else {
                    Debug.LogWarning("Fella cutscene is null");
                }

                ref GameObject cutsceneItemModel = ref cutsceneItemModelRef(GlobalHub.Instance);

                if (cutsceneItemModel != null) {
                    cutsceneItemModel.transform.SetParent(null);
                    GameObject.Destroy(cutsceneItemModel);
                }

                cutsceneItemModel = model;
                cutsceneItemModel.transform.SetParent(GlobalHub.Instance.player.collectibleBase.transform);
                cutsceneItemModel.transform.localPosition = new Vector3();
                cutsceneItemModel.transform.localRotation = new Quaternion();
                cutsceneItemModel.transform.localScale = new Vector3(1f, 1f, 1f);

                Animator a = cutsceneItemModel.GetComponentInChildren<Animator>();
                if (a != null) a.enabled = false;

                Rotate r = cutsceneItemModel.GetComponentInChildren<Rotate>();
                if (r != null) r.enabled = false;
            }

            static readonly AccessTools.FieldRef<GlobalHub, World> worldRef =
                AccessTools.FieldRefAccess<GlobalHub, World>("world");

            static bool Prefix(Collectible __instance)
            {
                __instance.model?.transform.SetParent(null);
                __instance.cutscene?.transform.SetParent(null);
                __instance.GetComponent<TwoStateExists>().SetFlag();
                collectEffectsInfo.Invoke(__instance, new object[] {});
                UnityEngine.Object.Destroy(__instance.gameObject);
                ref World world = ref worldRef(GlobalHub.Instance);

                // UIController.Instance.SetModelVisible(__instance.type);
                // GlobalHub.Instance.save.AddCollectible(__instance.type, __instance.amount);
                //// do stuff
                // UIController.Instance.collectibleCounter.text = string.Concat((object) GlobalHub.Instance.save.GetCollectible(__instance.type));
                return false;
            }

        }

        static readonly Regex _card_regex = new("^(CARD)_([A-Z]+)_([A-Z_]+)$");
        static readonly Regex _gratitude_regex = new("^GRATITUDE_([0-9]+)$");
        static readonly Regex _other_regex = new("^([A-Z]+)_([A-Z_]+)([0-9]+)$");

        [HarmonyPatch(typeof(Save), "DoesFlagMatchPattern")]
        [HarmonyPatch(new Type[] {typeof(string), typeof(Collectible.CollectibleType)})]
        static class FlagNamePatch1
        {
            static bool Prefix(ref bool __result, string flagName, Collectible.CollectibleType type)
            {
                switch (type) {
                    case Collectible.CollectibleType.CARD:
                        __result = _card_regex.IsMatch(flagName);
                        return false;
                    case Collectible.CollectibleType.GRATITUDE:
                        __result = _gratitude_regex.IsMatch(flagName);
                        return false;
                    default:
                        var m = _other_regex.Match(flagName);
                        if (!m.Success) {
                            __result = false;
                            return false;
                        }

                        var flag_type = m.Groups[1].Value;
                        string name = Enum.GetName(typeof(Collectible.CollectibleType), type);

                        __result = flag_type == name;
                        return false;
                }
            }
        }

        [HarmonyPatch(typeof(Save), "DoesFlagMatchPattern")]
        [HarmonyPatch(new Type[] {typeof(string), typeof(Collectible.CollectibleType), typeof(string)})]
        static class FlagNamePatch2
        {
            static bool Prefix(string flagName, Collectible.CollectibleType type, string worldName, ref bool __result)
            {
                if (type == Collectible.CollectibleType.GRATITUDE) {
                    __result = false;
                    return false;
                }

                Match m = (type == Collectible.CollectibleType.CARD ? _card_regex : _other_regex).Match(flagName);

                if (!m.Success) {
                    __result = false;
                    return false;
                }

                __result = worldName == m.Groups[2].Value;
                return false;
            }
        }
    }
}