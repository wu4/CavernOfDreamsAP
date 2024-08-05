using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class Nothing : StaticObjectPatcher
    {
        public static readonly new Replaces replaces = new(APCollectibleType.Nothing);

        public override void CollectJingle()
        {
            StockSFX.Instance.jingleCollectLarge.Play();
            GlobalHub.Instance.player.findSFX.Play();
        }

        public Nothing()
        {
            staticReplacement = new("Nothing");
            var collectible = staticReplacement.AddComponent<Collectible>();
            collectible.type = (Collectible.CollectibleType)APCollectibleType.Nothing;
        }
    }
}
