// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;
using UnityEngine.UI;

namespace SimsoftVR.UI
{
    public class SimulationInfoPanelRow : MonoBehaviour
    {
        public TrafficLightFeedback rowData;
        public Text statusText;
        public Text numberFeedbackText;
        public Image imageFeedback;
        public Button infoButton;

        void Start()
        {
            SetRowStatus(TrafficLightFeedback.TrafficLightStatus.CLEAR);
            infoButton.onClick.AddListener(OpenDescriptionPanel);

            if (rowData.usesRawNumericalValues && numberFeedbackText != null)
                SimsoftVR.SimulationManager.onVibrationChange += UpdateNumericalValue;
        }

        void OnDestroy()
        {
            infoButton.onClick.RemoveAllListeners();
            if (rowData.usesRawNumericalValues)
                SimsoftVR.SimulationManager.onVibrationChange -= UpdateNumericalValue;
        }

        public void SetRowStatus(TrafficLightFeedback.TrafficLightStatus desiredStatus)
        {
            foreach (TrafficLightFeedback.StatusVisualFeedback svf in rowData.statusAvailable)
            {
                if (svf.status == desiredStatus)
                {
                    ChangeUI(svf);
                    return;
                }
            }
         //   Debug.LogWarning("Status " + desiredStatus.ToString() + " for " + gameObject.name + " is not supported");
        }

        public void SetRowStatus(float treshold)
        {
            if (treshold == Mathf.Infinity || float.IsNaN(treshold))
                return;

            if (treshold > rowData.statusAvailable[0].thresholdFromPreviousState && treshold <= rowData.statusAvailable[1].thresholdFromPreviousState)
            {
                SetRowStatus(rowData.statusAvailable[1].status);
              //  Debug.Log("treshold 0 ---> " + treshold);
            }
            else if (treshold > rowData.statusAvailable[1].thresholdFromPreviousState)
            {
                SetRowStatus(rowData.statusAvailable[2].status);
              //  Debug.Log("treshold 1 ---> " + treshold);
            }
            else
            {
                SetRowStatus(rowData.statusAvailable[0].status);
              //  Debug.Log("treshold default ---> " + treshold);
            }
        }

        private void ChangeUI(TrafficLightFeedback.StatusVisualFeedback referencedStatus)
        {
            statusText.text = referencedStatus.statusText;
            statusText.color = referencedStatus.textColor;
            imageFeedback.sprite = referencedStatus.statusImage;
        }

        private void UpdateNumericalValue(float newValue)
        {
            if (newValue == Mathf.Infinity || float.IsNaN(newValue))
                numberFeedbackText.text = string.Format("{0}%", "0.0");
            else {
                numberFeedbackText.text = string.Format("{0}%", newValue.ToString("0.0"));
                SetRowStatus(newValue);
            }
        }

        private void OpenDescriptionPanel()
        {
            UIManager.instance.OpenDescriptionInfoPanel(rowData);
        }
    }
}
