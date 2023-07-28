// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System;
using UnityEngine;
using SimsoftVR.Readers;
using SimsoftVR.Entities;
using System.Linq;

namespace SimsoftVR
{
    public delegate void OnVibrationChange(float amountOfVibration);

    public class SimulationManager : MonoBehaviour
    {
        private RobotMetadata[] robots;

        public static event OnVibrationChange onVibrationChange;
        private JointsForVibration jointStrutct;

        [SerializeField] private bool animateRig;
        [SerializeField] private bool parentToSimulationOriginPlaceholder;

        [SerializeField] private Vector3 angleOffset;

        [Header("Prefabs refs")]
        [SerializeField] private GameObject nodePrefab;
        public static GameObject NodePrefab;

        [SerializeField] private GameObject jointPrefab;
        public static GameObject JointPrefab;

        [SerializeField] private GameObject linkPrefab;
        public static GameObject LinkPrefab;

        [Header("Models refs")]
        [Tooltip("Model pool for Links. Every model is associated to the Id of the Link we want to visualize")]
        [SerializeField] private ModelToLinkAssociation[] modelToLinkAssociations;
        private static ModelToLinkAssociation[] ModelToLinkAssociations;

        [Tooltip("Model pool for Joints. Every model is associated to the Id of the Joint we want to visualize")]
        [SerializeField] private ModelToJointAssociation[] modelToJointAssociations;
        private static ModelToJointAssociation[] ModelToJointAssociations;

        [Header("Scene Refs")]
        [SerializeField] private Transform simulationOriginPlaceholder;

        [Header("Other")]
        [SerializeField] private Material transparentMaterial;
        public static Material TransparentMaterial { get; private set; }

        Quaternion rotationOffset;

        private void Awake()
        {
            NodePrefab = nodePrefab;
            JointPrefab = jointPrefab;
            LinkPrefab = linkPrefab;
            ModelToLinkAssociations = modelToLinkAssociations;
            ModelToJointAssociations = modelToJointAssociations;
            TransparentMaterial = transparentMaterial;

            robots = (GameManager.CurrentReader as ROS_OutputReader).RobotMetadatas;
        }

        private void Start()
        {
            foreach (RobotMetadata robot in robots)
            {
                robot.OnJointsStartingPosesLoaded += InitJoints;
                    robot.OnLinkNodesStartingPosesLoaded += InitLink_Flex;
                if (robot.RobotSimulationType == RobotSimulationType.Rigid)
                    robot.OnLinkStartingPoseLoaded += InitLink_Rigid;
            }

            jointStrutct = new JointsForVibration(true);
        }

        private void Update()
        {
            if (animateRig)
            {
                foreach (RobotMetadata robot in robots)
                {
                    UpdateJointsPoses(robot);
                    UpdateLinkPoses(robot);
                }
            }
        }

