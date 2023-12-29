using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class APMinor : StaticObjectPatcher
    {
        public static new Replaces replaces = new(APCollectibleType.Minor);

        public override void CollectJingle()
        {
            APResources.Instance.apJingleSmall.Play();
        }

        public APMinor()
        {
            staticReplacement = GameObject.Instantiate(GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Collectibles/Notes/NotePathLake/NoteCave").gameObject, Container);
            staticReplacement.name = "AP Minor";
            staticReplacement.GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(APResources.apMinorTexture as Texture2D, new Rect{x = 0, y = 0, width = 64, height = 64}, new Vector2{x = 0.5f, y = 0.5f});
        }
    }
}