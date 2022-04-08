using System.Collections.Generic;
using PathCreation.Utility;
using Unity.Jobs;
using UnityEngine.Jobs;
using UnityEngine;
using Unity.Burst;
using PathCreation;
using Unity.Mathematics;
using Unity.Collections;

public struct RoadJob : IJob
{
    //basic
    public float roadWidthJ;
    public float thicknessJ;
    public bool flattenSurfaceJ;
    public float textureTilingJ;

    //mesh
    public NativeArray<float3> vertsJ;
    public NativeArray<float2> uvsJ;
    public NativeArray<float3> normalsJ;
    public NativeArray<int> roadTrianglesJ;
    public NativeArray<int> underRoadTrianglesJ;
    public NativeArray<int> sideOfRoadTrianglesJ;

    //path
    //PathCreation.VertexPath path;
    public PathSpace spaceJ;
    public int NumPointsJ;
    public bool isClosedLoopJ;
    public float3 upJ;
    public NativeArray<float> timesJ;
    public NativeArray<float3> GetPointJ;
    public NativeArray<float3> GetTangentJ;
    public NativeArray<float3> GetNormalJ;

    public void Execute()
    {
        int vertIndex = 0;
        int triIndex = 0;

        // Vertices for the top of the road are layed out:
        // 0  1
        // 8  9
        // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        bool usePathNormals = !(spaceJ == PathSpace.xyz && flattenSurfaceJ);

        for (int i = 0; i < NumPointsJ; i++)
        {
            float3 tangent = GetTangentJ[i];
            float3 normal = GetNormalJ[i];
            float3 localUp = (usePathNormals) ? new float3(tangent[1] * normal[2] - tangent[2] * normal[1], tangent[2] * normal[0] - tangent[0] * normal[2], tangent[0] * normal[1] - tangent[1] * normal[0]) : upJ;

            float3 localRight = (usePathNormals) ? normal : new float3(localUp[1] * tangent[2] - localUp[2] * tangent[1], localUp[2] * tangent[0] - localUp[0] * tangent[2], localUp[0] * tangent[1] - localUp[1] * tangent[0]);

            // Find position to left and right of current path vertex
            float3 vertSideA = GetPointJ[i] - localRight * Mathf.Abs(roadWidthJ);
            float3 vertSideB = GetPointJ[i] + localRight * Mathf.Abs(roadWidthJ);

            // Add top of road vertices
            vertsJ[vertIndex + 0] = vertSideA;
            vertsJ[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            vertsJ[vertIndex + 2] = vertSideA - localUp * thicknessJ;
            vertsJ[vertIndex + 3] = vertSideB - localUp * thicknessJ;

            // Duplicate vertices to get flat shading for sides of road
            vertsJ[vertIndex + 4] = vertsJ[vertIndex + 0];
            vertsJ[vertIndex + 5] = vertsJ[vertIndex + 1];
            vertsJ[vertIndex + 6] = vertsJ[vertIndex + 2];
            vertsJ[vertIndex + 7] = vertsJ[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvsJ[vertIndex + 0] = new Vector2(0, timesJ[i]);
            uvsJ[vertIndex + 1] = new Vector2(1, timesJ[i]);

            // Top of road normals
            normalsJ[vertIndex + 0] = localUp;
            normalsJ[vertIndex + 1] = localUp;
            // Bottom of road normals
            normalsJ[vertIndex + 2] = -localUp;
            normalsJ[vertIndex + 3] = -localUp;
            // Sides of road normals
            normalsJ[vertIndex + 4] = -localRight;
            normalsJ[vertIndex + 5] = localRight;
            normalsJ[vertIndex + 6] = -localRight;
            normalsJ[vertIndex + 7] = localRight;

            // Set triangle indices
            if (i < NumPointsJ - 1 || isClosedLoopJ)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    roadTrianglesJ[triIndex + j] = (vertIndex + triangleMap[j]) % vertsJ.Length;
                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTrianglesJ[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % vertsJ.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                {
                    sideOfRoadTrianglesJ[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % vertsJ.Length;
                }

            }

            vertIndex += 8;
            triIndex += 6;
        }
    }
}

namespace PathCreation.Examples
{
    public class RoadMeshCreatorJob : PathSceneTool
    {
        [Header("Road settings")]
        public float roadWidth = .4f;
        [Range(0, .5f)]
        public float thickness = .15f;
        public bool flattenSurface;

        [Header("Material settings")]
        public Material roadMaterial;
        public Material undersideMaterial;
        public float textureTiling = 1;

        //[SerializeField, HideInInspector]
        public GameObject meshHolder;

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Mesh mesh;

        public override void PathUpdated()
        {
            if (pathCreator != null)
            {
                AssignMeshComponents();
                AssignMaterials();
                CreateRoadMesh();
            }
        }

        public void CreateRoadMesh()
        {
            NativeArray<float3> verts = new NativeArray<float3>(path.NumPoints * 8, Allocator.TempJob);
            NativeArray<float2> uvs = new NativeArray<float2>(verts.Length, Allocator.TempJob);
            NativeArray<float3> normals = new NativeArray<float3>(verts.Length, Allocator.TempJob);
            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            NativeArray<int> roadTriangles = new NativeArray<int>(numTris * 3, Allocator.TempJob);
            NativeArray<int> underRoadTriangles = new NativeArray<int>(numTris * 3, Allocator.TempJob);
            NativeArray<int> sideOfRoadTriangles = new NativeArray<int>(numTris * 2 * 3, Allocator.TempJob);

            //path
            NativeArray<float3> GetPointArray = new NativeArray<float3>(path.NumPoints, Allocator.TempJob);
            NativeArray<float3> GetTangentArray = new NativeArray<float3>(path.NumPoints, Allocator.TempJob);
            NativeArray<float3> GetNormalArray = new NativeArray<float3>(path.NumPoints, Allocator.TempJob);

            for (int id = 0; id < verts.Length; id++)
            {
                uvs[id] = float2.zero;
                normals[id] = float3.zero;
            }

            for (int id = 0; id < numTris * 3; id++)
            {
                roadTriangles[id] = 0;
                underRoadTriangles[id] = 0;
                sideOfRoadTriangles[id * 2] = 0;
                sideOfRoadTriangles[id * 2 + 1] = 0;
            }

            for (int id = 0; id < path.NumPoints; id++)
            {
                verts[id * 8] = float3.zero;
                verts[id * 8 + 1] = float3.zero;
                verts[id * 8 + 2] = float3.zero;
                verts[id * 8 + 3] = float3.zero;
                verts[id * 8 + 4] = float3.zero;
                verts[id * 8 + 5] = float3.zero;
                verts[id * 8 + 6] = float3.zero;
                verts[id * 8 + 7] = float3.zero;
                GetPointArray[id] = path.GetPoint(id);
                GetTangentArray[id] = path.GetTangent(id);
                GetNormalArray[id] = path.GetNormal(id);
            }

            RoadJob roadJob = new RoadJob
            {
                //basic
                roadWidthJ = roadWidth,
                thicknessJ = thickness,
                flattenSurfaceJ = flattenSurface,
                textureTilingJ = textureTiling,

                //mesh
                vertsJ = verts,
                uvsJ = uvs,
                normalsJ = normals,
                roadTrianglesJ = roadTriangles,
                underRoadTrianglesJ = underRoadTriangles,
                sideOfRoadTrianglesJ = sideOfRoadTriangles,

                //path
                spaceJ = path.space,
                NumPointsJ = path.NumPoints,
                isClosedLoopJ = path.isClosedLoop,
                upJ = path.up,
                timesJ = new NativeArray<float>(path.times, Allocator.TempJob),
                GetPointJ = new NativeArray<float3>(GetPointArray, Allocator.TempJob),
                GetTangentJ = new NativeArray<float3>(GetTangentArray, Allocator.TempJob),
                GetNormalJ = new NativeArray<float3>(GetNormalArray, Allocator.TempJob)
            };

            //JobHandle jobHandle = RoadJobTask();
            //GetComponent<RoadInfo>().addJob(jobHandle);
            roadJob.Schedule().Complete();

            mesh.Clear();
            mesh.vertices = new Vector3[path.NumPoints * 8];
            mesh.vertices = verts.Reinterpret<Vector3>().ToArray();
            mesh.uv = new Vector2[verts.Length];
            mesh.uv = uvs.Reinterpret<Vector2>().ToArray();
            mesh.normals = new Vector3[verts.Length];
            mesh.normals = normals.Reinterpret<Vector3>().ToArray();
            mesh.subMeshCount = 3;
            mesh.SetTriangles(roadTriangles.ToArray(), 0);
            mesh.SetTriangles(underRoadTriangles.ToArray(), 1);
            mesh.SetTriangles(sideOfRoadTriangles.ToArray(), 2);
            mesh.RecalculateBounds();

            verts.Dispose();
            uvs.Dispose();
            normals.Dispose();
            roadTriangles.Dispose();
            underRoadTriangles.Dispose();
            sideOfRoadTriangles.Dispose();
            GetPointArray.Dispose();
            GetTangentArray.Dispose();
            GetNormalArray.Dispose();

            roadJob.timesJ.Dispose();
            roadJob.GetPointJ.Dispose();
            roadJob.GetTangentJ.Dispose();
            roadJob.GetNormalJ.Dispose();

            /*
            Vector3[] verts = new Vector3[path.NumPoints * 8];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            int[] roadTriangles = new int[numTris * 3];
            int[] underRoadTriangles = new int[numTris * 3];
            int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

            int vertIndex = 0;
            int triIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
            int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
            int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

            for (int i = 0; i < path.NumPoints; i++)
            {
                Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
                Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

                // Find position to left and right of current path vertex
                Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
                Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;
                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 4] = verts[vertIndex + 0];
                verts[vertIndex + 5] = verts[vertIndex + 1];
                verts[vertIndex + 6] = verts[vertIndex + 2];
                verts[vertIndex + 7] = verts[vertIndex + 3];

                // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
                uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
                uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;
                // Bottom of road normals
                normals[vertIndex + 2] = -localUp;
                normals[vertIndex + 3] = -localUp;
                // Sides of road normals
                normals[vertIndex + 4] = -localRight;
                normals[vertIndex + 5] = localRight;
                normals[vertIndex + 6] = -localRight;
                normals[vertIndex + 7] = localRight;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop)
                {
                    for (int j = 0; j < triangleMap.Length; j++)
                    {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++)
                    {
                        sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                    }

                }

                vertIndex += 8;
                triIndex += 6;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.subMeshCount = 3;
            mesh.SetTriangles(roadTriangles, 0);
            mesh.SetTriangles(underRoadTriangles, 1);
            mesh.SetTriangles(sideOfRoadTriangles, 2);
            mesh.RecalculateBounds();
            */
        }

        // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
        void AssignMeshComponents()
        {

            if (meshHolder == null)
            {
                //meshHolder = new GameObject ("Road Mesh Holder");
                meshHolder = this.gameObject;
            }

            meshHolder.transform.rotation = Quaternion.identity;
            meshHolder.transform.position = Vector3.zero;
            meshHolder.transform.localScale = Vector3.one;

            // Ensure mesh renderer and filter components are assigned
            if (!meshHolder.gameObject.GetComponent<MeshFilter>())
            {
                meshHolder.gameObject.AddComponent<MeshFilter>();
            }
            if (!meshHolder.GetComponent<MeshRenderer>())
            {
                meshHolder.gameObject.AddComponent<MeshRenderer>();
            }

            meshRenderer = meshHolder.GetComponent<MeshRenderer>();
            meshFilter = meshHolder.GetComponent<MeshFilter>();
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            meshFilter.sharedMesh = mesh;
        }

        void AssignMaterials()
        {
            if (roadMaterial != null && undersideMaterial != null)
            {
                meshRenderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial };
                meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3(1, textureTiling);
            }
        }

    }
}