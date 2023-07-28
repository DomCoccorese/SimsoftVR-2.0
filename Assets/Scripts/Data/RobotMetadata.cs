using System;
using System.Collections.Generic;
using UnityEngine;
using ROS2;
//using pose_msg = RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray;
//using int_msg = RosSharp.RosBridgeClient.MessageTypes.Std.Int32;

namespace SimsoftVR.Readers
{
    public enum RobotSimulationType { Flex, Rigid }

    [System.Serializable]
    public class RobotMetadata
    {
        [SerializeField] private string name;
        public string Name { get { return name; } }

        [SerializeField] private int id;
        public int Id { get { return id; } }

        [SerializeField] private RobotSimulationType robotSimulationType;
        public RobotSimulationType RobotSimulationType { get { return robotSimulationType; } }

        [SerializeField] private string rosNodeAddress;
        public string RosNodeAddress { get { return rosNodeAddress; } }

        [SerializeField] private int numberOfLinks;
        public int NumberOfLinks { get { return numberOfLinks; } }

        public int NumberOfJoints { get; private set; }

        public Pose[] JointsStartingPoses { get; private set; }

        public Dictionary<int, Pose[]> LinkNodesStartingPoses { get; set; }

        public event Action<RobotMetadata, Pose[]> OnJointsStartingPosesLoaded; // This event is called AFTER Joints starting positions loading
        public event Action<RobotMetadata, int, Pose> OnLinkStartingPoseLoaded;
        public event Action<RobotMetadata, int, Pose[]> OnLinkNodesStartingPosesLoaded;    // This event is called AFTER all links nodes starting positions loading 

        public Pose[] JointsCurrentPoses { get; set; }   // Joints current positions
        public Pose[] LinksCurrentPoses { get; set; }   // Rigid links current positions. Genereally they correspond to Joints positions
        public Dictionary<int, Pose[]> LinkNodesCurrentPoses { get; set; }   //Current Nodes positions for ALL the links. The key corresponds to Node Index

        private bool jointsStartingPosesLoaded = false;
        public bool JointsStartingPosesLoaded
        {
            get { return jointsStartingPosesLoaded; }
        }

        private bool[] linkNodesStartingPosesLoaded;
        public bool[] LinkNodesStartingPosesLoaded { get { return linkNodesStartingPosesLoaded; } }

        private bool[] linkDictionaryElementReady = null;
        public bool[] LinkDictionaryElementReady
        {
            get
            {
                //The array is initialized by setting all values to false
                if (linkDictionaryElementReady == null)
                {
                    linkDictionaryElementReady = new bool[NumberOfLinks];
                    for (int i = 0; i < LinkDictionaryElementReady.Length; i++)
                        linkDictionaryElementReady[i] = false;
                }
                return linkDictionaryElementReady;
            }
        }
        private List<GameObject> jointGOs = new List<GameObject>();
        public List<GameObject> JointsGOs
        {
            get { return jointGOs; }
            private set { jointGOs = value; }
        }

        private List<GameObject> linkGOs = new List<GameObject>();
        public List<GameObject> LinksGOs
        {
            get { return linkGOs; }
            private set { linkGOs = value; }
        }

        // Flag to notify if the Link dictionary has been initialized 
        public bool LinkDictionaryReady
        {
            get
            {
                if (linkDictionaryElementReady == null)
                    return false;

                foreach (bool element in linkDictionaryElementReady)
                    if (!element)
                        return false;
                return true;
            }
        }

        public void RetrieveNumberOfJoints(RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray _message)
        {
            NumberOfJoints = _message.poses.Length;
            Debug.Log(string.Format("Number of Joints for robot {0}: {1} ", Name, NumberOfJoints));
        }

        public void RetrieveNumberOfJoints(geometry_msgs.msg.PoseArray _message)
        {
            NumberOfJoints = _message.Poses.Length;
            Debug.Log(string.Format("Number of Joints for robot {0}: {1} ", Name, NumberOfJoints));
        }

