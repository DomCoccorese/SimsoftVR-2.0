using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimsoftVR.UI;

[CustomEditor(typeof(SimulationInfoPanelRow))]
public class SimulationInfoRowEditor : Editor
{
    private SimulationInfoPanelRow self;
    private SerializedProperty controlsRow;
    private SerializedProperty controlsVisibility;
    private SerializedProperty rowControlled;

    // private void OnEnable()
    // {
    //     self = (SimulationInfoPanelRow)target;
        
    //     controlsRow = serializedObject.FindProperty(nameof(self.isControllingAnotherRow));
    //     controlsVisibility = serializedObject.FindProperty(nameof(self.shouldChangeVisibility));
    //     rowControlled = serializedObject.FindProperty(nameof(self.rowToControl));
    // }
    

    // public override void OnInspectorGUI()
    // {
    //     base.OnInspectorGUI();

    //     EditorGUILayout.BeginToggleGroup("Controlled", controlsRow.boolValue);
    //     EditorGUI.indentLevel++;


    //     EditorGUI.indentLevel--;
    //     EditorGUILayout.EndToggleGroup();

    // }
}
