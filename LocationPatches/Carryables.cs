using System.Collections.Generic;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;
using System;

namespace CoDArchipelago.LocationPatches
{
    class Carryables : InstantiateOnGameSceneLoad
    {
        static readonly Dictionary<string, string> carryablePaths = new() {
            {"CAVE_CARRYABLE_BOOTS_MONSTERLOBBY","CAVE/Monster Lobby/Objects/HoverBootsReplacer"},

            {"LAKE_CARRYABLE_APPLE_LAKE","LAKE/Lake (Main)/Objects/Fruit Tree/Replacer"},
            {"LAKE_CARRYABLE_APPLE_ENTRY","LAKE/Lake (Main)/Objects/Fruit Tree (1)/Replacer"},
            {"LAKE_CARRYABLE_APPLE_DEEPENTRY","LAKE/Lake (Main)/Objects/Fruit Tree (2)/Replacer"},
            {"LAKE_CARRYABLE_APPLE_WINKYLEDGE","LAKE/Lake (Main)/Objects/Fruit Tree (3)/Replacer"},
            {"LAKE_CARRYABLE_APPLE_DEEPWOODS","LAKE/Lake (Main)/Objects/FruitTreeSidewaysReplacer"},
            {"LAKE_CARRYABLE_BOOTS_DEEPWOODS","LAKE/Lake (Main)/Objects/HoverBootsReplacer"},
            {"LAKE_CARRYABLE_APPLE_CRYPT","LAKE/Lake (Main)/Objects/Fruit Tree (5)/Replacer"},

            {"MONSTER_CARRYABLE_MEDICINE_MAIN","MONSTER/Monster/Rotate (Inside Monster)/Objects (Meeting)/Monster Throwable Replacer Variant"},
            {"MONSTER_CARRYABLE_MEDICINE_GREEN","MONSTER/Monster/Rotate (Inside Monster)/Objects (Garden)/Monster Throwable Replacer"},
            {"MONSTER_CARRYABLE_MEDICINE_LAB","MONSTER/Monster/Rotate (Inside Monster)/Objects (Lab)/Monster Throwable Replacer Variant"},
            {"MONSTER_CARRYABLE_MEDICINE_BEDROOM","MONSTER/Monster/Rotate (Inside Monster)/Objects (TwoWay)/Monster Throwable Replacer Variant"},
            {"MONSTER_CARRYABLE_MEDICINE_POOL","MONSTER/Monster/Rotate (Inside Monster)/Objects (Cargo)/Monster Throwable Replacer Variant"},
            {"MONSTER_CARRYABLE_WINGS","MONSTER/Sky (Main)/Objects/GliderReplacer"},

            {"PALACE_CARRYABLE_BOOTS","PALACE/Valley (Main)/Objects/HoverBootsReplacer"},
            {"PALACE_CARRYABLE_BUBBLECONCH_PALACE","PALACE/Palace/Objects/Torpedo Replacer"},
            {"PALACE_CARRYABLE_BUBBLECONCH_SANCTUM","PALACE/Sanctum/Objects/Torpedo Replacer"},

            {"GALLERY_CARRYABLE_WINGS_EARTHLOBBY","GALLERY/Earth Lobby/Objects (Castle)/PaintingItemMonsterReplacer"},
            {"GALLERY_CARRYABLE_FISH_FIRELOBBY","GALLERY/Fire Lobby/Objects/ReplacerPaintingItemKappa"},
            {"GALLERY_CARRYABLE_GLOVES_WATERLOBBY","GALLERY/Water Lobby/Objects Center/ReplacerPaintingItemSage"},
            {"GALLERY_CARRYABLE_HEAD_WATERLOBBY","GALLERY/Water Lobby/Objects Storage/ReplacerPaintingItemPrincess"},
            {"GALLERY_CARRYABLE_BOOTS_WATERLOBBY","GALLERY/Water Lobby/Objects Storage/HoverBootsReplacer Variant"},

            {"DROWN_CARRYABLE_BUBBLECONCH","DROWN/Drown (Main)/Objects/Torpedo Replacer"},
        };

        static readonly string[] pipeParts = new string[] {
            "dispenser",
            "emitter",
            "Shadow",
            "Particle System"
        };

        static void MoveMedicinePipeOut(Transform medicinePipe)
        {
            GameObject pipe = new("Medicine Pipe");
            pipe.transform.SetParent(medicinePipe.parent, false);

            foreach (string part in pipeParts) {
                medicinePipe.Find(part).SetParent(pipe.transform, true);
            }
        }

        class CarryableCollectible : Collectible
        {
            public override void Collect()
            {
                var flag = GetComponent<TwoState>().flag;
                if (!GlobalHub.Instance.save.GetFlag(flag).on)
                    GlobalHub.Instance.save.SetFlag(flag, true);
                Component.Destroy(GetComponent<SphereCollider>());
            }
        }

        [LoadOrder(Int32.MinValue + 1)]
        public Carryables()
        {
            foreach ((string carryableFlag, string carryablePath) in carryablePaths) {
                Transform carryable = GameScene.FindInSceneFullPath(carryablePath);

                if (carryablePath.Contains("Monster Throwable Replacer")) {
                    MoveMedicinePipeOut(carryable);
                }

                var collectible = carryable.gameObject.AddComponent<CarryableCollectible>();
                collectible.type = (Collectible.CollectibleType)APCollectibleType.Carryable;

                SphereCollider collider = carryable.gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.5f;
                collider.center = new Vector3(0f, 0.5f, 0f);
                collider.isTrigger = true;
                collider.tag = "NotPlayer";

                var ts = carryable.gameObject.GetComponent<TwoState>();
                ts.flag = carryableFlag;
            }
        }
    }
}
