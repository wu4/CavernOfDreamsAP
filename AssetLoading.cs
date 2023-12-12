using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CoDArchipelago
{
    static class AssetLoading
    {
        public static AudioClip LoadWavClip(string path)
        {
            AudioClip clip = null;
            Debug.Log("starting " + path);
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV))
            {
                uwr.SendWebRequest();

                // wrap tasks in try/catch, otherwise it'll fail silently
                try
                {
                    while (!uwr.isDone);

                    if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }

            Debug.Log("finished!");
            return clip;
        }

        public static Texture LoadTexture(string path)
        {
            Texture2D tex = new(2, 2);
            Debug.Log("starting " + path);

            var rawData = System.IO.File.ReadAllBytes(path);

            tex.LoadImage(rawData);

            return tex;
        }
    }
}