// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace SimsoftVR.UI
{
    public class SelectScenePanel : Panel
    {
        [SerializeField] private GameObject sceneButtonPrefab;

        [Header("Scene Refs")]
        [SerializeField] private Transform sceneContentPanel;

        private void OnEnable()
        {
            int numberOfScenes = SceneManager.NumberOfScenes;

            UIManager.CleanContentPanel(sceneContentPanel);
            UIManager.ResizeContentPanel(sceneContentPanel, sceneButtonPrefab.transform, numberOfScenes);

            List<GameObject> sceneButtons = new List<GameObject>();
            for (int i = 0; i < numberOfScenes; i++)
            {
                GameObject sceneButtonGO = Instantiate(sceneButtonPrefab, sceneContentPanel);
                int buildIndex = i;
                string scenePath = SceneManager.GetSceneNameByBuildIndex(i); //Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
                sceneButtonGO.GetComponentInChildren<Text>().text = scenePath; // SceneManager.GetSceneNameByBuildIndex(i);
                sceneButtonGO.GetComponent<Button>().onClick.AddListener(() => SceneManager.CallSceneLoad(buildIndex)); //(() => UnityEngine.SceneManagement.SceneManager.LoadScene(scenePath));
                sceneButtons.Add(sceneButtonGO);
            }
        }
    }
}