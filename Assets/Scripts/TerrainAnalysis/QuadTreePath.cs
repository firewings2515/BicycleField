using System;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.IO;

namespace QuadTerrain
{
    public class QuadTreePath : MonoBehaviour
    {
        [SerializeField]
        private int x_extents = 64; //2048f
        [SerializeField]
        private int z_extents = 64; //2048f
        [SerializeField]
        private Plane[] SourcePlanes = new Plane[6];
        [SerializeField]
        private bool DebugMode;
        private Camera virtual_cam;
        [SerializeField]
        private GameObject evaluate_camera;
        private Camera evaluate_cam;

        [SerializeField]
        private GameObject cyclist;
        [SerializeField]
        private GameObject camera;
        private Camera cam;
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
        [SerializeField]
        private bool display_dem_points;
        [SerializeField]
        private string node_level_file;
        [SerializeField]
        private string feature_file;
        [SerializeField]
        private bool load_node_level_file;
        [SerializeField]
        private bool save_node_level_file;
        [SerializeField]
        private bool load_feature_file;
        [SerializeField]
        private bool save_feature_file;
        [SerializeField]
        private bool generate_idw;
        [SerializeField]
        private bool evaluate;
        [SerializeField]
        private string dense_feature_file;
        [SerializeField]
        private int subdivide;
        [SerializeField]
        private bool load_dense_feature_file;
        [SerializeField]
        private bool save_dense_feature_file;
        [SerializeField]
        private string sparse_feature_file;
        [SerializeField]
        private int interval;
        [SerializeField]
        private bool load_sparse_feature_file;
        [SerializeField]
        private bool save_sparse_feature_file;

        private GameObject terrain_manager;
        private NativeArray<float4> Planes;
        private NativeList<RenderNode> RenderNodes;
        private bool is_initial = false;
        private KDTree kdtree;
        private Vector3[] born_corners;
        private Vector3[] opposite_corners;
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
            GameObject virtual_camera = new GameObject("VirtualCamera");
            virtual_cam = virtual_camera.AddComponent<Camera>();
            evaluate_cam = evaluate_camera.GetComponent<Camera>();
            evaluate_cam.depthTextureMode |= DepthTextureMode.Depth;
            is_initial = true;
            InvokeRepeating("buildQuadTree", 0, 1);
            //buildQuadTree(); // for testing
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

            if (save_node_level_file)
            {
                save_node_level_file = false;
                saveNodeLevel(Application.streamingAssetsPath + "//" + node_level_file);
            }

            if (load_node_level_file)
            {
                load_node_level_file = false;
                QuadTreePatch.node_level_dic.Clear();
                loadNodeLevel(Application.streamingAssetsPath + "//" + node_level_file);
            }

            if (save_feature_file)
            {
                save_feature_file = false;
                saveFeacture(Application.streamingAssetsPath + "//" + feature_file);
            }

            if (load_feature_file)
            {
                load_feature_file = false;
                loadFeature(Application.streamingAssetsPath + "//" + feature_file);
            }

            if (save_dense_feature_file)
            {
                save_dense_feature_file = false;
                saveDenseFeacture(Application.streamingAssetsPath + "//" + dense_feature_file, subdivide);
            }

            if (load_dense_feature_file)
            {
                load_dense_feature_file = false;
                loadFeature(Application.streamingAssetsPath + "//" + dense_feature_file);
            }

            if (save_sparse_feature_file)
            {
                save_sparse_feature_file = false;
                saveSparseFeacture(Application.streamingAssetsPath + "//" + sparse_feature_file, interval);
            }

            if (load_sparse_feature_file)
            {
                load_sparse_feature_file = false;
                loadFeature(Application.streamingAssetsPath + "//" + sparse_feature_file);
            }

            if (generate_idw)
            {
                generate_idw = false;
                generateIDWs();
            }

