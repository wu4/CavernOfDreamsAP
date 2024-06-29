using System.Collections.Generic;
using UnityEngine;
using System;
using CoDArchipelago.Collecting;
using CoDArchipelago.GlobalGameScene;
using System.Reflection;
using System.Linq;

namespace CoDArchipelago.VisualPatches
{
    static class VisualPatches
    {
        class ReplacementObjects : InstantiateOnGameSceneLoad
        {
            static readonly Dictionary<Replaces, ConstructorInfo> replacementObjectConstructors =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ObjectPatcher)))
                    .ToDictionary(
                        type => (Replaces)type.GetField(nameof(ObjectPatcher.replaces), BindingFlags.Public | BindingFlags.Static).GetValue(null),
                        type => type.GetConstructor(new Type[] {})
                    );

            static readonly Dictionary<Replaces, ConstructorInfo> replacementObjectConstructorsByType =
                replacementObjectConstructors
                .Where(kv => kv.Key.type != null)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value
                );

            static readonly Dictionary<string, Dictionary<string, ConstructorInfo>> replacementObjectConstructorsByGame =
                replacementObjectConstructors
                .Where(kv => kv.Key.gameItemsToReplace != null)
                .SelectMany<KeyValuePair<Replaces, ConstructorInfo>, (string rootName, string itemName, ConstructorInfo constructor)>(
                    kv => kv.Key.gameItemsToReplace.SelectMany(
                        root => root.Value.Select(itemName => (root.Key, itemName, kv.Value))
                    )
                )
                .GroupBy(
                    item => item.rootName
                )
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(
                        item => item.itemName,
                        item => item.constructor
                    )
                );

            static Dictionary<string, Dictionary<string, ObjectPatcher>> replacementObjectsByGame;
            static Dictionary<Enum, ObjectPatcher> replacementObjectsByType;
            public ReplacementObjects()
            {
                replacementObjectsByType = replacementObjectConstructorsByType
                    .ToDictionary(
                        kv => kv.Key.type,
                        kv => (ObjectPatcher)kv.Value.Invoke(new object[] {})
                    );

                replacementObjectsByGame = replacementObjectConstructorsByGame
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.ToDictionary(
                            kv2 => kv2.Key,
                            kv2 => (ObjectPatcher)kv2.Value.Invoke(new object[] {})
                        )
                    );
            }
            static bool TryGetOtherGameObject(string gameName, string itemName, out ObjectPatcher obj)
            {
                obj = null;
                if (!replacementObjectsByGame.TryGetValue(gameName, out var game)) return false;
                return game.TryGetValue(itemName, out obj);
            }

            static ObjectPatcher GetGameObject(Collecting.Item item)
            {
                if (
                    item is Collecting.TheirItem theirItem
                    && TryGetOtherGameObject("GAMENAME", theirItem.itemName, out ObjectPatcher rep)
                ) {
                    return rep;
                }
                return replacementObjectsByType[item.type];
            }

            public static GameObject ReplaceObject(GameObject obj, Collecting.Item item)
                => GetGameObject(item).Replace(obj, item);

            public static void CollectJingle(Collecting.Item item)
            {
                GetGameObject(item).CollectJingle();
            }
        }

        public static void CollectJingle(Collecting.Item item) =>
            ReplacementObjects.CollectJingle(item);

        static void PatchCollectible(Collectible col)
        {
            TwoState ts = col.GetComponent<TwoState>();
            GameObject obj = col.gameObject;

            Collecting.Item item;

            if (!Location.checks.TryGetValue(ts.flag, out item))
                item = new MyItem(ts.flag.Substring(9), randomized: false);

            GameObject newObj = ReplacementObjects.ReplaceObject(obj, item);
            if (collectibleTriggers.TryGetValue(ts.flag, out Action<GameObject> action)) {
                action(newObj);
            }
        }

        static readonly Dictionary<string, Action<GameObject>> collectibleTriggers = new();
        public static void RegisterTrigger(string locationFlag, Action<GameObject> action) =>
            collectibleTriggers.Add(locationFlag, action);

        class ResetCollectibleTriggers : InstantiateOnGameSceneLoad
        {
            [LoadOrder(Int32.MinValue)]
            public ResetCollectibleTriggers()
            {
                collectibleTriggers.Clear();
            }
        }

        class PatchAllVisuals : InstantiateOnGameSceneLoad
        {
            public PatchAllVisuals()
            {
                var cols = GameScene.GetComponentsInChildren<Area>(true).SelectMany(area => area.GetComponentsInChildren<Collectible>(true));

                foreach (Collectible col in cols) {
                    TwoState ts = col.GetComponent<TwoState>();

                    // Gallery lobby contains a fake egg. Its associated
                    // cutscene is skipped, so we never see the egg anyways
                    if (ts.flag == "GALLERY_TRAPDOOR_ACTIVE") {
                        GameObject.Destroy(col.gameObject);
                        continue;
                    }

                    ts.flag = "LOCATION_" + ts.flag;

                    PatchCollectible(col);
                }
            }
        }

        // [HarmonyPatch(typeof(Area), "Activate")]
        // static class PatchVisualsOnAreaLoad
        // {
        //     static bool Prefix(Area __instance, ref bool __runOriginal)
        //     {
        //         if (!__runOriginal) return false;

        //         if (__instance.transform.Find("AlreadyPatched") != null) return true;
        //     
        //         var cols = __instance.GetComponentsInChildren<Collectible>(true);

        //         foreach (Collectible col in cols) {
        //             TwoState ts = col.GetComponent<TwoState>();

        //             // Gallery lobby contains a fake egg. Its associated
        //             // cutscene is skipped, so we never see the egg anyways
        //             if (ts.flag == "GALLERY_TRAPDOOR_ACTIVE") {
        //                 GameObject.Destroy(col.gameObject);
        //                 continue;
        //             }

        //             ts.flag = "LOCATION_" + ts.flag;

        //             PatchCollectible(col);
        //         }

        //         GameObject g = new();
        //         g.name = "AlreadyPatched";
        //         g.transform.parent = __instance.transform;

        //         return true;
        //     }
        // }
    }
}