        private void InitJoints(RobotMetadata robot, Pose[] jointsStartingPoses)
        {
            Debug.Log("Initing Joints");

            for (int i = 0; i < jointsStartingPoses.Length; i++)
            {
                try
                {
                    GameObject jointGO;
                    if (parentToSimulationOriginPlaceholder)
                    {
                        jointGO = Instantiate(JointPrefab, simulationOriginPlaceholder);

                        // The Minus '-' sign for y and w components in necessary to take into account the different orientation of Simsoft reference system compared to Unity
                        jointGO.transform.localPosition = new Vector3(jointsStartingPoses[i].position.x, -jointsStartingPoses[i].position.y, jointsStartingPoses[i].position.z);
                        jointGO.transform.localRotation = new Quaternion(jointsStartingPoses[i].rotation.x, -jointsStartingPoses[i].rotation.y, jointsStartingPoses[i].rotation.z, -jointsStartingPoses[i].rotation.w);
                    }
                    else
                    {
                        jointGO = Instantiate(JointPrefab, jointsStartingPoses[i].position, jointsStartingPoses[i].rotation);
                    }

                    jointGO.GetComponent<Entities.Joint>().Init(i, robot.Id);
                    robot.AddJointGO(jointGO);

                    ModelToJointAssociation modelToJoint = GetAssociatedJointModel(i, robot.Id);
                    if (modelToJoint.hide == false && modelToJoint.associatedModel != null)
                    {
                        GameObject model = Instantiate(modelToJoint.associatedModel, jointGO.transform);
                        model.transform.localPosition = Vector3.zero;
                        model.transform.localRotation = Quaternion.identity;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Error during Joint Initialization: {0}", e.Message));
                }
            }
        }

        private void InitLink_Rigid(RobotMetadata robot, int id, Pose linkPose)
        {
            Debug.Log(string.Format("Robot {0}. Initing Rigid Link {1}", robot.Name, id));

            GameObject linkGO;
            if (parentToSimulationOriginPlaceholder)
            {
                linkGO = Instantiate(LinkPrefab, simulationOriginPlaceholder);

                // The Minus '-' sign for y and w components in necessary to take into account the different orientation of Simsoft reference system compared to Unity
                linkGO.transform.localPosition = new Vector3(linkPose.position.x, -linkPose.position.y, linkPose.position.z);
                linkGO.transform.localRotation = new Quaternion(linkPose.rotation.x, -linkPose.rotation.y, linkPose.rotation.z, -linkPose.rotation.w);
            }
            else
            {
                linkGO = Instantiate(LinkPrefab, linkPose.position, linkPose.rotation);
            }

            ModelToLinkAssociation modelToLink = GetAssociatedLinkModel(id, robot.Id);
            GameObject model = null;
            if (modelToLink.hide == false && modelToLink.associatedModel != null)
            {
                model = Instantiate(modelToLink.associatedModel, linkGO.transform);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
            }

            Link link = linkGO.GetComponent<Link>();
            link.Init(id, robot.Id);
            link.SetupLinkModel(model);
            robot.AddLinkGO(linkGO);
        }

        private void InitLink_Flex(RobotMetadata robot, int id, Pose[] nodePoses)
        {
            Debug.Log(string.Format("Robot {0}. Initing Flex Link {1}", robot.Name, id));

            GameObject linkGO;
            if (parentToSimulationOriginPlaceholder)
            {
                linkGO = Instantiate(LinkPrefab, simulationOriginPlaceholder);
                linkGO.transform.localPosition = robot.JointsGOs[id - 1].transform.position;
                linkGO.transform.localRotation = Quaternion.identity;
            }
            else
            {
                linkGO = Instantiate(LinkPrefab, robot.JointsGOs[id - 1].transform.position, Quaternion.identity);
            }

            if (linkGO.GetComponentInChildren<Rigger>() != null)
            {
                rotationOffset = linkGO.GetComponentInChildren<Rigger>().GetComponentInChildren<SkinnedMeshRenderer>().transform.rotation;
            }

            Link link = linkGO.GetComponent<Link>();
            link.Init(id, robot.Id);

            try
            {
                switch (robot.RobotSimulationType)
                {
                    case RobotSimulationType.Flex:
                        for (int i = 0; i < nodePoses.Length; i++)
                        {
                            GameObject nodeGO;

                            if (parentToSimulationOriginPlaceholder)
                            {
                                nodeGO = Instantiate(NodePrefab, simulationOriginPlaceholder);

                                // The Minus '-' sign for y and w components in necessary to take into account the different orientation of Simsoft reference system compared to Unity
                                nodeGO.transform.localPosition = new Vector3(nodePoses[i].position.x, -nodePoses[i].position.y, nodePoses[i].position.z);
                                nodeGO.transform.localRotation = new Quaternion(nodePoses[i].rotation.x, -nodePoses[i].rotation.y, nodePoses[i].rotation.z, -nodePoses[i].rotation.w);
                            }
                            else
                            {
                                nodeGO = Instantiate(NodePrefab);
                                nodeGO.transform.position = nodePoses[i].position;
                                nodeGO.transform.rotation = nodePoses[i].rotation;
                            }

                            Node node = nodeGO.GetComponent<Node>();
                            node.SetupNode(i, link);
                        }
                        break;
                }

                link.SetupLinkModel();
                robot.AddLinkGO(linkGO);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error during Link {0} Initialization: {1}", id, e.Message));
            }
        }

        private void UpdateJointsPoses(RobotMetadata robot)
        {
            Pose[] jointsCurrentPoses = robot.JointsCurrentPoses;

            if (robot.JointsGOs == null || jointsCurrentPoses == null)
                return;
            
            try
            {
                for (int i = 0; i < jointsCurrentPoses.Length; i++)
                {
                    if (parentToSimulationOriginPlaceholder)
                    {
                        // The Minus '-' sign for y and w components in necessary to take into account the different orientation of Simsoft reference system compared to Unity
                        robot.JointsGOs[i].transform.localPosition = new Vector3(jointsCurrentPoses[i].position.x, -jointsCurrentPoses[i].position.y, jointsCurrentPoses[i].position.z);
                        robot.JointsGOs[i].transform.localRotation = new Quaternion(jointsCurrentPoses[i].rotation.x, -jointsCurrentPoses[i].rotation.y, jointsCurrentPoses[i].rotation.z, -jointsCurrentPoses[i].rotation.w);
                    }
                    else
                    {
                        robot.JointsGOs[i].transform.position = jointsCurrentPoses[i].position;
                        robot.JointsGOs[i].transform.rotation = jointsCurrentPoses[i].rotation;
                    }

                    // OnVibrationChange setup
                    if (i == jointsCurrentPoses.Length -1 && jointStrutct.Init)
                    {
                        switch (robot.RobotSimulationType)
                        {
                            case RobotSimulationType.Flex: jointStrutct.flexJoint = robot.JointsGOs[i].transform.position; break;
                            case RobotSimulationType.Rigid: jointStrutct.rigidJoint = robot.JointsGOs[i].transform.position; break;
                            default: break;
                        }
                    }
                }
            }catch(Exception e)
            {
                Debug.LogError(string.Format("Error during Joint Poses update: {0}", e.Message));
            }
        }

        private void UpdateLinkPoses(RobotMetadata robot)
        {
            switch (robot.RobotSimulationType)
            {
                case RobotSimulationType.Flex:
                    if (!robot.LinkDictionaryReady)
                        return;

                    for (int i = 1; i <= robot.NumberOfLinks; i++)
                    {
                        Debug.Log(string.Format("Robot {0} moving link {1}", robot.Name, i));
                        Pose[] nodesCurrentPoses = robot.LinkNodesCurrentPoses[i];
                        Link link = robot.LinksGOs[i - 1].GetComponent<Link>();

                        for (int j = 0; j < link.Nodes.Count; j++)
                        {
                            if (parentToSimulationOriginPlaceholder)
                            {
                                // The Minus '-' sign for y and w components in necessary to take into account the different orientation of Simsoft reference system compared to Unity
                                link.Nodes[j].transform.localPosition = new Vector3(nodesCurrentPoses[j].position.x, -nodesCurrentPoses[j].position.y, nodesCurrentPoses[j].position.z);
                                link.Nodes[j].transform.localRotation = new Quaternion(nodesCurrentPoses[j].rotation.x, -nodesCurrentPoses[j].rotation.y, nodesCurrentPoses[j].rotation.z, -nodesCurrentPoses[j].rotation.w);
                            }
                            else
                            {
                                link.Nodes[j].transform.position = nodesCurrentPoses[j].position;
                                link.Nodes[j].transform.rotation = nodesCurrentPoses[j].rotation;
                            }
                        }
                    }
                    break;

                case RobotSimulationType.Rigid:
                    if (robot.JointsCurrentPoses == null)
                        return;

                    for (int i = 1; i <= robot.NumberOfLinks; i++)
                    {
                        GameObject linkGO = robot.LinksGOs[i - 1];
                        int startingJointId = SimulationManager.GetAssociatedStartJointFromLink(i);
                        Pose startingJointCurrentPose = robot.JointsCurrentPoses[startingJointId];
                        if (parentToSimulationOriginPlaceholder)
                        {
                            // The Minus '-' sign for y and w components in necessary to take into account the different orientation of Simsoft reference system compared to Unity
                            linkGO.transform.localPosition = new Vector3(startingJointCurrentPose.position.x, -startingJointCurrentPose.position.y, startingJointCurrentPose.position.z);
                            linkGO.transform.localRotation = new Quaternion(startingJointCurrentPose.rotation.x, -startingJointCurrentPose.rotation.y, startingJointCurrentPose.rotation.z, -startingJointCurrentPose.rotation.w);
                        }
                        else
                        {
                            linkGO.transform.position = startingJointCurrentPose.position;
                            linkGO.transform.rotation = startingJointCurrentPose.rotation;
                        }
                    }
                    break;
            }
        }

        public static ModelToLinkAssociation GetAssociatedLinkModel(int linkId, int robotId)
        {
            var result = from modelToLinkAssociation in ModelToLinkAssociations where modelToLinkAssociation.linkId.Equals(linkId) where modelToLinkAssociation.robotId.Equals(robotId) select modelToLinkAssociation;
            if (result.Count() > 0)
                return result.First();
            else
            {
                Debug.LogWarning(string.Format("Non � stato trovato un modello associato per il link con id {0}. Si provvede ad associare il primo modello registrato. Controllare il campo ModelToLinkAssociations nell'inspector del SimulationManager", linkId));
                return new ModelToLinkAssociation(0, robotId, null, -1, true, false); // ModelToLinkAssociations[0];
            }
        }

        public static ModelToJointAssociation GetAssociatedJointModel(int jointId, int robotId)
        {
            var result = from modelToJointAssociation in ModelToJointAssociations where modelToJointAssociation.jointId.Equals(jointId) select modelToJointAssociation;

            if (result.Count() > 0)
                return result.First();
            else
            {
                Debug.LogWarning(string.Format("Non � stato trovato un modello associato per il giunto con id {0}", jointId));
                return new ModelToJointAssociation(0, robotId,  null, true);
            }
        }

        private void CalculateVibration(Vector3 flexLastJoint, Vector3 rigidLastJoint)
        {
            float vibrationRatio = Math.Abs((((flexLastJoint - rigidLastJoint).magnitude) / rigidLastJoint.magnitude)) * 100f;
            onVibrationChange?.Invoke(vibrationRatio);
        }

        public static int GetAssociatedStartJointFromLink(int linkId)
        {
            foreach (ModelToLinkAssociation modelToLinkAssociation in ModelToLinkAssociations)
                if (modelToLinkAssociation.linkId == linkId)
                    return modelToLinkAssociation.startingJointId;
            Debug.LogError(string.Format("No associated StartJoint from Link {0}", linkId));
            return 0;
        }
    }

