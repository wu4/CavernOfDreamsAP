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
        public ILGenerator generator;

        public CodeInstructionParser(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            codes = instructions.ToList();
            generator = ilGenerator;
        }
        
        public CodeInstruction FirstCode(int start, params Func<CodeInstruction, bool>[] checks)
        {
            int index = FirstCodeIndex(start, checks);

            return codes[index];
        }
        public CodeInstruction FirstCode(params Func<CodeInstruction, bool>[] checks) =>
            FirstCode(0, checks);
        
        public int FirstCodeIndex(params Func<CodeInstruction, bool>[] checks) =>
            FirstCodeIndex(0, checks);

        public int FirstCodeIndex(int start, params Func<CodeInstruction, bool>[] checks) =>
            Enumerable.Range(start, codes.Count).First(
                instructionIndex =>
                    Enumerable.Range(0, checks.Length).All(
                        i => checks[i](codes[instructionIndex + i])
                    )
            );

        public void AddLabel(int index, Label label)
        {
            codes[index] = codes[index].WithLabels(label);
        }
    }
}