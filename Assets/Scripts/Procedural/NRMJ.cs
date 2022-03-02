using System.Collections.Generic;
using PathCreation.Utility;
using UnityEngine;
using System.Threading;

namespace PathCreation.Examples
{
    public class NRMJob {
        PathCreation.VertexPath path;
        bool flattenSurface;
        float roadWidth;
        float thickness;
        Mesh mesh;

        //self
        List<Vector3> points;
        List<Vector2> tangents;
        List<Vector3> outer_normals;

        Vector3[] verts;
        Vector2[] uvs;
        Vector3[] normals;
        int[] roadTriangles;
        int[] underRoadTriangles;
        int[] sideOfRoadTriangles;

        public NRMJob(ref Vector3[] _verts, ref Vector2[] _uvs, ref Vector3[] _normals, ref int[] _roadTriangles, ref int[] _underRoadTriangles, ref int[] _sideOfRoadTriangles, PathCreation.VertexPath _path, bool _flattenSurface, float _roadWidth, float _thickness, Mesh _mesh, List<Vector3> _points, List<Vector2> _tangents, List<Vector3> _outer_normals)
        {
            this.mesh = _mesh;
            this.path = _path;
            this.flattenSurface = _flattenSurface;
            this.roadWidth = _roadWidth;
            this.thickness = _thickness;
            this.points = _points;
            this.tangents = _tangents;
            this.outer_normals = _outer_normals;

            verts = _verts;
            uvs = _uvs;
            normals = _normals;
            roadTriangles = _roadTriangles;
            underRoadTriangles = _underRoadTriangles;
            sideOfRoadTriangles = _sideOfRoadTriangles;
        }

        public void DoWork()
        {
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
                Vector3 localUp = (usePathNormals) ? Vector3.Cross(tangents[i], outer_normals[i]) : path.up;
                Vector3 localRight = (usePathNormals) ? outer_normals[i] : Vector3.Cross(localUp, tangents[i]);

                // Find position to left and right of current path vertex
                Vector3 vertSideA = points[i] - localRight * Mathf.Abs(roadWidth);
                Vector3 vertSideB = points[i] + localRight * Mathf.Abs(roadWidth);

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
            Debug.Log(1);
        }
    }
    public class NRMJ : PathSceneTool
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

        [SerializeField, HideInInspector]
        GameObject meshHolder;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        public bool useJob = false;
        private int c = 0;

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
            if (useJob)
            {
                Vector3[] verts = new Vector3[path.NumPoints * 8];
                Vector2[] uvs = new Vector2[verts.Length];
                Vector3[] normals = new Vector3[verts.Length];

                int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
                int[] roadTriangles = new int[numTris * 3];
                int[] underRoadTriangles = new int[numTris * 3];
                int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

                List<Vector3> points = new List<Vector3>();
                List<Vector2> tangents = new List<Vector2>();
                List<Vector3> outer_normals = new List<Vector3>();
                for (int id = 0; id < path.NumPoints; id++)
                {
                    points.Add(path.GetPoint(id));
                    tangents.Add(path.GetTangent(id));
                    outer_normals.Add(path.GetNormal(id));
                }

                NRMJob threadWork = new NRMJob(ref verts, ref uvs, ref normals, ref roadTriangles, ref underRoadTriangles, ref sideOfRoadTriangles, path, flattenSurface, roadWidth, thickness, mesh, points, tangents, outer_normals);
                Thread newThread = new Thread(new ThreadStart(threadWork.DoWork));
                newThread.Start();

                mesh.Clear();
                mesh.vertices = verts;
                mesh.uv = uvs;
                mesh.normals = normals;
                mesh.subMeshCount = 3;
                mesh.SetTriangles(roadTriangles, 0);
                mesh.SetTriangles(underRoadTriangles, 1);
                mesh.SetTriangles(sideOfRoadTriangles, 2);
                mesh.RecalculateBounds();
                Debug.Log(2);
            }
            else
            {

                if (c != 0)
                {
                    c--;
                    return;
                }
                c = 11;

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
            }
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

        private void Update()
        {
            
        }
    }
}
