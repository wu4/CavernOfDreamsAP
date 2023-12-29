using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    public static class CodeMatchHelpers
    {
        public static CodeMatch LoadsField<C>(string name) =>
            new(o => o.LoadsField(AccessTools.Field(typeof(C), name)));
        
        public static CodeMatch Calls<C>(string name) =>
            new(o => o.Calls(AccessTools.Method(typeof(C), name)));

        public static CodeMatch Calls<C, T>(string name) =>
            new(o => o.Calls(AccessTools.Method(typeof(C), name).MakeGenericMethod(typeof(T))));
    }
}