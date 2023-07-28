// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;

namespace SimsoftVR.UI
{
    [CreateAssetMenu(menuName = "Data/SimulationInfoPanelData")]
    public class TrafficLightFeedback : ScriptableObject
    {   
        public string fieldDeclarationName;
        [TextAreaAttribute]
        public string description;
        public float[] transitionsThresholds;
        public enum TrafficLightStatus { CLEAR, WARNING, CRITICAL }
        public StatusVisualFeedback[] statusAvailable;
        public bool usesRawNumericalValues;

        [System.Serializable]
        public struct StatusVisualFeedback
        {
            public string StatusName;
            public TrafficLightStatus status;
            public float thresholdFromPreviousState;
            public Sprite statusImage;
            public string statusText;
            public Color textColor;
        }
    }
}
