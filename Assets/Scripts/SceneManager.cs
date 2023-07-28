using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimsoftVR
{
    public delegate void OnCallSceneLoad();
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager instance;
        public static int NumberOfScenes { get { return UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; } }
        public static OnCallSceneLoad onCallSceneLoad;

        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else
                Destroy(gameObject);
        }

        public static string GetSceneNameByBuildIndex(int buildIndex)
        {
            Debug.Log("build index " + buildIndex);
            return Path.GetFileNameWithoutExtension( SceneUtility.GetScenePathByBuildIndex(buildIndex));
        }

        public static void CallSceneLoad(int buildIndex)
        {
            Debug.Log(string.Format("Calling Scene {0}", buildIndex));
            //onCallSceneLoad.Invoke();
            //UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex);
            instance.StartCoroutine(LoadScene(buildIndex));
        }

        private static IEnumerator LoadScene(int buildIndex)
        {
            onCallSceneLoad.Invoke();
            yield return new WaitForSeconds(1);
            UnityEngine.SceneManagement.SceneManager.LoadScene(buildIndex);
        }
    }
}