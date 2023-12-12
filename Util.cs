using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace CoDArchipelago
{
    static class AreaExtensions
    {
        public static bool ContainsComponent<C>(this Area area, C component) where C: Component
        {
            return area.transform.GetComponentsInChildren<C>().Contains(component);
        }
    }

}