            //if (display_dem_points)
            //{
            //    display_dem_points = false;
            //    TerrainGenerator.displayDEMPoints();
            //}

            if (evaluate)
            {
                evaluate = false;
                exportDepthTexture();
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
            
            var virtual_pos = TerrainGenerator.getXAndZInDEM(cam.transform.position.x, cam.transform.position.z);
            virtual_cam.transform.position = new Vector3(virtual_pos.x, cam.transform.position.y, virtual_pos.z);
            virtual_cam.transform.eulerAngles = new Vector3(0, cam.transform.eulerAngles.y, 0);
            virtual_cam.fieldOfView = cam.fieldOfView;
            virtual_cam.farClipPlane = cam.farClipPlane / 32.0f;
            virtual_cam.nearClipPlane = cam.nearClipPlane / 32.0f;
            virtual_cam.targetDisplay = 2;
            //virtual_cam.depth = cam.depth;
            FrustumPlanes2.FromCamera(virtual_cam, SourcePlanes, Planes);

            RenderNodes.Clear();
            BuildTerrainJob job = BuildTerrainJob.Create(x_extents, z_extents, virtual_cam.transform.position, Planes, RenderNodes);


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
            foreach (var node in QuadTreePatch.node_level_dic)
            {
                //ans += $"{node.Key} {node.Value}\n";
                GameObject plane = Instantiate(plane_prefab);
                plane.transform.SetParent(terrain_manager.transform);
                var position = TerrainGenerator.demToXAndZ(node.Key.x, node.Key.z);
                plane.transform.localPosition = new Vector3((float)position.x, 0, (float)position.z);
                plane.transform.localScale = new Vector3(node.Value, 1, node.Value);
                plane.AddComponent<DEMFetcher>();
            }
            //Debug.Log(ans);
        }

        public void loadNodeLevel(string file_path)
        {
            using (StreamReader sr = new StreamReader(file_path))
            {
                int n = int.Parse(sr.ReadLine());
                for (int f_i = 0; f_i < n; f_i++)
                {
                    string[] inputs = sr.ReadLine().Split(' ');
                    QuadTreePatch.node_level_dic.Add(new DEMCoord(int.Parse(inputs[0]), int.Parse(inputs[1])), int.Parse(inputs[2]));
                }
            }
            Debug.Log("Read Node Level File " + file_path + " Successfully");
        }

        public void saveNodeLevel(string file_path)
        {
            using (StreamWriter sw = new StreamWriter(file_path))
            {
                sw.WriteLine(QuadTreePatch.node_level_dic.Count);
                foreach (var node in QuadTreePatch.node_level_dic)
                {
                    sw.WriteLine($"{node.Key.x} {node.Key.z} {node.Value}");
                }
            }
        }

