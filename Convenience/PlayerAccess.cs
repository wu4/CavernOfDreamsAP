using UnityEngine;

namespace CoDArchipelago
{
    /// <summary>
    /// A list of Player methods and fields that are either private or used in reflection
    /// </summary>
    static class PlayerAccess
    {
        public static void ReleaseCarryable(this Player player, Vector3 direction) => Methods.ReleaseCarryable.Invoke(player, direction);
        public static bool CanJump(this Player player) => Methods.CanJump.Invoke(player);
        public static bool IsJumpHeld(this Player player) => Methods.IsJumpHeld.Invoke(player);
        public static bool IsRollHeld(this Player player) => Methods.IsRollHeld.Invoke(player);
        public static bool IsSitting(this Player player) => Methods.IsSitting.Invoke(player);

        static class Methods
        {
            public static readonly Access.Method<Player, object> ReleaseCarryable = new("ReleaseCarryable");
            public static readonly Access.Method<Player, bool> CanJump = new("CanJump");
            public static readonly Access.Method<Player, bool> IsJumpHeld = new("IsJumpHeld");
            public static readonly Access.Method<Player, bool> IsRollHeld = new("IsRollHeld");
            public static readonly Access.Method<Player, bool> IsSitting = new("IsSitting");
        }
        
        public static class Fields
        {
            public static readonly Access.Field<Player, bool> bouncing = new("bouncing");
            public static readonly Access.Field<Player, bool> flying = new("flying");
            public static readonly Access.Field<Player, bool> isMoveInput = new("isMoveInput");
            public static readonly Access.Field<Player, Timer> jumpTimer = new("jumpTimer");
            public static readonly Access.Field<Player, float> momentum = new("momentum");
            public static readonly Access.Field<Player, bool> carryingParaglider = new("carryingParaglider");
        }
    }
}