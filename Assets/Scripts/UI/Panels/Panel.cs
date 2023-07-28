// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;

namespace SimsoftVR.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class Panel : MonoBehaviour
    {
        [SerializeField] protected bool hideOnAwake;

        //Il pannello è stato aperto dall'utente tramite l'UI? Se sì allora non chiuderlo tramite Awake()
        private bool calledByUser = false;

        protected virtual void Awake()
        {
            if (hideOnAwake && !calledByUser)
                Close();
        }

        public void OpenClose()
        {
            if (gameObject.activeInHierarchy)
            {
                Close();
                return;
            }

            Open();
        }

        public void Open()
        {
            calledByUser = true;
            gameObject.SetActive(true);
            Init();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Init() { }
    }
}