// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Rigger))]
[CanEditMultipleObjects]
public class RiggerEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Setup Rigger"))
        {
            Debug.Log("Setting up Rigger");
            Rigger myRigger = (Rigger)target;

            if (PrefabUtility.GetPrefabInstanceHandle(myRigger.gameObject) != null)
                PrefabUtility.UnpackPrefabInstance(myRigger.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            List<Transform> bonesList = new List<Transform>();
            Transform currentChild = myRigger.bonesRoot.transform;

            do
            {
                if (currentChild != myRigger.bonesRoot.transform)
                    bonesList.Add(currentChild);
                currentChild = currentChild.GetChild(0);
            } while (currentChild.childCount > 0);

            bonesList.Add(currentChild);

            myRigger.Bones = bonesList.ToArray();
        }
    }
}
