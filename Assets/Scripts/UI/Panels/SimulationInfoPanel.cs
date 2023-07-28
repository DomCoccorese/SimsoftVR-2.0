// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

namespace SimsoftVR.UI
{
    public class SimulationInfoPanel : Panel
    {
        public SimulationInfoPanelRow[] rows;
        public UnityEngine.UI.Text controlMode;

        protected override void Init()
        {
            base.Init();
            TogglePanel();
        }

        private void Start()
        {
            Settings.onSettingsChanged += TogglePanel;
        }

        private void OnDestroy()
        {
            Settings.onSettingsChanged -= TogglePanel;
        }

        private void TogglePanel()
        {
            gameObject.SetActive(Settings.current.IsInfoPanelActive);
        }
    }
}
