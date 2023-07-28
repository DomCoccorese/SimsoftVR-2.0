// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;

namespace SimsoftVR.Entities
{
    public class Joint : MonoBehaviour
    {
        public int Id { get; private set; }
        public int RobotId { get; private set; }

        public void Init(int jointId, int robotId)
        {
            Id = jointId;
            RobotId = robotId;
            gameObject.name = string.Format("Robot_{0}_Joint_{1}", RobotId, Id);
        }
    }
}