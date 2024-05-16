using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static CoDArchipelago.CodeMatchHelpers;

namespace CoDArchipelago
{
    class InstantiateOnGameSceneLoad {}

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
            // Dictionary<string, double> timings = new();
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
                /*
                Parallel.ForEach(
                    constructorGroup,
                    c => {
                        var (ClassName, Constructor) = c;
                        try {
                            Debug.Log("Firing constructor for " + ClassName);
                            // var start = DateTime.Now;
                            Constructor.Invoke(new object[] {});
                            // timings.Add(ClassName, (DateTime.Now - start).TotalSeconds);
                        } catch (Exception e) {
                            Debug.LogError("Error during constructor for " + ClassName + ":");
                            throw e;
                        }
                    }
                );
                */
            }
            /*
            foreach ((var name, var val) in timings.OrderByDescending(i => i.Value)) {
                Debug.Log(name + ": " + val + "s");
            }
            */
        }
    }

    static class InitPatches
    {
        [HarmonyPatch(typeof(GlobalHub), "Awake")]
        static class InitPatch
        {
            static void Init()
            {
                SaveHandler.LoadFile(GlobalHub.Instance.save, GlobalHub.numSaveFile);
                GlobalHub.Instance.save.Initialize();

                GameSceneLoadMethods.Invoke();

                ChangeStartLocation.SetStartLocation("Cavern of Dreams - Sage");
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
