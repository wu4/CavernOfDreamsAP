using UnityEngine;
using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.MiscPatches
{
    class FoyerAtelierPatches : InstantiateOnGameSceneLoad
    {
        [LoadOrder(-1)]
        public FoyerAtelierPatches()
        {
            MoveAtelierFoyerDestination();
            MoveFoyerAtelierDestination();
            // UpdateAtelierLobbyWarp();
            RemoveWarningTrigger();
            RemoveEndgameTrigger();
            MakeWindowMoreBreakable();
            MakeWindowAlwaysEnterable();
        }

        ///<summary>
        ///Normally, the Atelier window is made out of Dwarf Fortress candy,
        ///and can only be broken by cutscene triggers
        ///</summary>
        static void MakeWindowMoreBreakable()
        {
            Transform window = GameScene.FindInScene("GALLERY", "Atelier/atelier/BreakWindow");
            Breakable breakable = window.GetComponent<Breakable>();
            breakable.attackWorks = true;
            breakable.throwWorks = true;
        }

        static void MakeWindowAlwaysEnterable()
        {
            Transform windowWarp = GameScene.FindInScene("GALLERY", "Atelier/Warps/WarpFromAtelierWindowToGalleryLobby");
            Component.DestroyImmediate(windowWarp.GetComponent<TwoStateExists>());
        }

        ///<summary>
        ///When entering the Foyer from Atelier, Fynn is normally placed past
        ///the gratitude door. Let's place them behind the door instead.
        ///</summary>
        static void MoveAtelierFoyerDestination()
        {
            Transform dest = GameScene.FindInScene("GALLERY", "Foyer (Main)/Warps/DestFromAtelierToFoyer");
            dest.localPosition = new(73f, 18.5f, 0f);
        }

        ///<summary>
        ///When entering Atelier from the Foyer, Fynn starts in the center of
        ///the room, facing where Luna would be. Let's place them at the
        ///doorway instead.
        ///</summary>
        static void MoveFoyerAtelierDestination()
        {
            Transform dest = GameScene.FindInScene("GALLERY", "Atelier/Warps/DestFromFoyerToAtelier");
            dest.localPosition = new(-3f, 0f, -3f);
            dest.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        ///<summary>
        ///The warp is normally in front of the breakable window
        ///It also hides itself behind a flag. Let's give it the freedom it
        ///deserves!
        ///</summary>
        static void UpdateAtelierLobbyWarp()
        {
            Transform dest = GameScene.FindInScene("GALLERY", "Atelier/Warps/WarpFromAtelierWindowToGalleryLobby");
            dest.localPosition = dest.localPosition with {
                x = 11f
            };

            dest.gameObject.SetActive(true);
            Component.Destroy(dest.GetComponent<TwoState>());
        }

        ///<summary>
        ///Fynn simply does not care about the ramifications of entering
        ///Atelier anymore. They're in a randomizer.
        ///</summary>
        static void RemoveWarningTrigger()
        {
            GameObject.Destroy(GameScene.FindInScene("GALLERY", "Foyer (Main)/Cutscenes/WarningTrigger").gameObject);
        }

        ///<summary>
        ///Luna and Fynn aren't at odds anymore, so they shouldn't feel
        ///compelled to argue.
        ///</summary>
        static void RemoveEndgameTrigger()
        {
            GameObject.Destroy(GameScene.FindInScene("GALLERY", "Atelier/Cutscenes/AtelierCutsceneTrigger").gameObject);
        }
    }
}
