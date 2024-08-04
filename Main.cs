using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CoDArchipelago.CodeGenerationHelpers;

namespace CoDArchipelago
{
    class InstantiateOnGameSceneLoad {}
    class InstantiateOnMenuLoad {}

    [AttributeUsage(AttributeTargets.Constructor)]
    class LoadOrder : Attribute
    {
        public readonly int loadOrder;
        public LoadOrder(int loadOrder) => this.loadOrder = loadOrder;
    }

    static class GameSceneLoadMethods
    {
        static readonly List<List<(string ClassName, ConstructorInfo Constructor)>> groupedConstructors =
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(InstantiateOnGameSceneLoad)))
                .Select(type => (type.ToString(), type.GetConstructor(new Type[] {})))
                .GroupBy(item => item.Item2.GetCustomAttribute<LoadOrder>()?.loadOrder ?? 0)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList()
                )
                .OrderBy(item => item.Key)
                .Select(item => item.Value)
                .ToList();

        public static void Invoke()
        {
            foreach (var constructorGroup in groupedConstructors) {
                foreach (var (ClassName, Constructor) in constructorGroup) {
                    try {
                        Debug.Log("Firing constructor for " + ClassName);
                        Constructor.Invoke(new object[] {});
                    } catch (Exception e) {
                        Debug.LogError("Error during constructor for " + ClassName + ":");
                        throw e;
                    }
                }
            }
        }
    }

    static class MenuLoadMethods
    {
        static readonly List<List<(string ClassName, ConstructorInfo Constructor)>> groupedMenuConstructors =
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.IsSubclassOf(typeof(InstantiateOnMenuLoad)))
                .Select(type => (type.ToString(), type.GetConstructor(new Type[] {})))
                .GroupBy(item => item.Item2.GetCustomAttribute<LoadOrder>()?.loadOrder ?? 0)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList()
                )
                .OrderBy(item => item.Key)
                .Select(item => item.Value)
                .ToList();

        public static void Invoke()
        {
            foreach (var constructorGroup in groupedMenuConstructors) {
                foreach (var (ClassName, Constructor) in constructorGroup) {
                    try {
                        Debug.Log("Firing constructor for " + ClassName);
                        Constructor.Invoke(new object[] {});
                    } catch (Exception e) {
                        Debug.LogError("Error during constructor for " + ClassName + ":");
                        throw e;
                    }
                }
            }
        }
    }


    static class InitPatches
    {
        public static void AddMenuInit()
        {
            SceneManager.sceneLoaded += (scene, mode) => {
                if (scene.name == "MainMenu") {
                    MenuLoadMethods.Invoke();
                } else {
                    Debug.LogError("Not firing");
                }
            };
        }

        [HarmonyPatch(typeof(GlobalHub), "Awake")]
        static class InitPatch
        {
            static void Init()
            {
                SaveHandler.LoadFile(GlobalHub.Instance.save, GlobalHub.numSaveFile);
                GlobalHub.Instance.save.Initialize();

                GameSceneLoadMethods.Invoke();

                MiscPatches.ChangeStartLocation.SetStartLocation("Cavern of Dreams - Sage");
            }

            static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions,
                ILGenerator generator
            ) {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchForward(
                    false,
                    Calls<UnityEngine.Object, Area>("FindObjectOfType"),
                    new(OpCodes.Stloc_0)
                );
                matcher.ThrowIfInvalid("Start of removal range");

                int startPos = matcher.Pos;

                var matcherClone = matcher.Clone();
                int endPos = matcherClone.MatchForward(
                    false,
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldc_R4, 60f),
                    new(OpCodes.Newobj),
                    new(OpCodes.Stfld)
                ).Pos;
                matcherClone.ThrowIfInvalid("End of removal range");

                matcher.RemoveInstructions(
                    endPos - startPos
                );

                matcher.Advance(1);
                matcher.Insert(
                    CodeInstruction.Call(typeof(InitPatch), nameof(InitPatch.Init))
                );

                return matcher.InstructionEnumeration();
            }
        }
    }
}
