// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum RigMainAxis { x, y, z };
public class Rigger : MonoBehaviour
{
    [SerializeField] private RigMainAxis rigMainAxis;
    [SerializeField] private Transform[] bones;

    public Transform[] Bones { get { return bones; } set { bones = value; } }
    public GameObject bonesRoot;

    public void InitBones()
    {
        if (bones.Length == 0)
            MakeRig();
        else
            Bones = bones;
    }

    private void MakeRig()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = GetComponent<MeshRenderer>().materials;
        float modelLenght = GetMainCoordinate(mesh.bounds.extents, rigMainAxis)*2;
        Debug.Log("model lenght " + modelLenght);

        SkinnedMeshRenderer rend = gameObject.AddComponent<SkinnedMeshRenderer>();
        rend.materials = materials;

        int numberOfVertices = mesh.vertices.Length;

        //Setup VertexGroups
        List<VertexGroup> groups = new List<VertexGroup>();
        foreach (Vector3 vertex in mesh.vertices)
        {
            float mainCoordinate = GetMainCoordinate(vertex, rigMainAxis);

            VertexGroup group = VertexGroup.GetVertexGroup(groups, mainCoordinate);
            if (group != null)
                group.AddVertex(vertex);
            else
            {
                VertexGroup newVertexGroup = new VertexGroup(mainCoordinate, rigMainAxis, GetMainCoordinate(transform.localScale, rigMainAxis));
                newVertexGroup.AddVertex(vertex);
                groups.Add(newVertexGroup);
            }
        }

        // Sort group list according to CommonCoordinate value
        groups.Sort();

        // Instantiate a Sphere for every vertex in the mesh. Useful for debug
        for (int i = 0; i < groups.Count; i++)
        {
            Color groupColor = UnityEngine.Random.ColorHSV();
            for (int j = 0; j < groups[i].Vertices.Length; j++)
            {
                GameObject viewer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                viewer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                viewer.transform.parent = transform;
                viewer.transform.localPosition = groups[i].Vertices[j];
                viewer.GetComponent<MeshRenderer>().material.color = groupColor;

                float mainCoordinate = GetMainCoordinate(groups[i].Vertices[j], rigMainAxis);

                viewer.name = string.Format("Vertex_{0}_{1}_{2}", i, j, mainCoordinate);
            }
        }

        List<Transform> bonesTransforms = new List<Transform>();

        for (int i = 0; i < groups.Count; i++)
        {
            GameObject boneGO = new GameObject(string.Format("Bone_{0}", i));
            boneGO.transform.parent = transform;
            boneGO.transform.localPosition = MainCoordinateToVector(groups[i].CommonCoordinate, rigMainAxis);
            bonesTransforms.Add(boneGO.transform.transform);
        }

        Matrix4x4[] bindPoses = new Matrix4x4[bonesTransforms.Count];

        // Assign bone weights to mesh
        BoneWeight[] weights = new BoneWeight[numberOfVertices];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i].boneIndex0 = VertexGroup.GetAssociatedBoneIndex(mesh.vertices[i], bonesTransforms);
            weights[i].weight0 = 1.0f;
        }

        mesh.boneWeights = weights;

        for (int i = 0; i < bonesTransforms.Count; i++)
            bindPoses[i] = bonesTransforms[i].worldToLocalMatrix * transform.localToWorldMatrix;

        // BindPoses was created earlier and was updated with the required matrix.
        // The bindPoses array will now be assigned to the bindposes in the Mesh.
        mesh.bindposes = bindPoses;

        //Assign bones and bind poses
        rend.bones = bonesTransforms.ToArray();
        rend.sharedMesh = mesh;

        rend.updateWhenOffscreen = true; // Render the mesh even if the camera is not pointing to the bounding box

        Bones = bonesTransforms.ToArray();
    }

    private float GetMainCoordinate(Vector3 vertex, RigMainAxis rigMainAxis)
    {
        return GetMainCoordinate(vertex, rigMainAxis, 0.0f);
    }

    private float GetMainCoordinate(Vector3 vertex, RigMainAxis rigMainAxis, float offset)
    {
        switch (rigMainAxis)
        {
            case RigMainAxis.x:
                return vertex.x - offset;
            case RigMainAxis.y:
                return vertex.y - offset;
            case RigMainAxis.z:
                return vertex.z - offset;
            default:
                Debug.LogWarning("Main Coordinate not recognised");
                return 0;
        }
    }

    private Vector3 MainCoordinateToVector(float mainCoordinate, RigMainAxis rigMainAxis)
    {
        switch (rigMainAxis)
        {
            case RigMainAxis.x:
                return new Vector3(mainCoordinate, 0, 0);
            case RigMainAxis.y:
                return new Vector3(0, mainCoordinate, 0);
            case RigMainAxis.z:
                 return new Vector3(0,0,mainCoordinate);
            default:
                Debug.LogWarning("Main Coordinate not recognised");
                return Vector3.zero;
        }
    }
}