        public void loadFeature(string file_path)
        {
            using (StreamReader sr = new StreamReader(file_path))
            {
                //string input = sr.ReadLine(); //boundary_min.x + " " + boundary_min.y
                //input = sr.ReadLine(); //origin_pos.x + " " + origin_pos.y + " " + origin_pos.z
                //input = sr.ReadLine(); //terrain_min.x.ToString() + " " + terrain_min.y.ToString() + " " + terrain_min.z.ToString() + " " + terrain_max.x.ToString() + " " + terrain_max.y.ToString() + " " + terrain_max.z.ToString()
                int n = int.Parse(sr.ReadLine()); //features.Length
                kdtree = new KDTree();
                kdtree.nodes = new WVec3[n];
                kdtree.parent = new int[n];
                kdtree.left = new int[n];
                kdtree.right = new int[n];
                born_corners = new Vector3[n];
                opposite_corners = new Vector3[n];
                for (int f_i = 0; f_i < n; f_i++)
                {
                    string[] inputs = sr.ReadLine().Split(' ');
                    float x = float.Parse(inputs[0]);
                    float y = float.Parse(inputs[1]);
                    float z = float.Parse(inputs[2]);
                    float w = float.Parse(inputs[3]);
                    kdtree.nodes[f_i].x = x;
                    kdtree.nodes[f_i].y = y;
                    kdtree.nodes[f_i].z = z;
                    kdtree.nodes[f_i].w = w;
                    int p = int.Parse(inputs[4]);
                    kdtree.parent[f_i] = p;
                    int l = int.Parse(inputs[5]);
                    kdtree.left[f_i] = l;
                    int r = int.Parse(inputs[6]);
                    kdtree.right[f_i] = r;

                    if (w < 0)
                        born_corners[f_i] = new Vector3(x, y, z);

                    //if (w != -1)
                    //{
                    //    x = float.Parse(inputs[7]);
                    //    y = float.Parse(inputs[8]);
                    //    z = float.Parse(inputs[9]);
                    //    opposite_corners[f_i] = new Vector3(x, y, z);
                    //}
                }
            }
            Debug.Log("Read Feature File " + file_path + " Successfully");
            
            if (display_dem_points)
            {
                GameObject dem_points_manager = new GameObject("DEM Corner");
                TerrainGenerator.showPoint(born_corners, "DEM Corner", dem_points_manager.transform, TerrainGenerator.feature_ball_prefab, 4.0f);
            }
        }

