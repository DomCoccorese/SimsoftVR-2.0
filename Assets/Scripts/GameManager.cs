// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using SimsoftVR.Readers;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField] private OutputReader currentReader;
    public static OutputReader CurrentReader { get; private set; }
    public bool XRDevicesAvailable { get; private set; }
    public bool PrimitiveMode { get; private set; }

    //Temporaneo, almeno finchè gli sviluppatori di Unity non si decidono ad esporre un metodo che consente in automatico di rilevare se c'è un headset collegato
    [Tooltip("Is there a VR device connected?")]
    [SerializeField] private bool xrDeviceAvailable;

    [Tooltip("Are we using primitives or prefabs in SimulationManager for Link visualitation?")]
    [SerializeField] private bool usePrimitive;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
            Destroy(gameObject);

        if (CurrentReader == null)
            CurrentReader = currentReader;
    }

    private void Start()
    {
        XRDevicesAvailable = xrDeviceAvailable;

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
        }
    }

    public static void QuitApplication()
    {
        Application.Quit();
    }

    private bool XRAvailable()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                Debug.Log("XR Available");
                return true;
            }
        }
        Debug.Log("XR not Available");
        return false;
    }
}
