using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimsoftVR.UI
{
    public class MainMenuPanel : Panel
    {
        public void CallQuitApplication()
        {
            GameManager.QuitApplication();
        }
    }
}