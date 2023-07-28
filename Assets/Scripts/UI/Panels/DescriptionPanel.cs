// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine.UI;

namespace SimsoftVR.UI
{
    public class DescriptionPanel : Panel
    {
        public Text title;
        public Text description;
        
        public void CloseDescriptionPanel()
        {
            Close();
            title.text = "";
            description.text = "";
        }
    }
}
