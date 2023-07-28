// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;
using UnityEngine.UI;

namespace SimsoftVR.UI
{
    public class SimulationInfoPanelControlType : MonoBehaviour
    {
        public Text statusText;
        public bool isControllingAnotherRow = false;
        public bool shouldChangeVisibility = false;
        public SimulationInfoPanelRow rowToControl;

        [SerializeField] private OperationModes operationMode;

        void Start()
        {
            GameManager.CurrentReader.onControlTypeLoaded += UpdateControlType;
            SetMode(operationMode); // Per ora hardcodo così. Da impostare nel pannello settings
        }

        void OnDestroy()
        {
            GameManager.CurrentReader.onControlTypeLoaded -= UpdateControlType;
        }

        private void UpdateControlType(string controlType)
        { 
            statusText.text = controlType;
        }

        public void SetMode(SimsoftVR.OperationModes operationMode)
        {
            if (isControllingAnotherRow && shouldChangeVisibility)
            {
                switch (operationMode)
                {
                    case SimsoftVR.OperationModes.OPERATION: rowToControl.gameObject.SetActive(true); break;
                    case SimsoftVR.OperationModes.PLANNING: rowToControl.gameObject.SetActive(false); break;
                    default: break;
                };
            }
        }
    }    
}
