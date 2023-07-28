// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;
using UnityEngine.UI;

namespace SimsoftVR.UI
{
    public class SettingsPanel : Panel
    {
        [SerializeField] private IPInputField ipInputBox;
        [SerializeField] private ToggleGroupExtended operationModeToggle;
        [SerializeField] private Toggle showInfoPanel;
        public Settings defaultSettings;
        private Settings tempSettings;

        protected override void Init()
        {
            base.Init();
            Settings.LoadFromReg(defaultSettings);
            tempSettings = Settings.current;
            InitUI(tempSettings);
        }

        private void InitUI(Settings settings)
        {
            // IP
            ipInputBox.ipInputField.placeholder.GetComponent<Text>().text = settings.IpAddress;

            // INFOPANEL ACTIVE
            showInfoPanel.isOn = settings.IsInfoPanelActive;
        }

        public void Apply()
        {
            // IP
            if (ipInputBox.IsAddressValid)
                tempSettings.SetIPAddress(ipInputBox.ipInputField.textComponent.text);
            else
                ipInputBox.GiveErrorFeedback();

            // INFOPANEL ACTIVE
            tempSettings.SetInfoPanelVisibility(showInfoPanel.isOn);

            tempSettings.Apply();
        }

        public void ApplyAndClose()
        {
            Apply();
            Close();
        }

        public void Discard()
        {
            Close();
        }
    }
}