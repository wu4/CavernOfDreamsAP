using CoDArchipelago.GlobalGameScene;
using UnityEngine;

namespace CoDArchipelago.LocationPatches
{
    class MoonCavernDivespots : InstantiateOnGameSceneLoad
    {
        ///<summary>
        ///The divespots in Moon Cavern are very small. Sometimes, small enough
        ///to where some items poke out of the top. Let's hide the item
        ///clusters beneath them until the spots are opened.
        ///</summary>
        public MoonCavernDivespots()
        {
            var parent = GameScene.FindInScene("CAVE", "Moon Cavern");
            var depths3 = parent.Find("depths3");
            var notes = parent.Find("Collectibles/Notes");

            LinkDivespotToCluster(
                depths3.Find("Divespot").gameObject,
                notes.Find("DiveSpotNoteCluster").gameObject
            );

            LinkDivespotToCluster(
                depths3.Find("Divespot_2").gameObject,
                notes.Find("DiveSpotNoteCluster (1)").gameObject
            );
        }

        class LinkedDivespot : Whackable
        {
            public TwoStateExists divespotExists;
            public TwoStateExists clusterExists;
            public AudioSolo breakSFX;
            public string flag;

            protected override bool HandleWhack(Whackable.WhackType type, GameObject source)
            {
                GlobalHub.Instance.save.SetFlag(flag, true);
                // breakSFX.Play();
                divespotExists.SetState();
                clusterExists.SetState();
                return true;
            }
        }

        static void LinkDivespotToCluster(GameObject divespot, GameObject cluster)
        {
            var breakable = divespot.GetComponent<Breakable>();
            var divespotExists = divespot.AddComponent<TwoStateExists>();
            string flagName = divespot.transform.GetPath();
            divespotExists.flag = flagName;
            var clusterExists = cluster.AddComponent<TwoStateExists>();
            clusterExists.flag = flagName;
            clusterExists.flagOnExists = true;

            var linkedDivespot = divespot.AddComponent<LinkedDivespot>();
            linkedDivespot.flag = flagName;
            linkedDivespot.divespotExists = divespotExists;
            linkedDivespot.clusterExists = clusterExists;

            linkedDivespot.breakSFX = breakable.breakSFX;
            linkedDivespot.attackWorks = breakable.attackWorks;
            linkedDivespot.cutsceneWorks = breakable.cutsceneWorks;
            linkedDivespot.diveWorks = breakable.diveWorks;
            linkedDivespot.projectileWorks = breakable.projectileWorks;
            linkedDivespot.rollWorks = breakable.rollWorks;
            linkedDivespot.specialWorks = breakable.specialWorks;
            linkedDivespot.throwWorks = breakable.throwWorks;

            Component.Destroy(breakable);
        }
    }
}
