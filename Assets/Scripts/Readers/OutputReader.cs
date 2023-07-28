using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimsoftVR.Readers
{
    public delegate void OnControlTypeLoaded(string controlType);
    public delegate void OnActiveJointChanged(int activeJointId);

    public abstract class OutputReader : MonoBehaviour
    {
        private static OutputReader instance = null;

        public string ControlType { get; protected set; }

        public double Amplitude { get; protected set; }

        protected int activeJointId = -1;

        public int ActiveJointId
        {
            get
            { return activeJointId; }
            set
            {
                activeJointId = value;
            }
        }

        public event OnControlTypeLoaded onControlTypeLoaded;
        //public event OnActiveJointChanged onActiveJointChanged;

        public abstract void ReadOutput();

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            if (GameManager.CurrentReader != this)
            {
                enabled = false;
                return;
            }
            ReadOutput();
        }

        public void InvokeOnControlTypeLoaded()
        {
            onControlTypeLoaded.Invoke(ControlType);
        }
    }


}