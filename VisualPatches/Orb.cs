using UnityEngine;
using CoDArchipelago.GlobalGameScene;
using CoDArchipelago.Collecting;

namespace CoDArchipelago.VisualPatches
{
    class Orb : StaticObjectPatcher
    {
        public static new Replaces replaces = new(APCollectibleType.Ability);

        public override void CollectJingle()
        {
            APResources.Instance.grabAbilityJingle.Play();
            GlobalHub.Instance.player.curiousSFX.Play();
        }

        public Orb()
        {
            Transform caveCutsceneObjects = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Cutscenes/Cutscene Objects");

            GameObject orbFX = GameObject.Instantiate(caveCutsceneObjects.Find("LearnSkillEffect").gameObject, Container, false);
            orbFX.transform.position = new Vector3();
            staticReplacement = GameObject.Instantiate(caveCutsceneObjects.Find("Magic Orb Attack").gameObject, Container, false);
            staticReplacement.SetActive(true);
            staticReplacement.transform.position = new Vector3();
            Transform orbObjectModel = staticReplacement.transform.Find("magic_orb");
            orbObjectModel.localPosition = new Vector3(0f, 0.5f, 0f);
            orbObjectModel.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            SphereCollider collider = staticReplacement.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.isTrigger = true;
            collider.tag = "NotPlayer";
            Component.DestroyImmediate(staticReplacement.GetComponent<MagicOrb>());
            TwoState orbTS = staticReplacement.AddComponent<TwoStateExists>();
            Collectible orbCollectible = staticReplacement.AddComponent<Collectible>();
            orbCollectible.type = Collectible.CollectibleType.NOTE;
            orbCollectible.amount = 1;
            orbCollectible.cutscene = null;
            orbCollectible.fx = orbFX.GetComponent<ParticleSystem>();
        }
    }
}