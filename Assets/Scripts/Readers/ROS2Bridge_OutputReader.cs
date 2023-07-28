// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System;
using System.Collections;
using UnityEngine;
using RosSharp.RosBridgeClient;
using string_msg = RosSharp.RosBridgeClient.MessageTypes.Std.String;
using pose_msg = RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray;
using int_msg = RosSharp.RosBridgeClient.MessageTypes.Std.Int32;
using float_msg = RosSharp.RosBridgeClient.MessageTypes.Std.Float64;

namespace SimsoftVR.Readers
{
    public class ROS2Bridge_OutputReader : ROS_OutputReader
    {
        private static ROS2Bridge_OutputReader instance = null;

        [SerializeField] private string ws_uri;

        private RosSocket rosSocket;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        public override void ReadOutput()
        {
            StartCoroutine(ReadData());
        }

        private IEnumerator ReadData()
        {
            yield return StartCoroutine(ReadMetadata());
            while (!metadataLoaded)
                yield return null;

            yield return StartCoroutine(ReadCurrentPoses());
        }

        private void GetControlType(string_msg _controlType)
        {
            ControlType = _controlType.data;
        }

        private void GetActiveJoint(int_msg _activeJoint)
        {
            // Increase by 1 since joints are indexed from 1 to N
            ActiveJointId = _activeJoint.data + 1;
        }

        private void GetAmplitude(float_msg amplitudeValue)
        {
            Amplitude = amplitudeValue.data;
        }

        // Get all metadatas from ROS Bridge
        private IEnumerator ReadMetadata()
        {
            if (!Uri.IsWellFormedUriString(ws_uri, UriKind.RelativeOrAbsolute))
            {
                Debug.LogWarning("URL to ROS workspace is not valid");
                StopCoroutine(ReadMetadata());
            }

            // Connect to ROS node
            rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(ws_uri)); //this line does the same as the script RosConnector
            Debug.Log("Established connection with ros");

            rosSocket.Subscribe<string_msg>(controlTypeRosAddress, GetControlType);
            while (ControlType == null)
            {
                yield return null;
                Debug.Log("Waiting for control type");
            }
            InvokeOnControlTypeLoaded();

            ActiveJointId = -1;
            rosSocket.Subscribe<int_msg>(activeJointAddress, GetActiveJoint);
            rosSocket.Subscribe<float_msg>(amplitudeAddress, GetAmplitude);

            foreach (RobotMetadata robot in robotMetaDatas)
            {
                string pointsAddress = robot.GetPointsAddress();
                Debug.Log(string.Format("Robot {0}. Reading points on {1}", robot.Name, pointsAddress));

                // Get number of Joints
                rosSocket.Subscribe<pose_msg>(pointsAddress, robot.RetrieveNumberOfJoints);
                while (robot.NumberOfJoints == 0)
                {
                    Debug.Log("Waiting for Number of Joints");
                    yield return null;
                }

                // Get Joints starting positions
                rosSocket.Subscribe<pose_msg>(pointsAddress, robot.RetrieveJointsStartingPoses);
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
                            rosSocket.Subscribe<pose_msg>(linkAddress, (pose_msg) => robot.RetrieveLinkNodesStartingPoses(pose_msg, linkIndex));

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
                rosSocket.Subscribe<pose_msg>(pointsAddress, robot.UpdateJointsCurrentPoses);
                yield return null;

                switch (robot.RobotSimulationType)
                {
                    case RobotSimulationType.Flex:
                        // Current Links positions
                        for (int i = 1; i <= robot.NumberOfLinks; i++)
                        {
                            int linkIndex = i;
                            string linkAddress = robot.GetLinkAddress(linkIndex);
                            rosSocket.Subscribe<pose_msg>(linkAddress, (pose_msg) => robot.UpdateLinkNodesCurrentPoses(pose_msg, linkIndex));
                        }
                        break;
                }
            }
        }
    }
}