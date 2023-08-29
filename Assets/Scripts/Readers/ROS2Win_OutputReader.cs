// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ROS2;
using string_msg = std_msgs.msg.String;
using pose_msg = geometry_msgs.msg.PoseArray;
using int_msg = std_msgs.msg.Int32;
using float_msg = std_msgs.msg.Float64;

namespace SimsoftVR.Readers
{
    [RequireComponent(typeof(ROS2UnityComponent))]
    public class ROS2Win_OutputReader : ROS_OutputReader
    {
        private static ROS2Win_OutputReader instance = null;

        private ROS2UnityComponent ros2Unity;
        private static ROS2Node ros2Node;
        private ISubscription<string_msg> string_sub;
        private ISubscription<int_msg> int_sub;
        private ISubscription<float_msg> float_sub;
        private ISubscription<pose_msg> pose_sub_NumberOfJoints;
        private ISubscription<pose_msg> pose_sub_JointsStartingPoses;
        private ISubscription<pose_msg> pose_sub_LinkNodesStartingPoses;
        private ISubscription<pose_msg> pose_sub_JointsCurrentPoses;
        private ISubscription<pose_msg> pose_sub_LinkNodesCurrentPoses;

        private float timer = 0;
        private float maxWaitTime = 10.0f;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);

            SceneManager.onCallSceneLoad += ClearRosNodes;
        }

        public override void ReadOutput()
        {
            ros2Unity = GetComponent<ROS2UnityComponent>();
            StartCoroutine(ReadData());
        }

        private IEnumerator ReadData()
        {
            yield return StartCoroutine(ReadMetadata());
            while (!metadataLoaded)
                yield return null;

            yield return StartCoroutine(ReadCurrentPoses());
        }

        private IEnumerator ReadMetadata()
        {
            if(ros2Node == null)
                ros2Node = ros2Unity.CreateNode("SimsoftVR_ListenerNode");

            while(!ros2Unity.Ok())
            {
                yield return null;
                Debug.Log("Waiting for ROS");
            }

            string_sub = ros2Node.CreateSubscription<string_msg>(controlTypeRosAddress, GetControlType);

            //while (ControlType == null)
            //{
            //    yield return null;
            //    Debug.Log("Waiting for control type ");
            //}

            InvokeOnControlTypeLoaded();

            JointSetup:
            ActiveJointId = -1;
            int_sub = ros2Node.CreateSubscription<int_msg>(activeJointAddress, GetActiveJoint);
            float_sub = ros2Node.CreateSubscription<float_msg>(amplitudeAddress, GetAmplitude);

            foreach (RobotMetadata robot in robotMetaDatas)
            {
                string pointsAddress = robot.GetPointsAddress();
                Debug.Log(string.Format("Robot {0}. Reading points on {1}", robot.Name, pointsAddress));

                // Get number of Joints
                pose_sub_NumberOfJoints = ros2Node.CreateSubscription<pose_msg>(pointsAddress, robot.RetrieveNumberOfJoints);
                while (robot.NumberOfJoints == 0)
                {
                    Debug.Log("Waiting for Number of Joints. Robot "+robotMetaDatas[0].Name);
                    yield return null;
                }

                // Get Joints starting positions
                pose_sub_JointsStartingPoses = ros2Node.CreateSubscription<pose_msg>(pointsAddress, robot.RetrieveJointsStartingPoses);
                while (!robot.JointsStartingPosesLoaded)
                {
                    Debug.Log(string.Format("Waiting for Retrieving Joints Starting Poses for robot {0}", robot.Name));
                    yield return null;
                }
                robot.InvokeOnJointsStartingPosesLoaded();

                yield return null;

                switch (robot.RobotSimulationType)
                {
                    case RobotSimulationType.Flex:
                        // Dictionary starting pose initialization
                        robot.InitLinkNodesStartingPoses();

                        // Get Nodes starting positions for every Link
                        for (int i = 1; i <= robot.NumberOfLinks; i++)
                        {
                            int linkIndex = i;
                            string linkAddress = robot.GetLinkAddress(linkIndex);
                            Debug.Log(string.Format("Robot {0}. Reading on LinkAddress {1}", robot.Name, linkAddress));
                            pose_sub_LinkNodesStartingPoses = ros2Node.CreateSubscription<pose_msg>(linkAddress, (pose_msg) => robot.RetrieveLinkNodesStartingPoses(pose_msg, linkIndex));

                            while (!robot.LinkNodesStartingPosesLoaded[linkIndex - 1])
                            {
                                Debug.Log(string.Format("Waiting for Link Nodes Starting Poses for link {0}", linkIndex - 1));
                                yield return null;
                            }
                        }

                        for (int i = 1; i <= robot.NumberOfLinks; i++)
                        {
                            robot.InvokeOnLinkNodesStartingPosesLoaded(i, robot.LinkNodesStartingPoses[i]);
                        }
                        break;

                    case RobotSimulationType.Rigid:
                        for (int i = 1; i <= robot.NumberOfLinks; i++)
                        {
                            int startingJointId = SimulationManager.GetAssociatedStartJointFromLink(i);
                            Pose startingJointPose = robot.JointsStartingPoses[startingJointId];
                            robot.InvokeOnLinkStartingPoseLoaded(i, startingJointPose);
                        }
                        break;
                }

                yield return null;
            }

            metadataLoaded = true;
        }

        // Read Nodes and Joints current positions
        private IEnumerator ReadCurrentPoses()
        {
            foreach (RobotMetadata robot in robotMetaDatas)
            {
                robot.JointsCurrentPoses = null;

                // Current Joints positions
                string pointsAddress = robot.GetPointsAddress();
                pose_sub_JointsCurrentPoses = ros2Node.CreateSubscription<pose_msg>(pointsAddress, robot.UpdateJointsCurrentPoses);
                yield return null;

                switch (robot.RobotSimulationType)
                {
                    case RobotSimulationType.Flex:
                        // Current Links positions
                        for (int i = 1; i <= robot.NumberOfLinks; i++)
                        {
                            int linkIndex = i;
                            string linkAddress = robot.GetLinkAddress(linkIndex);
                            pose_sub_LinkNodesCurrentPoses = ros2Node.CreateSubscription<pose_msg>(linkAddress, (pose_msg) => robot.UpdateLinkNodesCurrentPoses(pose_msg, linkIndex));
                        }
                        break;
                }
            }
        }

        private void GetControlType(string_msg _controlType)
        {
            ControlType = _controlType.Data;
        }

        private void GetActiveJoint(int_msg _activeJoint)
        {
            // Increase by 1 since joints are indexed from 1 to N
            ActiveJointId = _activeJoint.Data + 1;
        }

        private void GetAmplitude(float_msg amplitudeValue)
        {
            Amplitude = amplitudeValue.Data;
        }

        private void ClearRosNodes()
        {
            Debug.Log("Clearing ROS nodes...");
            ros2Node.RemoveSubscription<pose_msg>(pose_sub_NumberOfJoints);
            ros2Node.RemoveSubscription<pose_msg>(pose_sub_JointsStartingPoses);
            ros2Node.RemoveSubscription<pose_msg>(pose_sub_LinkNodesStartingPoses);
            ros2Node.RemoveSubscription<pose_msg>(pose_sub_JointsCurrentPoses);
            ros2Node.RemoveSubscription<pose_msg>(pose_sub_LinkNodesCurrentPoses);
           // ros2Unity.RemoveNode(ros2Node);
        }
    }
}