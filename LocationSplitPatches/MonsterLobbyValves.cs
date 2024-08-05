using CoDArchipelago.GlobalGameScene;
using UnityEngine;

namespace CoDArchipelago.LocationSplitPatches
{
    class MonsterLobbyValves : InstantiateOnGameSceneLoad
    {
        public MonsterLobbyValves()
        {
            var parent = GameScene.FindInScene("CAVE", "Monster Lobby/monster_lobby");
            PatchValve(parent.Find("Valve"));
            PatchValve(parent.Find("Valve_001"));
            PatchCutscenes();
        }

        static void PatchValve(Transform valve)
        {
            var tsc = valve.GetComponent<TwoStateComponent>();
            string valveFlag = $"LOCATION_{tsc.flag}";
            tsc.flag = valveFlag;
            var valveRotate = valve.GetComponent<Rotate>();

            Collecting.Location.RegisterTrigger(valveFlag, () => {
                valveRotate.enabled = true;
            });
        }

        ///<summary>
        ///Removes the valves spinning from the cutscenes
        ///</summary>
        static void PatchCutscenes()
        {
            Cutscenes.Patching.PatchCutscene(
                "CAVE", "Monster Lobby/Cutscenes/PipesRisen",
                Cutscenes.WLOptions.None,
                "Raise Pipe",
                "Raise Pipe (1)",
                "Raise Pipe (2)",
                "Raise Pipe (3)",
                "Raise Pipe (4)"
            );
            Cutscenes.Patching.PatchCutscene(
                "CAVE", "Monster Lobby/Cutscenes/Steam",
                Cutscenes.WLOptions.None,
                "AwakenSteam"
            );
        }
    }
}