        public void saveFeacture(string file_path)
        {
            QuadTreePatch.getAllNode();
            List<EarthCoord> all_coords = new List<EarthCoord>();
            List<float> xs_list = new List<float>();
            List<float> zs_list = new List<float>();
            float[] xs, zs;
            List<bool> is_born_corner_list = new List<bool>();
            int index = 0;
            List<int> corner_index_list = new List<int>();
            List<int> opposite_corner_list = new List<int>();
            foreach (var node in QuadTreePatch.node_set)
            {
                bool is_born_corner = QuadTreePatch.node_level_dic.ContainsKey(node.born_corner);
                is_born_corner_list.Add(is_born_corner);
                var lon_lat = TerrainGenerator.mapToDEM(node.born_corner.x, node.born_corner.z);
                all_coords.Add(new EarthCoord((float)lon_lat.lon, (float)lon_lat.lat));
                var unity_loc = TerrainGenerator.demToXAndZ(node.born_corner.x, node.born_corner.z);
                xs_list.Add((float)unity_loc.x);
                zs_list.Add((float)unity_loc.z);
                //if (is_born_corner)
                //{
                //    var lon_lat = TerrainGenerator.mapToDEM(node.born_corner.x, node.born_corner.z);
                //    all_coords.Add(new EarthCoord((float)lon_lat.lon, (float)lon_lat.lat));
                //    var unity_loc = TerrainGenerator.demToXAndZ(node.born_corner.x, node.born_corner.z);
                //    xs_list.Add((float)unity_loc.x);
                //    zs_list.Add((float)unity_loc.z);

                //    lon_lat = TerrainGenerator.mapToDEM(node.opposite_corner.x, node.opposite_corner.z);
                //    all_coords.Add(new EarthCoord((float)lon_lat.lon, (float)lon_lat.lat));
                //    unity_loc = TerrainGenerator.demToXAndZ(node.opposite_corner.x, node.opposite_corner.z);
                //    xs_list.Add((float)unity_loc.x);
                //    zs_list.Add((float)unity_loc.z);
                //}
                //else
                //{
                //    var lon_lat = TerrainGenerator.mapToDEM(node.born_corner.x, node.born_corner.z);
                //    all_coords.Add(new EarthCoord((float)lon_lat.lon, (float)lon_lat.lat));
                //    var unity_loc = TerrainGenerator.demToXAndZ(node.born_corner.x, node.born_corner.z);
                //    xs_list.Add((float)unity_loc.x);
                //    zs_list.Add((float)unity_loc.z);
                //}
            }
            xs = xs_list.ToArray();
            float[] ys = HgtReader.getElevations(all_coords, true).ToArray();
            zs = zs_list.ToArray();
            WVec3[] features = new WVec3[ys.Length];
            bool[] is_born_corner_array = is_born_corner_list.ToArray();
            //int opposite_index = 0;
            for (int i = 0; i < is_born_corner_array.Length; i++)
            {
                //if (is_born_corner_array[i])
                //{
                //    features[index] = new WVec3(xs[index], ys[index], zs[index], opposite_corner_list[opposite_index]);
                //    index++;
                //}
                features[index] = new WVec3(xs[index], ys[index], zs[index], -1);
                index++;
            }

            KDTree kdtree = new KDTree();
            kdtree.buildKDTree(features);

            Debug.Log("Writing " + file_path);
            using (StreamWriter sw = new StreamWriter(file_path))
            {
                //sw.WriteLine(boundary_min.x + " " + boundary_min.y);
                //sw.WriteLine(origin_pos.x + " " + origin_pos.y + " " + origin_pos.z);
                //sw.WriteLine(terrain_min.x.ToString() + " " + terrain_min.y.ToString() + " " + terrain_min.z.ToString() + " " + terrain_max.x.ToString() + " " + terrain_max.y.ToString() + " " + terrain_max.z.ToString());
                sw.WriteLine(features.Length);
                for (int point_index = 0; point_index < features.Length; point_index++)
                {
                    //Vector3 feature_out = new Vector3(kdtree.nodes[point_index].x/* - origin_pos.x*/, kdtree.nodes[point_index].y, kdtree.nodes[point_index].z/* - origin_pos.z*/);
                    //int opposite_corner_index = Mathf.RoundToInt(kdtree.nodes[point_index].w);
                    //if (opposite_corner_index != -1)
                    //{
                    //    WVec3 opposite_corner = features[opposite_corner_index];
                    //    sw.WriteLine($"{kdtree.nodes[point_index].x} {kdtree.nodes[point_index].y} {kdtree.nodes[point_index].z} {kdtree.nodes[point_index].w} {kdtree.parent[point_index]} {kdtree.left[point_index]} {kdtree.right[point_index]} {opposite_corner.x} {opposite_corner.y} {opposite_corner.z}");
                    //}
                    //else
                    //{
                        sw.WriteLine($"{kdtree.nodes[point_index].x} {kdtree.nodes[point_index].y} {kdtree.nodes[point_index].z} {kdtree.nodes[point_index].w} {kdtree.parent[point_index]} {kdtree.left[point_index]} {kdtree.right[point_index]}");
                    //}
                }
            }
            Debug.Log("Write " + file_path + " Successfully!");

            //Vector3 boundary_min = new Vector3(TerrainGenerator.boundary_min_x, 0, TerrainGenerator.boundary_min_z);
            //Vector3 terrain_min = new Vector3(road_integration.terrain_min_x - PublicOutputInfo.origin_pos.x, -PublicOutputInfo.origin_pos.y, road_integration.terrain_min_z - PublicOutputInfo.origin_pos.z);
            //Vector3 terrain_max = new Vector3(road_integration.terrain_max_x - PublicOutputInfo.origin_pos.x, -PublicOutputInfo.origin_pos.y, road_integration.terrain_max_z - PublicOutputInfo.origin_pos.z);
            //Vector3 terrain_min = new Vector3();
            //Vector3 terrain_max = new Vector3();
            //PublicOutputInfo.writeFeatureFile(file_path, features, new int[0], boundary_min, terrain_min, terrain_max);
        }

