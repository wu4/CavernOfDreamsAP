using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;
using UnityEngine.UI;

namespace CoDArchipelago.VisualPatches
{
    class Event : ObjectPatcher
    {
        public static readonly new Replaces replaces = new(APCollectibleType.Event);

        readonly Dictionary<string, Sprite> eventSprites;

        static readonly Regex eventWorldRegex = new("^([A-Z]+)_");

        readonly GameObject eventObj;

        static readonly Dictionary<string, string> eventWorldMap = new(){
            {"MONSTER_LOBBY_PIPES_RISEN", "CAVE"},
            {"MONSTER_LOBBY_STEAM", "CAVE"},
            {"PALACE_LOBBY_FAUCET_ON", "CAVE"},
            {"PALACE_LOBBY_WHIRLPOOL_ON", "CAVE"},

            {"TELEPORT_LAKE", "LAKE"},
            {"TELEPORT_MONSTER", "MONSTER"},
            {"TELEPORT_PALACE", "PALACE"},
            {"TELEPORT_GALLERY", "GALLERY"},
        };

        public override GameObject Replace(GameObject toReplace, Item item)
        {
            string flagName = item.GetFlag();

            if (!eventWorldMap.TryGetValue(flagName, out string worldName))
                worldName = GetWorldFromFlag(flagName, eventWorldRegex)
                    ?? throw new Exception("failed to identify world for event " + flagName);

            GameObject obj = ReplaceWith(toReplace, eventObj);
            obj.GetComponentInChildren<SpriteRenderer>().sprite = eventSprites[worldName];

            return obj;
        }

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleGood.Play();
        }

        public Event()
        {
            eventObj = GameObject.Instantiate(GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles/Notes/NotePathLake/NoteCave").gameObject, Container);
            eventObj.name = "Event";

            eventSprites = GetEventSprites();
        }

        static readonly Dictionary<string, string> menuOptions = new() {
            {"CAVE", "Quit Game"},
            {"PALACE", "Totals"},
            {"LAKE", "Options"},
            {"MONSTER", "Encyclopedia"},
            {"GALLERY", "Controls"}
        };

        Dictionary<string, Sprite> GetEventSprites()
        {
            var pauseMenuT = GameScene.FindInScene("Rendering", "Canvas/PauseMenu/PauseMenuPage1");

            return menuOptions.ToDictionary(
                o => o.Key,
                o => {
                    var tex = pauseMenuT.Find(o.Value + " Button/Icon").GetComponent<RawImage>().texture as Texture2D;
                    return Sprite.Create(tex, new Rect{x = 0, y = 0, width = tex.width, height = tex.height}, new Vector2{x = 0.5f, y = 0.5f});
                }
            );
        }
    }
}
