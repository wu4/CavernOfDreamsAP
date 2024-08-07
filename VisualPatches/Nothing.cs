using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class Nothing : StaticObjectPatcher
    {
        public static readonly new Replaces replaces = new(APCollectibleType.Nothing);

        public override void CollectJingle()
        {
        }

        public Nothing()
        {
            staticReplacement = new("Nothing");
            var collectible = staticReplacement.AddComponent<Collectible>();
            collectible.type = (Collectible.CollectibleType)APCollectibleType.Nothing;
            var twostate = staticReplacement.AddComponent<TwoStateExists>();
        }
    }
}
