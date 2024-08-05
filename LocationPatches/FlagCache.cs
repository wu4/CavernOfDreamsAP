using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CoDArchipelago.FlagCache
{
    class LoadsFromFlag : Attribute
    {
        public string flagParentName;
        public string flagName;

        public LoadsFromFlag(string flagParentName, string flagName)
        {
            this.flagParentName = flagParentName;
            this.flagName = flagName;
        }
    }

    public static class CachedSkillFlags
    {
        [LoadsFromFlag("SKILL", "GROUNDATTACK")]
        public static bool groundAttack = false;

        [LoadsFromFlag("SKILL", "AIRATTACK")]
        public static bool airAttack = false;

        [LoadsFromFlag("SKILL", "HOVER")]
        public static bool hover = false;

        [LoadsFromFlag("SKILL", "DIVE")]
        public static bool dive = false;

        [LoadsFromFlag("SKILL", "PROJECTILE")]
        public static bool projectile = false;

        [LoadsFromFlag("SKILL", "FLIGHT")]
        public static bool flight = false;

        [LoadsFromFlag("SKILL", "ROLL")]
        public static bool roll = false;
        [LoadsFromFlag("SKILL", "DOUBLEJUMP")]
        public static bool doubleJump = false;
        [LoadsFromFlag("SKILL", "HIGHJUMP")]
        public static bool highJump = false;
        [LoadsFromFlag("SKILL", "SPRINT")]
        public static bool sprint = false;
        [LoadsFromFlag("SKILL", "SUPERBOUNCE")]
        public static bool superBounce = false;
        [LoadsFromFlag("SKILL", "SUPERBUBBLEJUMP")]
        public static bool superBubbleJump = false;
        [LoadsFromFlag("SKILL", "CLIMB")]
        public static bool climb = false;
        [LoadsFromFlag("SKILL", "CARRY")]
        public static bool carry = false;
        [LoadsFromFlag("SKILL", "SWIM")]
        public static bool swim = false;
        [LoadsFromFlag("SKILL", "AIRSWIM")]
        public static bool airSwim = false;
    }

    public static class CachedOtherFlags
    {
        // [LoadsFromFlag("PALACE", "MELTED_ICE")]
        // public static bool palaceMeltedIce = false;
        [LoadsFromFlag("LOCATION", "PALACE_MELTED_ICE")]
        public static bool locationPalaceMeltedIce = false;
    }

    public static class CachedAPFlags
    {
        public static bool splitGratitudeAndTeleports = false;
    }

    public static class CachedTeleportFlags
    {

        [LoadsFromFlag("TELEPORT", "LAKE")]
        public static bool lake = false;
        [LoadsFromFlag("TELEPORT", "MONSTER")]
        public static bool monster = false;
        [LoadsFromFlag("TELEPORT", "PALACE")]
        public static bool palace = false;
        [LoadsFromFlag("TELEPORT", "GALLERY")]
        public static bool gallery = false;
    }

    class SetCachedFlags : InstantiateOnGameSceneLoad
    {
        static readonly Dictionary<string, Dictionary<string, FieldInfo>> cachedFlagFields =
            // typeof(CachedSkillFlags)
            //     .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            Enumerable.Empty<Type>()
            .Append(typeof(CachedSkillFlags))
            .Append(typeof(CachedOtherFlags))
            .Append(typeof(CachedAPFlags))
            .Append(typeof(CachedTeleportFlags))
            // Assembly.GetExecutingAssembly()
            //     .GetTypes()

                .SelectMany(type => type.GetFields())
                .Where(field => field.IsDefined(typeof(LoadsFromFlag), false))
                .GroupBy(field => field.GetCustomAttribute<LoadsFromFlag>().flagParentName)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(field =>
                        field.GetCustomAttribute<LoadsFromFlag>().flagName,
                        field => field
                    )
                );

        static bool GetFlagOn(string flag_name) =>
            GlobalHub.Instance.save.GetFlag(flag_name).on;

        [LoadOrder(int.MinValue)]
        public SetCachedFlags()
        {
            foreach ((string category, var parent) in cachedFlagFields) {
                foreach ((string flagName, FieldInfo field) in parent) {
                    field.SetValue(null, GetFlagOn(category + "_" + flagName));
                }
            }
        }

        [HarmonyPatch(typeof(Save), nameof(Save.SetFlag))]
        static class SetCachedFlag
        {
            static void Postfix(string name, bool b)
            {
                foreach ((string category, var child) in cachedFlagFields) {
                    if (!name.StartsWith(category)) continue;

                    if (child.TryGetValue(name.Substring(category.Length + 1), out FieldInfo field)) {
                        field.SetValue(null, b);
                    }

                    return;
                }
            }
        }
    }
}