        public void RetrieveJointsStartingPoses(RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray _message)
        {
            Debug.Log(string.Format("Retrieving Joints starting poses for robot {0}", Name));
            if (!JointsStartingPosesLoaded)
            {
                JointsStartingPoses = new Pose[NumberOfJoints];
                for (int i = 0; i < NumberOfJoints; i++)
                {
                    JointsStartingPoses[i] = ROS_OutputReader.GetPose(_message.poses[i]);
                    Debug.Log("retrieving joint starting pose: " + JointsStartingPoses[i]);
                }
                jointsStartingPosesLoaded = true;
            }
        }

        public void RetrieveJointsStartingPoses(geometry_msgs.msg.PoseArray _message)
        {
            Debug.Log(string.Format("Retrieving Joints starting poses for robot {0}", Name));
            if (!JointsStartingPosesLoaded)
            {
                JointsStartingPoses = new Pose[NumberOfJoints];
                for (int i = 0; i < NumberOfJoints; i++)
                {
                    JointsStartingPoses[i] = ROS_OutputReader.GetPose(_message.Poses[i]);
                    Debug.Log("retrieving joint starting pose: " + JointsStartingPoses[i]);
                }
                jointsStartingPosesLoaded = true;
            }
        }

        public void RetrieveNumberOfLinks(RosSharp.RosBridgeClient.MessageTypes.Std.Int32 _message)
        {
            numberOfLinks = _message.data;
            Debug.Log(string.Format("Number of Links for robot {0}: {1}", Name, numberOfLinks));
        }

