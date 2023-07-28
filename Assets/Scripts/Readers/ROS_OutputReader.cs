using System;
using UnityEngine;

namespace SimsoftVR.Readers
{
    public abstract class ROS_OutputReader : OutputReader
    {
        private static ROS_OutputReader instance = null;

        [Header("Simulation Metadata")] // Just for "hardcode" initialization
        [SerializeField] protected RobotMetadata[] robotMetaDatas;
        public RobotMetadata[] RobotMetadatas { get { return robotMetaDatas; } }

        //Flag to notify if all metadatas have been loaded (n of links, joints, starting positions)
        protected bool metadataLoaded = false;

        [SerializeField] protected string controlTypeRosAddress = "/control_type";
        [SerializeField] protected string activeJointAddress = "/selected_joint";
        [SerializeField] protected string amplitudeAddress = "/amplitude_norm";

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        /// <summary>
        /// Use this method to convert from Pose (ROS Bridge) to Pose (Unity)
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public static Pose GetPose(RosSharp.RosBridgeClient.MessageTypes.Geometry.Pose pose)
        {
            float posX = Convert.ToSingle(pose.position.z);
            float posY = Convert.ToSingle(pose.position.y);
            float posZ = -Convert.ToSingle(pose.position.x);
            float rotW = Convert.ToSingle(pose.orientation.w);
            float rotX = Convert.ToSingle(pose.orientation.z);
            float rotY = Convert.ToSingle(pose.orientation.y);
            float rotZ = -Convert.ToSingle(pose.orientation.x);
            return new Pose(new Vector3(posX, posY, posZ), new Quaternion(rotX, rotY, rotZ, rotW));
        }

        /// <summary>
        /// Use this method to convert from Pose (ROS for Unity) to Pose (Unity)
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public static Pose GetPose(geometry_msgs.msg.Pose pose)
        {
            float posX = Convert.ToSingle(pose.Position.Z);
            float posY = Convert.ToSingle(pose.Position.Y);
            float posZ = -Convert.ToSingle(pose.Position.X);
            float rotW = Convert.ToSingle(pose.Orientation.W);
            float rotX = Convert.ToSingle(pose.Orientation.Z);
            float rotY = Convert.ToSingle(pose.Orientation.Y);
            float rotZ = -Convert.ToSingle(pose.Orientation.X);
            return new Pose(new Vector3(posX, posY, posZ), new Quaternion(rotX, rotY, rotZ, rotW));
        }

        public static RobotMetadata GetRobot(int id)
        {
            RobotMetadata[] robots = (instance as ROS_OutputReader).robotMetaDatas;
            foreach (RobotMetadata robot in robots)
                if (robot.Id == id)
                    return robot;
            return null;
        }
    }
}
