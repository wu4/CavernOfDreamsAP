using System;

namespace CoDArchipelago.Collecting
{
    class TheirItem : Item
    {
        public readonly int playerId;
        public readonly string itemName;
        public readonly string itemPrettyName;

        public override string GetFlag() {
            if (Data.allItemsByName.TryGetValue(itemName, out string ret)) return ret;

            throw new Exception("invalid use of GetFlag: flag for " + itemName + " does not exist");
        }

        public TheirItem(int playerId, string itemName, string itemPrettyName, bool isMajor, long? locationId = null) : base(locationId)
        {
            this.playerId = playerId;
            this.itemName = itemName;
            this.itemPrettyName = itemPrettyName;

            type = isMajor ? APCollectibleType.Major : APCollectibleType.Minor;
        }
    }
}