public class VertexGroup : IComparable<VertexGroup>
{
    private List<Vector3> vertices;
    public Vector3[] Vertices { get { return vertices.ToArray(); } }

    private RigMainAxis commonCoordinateAxis;
    private float scaleFactor;
    public float CommonCoordinate { get; private set; }

    private const float CommonCoordinateTolerance = 0.01f;
    private static float commonCoordinateToleranceScaled;

    public VertexGroup(float commonCoordinate, RigMainAxis commonCoordinateAxis, float scaleFactor)
    {
        CommonCoordinate = commonCoordinate;
        this.commonCoordinateAxis = commonCoordinateAxis;
        this.scaleFactor = scaleFactor;
        commonCoordinateToleranceScaled = CommonCoordinateTolerance / scaleFactor;
        vertices = new List<Vector3>();
    }

    public void AddVertex(Vector3 vertexToAdd)
    {
        float mainCoordinate = 0;
        switch (commonCoordinateAxis)
        {
            case RigMainAxis.x:
                mainCoordinate = vertexToAdd.x;
                break;
            case RigMainAxis.y:
                mainCoordinate = vertexToAdd.y;
                break;
            case RigMainAxis.z:
                mainCoordinate = vertexToAdd.z;
                break;
        }

        if ((Mathf.Abs(mainCoordinate - CommonCoordinate) > commonCoordinateToleranceScaled))
            Debug.LogWarning(string.Format("Si sta tentando di aggiungere un vertice con coordinata errata al VertexGroup con coordinata {0}. Coordinate vertice: {1}", CommonCoordinate, mainCoordinate));

        vertices.Add(vertexToAdd);
    }

    public void TryToAddVertices(Vector3[] verticesToAdd, RigMainAxis axis)
    {
        bool coordinateMatched;
        foreach (Vector3 vertex in verticesToAdd)
        {
            coordinateMatched = false;
            switch (commonCoordinateAxis)
            {
                case RigMainAxis.x:
                    if (Mathf.Abs(vertex.x - CommonCoordinate) < commonCoordinateToleranceScaled)
                        coordinateMatched = true;
                    break;
                case RigMainAxis.y:
                    if (Mathf.Abs(vertex.y - CommonCoordinate) < commonCoordinateToleranceScaled)
                        coordinateMatched = true;
                    break;
                case RigMainAxis.z:
                    if (Mathf.Abs(vertex.z - CommonCoordinate) < commonCoordinateToleranceScaled)
                        coordinateMatched = true;
                    break;
            }

            if (coordinateMatched)
                vertices.Add(vertex);
        }
    }

    public static bool VertexGroupExists(IEnumerable<VertexGroup> groups, float commonCoordinate)
    {
        foreach (VertexGroup group in groups)
            if (Mathf.Abs(group.CommonCoordinate - commonCoordinate) < commonCoordinateToleranceScaled)
                return true;

        return false;
    }

    public static VertexGroup GetVertexGroup(IEnumerable<VertexGroup> groups, float commonCoordinate)
    {
        foreach (VertexGroup group in groups)
            if (Mathf.Abs(group.CommonCoordinate - commonCoordinate) < commonCoordinateToleranceScaled)
                return group;
        return null;
    }

    public static int GetAssociatedBoneIndex(Vector3 vertex, IEnumerable<Transform> boneTransforms)
    {
        float distance;
        float minimumDistance = Mathf.Infinity;
        int nearestBoneIndex = 0;
        Transform[] boneTransformsArray = boneTransforms.ToArray();
        for (int i = 0; i < boneTransformsArray.Length; i++)
        {
            distance = Vector3.Magnitude(vertex - boneTransformsArray[i].localPosition);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                nearestBoneIndex = i;
            }
        }
        return nearestBoneIndex;
    }

    public int CompareTo(VertexGroup group)
    {
        return this.CommonCoordinate.CompareTo(group.CommonCoordinate);
    }
}
