using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    class CodeInstructionParser
    {
        public class CodeByIndex
        {
            public readonly CodeInstruction Item;
            public readonly int Index;
            public CodeByIndex(CodeInstruction item, int index)
            {
                Item = item;
                Index = index;
            }
        }

        public CodeInstruction this[int i]
        {
            get => codes[i];
        }

        public List<CodeInstruction> codes;
        public IEnumerable<CodeByIndex> byIndex;
        public IEnumerable<CodeByIndex> instByIndex;
        public ILGenerator generator;

        public CodeInstructionParser(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            codes = instructions.ToList();
            generator = ilGenerator;

            byIndex =
                codes.Select((item, index) => new CodeByIndex(item, index));

            instByIndex = 
                byIndex
                .Where(o => o.Item.opcode == OpCodes.Ldarg_0);
        }

        public CodeByIndex FirstInstanceCode(Func<CodeByIndex, bool> check) => instByIndex.First(check);
        public int FirstInstanceCodeIndex(Func<CodeByIndex, bool> check) => instByIndex.First(check).Index;
        public CodeByIndex FirstCode(Func<CodeByIndex, bool> check) => byIndex.First(check);
        public int FirstCodeIndex(Func<CodeByIndex, bool> check) => byIndex.First(check).Index;

        public CodeInstruction AssertGetCode(int index, Func<CodeInstruction, bool> check, string exception)
        {
            var code = codes[index];
            if (!check(code)) {
                Debug.LogError("Assert failed at code " + code);
                throw new Exception("Assert failed (" + code + "): " + exception);
            }
            return code;
        }

        public void AssertAddLabel(int index, Label label, Func<CodeInstruction, bool> check, string exception)
        {
            var code = AssertGetCode(index, check, exception);
            codes[index] = code.WithLabels(label);
        }

        public void AddLabel(int index, Label label)
        {
            codes[index] = codes[index].WithLabels(label);
        }
    }
}