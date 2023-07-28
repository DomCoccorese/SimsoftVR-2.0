// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SimsoftVR.Entities
{
    public class Node : MonoBehaviour
    {
        public int NodeId { get; private set; }
        public Link ParentLink { get; private set; }

        public void SetupNode(int id, Link parent)
        {
            NodeId = id;
            ParentLink = parent;

            gameObject.name = string.Format("Link_{0}_Node_{1}", parent.Id, NodeId);
            ParentLink.AddNode(this);
        }
    }
}