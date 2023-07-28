// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;
using UnityEngine.UI;

namespace SimsoftVR.UI
{
    public class ActiveJointPanel : MonoBehaviour
    {
        [SerializeField] private Text feedbackText;

        // Update is called once per frame
        void Update()
        {
            UpdateActiveJointText();
        }

        private void UpdateActiveJointText()
        {
            int activeJoint = GameManager.CurrentReader.ActiveJointId;
            if(activeJoint == 0)
                feedbackText.text = string.Format("None");
            else if(activeJoint == -1)
                feedbackText.text = string.Format("All");
            else
            feedbackText.text = string.Format("J {0}", GameManager.CurrentReader.ActiveJointId);
        }
    }
}