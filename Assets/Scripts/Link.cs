// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections.Generic;
using UnityEngine;

namespace SimsoftVR.Entities
{
    public class Link : MonoBehaviour
    {
        public int Id { get; private set; }
        public int RobotId { get; private set; }
        public List<Node> Nodes { get; private set; }

        private GameObject linkModelPrefab;
        private GameObject linkModelGO;

        private int numberOfNodes;
        private float linkLenght;   //for debug
        
        [SerializeField] private RigMainAxis rigMainAxis;

        private Transform[] modelBones;

        [SerializeField] private Quaternion[] modelBonesStartingRotationalValues;

        private void Update()
        {
            if (modelBones == null)
                return;

            for (int i = 0; i < modelBones.Length; i++)
            {
                modelBones[i].position = new Vector3(Nodes[i].transform.position.x, Nodes[i].transform.position.y, Nodes[i].transform.position.z);
                modelBones[i].rotation = Quaternion.Euler(Nodes[i].transform.rotation.eulerAngles.x, Nodes[i].transform.rotation.eulerAngles.y, Nodes[i].transform.rotation.eulerAngles.z);
            }
        }

        public void Init(int linkId, int robotId)
        {
            Id = linkId;
            RobotId = robotId;
            gameObject.name = string.Format("Robot_{0}_Link_{1}", robotId, linkId);
        }

        public void AddNode(Node node)
        {
            if (Nodes == null)
                Nodes = new List<Node>();

            Nodes.Add(node);
        }

        public void SetupLinkModel(GameObject model)
        {
            ModelToLinkAssociation modelToLinkAssociation = SimulationManager.GetAssociatedLinkModel(Id, RobotId);
            if (modelToLinkAssociation.hide)
                return;

            linkModelGO = model;
            if (modelToLinkAssociation.useTransparentMaterial)
                ReplaceModelMaterial(SimulationManager.TransparentMaterial);
        }

        public void SetupLinkModel()
        {
            ModelToLinkAssociation modelToLinkAssociation = SimulationManager.GetAssociatedLinkModel(Id, RobotId);
            if (modelToLinkAssociation.hide)
                return;
            linkModelPrefab = modelToLinkAssociation.associatedModel;
            linkModelGO = GameObject.Instantiate(linkModelPrefab);

            if (modelToLinkAssociation.useTransparentMaterial)
                ReplaceModelMaterial(SimulationManager.TransparentMaterial);

            linkLenght = Vector3.Magnitude(Nodes[Nodes.Count - 1].transform.position - Nodes[0].transform.position); // for debug

            Rigger rigger = linkModelGO.GetComponent<Rigger>();
            rigger.InitBones();

            linkModelGO.transform.parent = transform;
            linkModelGO.transform.localPosition = Vector3.zero;
            linkModelGO.transform.localRotation = Quaternion.identity;

            modelBones = rigger.Bones;

            modelBonesStartingRotationalValues = new Quaternion[modelBones.Length];
            for (int i = 0; i < modelBones.Length; i++)
                modelBonesStartingRotationalValues[i] = modelBones[i].rotation;
        }

        private void ReplaceModelMaterial(Material newMaterial)
        {
            MeshRenderer[] meshRenderers = linkModelGO.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = linkModelGO.GetComponentsInChildren<SkinnedMeshRenderer>();

            if (meshRenderers.Length > 0)
                foreach (MeshRenderer renderer in meshRenderers)
                {
                    // Workround necessary since Unity doesn't allow to use renderer.sharedMaterials[i] = newMaterial to set materials
                    Material[] newMaterialArray = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMaterialArray.Length; i++)
                        newMaterialArray[i] = newMaterial;

                    renderer.sharedMaterials = newMaterialArray;
                }

            if (skinnedMeshRenderers.Length > 0)
                foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
                {
                    // Workround necessary since Unity doesn't allow to use renderer.sharedMaterials[i] = newMaterial to set materials
                    Material[] newMaterialArray = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMaterialArray.Length; i++)
                        newMaterialArray[i] = newMaterial;

                    renderer.sharedMaterials = newMaterialArray;
                }
        }
    }
}