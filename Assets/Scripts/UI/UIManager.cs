// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;

namespace SimsoftVR.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance = null;

        public DescriptionPanel descriptionPanel;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            else if (instance != this)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        public void CallQuitApplication()
        {
            GameManager.QuitApplication();
        }

        public static void CloseAllPanels()
        {
            Panel[] panels = FindObjectsOfType<Panel>();
            foreach (Panel panel in panels)
                panel.Close();
        }

        public void OpenDescriptionInfoPanel(TrafficLightFeedback rowData)
        {
            descriptionPanel.title.text = rowData.fieldDeclarationName;
            descriptionPanel.description.text = rowData.description;
            descriptionPanel.Open();
        }

        //public static void InitContentPanel(Transform contentPanel, GameObject[] elements)
        //{
        //    for (int i = 0; i < contentPanel.childCount; i++)
        //    {
        //        Destroy(contentPanel.GetChild(i).gameObject);
        //    }

        //    float contentPanelHeight = 0;
        //    foreach (GameObject element in elements)
        //    {
        //        contentPanelHeight += element.GetComponent<RectTransform>().rect.height;
        //        element.transform.SetParent(contentPanel);
        //    }
        //    contentPanel.GetComponent<RectTransform>(). sizeDelta = new Vector2(0, contentPanelHeight);
        //}

        public static void CleanContentPanel(Transform contentPanel)
        {
            for (int i = 0; i < contentPanel.childCount; i++)
            {
                Destroy(contentPanel.GetChild(i).gameObject);
            }
        }

        public static void ResizeContentPanel(Transform contentPanel, Transform elementPrefab, int numberOfElements)
        {
            contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, elementPrefab.GetComponent<RectTransform>().rect.height * numberOfElements);
        }

        public static void ResizeContentPanel(Transform contentPanel, Transform elementPrefab)
        {
            contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, elementPrefab.GetComponent<RectTransform>().rect.height * contentPanel.childCount);
        }
    }
}