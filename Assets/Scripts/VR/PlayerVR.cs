// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;

public class PlayerVR : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!GameManager.instance.XRDevicesAvailable)
        {
            gameObject.SetActive(false);
        }
    }
}
