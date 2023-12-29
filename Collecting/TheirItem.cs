using System;

namespace CoDArchipelago.Collecting
{
    class TheirItem : Item
    {
        public readonly int playerId;
        public readonly string itemName;

        public override string GetFlag() {
            if (Data.allItemsByName.TryGetValue(itemName, out string ret)) return ret;

            throw new Exception("invalid use of GetFlag: flag for " + itemName + " does not exist");
        }

        public TheirItem(int playerId, string itemName, bool isMajor)
        {
            this.playerId = playerId;
            this.itemName = itemName;

            type = isMajor ? APCollectibleType.Major : APCollectibleType.Minor;
        }

        public override void Collect()
        {
            // CollectJingle(type);
            // SaveHandler.SaveFile(GlobalHub.Instance.save, GlobalHub.numSaveFile);
        }
    }
}