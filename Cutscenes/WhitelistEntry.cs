using System;
using System.Collections.Generic;
using System.Linq;
using CoDArchipelago.GlobalGameScene;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago.Cutscenes
{
    [Flags]
    public enum WLOptions : short
    {
        None = 0,
        Interrupt = 1,
        MakeFast = 2
    }

    public class WhitelistEntry
    {
        readonly WLOptions options;
        readonly string[] whitelist;

        public WhitelistEntry(WLOptions options, params string[] whitelist)
        {
            this.options = options;
            this.whitelist = whitelist;
        }

        public WhitelistEntry(WLOptions options)
        {
            this.options = options;
            this.whitelist = new string[]{};
        }

        static void MakeEventFast(Event ev)
        {
            ev.start = 0;

            if (ev is CutsceneActivationEvent activationEvent) {
                Activation activation = activationEvent.activation;

                if (activation is Raise raise) {
                    raise.raiseTime = 1;
                    raise.GetTimer().SetMax(1);
                } else if (activation is GrowFromNothingActivation grow) {
                    grow.duration = 1;
                    grow.GetTimer().SetMax(1);
                }
            }
        }

        class ResetCulledCutscenes : InstantiateOnGameSceneLoad
        {
            [LoadOrder(int.MinValue)]
            public ResetCulledCutscenes() => patchedCutscenes.Clear();
        }

        class CheckCulledCutscenes : InstantiateOnGameSceneLoad
        {
            [LoadOrder(int.MaxValue)]
            public CheckCulledCutscenes()
            {
                List<string> unculledCutscenePaths = new();
                foreach (string flagName in Data.eventItems.Keys) {
                    IEnumerable<Cutscene> thisCutscenes = GameScene.GetComponentsInChildren<Cutscene>(true)
                        .Where(cutscene => !cutscene.interrupt && cutscene.flag == flagName);
                    foreach (Cutscene cutscene in thisCutscenes) {
                        string path = cutscene.transform.GetPath();

                        if (!patchedCutscenes.Contains(cutscene))
                            unculledCutscenePaths.Add(path);
                    }
                }

                if (unculledCutscenePaths.Count > 0) {
                    Debug.LogError("Some cutscenes are associated with locations, but are not culled:\n" + string.Join("\n", unculledCutscenePaths));
                }
            }
        }

        static readonly HashSet<Cutscene> patchedCutscenes = new();

        public void PatchCutscene(Cutscene cutscene)
        {
            cutscene.interrupt = options.HasFlag(WLOptions.Interrupt);

            if (options.HasFlag(WLOptions.MakeFast)) {
                cutscene.durationAfterFinal = 1;

                cutscene.GetComponentsInChildren<Event>(true).Do(MakeEventFast);
            }

            void deleteChild(int index) => GameObject.DestroyImmediate(cutscene.transform.GetChild(index).gameObject);

            int whitelistCount = whitelist.Length;
            for (int cursor = 0; cutscene.transform.childCount > whitelistCount; cursor++) {
                if (cursor == whitelistCount || cutscene.transform.GetChild(cursor).name != whitelist[cursor]) {
                    deleteChild(cursor);
                    cursor--;
                }
            }

            patchedCutscenes.Add(cutscene);
        }
    }
}
