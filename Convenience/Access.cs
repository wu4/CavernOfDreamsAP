using System.Reflection;
using HarmonyLib;

namespace CoDArchipelago
{
    static public class Access {
        public class Method<O, T> {
            readonly MethodInfo method;
            public Method(string name)
            {
                method = AccessTools.Method(typeof(O), name);
            }

            public T Invoke(O obj, params dynamic[] args) => (T)method.Invoke(obj, args);
        }

        public class Action<O> {
            readonly MethodInfo method;
            public Action(string name)
            {
                method = AccessTools.Method(typeof(O), name);
            }

            public void Invoke(O obj, params dynamic[] args) {
                method.Invoke(obj, args);
            }
        }

        public class Field<O, T> {
            readonly FieldInfo field;
            public Field(string name)
            {
                field = AccessTools.Field(typeof(O), name);
            }

            public T Get(O obj) => (T)field.GetValue(obj);

            public void Set(O obj, T value) => field.SetValue(obj, value);
        }
    }
}