        public void saveDenseFeacture(string file_path, int subdivide)
        {
            QuadTreePatch.getAllNode();
            QuadTreePatch.denseNode(subdivide);

            List<EarthCoord> all_coords = new List<EarthCoord>();
            List<float> xs_list = new List<float>();
            List<float> zs_list = new List<float>();
            List<bool> is_corners_list = new List<bool>();
            float[] xs, zs;
            bool[] is_corners;
            int index = 0;
            foreach (var node in QuadTreePatch.dense_node_set)
            {
                var lon_lat = TerrainGenerator.mapToDEMF(node.x, node.z);
                all_coords.Add(new EarthCoord((float)lon_lat.lon, (float)lon_lat.lat));
                var unity_loc = TerrainGenerator.demFToXAndZ(node.x, node.z);
                xs_list.Add((float)unity_loc.x);
                zs_list.Add((float)unity_loc.z);
                is_corners_list.Add(node.is_corner);
            }
            xs = xs_list.ToArray();
            float[] ys = HgtReader.getElevations(all_coords, true).ToArray();
            zs = zs_list.ToArray();
            is_corners = is_corners_list.ToArray();
            WVec3[] features = new WVec3[ys.Length];
            for (int i = 0; i < xs.Length; i++)
            {
                features[index] = new WVec3(xs[index], ys[index], zs[index], is_corners[i] ? -1 : 1);
                index++;
            }

            KDTree kdtree = new KDTree();
            kdtree.buildKDTree(features);

            Debug.Log("Writing " + file_path);
            using (StreamWriter sw = new StreamWriter(file_path))
            {
                sw.WriteLine(features.Length);
                for (int point_index = 0; point_index < features.Length; point_index++)
                {
                    sw.WriteLine($"{kdtree.nodes[point_index].x} {kdtree.nodes[point_index].y} {kdtree.nodes[point_index].z} {kdtree.nodes[point_index].w} {kdtree.parent[point_index]} {kdtree.left[point_index]} {kdtree.right[point_index]}");
                }
            }
            Debug.Log("Write " + file_path + " Successfully!");
        }

        public void saveSparseFeacture(string file_path, int interval)
        {
            QuadTreePatch.getAllNode();
            QuadTreePatch.sparseNode(interval);

            List<EarthCoord> all_coords = new List<EarthCoord>();
            List<float> xs_list = new List<float>();
            List<float> zs_list = new List<float>();
            float[] xs, zs;
            int index = 0;
            foreach (var node in QuadTreePatch.sparse_node_set)
            {
                var lon_lat = TerrainGenerator.mapToDEMF(node.x, node.z);
                all_coords.Add(new EarthCoord((float)lon_lat.lon, (float)lon_lat.lat));
                var unity_loc = TerrainGenerator.demFToXAndZ(node.x, node.z);
                xs_list.Add((float)unity_loc.x);
                zs_list.Add((float)unity_loc.z);
            }
            xs = xs_list.ToArray();
            float[] ys = HgtReader.getElevations(all_coords, true).ToArray();
            zs = zs_list.ToArray();
            WVec3[] features = new WVec3[ys.Length];
            for (int i = 0; i < xs.Length; i++)
            {
                features[index] = new WVec3(xs[index], ys[index], zs[index], -1);
                index++;
            }

            KDTree kdtree = new KDTree();
            kdtree.buildKDTree(features);

            Debug.Log("Writing " + file_path);
            using (StreamWriter sw = new StreamWriter(file_path))
            {
                sw.WriteLine(features.Length);
                for (int point_index = 0; point_index < features.Length; point_index++)
                {
                    sw.WriteLine($"{kdtree.nodes[point_index].x} {kdtree.nodes[point_index].y} {kdtree.nodes[point_index].z} {kdtree.nodes[point_index].w} {kdtree.parent[point_index]} {kdtree.left[point_index]} {kdtree.right[point_index]}");
                }
            }
            Debug.Log("Write " + file_path + " Successfully!");
        }

