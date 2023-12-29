using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoDArchipelago.GlobalGameScene
{
    class GameScene : InstantiateOnGameSceneLoad
    {
        [LoadOrder(int.MinValue)]
        public GameScene()
        {
            gameScene = SceneManager.GetActiveScene();
        }

        static Scene gameScene;

        public static GameObject GetRootObjectByName(string name) =>
            gameScene.GetRootGameObjects().First(x => x.name == name);

        public static Transform FindInScene(string rootName, string path) =>
            GetRootObjectByName(rootName).transform.Find(path);

        public static IEnumerable<T> GetComponentsInChildren<T>(bool includeInactive = false) =>
            gameScene
                .GetRootGameObjects()
                .Where(x => x.name != "AP Container")
                .SelectMany(x => x.GetComponentsInChildren<T>(includeInactive));
        
        public static Area GetContainingArea(Transform transform) {
            if (transform.parent == null)
                return null;
            Area area = transform.parent.GetComponent<Area>();
            if (area != null)
                return area;

            return GetContainingArea(transform.parent);
        }
    }
}