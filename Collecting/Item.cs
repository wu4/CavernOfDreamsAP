using System;

namespace CoDArchipelago.Collecting
{
    enum APCollectibleType {
        Minor,
        Major,
        Event,
        Ability,
        Carryable
    }

    abstract class Item
    {
        public long? locationId;

        public Enum type;

        public abstract string GetFlag();

        public virtual void Collect()
        {
            if (locationId.HasValue) {
                APClient.Client.Instance.SendLocationCollected(locationId.Value);
            }
        }

        public Item(long? locationId = null)
        {
            this.locationId = locationId;
        }
    }
}
