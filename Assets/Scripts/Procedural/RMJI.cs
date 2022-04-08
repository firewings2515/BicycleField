using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public struct RMJob : IJobParallelFor
{
    public NativeArray<NativeArray<float3>> verts;
    public NativeArray<NativeArray<float2>> uvs;
    public NativeArray<NativeArray<float3>> normals;

    public NativeArray<NativeArray<int>> roadTriangles;
    public NativeArray<NativeArray<int>> underRoadTriangles;
    public NativeArray<NativeArray<int>> sideOfRoadTriangles;

    public NativeArray<PathSpace> spaces;

    public NativeArray<bool> flattenSurfaces;
    public NativeArray<float> roadWidths;
    public NativeArray<float> thicknesses;

    public NativeArray<int> NumPoints;
    public NativeArray<bool> isClosedLoop;

    public NativeArray<NativeArray<float>> times;
    public NativeArray<float3> up;
    public NativeArray<NativeArray<float3>> point;
    public NativeArray<NativeArray<float3>> tangent;
    public NativeArray<NativeArray<float3>> normal;

    public void Execute(int index)
    {
        int vertIndex = 0;
        int triIndex = 0;

        // Vertices for the top of the road are layed out:
        // 0  1
        // 8  9
        // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        bool usePathNormals = !(spaces[index] == PathSpace.xyz && flattenSurfaces[index]);

        for (int i = 0; i < NumPoints[index]; i++)
        {
            NativeArray<float3> p = point[index];
            NativeArray<float3> t = tangent[index];
            NativeArray<float3> n = normal[index];

            float3 localUp = (usePathNormals) ? new float3(t[i][1] * n[i][2] - t[i][2] * n[i][1], t[i][2] * n[i][0] - t[i][0] * n[i][2], t[i][0] * n[i][1] - t[i][1] * n[i][0]) : up[index];

            float3 localRight = (usePathNormals) ? n[i] : new float3(localUp[1] * t[i][2] - localUp[2] * t[i][1], localUp[2] * t[i][0] - localUp[0] * t[i][2], localUp[0] * t[i][1] - localUp[1] * t[i][0]);

            //float3 localUp = (usePathNormals) ? Vector3.Cross(paths[index].GetTangent(i), paths[index].GetNormal(i)) : paths[index].up;
            //float3 localRight = (usePathNormals) ? paths[index].GetNormal(i) : Vector3.Cross(localUp, paths[index].GetTangent(i));

            // Find position to left and right of current path vertex
            float3 vertSideA = p[i] - localRight * Mathf.Abs(roadWidths[index]);
            float3 vertSideB = p[i] + localRight * Mathf.Abs(roadWidths[index]);

            NativeArray<float3> float3s = verts[index];
            // Add top of road vertices
            float3s[vertIndex + 0] = vertSideA;
            float3s[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            float3s[vertIndex + 2] = vertSideA - localUp * thicknesses[index];
            float3s[vertIndex + 3] = vertSideB - localUp * thicknesses[index];

            // Duplicate vertices to get flat shading for sides of road
            float3s[vertIndex + 4] = float3s[vertIndex + 0];
            float3s[vertIndex + 5] = float3s[vertIndex + 1];
            float3s[vertIndex + 6] = float3s[vertIndex + 2];
            float3s[vertIndex + 7] = float3s[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            NativeArray<float2> float2s = uvs[index];

            float2s[vertIndex + 0] = new float2(0, times[index][i]);
            float2s[vertIndex + 1] = new float2(1, times[index][i]);

            float3s = normals[index];
            // Top of road normals
            float3s[vertIndex + 0] = localUp;
            float3s[vertIndex + 1] = localUp;
            // Bottom of road normals
            float3s[vertIndex + 2] = -localUp;
            float3s[vertIndex + 3] = -localUp;
            // Sides of road normals
            float3s[vertIndex + 4] = -localRight;
            float3s[vertIndex + 5] = localRight;
            float3s[vertIndex + 6] = -localRight;
            float3s[vertIndex + 7] = localRight;

            // Set triangle indices
            if (i < NumPoints[index] - 1 || isClosedLoop[index])
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    NativeArray<int> nativeArray = roadTriangles[index];
                    nativeArray[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    nativeArray = underRoadTriangles[index];
                    nativeArray[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                {
                    NativeArray<int> nativeArray = sideOfRoadTriangles[index];
                    nativeArray[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                }

            }

            vertIndex += 8;
            triIndex += 6;
        }
    }
}

public class RMJI : MonoBehaviour
{
    public List<VertexPath> paths = new List<VertexPath>();
    public List<Mesh> meshes = new List<Mesh>();
    public List<bool> flattenSurfaces = new List<bool>();
    public List<float> roadWidths = new List<float>();
    public List<float> thicknesses = new List<float>();

    public List<RMJob> jobs = new List<RMJob>();

    private int times_per_frame = 12;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        NativeArray<NativeArray<float3>> vertss = new NativeArray<NativeArray<float3>>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<float2>> uvss = new NativeArray<NativeArray<float2>>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<float3>> normalss = new NativeArray<NativeArray<float3>>(paths.Count, Allocator.TempJob);

        NativeArray<NativeArray<int>> roadTriangless = new NativeArray<NativeArray<int>>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<int>> underRoadTriangless = new NativeArray<NativeArray<int>>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<int>> sideOfRoadTriangless = new NativeArray<NativeArray<int>>(paths.Count, Allocator.TempJob);

        NativeArray<PathSpace> spacess = new NativeArray<PathSpace>(paths.Count, Allocator.TempJob);

        NativeArray<bool> flattenSurfacess = new NativeArray<bool>(paths.Count, Allocator.TempJob);
        NativeArray<float> roadWidthss = new NativeArray<float>(paths.Count, Allocator.TempJob);
        NativeArray<float> thicknessess = new NativeArray<float>(paths.Count, Allocator.TempJob);

        NativeArray<int> NumPointss = new NativeArray<int>(paths.Count, Allocator.TempJob);
        NativeArray<bool> isClosedLoops = new NativeArray<bool>(paths.Count, Allocator.TempJob);

        NativeArray<NativeArray<float>> timess = new NativeArray<NativeArray<float>>(paths.Count, Allocator.TempJob);
        NativeArray<float3> ups = new NativeArray<float3>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<float3>> points = new NativeArray<NativeArray<float3>>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<float3>> tangents = new NativeArray<NativeArray<float3>>(paths.Count, Allocator.TempJob);
        NativeArray<NativeArray<float3>> normals = new NativeArray<NativeArray<float3>>(paths.Count, Allocator.TempJob);

        for (int id = 0; id < paths.Count; id++)
        {
            vertss[id] = new NativeArray<float3>(paths[id].NumPoints * 8, Allocator.TempJob);
            uvss[id] = new NativeArray<float2>(vertss[id].Length, Allocator.TempJob);
            normalss[id] = new NativeArray<float3>(vertss[id].Length, Allocator.TempJob);

            int numTris = 2 * (paths[id].NumPoints - 1) + ((paths[id].isClosedLoop) ? 2 : 0);
            roadTriangless[id] = new NativeArray<int>(numTris * 3, Allocator.TempJob);
            underRoadTriangless[id] = new NativeArray<int>(numTris * 3, Allocator.TempJob);
            sideOfRoadTriangless[id] = new NativeArray<int>(numTris * 2 * 3, Allocator.TempJob);

            spacess[id] = paths[id].space;

            NumPointss[id] = paths[id].NumPoints;
            isClosedLoops[id] = paths[id].isClosedLoop;

            timess[id] = new NativeArray<float>(paths[id].times.Length, Allocator.TempJob);
            ups[id] = paths[id].up;
            points[id] = new NativeArray<float3>(paths[id].NumPoints, Allocator.TempJob);
            tangents[id] = new NativeArray<float3>(paths[id].NumPoints, Allocator.TempJob);
            normals[id] = new NativeArray<float3>(paths[id].NumPoints, Allocator.TempJob);
            for (int num = 0; num < paths[id].NumPoints; num++)
            {
                NativeArray<float3> float3s = points[id];
                float3s[num] = paths[id].GetPoint(num);
                float3s = tangents[id];
                float3s[num] = paths[id].GetTangent(num);
                float3s = normals[id];
                float3s[num] = paths[id].GetNormal(num);
            }
        }
        flattenSurfacess = flattenSurfaces.ToNativeArray(Allocator.TempJob);
        roadWidthss = roadWidths.ToNativeArray(Allocator.TempJob);
        thicknessess = thicknesses.ToNativeArray(Allocator.TempJob);

        RMJob job = new RMJob
        {
            verts = vertss,
            uvs = uvss,
            normals = normalss,

            roadTriangles = roadTriangless,
            underRoadTriangles = underRoadTriangless,
            sideOfRoadTriangles = sideOfRoadTriangless,

            spaces = spacess,

            flattenSurfaces = flattenSurfacess,
            roadWidths = roadWidthss,
            thicknesses = thicknessess,

            NumPoints = NumPointss,
            isClosedLoop = isClosedLoops,

            times = timess,
            up = ups,
            point = points,
            tangent = tangents,
            normal = normals
        };

        for (int id = 0; id < paths.Count; id++)
        {
            meshes[id].Clear();
            meshes[id].vertices = job.verts[id].Reinterpret<Vector3>().ToArray();
            meshes[id].uv = job.uvs[id].Reinterpret<Vector2>().ToArray();
            meshes[id].normals = job.normals[id].Reinterpret<Vector3>().ToArray();
            meshes[id].subMeshCount = 3;
            meshes[id].SetTriangles(job.roadTriangles[id].Reinterpret<int>().ToArray(), 0);
            meshes[id].SetTriangles(job.underRoadTriangles[id].Reinterpret<int>().ToArray(), 1);
            meshes[id].SetTriangles(job.sideOfRoadTriangles[id].Reinterpret<int>().ToArray(), 2);
            meshes[id].RecalculateBounds();
        }
        jobs.Clear();
        paths.Clear();
        meshes.Clear();
        flattenSurfaces.Clear();
        roadWidths.Clear();
        thicknesses.Clear();
    }
}