    public struct JointsForVibration
    {
        public Vector3 flexJoint;
        public Vector3 rigidJoint;
        private bool _init;
        public bool Init { get { return _init; } }
        public JointsForVibration(bool init)
        {
            flexJoint = new Vector3();
            rigidJoint = new Vector3();
            _init = init;
        }
    }

    // Struttura per consentire l'associazione tra l'Id di un Link e il modello deformabile da associargli
    [Serializable]
    public struct ModelToLinkAssociation
    {
        public int linkId;
        public int robotId;
        public GameObject associatedModel;
        public int startingJointId;
        public bool hide;
        public bool useTransparentMaterial;

        public ModelToLinkAssociation(int id, int robotId, GameObject associatedModel, int startingJointID, bool hide, bool useTransparentMaterial)
        {
            this.linkId = id;
            this.robotId = robotId;
            this.associatedModel = associatedModel;
            this.startingJointId = startingJointID;
            this.hide = hide;
            this.useTransparentMaterial = useTransparentMaterial;
        }
    }

    // Struttura per consentire l'associazione tra l'Id di un Giunto e il modello da associargli
    [Serializable]
    public struct ModelToJointAssociation
    {
        public int jointId;
        public int robotId;
        public GameObject associatedModel;
        public bool hide;

        public ModelToJointAssociation(int id, int robotId, GameObject associatedModel, bool hide)
        {
            this.jointId = id;
            this.robotId = robotId;
            this.associatedModel = associatedModel;
            this.hide = hide;
        }
    }
}