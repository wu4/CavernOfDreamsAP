using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoDArchipelago
{
    static class GlobalGameScene
    {
        public static Scene gameScene;

        public static void Init()
        {
            gameScene = SceneManager.GetActiveScene();
        }

        static readonly AccessTools.FieldRef<GlobalHub, Area> areaCurrentRef = AccessTools.FieldRefAccess<GlobalHub, Area>("areaCurrent");

        public static ref Area GetCurrentArea()
        {
            return ref areaCurrentRef(GlobalHub.Instance);
        }

        public static GameObject GetRootObjectByName(string name)
        {
            return gameScene.GetRootGameObjects().First(x => x.name == name);
        }

        public static Transform FindInScene(string rootName, string path)
        {
            return GetRootObjectByName(rootName).transform.Find(path);
        }

        public static IEnumerable<T> GetComponentsInChildren<T>(bool includeInactive = false)
        {
            return gameScene.GetRootGameObjects().Where(x => x.name != "AP Container").SelectMany(x => x.GetComponentsInChildren<T>(includeInactive));
        }
    }
}