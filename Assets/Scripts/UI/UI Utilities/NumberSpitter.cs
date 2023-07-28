// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections;
using UnityEngine;

public class NumberSpitter : MonoBehaviour
{
    public SimsoftVR.UI.SimulationInfoPanelRow row1;
    public SimsoftVR.UI.SimulationInfoPanelRow row2;
    public SimsoftVR.UI.SimulationInfoPanelRow row3;
    
    void Start()
    {
        StartCoroutine(ChangeNumber1());
        StartCoroutine(ChangeNumber2());
        StartCoroutine(ChangeNumber3());
    }

    private IEnumerator ChangeNumber1()
    {
        while (true)
        {
            float number = Random.Range(0f, 10f);
            SimsoftVR.UI.TrafficLightFeedback.TrafficLightStatus newStatus = CheckThreshold(number, row1);
            row1.SetRowStatus(newStatus);
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator ChangeNumber2()
    {
        while (true)
        {
            float number = Random.Range(0f, 10f);
            SimsoftVR.UI.TrafficLightFeedback.TrafficLightStatus newStatus = CheckThreshold(number, row2);
            row2.SetRowStatus(newStatus);
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator ChangeNumber3()
    {
        while (true)
        {
            float number = Random.Range(0f, 10f);
            SimsoftVR.UI.TrafficLightFeedback.TrafficLightStatus newStatus = CheckThreshold(number, row3);
            row3.SetRowStatus(newStatus);
            yield return new WaitForSeconds(1);
        }
    }

    private SimsoftVR.UI.TrafficLightFeedback.TrafficLightStatus CheckThreshold(float amount, SimsoftVR.UI.SimulationInfoPanelRow referencedRow)
    {
        if (referencedRow.rowData.transitionsThresholds.Length == 2)
        {
            switch (amount)
            {
                case float i when i <= referencedRow.rowData.transitionsThresholds[0]: return referencedRow.rowData.statusAvailable[0].status;
                case float i when i > referencedRow.rowData.transitionsThresholds[0] && i <= referencedRow.rowData.transitionsThresholds[1]: return referencedRow.rowData.statusAvailable[1].status;
                case float i when i >= referencedRow.rowData.transitionsThresholds[1]: return referencedRow.rowData.statusAvailable[2].status;
                default: break;
            }
        }
        return SimsoftVR.UI.TrafficLightFeedback.TrafficLightStatus.CLEAR;
    }
}
