using System.Text.RegularExpressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace CoDArchipelago
{
    static class CollectiblePatches
    {
        [HarmonyPatch(typeof(CollectibleItem), "Collect")]
        static class CollectibleItemPatch {
            static bool Prefix(CollectibleItem __instance, ref bool __runOriginal) {
                if (!__runOriginal) return false;

                // TwoState ts = __instance.GetComponent<TwoStateExists>();
                // GlobalHub.Instance.save.SetFlag("HAS_" + ts.flag, true);

                return CollectPatch.Prefix(__instance, ref __runOriginal);
            }
        }
            
        [HarmonyPatch(typeof(Collectible), "Collect", new Type[] {})]
        public static class CollectPatch
        {
            static readonly MethodInfo collectEffectsInfo = AccessTools.Method(typeof(Collectible), "CollectEffects");

            static readonly AccessTools.FieldRef<GlobalHub, GameObject> cutsceneItemModelRef =
                AccessTools.FieldRefAccess<GlobalHub, GameObject>("cutsceneItemModel");

            static readonly AccessTools.FieldRef<Player, Animator> animatorRef =
                AccessTools.FieldRefAccess<Player, Animator>("animator");

            static void DoPlayGotItemCutscene(Collectible collectible)
            {
                GameObject model = collectible.model;
                Cutscene itemCutscene = collectible.cutscene;
                if (itemCutscene == null) {
                    throw new Exception("Fella cutscene is null");
                } else {
                    // force animation reset
                    animatorRef(GlobalHub.Instance.player).Play("Attack", -1, 0);
                    GlobalHub.Instance.SetCutscene(itemCutscene);
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

            public static bool Prefix(Collectible __instance, ref bool __runOriginal)
            {
                if (!__runOriginal) return false;

                __instance.model?.transform.SetParent(null);
                __instance.cutscene?.transform.SetParent(null);
                __instance.GetComponent<TwoStateExists>().SetFlag();
                collectEffectsInfo.Invoke(__instance, new object[] {});
                GameObject.Destroy(__instance.gameObject);

                // LocationPatching.CollectJingle(__instance.type);
                if (
                    __instance.type == Collectible.CollectibleType.FELLA
                    || __instance.type == Collectible.CollectibleType.ITEM
                ) {
                    DoPlayGotItemCutscene(__instance);
                }
                // ref World world = ref worldRef(GlobalHub.Instance);

                // GlobalHub.Instance.save.AddCollectible(__instance.type, __instance.amount);
                //// do stuff
                return false;
            }

        }

        static readonly Regex _card_regex = new("^(CARD)_([A-Z]+)_([A-Z_]+)$");
        static readonly Regex _gratitude_regex = new("^GRATITUDE([0-9]+)$");
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