        void generateIDWs()
        {
            for (int i = 0; i < born_corners.Length; i++)
            {
                float expanded_length = 50;
                int[] area_features_index = kdtree.getAreaPoints(born_corners[i].x - expanded_length, born_corners[i].z - expanded_length, born_corners[i].x + 2 * expanded_length, born_corners[i].z + 2 * expanded_length);
                Vector4[] area_features = new Vector4[area_features_index.Length];
                for (int area_features_index_index = 0; area_features_index_index < area_features_index.Length; area_features_index_index++)
                {
                    WVec3 feature = kdtree.nodes[area_features_index[area_features_index_index]];
                    area_features[area_features_index_index] = new Vector4(feature.x, feature.y, feature.z, feature.w);
                }
                generateIDW(area_features, born_corners[i]);
            }
        }

        void generateIDW(Vector4[] area_features, Vector3 corner)
        {
            GameObject idw_plane = Instantiate(plane_prefab);
            idw_plane.name = $"IDW_Plane_{corner.x}_{corner.z}";
            idw_plane.transform.parent = terrain_manager.transform;
            MeshFilter mf = idw_plane.GetComponent<MeshFilter>();

            Vector3[] vertices = mf.mesh.vertices;
            float[] xs = new float[vertices.Length];
            float[] zs = new float[vertices.Length];
            float find_x_min = float.MaxValue;
            float find_z_min = float.MaxValue;
            int center_index = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                xs[i] = corner.x + vertices[i].x;
                zs[i] = corner.z + vertices[i].z;
                if (xs[i] > find_x_min)
                {
                    find_x_min = xs[i];
                    center_index = i;
                }
                if (zs[i] > find_z_min)
                {
                    find_z_min = zs[i];
                    center_index = i;
                }
            }
            float[] ys = IDW.inverseDistanceWeightings(area_features, xs, zs);

            idw_plane.transform.position = new Vector3(corner.x, ys[center_index], corner.z);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y = ys[i] - idw_plane.transform.position.y;
            }
            mf.mesh.vertices = vertices;
            //Recalculations
            mf.mesh.RecalculateNormals();
            mf.mesh.RecalculateBounds();
            mf.mesh.Optimize();
        }

        private void OnDrawGizmos()
        {
            if (!DebugMode || !is_initial)
            {
                return;
            }

            cam = camera.GetComponent<Camera>();
            float2 camPosition = new float2(virtual_cam.transform.position.x, virtual_cam.transform.position.z);

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
                if (distance < 64) //2048
                {
                    UnityEditor.Handles.Label(center, node.Bounds.Size.x.ToString(), style);
                    //UnityEditor.Handles.Label(center, $"{i}", style);
                }

            }
        }

        private void OnDisable()
        {
            Planes.Dispose();
            RenderNodes.Dispose();
        }

        void exportDepthTexture()
        {
            //Shader.SetGlobalTexture("_CameraDepthTexture", targetDepth);

            //first Make sure you're using RGB24 as your texture format
            //Texture mainTexture = evaluate_cam.GetComponent<Renderer>().sharedMaterial.GetTexture("_CameraDepthTexture");
            Texture depthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
            Texture2D texture2D = new Texture2D(depthTexture.width, depthTexture.height, TextureFormat.RGBA32, false);
            Debug.Log($"{depthTexture.width} {depthTexture.height}");
            RenderTexture currentRT = RenderTexture.active;

            RenderTexture targetDepth = new RenderTexture(depthTexture.width, depthTexture.height, 24, RenderTextureFormat.Depth);
            Graphics.Blit(depthTexture, targetDepth);
            //RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
            //Graphics.Blit(mainTexture, renderTexture, GetComponent<Renderer>().sharedMaterial);

            RenderTexture.active = targetDepth;
            texture2D.ReadPixels(new Rect(0, 0, targetDepth.width, targetDepth.height), 0, 0);
            texture2D.Apply();

            Color[] pixels = texture2D.GetPixels();

            RenderTexture.active = currentRT;

            //then Save To Disk as PNG
            byte[] bytes = texture2D.EncodeToPNG();
            var dirPath = Application.dataPath + "/Resources/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "depth" + ".png", bytes);
        }
    }
}