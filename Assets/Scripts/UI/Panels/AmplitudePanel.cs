// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;
using UnityEngine.UI;

namespace SimsoftVR.UI
{
    public class AmplitudePanel : MonoBehaviour
    {
        [SerializeField] private Text feedbackText;
        [SerializeField] private SimulationInfoPanelRow simPanelRow;

        void Update()
        {
            UpdateAmplitude();
        }

        private void UpdateAmplitude()
        {
            double amplitude = GameManager.CurrentReader.Amplitude;
            feedbackText.text = string.Format("{0} mm", amplitude.ToString("0.00"));

            simPanelRow.SetRowStatus((float)amplitude);
        }
    }
}