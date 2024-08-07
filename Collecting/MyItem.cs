using System;
using System.Collections.Generic;

namespace CoDArchipelago.Collecting
{
    class MyItem : Item
    {
        bool silent;

        static readonly Dictionary<string, Action<bool>> itemTriggers = new();
        public static void RegisterTrigger(string itemFlag, Action<bool> action) =>
            itemTriggers.Add(itemFlag, action);

        class ResetTriggersOnLoad : InstantiateOnGameSceneLoad
        {
            [LoadOrder(int.MinValue)]
            public ResetTriggersOnLoad() => itemTriggers.Clear();
        }

        static Enum GetFlagCollectibleType(string flag)
        {
            if (flag == "Shroom") return Collectible.CollectibleType.NOTE;
            if (Data.gratitudeItems.ContainsKey(flag))         return Collectible.CollectibleType.GRATITUDE;
            if (Data.pickupItems.ContainsKey(flag))            return Collectible.CollectibleType.ITEM;
            if (Data.abilityItems.ContainsKey(flag) ||
                Data.nonVanillaAbilityItems.ContainsKey(flag)) return APCollectibleType.Ability;
            if (Data.cardItems.ContainsKey(flag))              return Collectible.CollectibleType.CARD;
            if (Data.eggItems.ContainsKey(flag))               return Collectible.CollectibleType.FELLA;
            if (Data.eventItems.ContainsKey(flag) ||
                Data.teleportItems.ContainsKey(flag))          return APCollectibleType.Event;
            if (Data.carryableItems.ContainsKey(flag) ||
                Data.carryableItems.ContainsValue(flag))       return APCollectibleType.Carryable;

            throw new System.Exception("Unknown type for collectible " + flag);
        }

        readonly string flag;
        public override string GetFlag() => flag;

        public readonly bool randomized;

        public MyItem(string flag, bool randomized = true, bool silent = false, long? locationId = null) : base(locationId)
        {
            this.flag = flag;
            this.randomized = randomized;
            this.silent = silent;
            type = GetFlagCollectibleType(flag);
        }

        public override void Collect()
        {
            if (itemTriggers.TryGetValue(flag, out Action<bool> itemTrigger)) {
                itemTrigger(randomized);
            }

            bool collectedAsCutscene = Cutscenes.Collecting.TryCollect(flag, silent);

            if (!collectedAsCutscene) {
                GlobalHub.Instance.save.SetFlag(flag, true);
            }

            if (type is Collectible.CollectibleType vanillaType && vanillaType != Collectible.CollectibleType.ITEM) {
                GlobalHub.Instance.save.AddCollectible(vanillaType, 1);
                if (!silent) {
                    UIController.Instance.SetModelVisible(vanillaType);
                    UIController.Instance.collectibleCounter.text = GlobalHub.Instance.save.GetCollectible(vanillaType).ToString();
                }
            }

            base.Collect();
        }
    }
}
