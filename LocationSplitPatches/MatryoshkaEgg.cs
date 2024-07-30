using CoDArchipelago.GlobalGameScene;

namespace CoDArchipelago.LocationPatches
{
    class MatryoshkaEgg : InstantiateOnGameSceneLoad
    {
        public MatryoshkaEgg()
        {
            GameScene.FindInScene("GALLERY", "Foyer (Main)/Objects/Matroyshka").GetComponent<TwoStateExists>().flag = "LOCATION_FELLA_GALLERY1";
        }
    }
}
