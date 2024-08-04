using System;
using System.Collections.Generic;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class CarryablePatch : ObjectPatcher
    {
        public static readonly new Replaces replaces = new(APCollectibleType.Carryable);

        public static readonly Dictionary<string, GameObject> carryables = new();

        public override GameObject Replace(GameObject toReplace, Item item)
        {
            string carryableType = item.GetFlag();

            string carryable = carryableType;
            if (Data.carryableItems.TryGetValue(carryableType, out var val)) {
                Debug.LogError($"received flag for {carryable} instead of item");
                carryable = val;
            }

            GameObject newObject = UnityEngine.Object.Instantiate(carryables[carryable], new Transform());

            newObject.name = toReplace.name;
            newObject.transform.parent   = toReplace.transform.parent;
            newObject.transform.position = toReplace.transform.position;
            newObject.transform.rotation = toReplace.transform.rotation;

            GameObject.Destroy(toReplace);

            return newObject;
        }

        public override void CollectJingle()
        {
            throw new NotImplementedException();
        }

        static GameObject CopyCarryable(string fullPath)
        {
            var toCopy = GameScene.FindInSceneFullPath(fullPath).gameObject;
            var obj = GameObject.Instantiate(toCopy, Container.transform);

            return obj;
        }

        public CarryablePatch()
        {
            carryables.Clear();
            carryables["Jester Boots"] = CopyCarryable("CAVE/Monster Lobby/Objects/HoverBootsReplacer");
            carryables["Apple"] = CopyCarryable("LAKE/Lake (Main)/Objects/Fruit Tree/Replacer");

            carryables["Bubble Conch"] = CopyCarryable("PALACE/Sanctum/Objects/Torpedo Replacer");

            carryables["Medicine"] = CopyCarryable("MONSTER/Monster/Rotate (Inside Monster)/Objects (Cargo)/Monster Throwable Replacer Variant");

            carryables["Mr. Kerrington's Wings"] = CopyCarryable("MONSTER/Sky (Main)/Objects/GliderReplacer");
            carryables["Sage's Gloves"] = CopyCarryable("GALLERY/Water Lobby/Objects Center/ReplacerPaintingItemSage");
            carryables["Lady Opal's Head"] = CopyCarryable("GALLERY/Water Lobby/Objects Storage/ReplacerPaintingItemPrincess");
            carryables["Shelnert's Fish"] = CopyCarryable("GALLERY/Fire Lobby/Objects/ReplacerPaintingItemKappa");
        }
    }
}
