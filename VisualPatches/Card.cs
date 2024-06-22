using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.VisualPatches
{
    class Card : StaticObjectPatcher
    {
        public static readonly new Replaces replaces = new(CollectibleItem.CollectibleType.CARD);

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleCollectSmall.Play();
        }

        public Card()
        {
            staticReplacement = GameObject.Instantiate(GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles/CardPack MUSHROOM").gameObject, Container);
            staticReplacement.name = "Card";
        }
    }
}