        public void RetrieveLinkNodesStartingPoses(RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray _message, int linkIndex)
        {
            try
            {
                Debug.Log(string.Format("Robot {0}. Reading link {1}", Name, linkIndex));
                if (!LinkNodesStartingPosesLoaded[linkIndex - 1])
                {
                    Pose[] poses = new Pose[_message.poses.Length];
                    for (int i = 0; i < _message.poses.Length; i++)
                    {
                        poses[i] = ROS_OutputReader.GetPose(_message.poses[i]);
                        Debug.Log(string.Format("Robot {0}. Retrieving link {1} starting poses with pose {2}", Name, linkIndex, poses[i]));
                    }

                    LinkNodesStartingPoses.Add(linkIndex, poses);
                    LinkNodesStartingPosesLoaded[linkIndex - 1] = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Robot {0}. Error during retrieving link nodes: {1}", Name, e.Message));
            }
        }

        public void RetrieveLinkNodesStartingPoses(geometry_msgs.msg.PoseArray _message, int linkIndex)
        {
            try
            {
                Debug.Log(string.Format("Robot {0}. Reading link {1}", Name, linkIndex));
                if (!LinkNodesStartingPosesLoaded[linkIndex - 1])
                {
                    Pose[] poses = new Pose[_message.Poses.Length];
                    for (int i = 0; i < _message.Poses.Length; i++)
                    {
                        poses[i] = ROS_OutputReader.GetPose(_message.Poses[i]);
                        Debug.Log(string.Format("Robot {0}. Retrieving link {1} starting poses with pose {2}", Name, linkIndex, poses[i]));
                    }

                    LinkNodesStartingPoses.Add(linkIndex, poses);
                    LinkNodesStartingPosesLoaded[linkIndex - 1] = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Robot {0}. Error during retrieving link nodes: {1}", Name, e.Message));
            }
        }

        public void UpdateJointsCurrentPoses(RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray _message)
        {
            if (JointsCurrentPoses == null)
                JointsCurrentPoses = new Pose[NumberOfJoints];

            for (int i = 0; i < NumberOfJoints; i++)
            {
                JointsCurrentPoses[i] = ROS_OutputReader.GetPose(_message.poses[i]);
            }
        }

        public void UpdateJointsCurrentPoses(geometry_msgs.msg.PoseArray _message)
        {
            if (JointsCurrentPoses == null)
                JointsCurrentPoses = new Pose[NumberOfJoints];

            for (int i = 0; i < NumberOfJoints; i++)
            {
                JointsCurrentPoses[i] = ROS_OutputReader.GetPose(_message.Poses[i]);
            }
        }

        public void UpdateLinkNodesCurrentPoses(RosSharp.RosBridgeClient.MessageTypes.Geometry.PoseArray _message, int currentReadLinkIndex)
        {
            if (LinkNodesCurrentPoses == null)
                LinkNodesCurrentPoses = new Dictionary<int, Pose[]>();

            Pose[] poses = new Pose[_message.poses.Length];
            for (int i = 0; i < _message.poses.Length; i++)
            {
                poses[i] = ROS_OutputReader.GetPose(_message.poses[i]);
            }

            if (LinkNodesCurrentPoses.ContainsKey(currentReadLinkIndex))
            {
                Debug.Log("overwriting key " + currentReadLinkIndex);
                LinkNodesCurrentPoses[currentReadLinkIndex] = poses;
            }
            else
            {
                Debug.Log("adding key " + currentReadLinkIndex);
                LinkNodesCurrentPoses.Add(currentReadLinkIndex, poses);
            }

            LinkDictionaryElementReady[currentReadLinkIndex - 1] = true;
        }

        public void UpdateLinkNodesCurrentPoses(geometry_msgs.msg.PoseArray _message, int currentReadLinkIndex)
        {
            if (LinkNodesCurrentPoses == null)
                LinkNodesCurrentPoses = new Dictionary<int, Pose[]>();

            Pose[] poses = new Pose[_message.Poses.Length];
            for (int i = 0; i < _message.Poses.Length; i++)
            {
                poses[i] = ROS_OutputReader.GetPose(_message.Poses[i]);
            }

            if (LinkNodesCurrentPoses.ContainsKey(currentReadLinkIndex))
            {
                Debug.Log("overwriting key " + currentReadLinkIndex);
                LinkNodesCurrentPoses[currentReadLinkIndex] = poses;
            }
            else
            {
                Debug.Log("adding key " + currentReadLinkIndex);
                LinkNodesCurrentPoses.Add(currentReadLinkIndex, poses);
            }

            LinkDictionaryElementReady[currentReadLinkIndex - 1] = true;
        }

        public void InvokeOnJointsStartingPosesLoaded()
        {
            OnJointsStartingPosesLoaded.Invoke(this, JointsStartingPoses);
        }

        public void InvokeOnLinkStartingPoseLoaded(int linkIndex, Pose joint1)
        {
            OnLinkStartingPoseLoaded.Invoke(this, linkIndex, joint1);
        }

        public void InvokeOnLinkNodesStartingPosesLoaded(int linkIndex, Pose[] linkNodesStartingPoses)
        {
            OnLinkNodesStartingPosesLoaded.Invoke(this, linkIndex, linkNodesStartingPoses);
        }

        public void InitLinkNodesStartingPoses()
        {
            linkNodesStartingPosesLoaded = new bool[NumberOfLinks];
            for (int i = 0; i < LinkNodesStartingPosesLoaded.Length; i++)
                linkNodesStartingPosesLoaded[i] = false;

            LinkNodesStartingPoses = new Dictionary<int, Pose[]>();
        }

        /// <summary>
        /// ROS address for /points
        /// </summary>
        /// <returns></returns>
        public string GetPointsAddress()
        {
            return string.Concat(RosNodeAddress, "/points");  //Costruisco l'indirizzo ROS da cui leggere i punti. Non uso Path.Combine perchè questo si ostina a utilizzare '\' al posto di '/'
        }

        /// <summary>
        /// ROS addess for /links
        /// </summary>
        /// <returns></returns>
        public string GetLinkAddress(int linkIndex)
        {
            return string.Concat(RosNodeAddress, string.Concat("/L", linkIndex));     //Costruisco l'indirizzo ROS da cui leggere i link        }
        }

        public GameObject GetJointGO(int id)
        {
            foreach (GameObject jointGO in jointGOs)
            {
                if (jointGO.GetComponent<Entities.Joint>().Id == id && jointGO.GetComponent<Entities.Joint>().RobotId == this.id)
                    return jointGO;
            }
            return null;
        }

        public void AddJointGO(GameObject jointGO)
        {
            JointsGOs.Add(jointGO);
        }

        public void AddLinkGO(GameObject linkGO)
        {
            LinksGOs.Add(linkGO);
        }

        public GameObject GetLinkGO(int id)
        {
            foreach (GameObject linkGO in linkGOs)
            {
                if (linkGO.GetComponent<Entities.Link>().Id == id && linkGO.GetComponent<Entities.Link>().RobotId == this.id)
                    return linkGO;
            }
            return null;
        }
    }
}