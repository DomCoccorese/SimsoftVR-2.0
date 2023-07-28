// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class IPInputField : MonoBehaviour
{
    private bool isAddressValid;
    public bool IsAddressValid { get { return isAddressValid; } }

    public InputField ipInputField;
    public Text ipInputFeedback;

    public string successMessage;
    public Color successColor;
    public string failMessage;
    public string errorApplyMessage;
    public Color failColor;

    public void ValidateIPAddress()
    {
        IPAddress ip;
        isAddressValid = IPAddress.TryParse(ipInputField.textComponent.text, out ip);
        if (IsAddressValid)
        {
            print("Valid IP");
            ipInputFeedback.text = successMessage;
            ipInputFeedback.color = successColor;
        }
        else
        {
            print("Not valid IP");
            ipInputFeedback.text = failMessage;
            ipInputFeedback.color = failColor;
        }
    }

    public void GiveErrorFeedback()
    {
        ipInputFeedback.text = errorApplyMessage;
        ipInputFeedback.color = failColor;
    }
}
