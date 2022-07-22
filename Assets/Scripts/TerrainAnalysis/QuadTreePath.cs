using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.IO;

namespace QuadTerrain
{
    public class QuadTreePath : MonoBehaviour
    {
        [SerializeField]
        private float Extents = 2048f;
        [SerializeField]
        private Plane[] SourcePlanes = new Plane[6];
        [SerializeField]
        private bool DebugMode;

        [SerializeField]
        private GameObject camera;
        private Camera cam;
        [SerializeField]
        private Mesh plane_mesh;
        [SerializeField]
        private Material plane_material;
        [SerializeField]
        private GameObject plane_prefab;
        [SerializeField]
        private bool debug;

        private GameObject terrain_manager;
        private NativeArray<float4> Planes;
        private NativeList<RenderNode> RenderNodes;
        // Start is called before the first frame update
        void Start()
        {
            cam = camera.GetComponent<Camera>();
            terrain_manager = new GameObject("TerrainManager");
            Planes = new NativeArray<float4>(6, Allocator.Persistent);
            RenderNodes = new NativeList<RenderNode>(Allocator.Persistent);
            readBoundaryMin(Application.streamingAssetsPath + "//" + TerrainGenerator.file_path);
            TerrainGenerator.is_initial = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (debug)
            {
                debug = false;
                buildQuadTree();
                displayRenderNodes();
            }
        }

        void readBoundaryMin(string file_path)
        {
            using (StreamReader sr = new StreamReader(file_path))
            {
                string[] inputs = sr.ReadLine().Split(' ');
                TerrainGenerator.boundary_min_x = float.Parse(inputs[0]);    // for DEM
                TerrainGenerator.boundary_min_z = float.Parse(inputs[1]);    // for DEM
                inputs = sr.ReadLine().Split(' ');
                TerrainGenerator.origin_x = float.Parse(inputs[0]);
                TerrainGenerator.origin_y = float.Parse(inputs[1]);
                TerrainGenerator.origin_z = float.Parse(inputs[2]);
            }
        }

        public void buildQuadTree()
        {
            FrustumPlanes2.FromCamera(cam, SourcePlanes, Planes);

            RenderNodes.Clear();
            BuildTerrainJob job = BuildTerrainJob.Create(Extents, cam.transform.position, Planes, RenderNodes);


            var watch = System.Diagnostics.Stopwatch.StartNew();
            job.Run();

            watch.Stop();
            Debug.LogFormat("Construct: {0}", watch.ElapsedTicks);


            Debug.LogFormat("RenderNode count {0}", RenderNodes.Length);
        }

        public void displayRenderNodes()
        {
            for (int i = 0; i < RenderNodes.Length; i++)
            {
                RenderNode node = RenderNodes[i];
                GameObject plane = Instantiate(plane_prefab);
                plane.transform.SetParent(terrain_manager.transform);
                plane.transform.localPosition = node.WorldPosition;
                plane.transform.localScale = node.WorldScale;
                plane.AddComponent<DEMFetcher>();
            }
        }

        private void OnDrawGizmos()
        {
            if (!DebugMode)
            {
                return;
            }

            var cam = camera.GetComponent<Camera>();
            float2 camPosition = new float2(cam.transform.position.x, cam.transform.position.z);

            GUIStyle style = new GUIStyle();
            style.fontSize = 24;

            Gizmos.color = Color.green;
            for (int i = 0; i < RenderNodes.Length; i++)
            {

                var node = RenderNodes[i].Node;

                if (!node.IsLeaf)
                {
                    continue;
                }

                Vector3 center = new Vector3(node.Bounds.Center.x, 0f, node.Bounds.Center.y);
                Vector3 size = new Vector3(node.Bounds.Size.x, 1f, node.Bounds.Size.y);
                Gizmos.DrawWireCube(center, size);

                float distance = math.distance(camPosition, node.Bounds.Center);
                if (distance < 1024)
                {
                    UnityEditor.Handles.Label(center, node.Bounds.Size.x.ToString(), style);
                }

            }
        }

        private void OnDisable()
        {
            Planes.Dispose();
            RenderNodes.Dispose();
        }
    }
}