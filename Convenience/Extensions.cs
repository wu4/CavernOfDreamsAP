using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace CoDArchipelago
{
    static class Extensions
    {
        static readonly Access.Field<Timer, float> maxAccess = new("max");
        public static void SetMax(this Timer timer, float value) => maxAccess.Set(timer, value);

        static readonly Access.Field<Raise, Timer> raiseTimerAccess = new("raiseTimer");
        public static Timer GetTimer(this Raise raise) => raiseTimerAccess.Get(raise);

        static readonly Access.Field<GrowFromNothingActivation, Timer> growTimerAccess = new("growTimer");
        public static Timer GetTimer(this GrowFromNothingActivation grow) => growTimerAccess.Get(grow);

        public static bool ContainsComponentInChildren<P, C>(this P component, C childComponent, bool includeInactive = false)
            where P: Component
            where C: Component =>
            component.GetComponentsInChildren<C>(includeInactive).Contains(childComponent);

        static readonly LocalizedStringDatabase db = LocalizationSettings.Instance.GetStringDatabase();

        public static void PatchText(this LocalizedString str, string text) =>
            db.GetTable(str.TableReference).AddEntryFromReference(str.TableEntryReference, text);

        public static string GetPath(this Transform current) {
            if (current.parent == null)
                return "/" + current.name;
            return current.parent.GetPath() + "/" + current.name;
        }

        public static bool LoadsField<T>(this CodeInstruction code, string name) =>
            code.LoadsField(AccessTools.Field(typeof(T), name));

        public static bool Calls<T>(this CodeInstruction code, string name) =>
            code.Calls(AccessTools.Method(typeof(T), name));

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key
        ) where TValue : new()
        {
            TValue oldValue;
            if (dictionary.TryGetValue(key, out oldValue))
                return oldValue;
            else
            {
                var newValue = new TValue();
                dictionary.Add(key, newValue);
                return newValue;
            }
        }
    }
}