using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class Shroom : ObjectPatcher
    {
        public static readonly new Replaces replaces = new(CollectibleItem.CollectibleType.NOTE);

        readonly Dictionary<string, Sprite> shroomSprites;
        readonly GameObject shroom;

        public override GameObject Replace(GameObject toReplace, Item item)
        {
            string flagName = item.GetFlag();

            string worldName = GetWorldFromFlag(flagName, shroomWorldRegex)
                ?? throw new Exception("failed to identify world for shroom " + flagName);

            GameObject obj = ReplaceWith(toReplace, shroom);
            obj.GetComponentInChildren<SpriteRenderer>().sprite = shroomSprites[worldName];

            return obj;
        }

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleCollectTiny.Play();
        }

        public Shroom()
        {
            shroom = GameObject.Instantiate(GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles/Notes/NotePathLake/NoteCave").gameObject, Container.transform);
            shroom.name = "Shroom";

            shroomSprites = GetShroomSprites();
        }

        static readonly Dictionary<string, string> sampleShrooms = new() {
            {"CAVE", "Sun Cavern (Main)/Collectibles/Notes/NotePathLake/NoteCave"},
            {"LAKE", "Lake (Main)/Collectibles/Notes/NoteBranches/NoteLake"},
            {"MONSTER", "Sky (Main)/Collectibles/Notes/NoteMonster"},
            {"PALACE", "Valley (Main)/Collectibles/Notes/EntranceNotes/NotePalace"},
        };

        Dictionary<string, Sprite> GetShroomSprites() =>
            sampleShrooms.ToDictionary(
                o => o.Key,
                o => GameScene.FindInScene(o.Key, o.Value).GetComponentInChildren<SpriteRenderer>().sprite
            );

        static readonly Regex shroomWorldRegex = new("^NOTE_([A-Z]+)[0-9]+$");
    }
}
