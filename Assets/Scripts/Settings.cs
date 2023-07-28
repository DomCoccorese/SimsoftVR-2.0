// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;

namespace SimsoftVR
{
    public enum OperationModes { PLANNING, OPERATION }

    [System.Serializable]
    public class Settings
    {
        public static Settings current;
        [SerializeField]
        private static Settings _defaultSettings;

        public delegate void OnSettingsChanged();
        public static OnSettingsChanged onSettingsChanged;

        private static string customSettingsAvailableKeyName = "CUSTOM_SETTINGS_AVAILABLE";
        private static string ipAddressKeyName = "CUSTOM_IP_ADDRESS";
        private static string infoPanelShownName = "CUSTOM_INFOPANEL_SHOW";

        public void Apply()
        {
            current = this;
            SetRegKeyValues();
            onSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Carica i Settings se presenti nel registro. Altrimenti carica quelli passati attraverso il parametro defaultSettings
        /// </summary>
        /// <param name="defaultSettings">I settings di default da caricare nel caso in cui non ci sono Settings salvati nel registro</param>
        public static void LoadFromReg(Settings defaultSettings)
        {
            _defaultSettings = defaultSettings;
            current = PlayerPrefs.HasKey(customSettingsAvailableKeyName) ? GetSettingsFromReg() : _defaultSettings;
        }

        /// <summary>
        /// Carica i Settings se presenti nel registro. Se assenti carica i settaggi di default Inizializzati tramite il metodo LoadFromReg(Settings defaultSettings)
        /// </summary>
        public static void LoadFromReg()
        {
            if (_defaultSettings == null)
            {
                Debug.LogError("Settaggi di default non inizializzati all'interno della classe Settings");
                return;
            }

            LoadFromReg(_defaultSettings);
            onSettingsChanged?.Invoke();
        }

        private void SetRegKeyValues()
        {
            PlayerPrefs.SetInt(customSettingsAvailableKeyName, 1);
            PlayerPrefs.SetString(ipAddressKeyName, IpAddress);
            PlayerPrefs.SetInt(infoPanelShownName, isInfoPanelActive == false ? 0 : 1);
            PlayerPrefs.Save();
        }

        private static Settings GetSettingsFromReg()
        {
            Settings settings = new Settings();
            settings.SetIPAddress(PlayerPrefs.GetString(ipAddressKeyName));
            settings.SetInfoPanelVisibility(PlayerPrefs.GetInt(infoPanelShownName));
            return settings;
        }
        
#region IP_ADDRESS
        [SerializeField]
        private string m_ipAddress;
        [SerializeField]
        public string IpAddress { get { return m_ipAddress; } }

        public void SetIPAddress(string ip)
        {
            m_ipAddress = ip;
        }
#endregion

#region OPERATION_MODE
        // [SerializeField]
        // private int operationModeIndex; // 0 = Operation; 1 = Planning
        // [SerializeField]
        // public OperationModes OperationMode { get { return (OperationModes) operationModeIndex; }}

        // public void SetOperationMode(int index)
        // {
        //     operationModeIndex = index;
        // }
#endregion

#region INFOPANEL_SHOW
        [SerializeField]
        private bool isInfoPanelActive;
        [SerializeField]
        public bool IsInfoPanelActive { get { return isInfoPanelActive; } }

        public void SetInfoPanelVisibility(bool isActive)
        {
            isInfoPanelActive = isActive;
        }

        public void SetInfoPanelVisibility(int isActive)
        {
            isInfoPanelActive = isActive == 0 ? false : true;
        }
#endregion

    }
}