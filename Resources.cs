using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago
{
    class APResources : InstantiateOnGameSceneLoad
    {
        public static APResources Instance;

        readonly GameObject apContainer;

        [LoadOrder(int.MinValue + 1)]
        public APResources()
        {
            Instance = this;

            apContainer = new("AP Container");
            apContainer.SetActive(false);

            GameObject soundsContainer = new("AP Sounds");

            Transform caveCutsceneObjects = GameScene.FindInScene("CAVE", "Sun Cavern (Main)/Cutscenes/Cutscene Objects");
            grabAbilityJingle = CreateAudioSourceWithClip(soundsContainer, caveCutsceneObjects.Find("LearnSkillSound").GetComponent<AudioSource>().clip);
            apJingleSmall = CreateAudioSourceWithClip(soundsContainer, apJingleSmallClip);
        }

        static readonly AssetBundle bundle =
            AssetBundle.LoadFromFile(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "Archipelago", "bundle"));
        public static readonly GameObject apMajorMesh
            = bundle.LoadAsset<GameObject>("major_ap_model.blend");
        public static readonly Texture apMinorTexture
            = bundle.LoadAsset<Texture>("archipelago_icon.png");
        static readonly AudioClip apJingleSmallClip
            = bundle.LoadAsset<AudioClip>("minor.wav");


        public readonly AudioSource apJingleSmall;
        
        public AudioSource grabAbilityJingle;

        static AudioSource CreateAudioSourceWithClip(GameObject obj, AudioClip clip)
        {
            AudioSource newSource = obj.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.enabled = true;
            return newSource;
        }
    }
}