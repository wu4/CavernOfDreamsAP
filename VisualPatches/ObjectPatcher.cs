using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoDArchipelago.GlobalGameScene;
using UnityEngine;

namespace CoDArchipelago.VisualPatches
{
    class Replaces
    {
        public readonly Enum type;
        public readonly Dictionary<string, List<string>> gameItemsToReplace;

        public Replaces(Enum type) =>
            this.type = type;
        
        public Replaces(Dictionary<string, List<string>> gameItemsToReplace) =>
            this.gameItemsToReplace = gameItemsToReplace;
    }
    
    abstract class StaticObjectPatcher : ObjectPatcher
    {
        protected GameObject staticReplacement;

        public sealed override GameObject Replace(GameObject toReplace, Collecting.Item item) =>
            ReplaceWith(toReplace, staticReplacement);
    }

    abstract class ObjectPatcher
    {
        public static Replaces replaces;

        static Transform _container;
        protected static Transform Container {get => _container;}

        static Cutscene _fellaCutscene;
        protected static Cutscene FellaCutscene {get => _fellaCutscene;}
        class OnInit : InstantiateOnGameSceneLoad
        {
            [LoadOrder(int.MinValue)]
            public OnInit()
            {
                GameObject container = new("Replacement Objects");
                container.SetActive(false);
                _container = container.transform;

                Transform fellaCutsceneT = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles/Fella 2 (Waterfall)/GetFellaCutscene");
                _fellaCutscene = fellaCutsceneT.GetComponent<Cutscene>();
                Cutscenes.Patching.PatchCutscene(_fellaCutscene, Cutscenes.WLOptions.Interrupt, "PlayerGetItemPose");
                fellaCutsceneT.GetComponentInChildren<CutscenePlayerAnimEvent>().start = 1;
                fellaCutsceneT.SetParent(_container, false);
            }
        }

        protected static string GetWorldFromFlag(string flagName, Regex regex)
        {
            var m = regex.Match(flagName);
            if (m.Success) {
                return m.Groups[1].Value;
            } else {
                return null;
            }
        }
        
        protected GameObject ReplaceWith(GameObject toReplace, GameObject replaceWith)
        {
            GameObject newObject = UnityEngine.Object.Instantiate(replaceWith, new Transform());
            newObject.GetComponent<TwoState>().flag = toReplace.GetComponent<TwoState>().flag;
            newObject.name = toReplace.name;
            newObject.transform.parent   = toReplace.transform.parent;
            newObject.transform.position = toReplace.transform.position;
            newObject.transform.rotation = toReplace.transform.rotation;

            GameObject.Destroy(toReplace);

            return newObject;
        }
        
        public abstract void CollectJingle();

        public abstract GameObject Replace(GameObject toReplace, Collecting.Item item);
    }
}