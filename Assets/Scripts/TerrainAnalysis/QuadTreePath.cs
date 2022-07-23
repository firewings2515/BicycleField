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
        private GameObject cyclist;
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
        private bool build;
        [SerializeField]
        private bool generate;
        [SerializeField]
        private bool stop_build;

        private GameObject terrain_manager;
        private NativeArray<float4> Planes;
        private NativeList<RenderNode> RenderNodes;
        private bool is_initial = false;
        // Start is called before the first frame update
        void Start()
        {
            cam = camera.GetComponent<Camera>();
            terrain_manager = new GameObject("TerrainManager");
            Planes = new NativeArray<float4>(6, Allocator.Persistent);
            RenderNodes = new NativeList<RenderNode>(Allocator.Persistent);
            readBoundaryMin(Application.streamingAssetsPath + "//" + TerrainGenerator.file_path);
            TerrainGenerator.is_initial = true;
            QuadTreePatch.initial();
            cyclist.GetComponent<PathCreation.Examples.PathFollower>().speed = 20;
            is_initial = true;
            InvokeRepeating("buildQuadTree", 0, 1);
        }

        // Update is called once per frame
        void Update()
        {
            if (build)
            {
                build = false;
                buildQuadTree();
                //displayRenderNodes();
            }

            if (generate)
            {
                generate = false;
                generateByQuadTreePatch();
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
            if (stop_build)
            {
                cyclist.GetComponent<PathCreation.Examples.PathFollower>().speed = 0;
                return;
            }

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

        public void generateByQuadTreePatch()
        {
            //string ans = "";
            foreach (var node in QuadTreePatch.node_dic)
            {
                //ans += $"{node.Key} {node.Value}\n";
                GameObject plane = Instantiate(plane_prefab);
                plane.transform.SetParent(terrain_manager.transform);
                plane.transform.localPosition = new Vector3(node.Key.x, 0, node.Key.y);
                plane.transform.localScale = new Vector3(node.Value / 32, 1, node.Value / 32);
                plane.AddComponent<DEMFetcher>();
            }
            //Debug.Log(ans);
        }

        private void OnDrawGizmos()
        {
            if (!DebugMode || !is_initial)
            {
                return;
            }

            var cam = camera.GetComponent<Camera>();
            float2 camPosition = new float2(cam.transform.position.x, cam.transform.position.z);

            GUIStyle style = new GUIStyle();
            style.fontSize = 12;

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
                if (distance < 2048)
                {
                    //UnityEditor.Handles.Label(center, node.Bounds.Size.x.ToString(), style);
                    UnityEditor.Handles.Label(center, $"{i}", style);
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