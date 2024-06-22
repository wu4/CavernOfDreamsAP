using HarmonyLib;

namespace CoDArchipelago
{
    ///<summary>
    ///Adds various helper functions to ease readability when matching through
    ///or generating IL.
    ///</summary>
    public static class CodeGenerationHelpers
    {
        public static CodeMatch LoadsField<C>(string name) =>
            new(o => o.LoadsField(AccessTools.Field(typeof(C), name)));

        public static CodeMatch Calls<C>(string name) =>
            new(o => o.Calls(AccessTools.Method(typeof(C), name)));

        public static CodeMatch Calls<C, T>(string name) =>
            new(o => o.Calls(AccessTools.Method(typeof(C), name).MakeGenericMethod(typeof(T))));


        public static CodeInstruction Call(System.Type methodType, string name) =>
            CodeInstruction.Call(methodType, name);

        public static CodeInstruction Call<C>(string name) =>
            CodeInstruction.Call(typeof(C), name);

        public static CodeInstruction LoadField(System.Type fieldType, string name) =>
            CodeInstruction.LoadField(fieldType, name);

        public static CodeInstruction LoadField<C>(string name) =>
            CodeInstruction.LoadField(typeof(C), name);

        public static CodeInstruction StoreField(System.Type fieldType, string name) =>
            CodeInstruction.StoreField(fieldType, name);

        public static CodeInstruction StoreField<C>(string name) =>
            CodeInstruction.StoreField(typeof(C), name);
    }
}
