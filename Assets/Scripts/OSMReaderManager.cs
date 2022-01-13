using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using PathCreation;
using PathCreation.Examples;
using Unity.Jobs;
using Unity.Burst;
//using Cinemachine;

public class OSMReaderManager : MonoBehaviour
{
    public string file_name = "mapSimple.osm";
    public string osm3d_file_name = "YangJin3D.osm";
    public bool need_write_osm3d = false;
    public string set_camera_to_point_id = "45263678_226830312+0"; // 45263678_226830312+0還金路 4486262148竹子湖路 2037722609公館圓環
    //public Vector2 OSM_size;
    public GameObject sphere_prefab;
    public GameObject tree_prefab;
    public GameObject view_instance;
    public GameObject cam;
    int DIVIDE_LINE = 1000;
    int count = 0;
    GameObject point_manager;
    GameObject house_polygon_manager;
    GameObject[] houses_polygon;
    public Material houses_polygon_mat;
    GameObject[] house_mesh;
    GameObject road_manager;
    //GameObject[] roads_polygon;
    public Material roads_polygon_mat;
    GameObject tree_manager;
    HierarchyControl hierarchy_c;
    int camera_path_index;
    public Dictionary<string, List<GameObject>> pathes_objects;
    public OSMReader osm_reader;
    //public CinemachineVirtualCamera virtualCameraPrefab;
    //CinemachineVirtualCamera virtualCamera;
    bool show_osm_points = false; // show all points in OSM data
    bool finish_create = false;
    [Header("Procedural Modeling of house")]
    public bool build_house = false;
    public List<PathCreator> all_pc;
    private GameObject all_road_obj;
    List<string> road_id_list;
    public Dropdown road_choose;
    string getDigits(string s) // for parser
    {
        return Regex.Match(s, "[+-]?([0-9]*[.])?[0-9]+").ToString();
    }

    public void getNextStep(int dir, float pos, float gap, ref float next)
    {
        getNextStep(dir, pos, gap, ref next, osm_reader.toPositions(osm_reader.pathes[camera_path_index].ref_node));
        if (next >= osm_reader.pathes[camera_path_index].ref_node.Count - 1)
        {
            if (dir == 1)
                next = osm_reader.pathes[camera_path_index].ref_node.Count - 1;
            else
                next = 0;
        }
        else if (next < 0)
            next = 0;
    }

    void getNextStep(int dir, float pos, float gap, ref float next, List<Vector3> path_point) // arcLength
    {
        Vector3 qt1, qt0;
        float t0 = 0.0f, t1;
        float d = 0.0f;
        int posOnPointIndex = (int)pos;
        pos -= posOnPointIndex;
        t1 = t0 = pos;
        //pos
        Vector3 cp_pos_p1 = path_point[posOnPointIndex % path_point.Count];
        Vector3 cp_pos_p2 = path_point[(posOnPointIndex + 1) % path_point.Count];

        float percent = 1.0f / DIVIDE_LINE;

        qt1 = (1 - t1) * cp_pos_p1 + t1 * cp_pos_p2;

        while (d < gap)
        {
            qt0 = qt1;
            t0 = t1;
            t1 += dir * percent;
            if (t1 < 0)
            {
                qt1 = path_point[posOnPointIndex % path_point.Count];
                d += Vector3.Distance(qt0, qt1);

                if (d > gap)
                {
                    t1 = -pos * gap / d + pos;
                    next = posOnPointIndex % path_point.Count + t1;
                    return;
                }

                t1 = 1.0f - percent;
                pos = 1.0f;
                gap -= d;
                d = 0.0f;

                posOnPointIndex--;
                if (posOnPointIndex < 0)
                    posOnPointIndex += path_point.Count;

                cp_pos_p2 = cp_pos_p1;
                cp_pos_p1 = path_point[posOnPointIndex % path_point.Count];
                qt0 = cp_pos_p2;
            }
            else if (t1 > 1)
            {
                qt1 = path_point[(posOnPointIndex + 1) % path_point.Count];
                d += Vector3.Distance(qt0, qt1);

                if (d > gap)
                {
                    t1 = (1 - pos) * gap / d + pos;
                    next = posOnPointIndex % path_point.Count + t1;
                    return;
                }
                t1 = percent;
                pos = 0.0f;
                gap -= d;
                d = 0.0f;

                posOnPointIndex++;
                if (posOnPointIndex >= path_point.Count)
                    posOnPointIndex -= path_point.Count;

                cp_pos_p1 = cp_pos_p2;
                cp_pos_p2 = path_point[(posOnPointIndex + 1) % path_point.Count];
                qt0 = cp_pos_p1;
            }

            qt1 = (1 - t1) * cp_pos_p1 + t1 * cp_pos_p2;
            d += Vector3.Distance(qt0, qt1); //t:d=tx:gap tx=t/d*gap
        }

        t1 = (t1 - pos) * gap / d + pos;
        next = posOnPointIndex % path_point.Count + t1;
    }

    Vector3 GMT(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int matrix_type, float t) {

        Matrix4x4 M = new Matrix4x4();
        float tension = 0.5f;
        if (tension < 0.01) tension = 0.01f;
        switch (matrix_type)
        {
            case 1: {
                    M.SetRow(0,new Vector4(0, 0, 0, 0));
                    M.SetRow(1,new Vector4(0, 0, -1, 1));
                    M.SetRow(2,new Vector4(0, 0, 1, 0));
                    M.SetRow(3,new Vector4(0, 0, 0, 0));
            }
            break;
            case 2: {
                    M.SetRow(0, (new Vector4(-1.0f, 2.0f, -1.0f, 0.0f)) * tension);
                    M.SetRow(1, (new Vector4(2.0f / tension- 1.0f, 1.0f - 3.0f / tension, 0.0f, 1.0f / tension)) *tension);
                    M.SetRow(2, (new Vector4(1.0f - 2.0f / tension, 3.0f / tension- 2.0f, 1.0f, 0.0f))*tension);
                    M.SetRow(3, (new Vector4(1, -1, 0, 0)) * tension);
                }
                break;
            case 3: default: {
                    M.SetRow(0, (new Vector4(-1, 3, -3, 1)) / 6.0f);
                    M.SetRow(1, (new Vector4(3, -6, 0, 4))/6.0f);
                    M.SetRow(2, (new Vector4(-3, 3, 3, 1))/6.0f);
                    M.SetRow(3, (new Vector4(1, 0, 0, 0))/6.0f);
                }
                break;
        }
        //M = M.transpose;
        Matrix4x4 G = new Matrix4x4();
        G.SetRow(0, new Vector4(p0.x, p0.y, p0.z, 1.0f));
        G.SetRow(1, new Vector4(p1.x, p1.y, p1.z, 1.0f));
        G.SetRow(2, new Vector4(p2.x, p2.y, p2.z, 1.0f));
        G.SetRow(3, new Vector4(p3.x, p3.y, p3.z, 1.0f));
        G = G.transpose;
        Vector4 T = new Vector4( t * t * t, t * t, t, 1.0f );
        Vector4 result = G * M * T;
        return new Vector3(result[0], result[1], result[2]);
    }

    void createArcTree(List<Vector3> path_point, string path_id, float road_width)
    {
        float t0 = 0.0f;
        float t1 = 0.0f;
        float t2 = 0.0f;
        float percent = 1.0f / DIVIDE_LINE;
        bool breakLoop = false;
        Vector3 qt0, qt1, cross_t;
        Vector3 orient_t = new Vector3(0, 1, 0);
        while (!breakLoop)
        {
            getNextStep(1, t0, 20, ref t1, path_point);

            if (t1 >= path_point.Count - 1 || t0 > t1)
            {
                breakLoop = true;
                break;
            }

            t2 = t1;

            int pos0OnPointIndex = (int)t0;
            t0 -= pos0OnPointIndex;
            int pos1OnPointIndex = (int)t1;
            t1 -= pos1OnPointIndex;

            // pos
            Vector3 cp_pos_p1 = path_point[pos0OnPointIndex % path_point.Count];
            Vector3 cp_pos_p2 = path_point[(pos0OnPointIndex + 1) % path_point.Count];

            percent = 1.0f / DIVIDE_LINE;

            qt0 = (1 - t0) * cp_pos_p1 + t0 * cp_pos_p2;

            // pos
            cp_pos_p1 = path_point[pos1OnPointIndex % path_point.Count];
            cp_pos_p2 = path_point[(pos1OnPointIndex + 1) % path_point.Count];

            qt1 = (1 - t1) * cp_pos_p1 + t1 * cp_pos_p2;

            // cross
            cross_t = Vector3.Cross((qt1 - qt0), orient_t).normalized;
            cross_t = cross_t * road_width;

            Vector3 spawn_pos = qt0 + cross_t;
            int spawn_x = 0;
            int spawn_y = 0;
            hierarchy_c.calcLocation(spawn_pos.x, spawn_pos.z, ref spawn_x, ref spawn_y);
            GameObject instance_r = Instantiate(view_instance, spawn_pos, Quaternion.identity);
            instance_r.GetComponent<ViewInstance>().cam = cam;
            instance_r.GetComponent<ViewInstance>().prefab = tree_prefab;
            instance_r.GetComponent<ViewInstance>().points = new Vector3[1];
            instance_r.GetComponent<ViewInstance>().points[0] = spawn_pos;
            instance_r.GetComponent<ViewInstance>().setup(true);
            instance_r.name = "Tree_" + path_id;
            instance_r.transform.parent = tree_manager.transform;
            hierarchy_c.heirarchy_master[spawn_x, spawn_y].objects.Add(instance_r);

            spawn_pos = qt0 - cross_t;
            hierarchy_c.calcLocation(spawn_pos.x, spawn_pos.z, ref spawn_x, ref spawn_y);
            GameObject instance_l = Instantiate(view_instance, spawn_pos, Quaternion.identity);
            instance_l.GetComponent<ViewInstance>().cam = cam;
            instance_l.GetComponent<ViewInstance>().prefab = tree_prefab;
            instance_l.GetComponent<ViewInstance>().points = new Vector3[1];
            instance_l.GetComponent<ViewInstance>().points[0] = spawn_pos;
            instance_l.GetComponent<ViewInstance>().setup(true);
            instance_l.name = "Tree_" + path_id;
            instance_l.transform.parent = tree_manager.transform;
            hierarchy_c.heirarchy_master[spawn_x, spawn_y].objects.Add(instance_l);
            count += 2;
            t0 = t2; //next start t
        }
    }
    float vec_len(Vector3 input) {
        return input.x * input.x + input.y * input.y + input.z * input.z;
    }
    void createSmoothRoadPolygons(List<Vector3> path_point, string path_id, float road_width)
    {
        Transform[] trans = new Transform[path_point.Count];
        GameObject road_obj = new GameObject(path_id);
        PathCreator pc = road_obj.AddComponent<PathCreator>();
        road_obj.transform.parent = all_road_obj.transform;
        for (int i = 0; i < path_point.Count; i++)
        {
            GameObject road_point_obj = new GameObject("road_point_" + i.ToString());
            road_point_obj.transform.parent = road_obj.transform;
            trans[i] = road_point_obj.transform;
            trans[i].position = new Vector3(path_point[i].x, path_point[i].y, path_point[i].z);
        }
        pc.bezierPath = new BezierPath(trans, false, PathSpace.xyz);
        all_pc.Add(pc);
        road_id_list.Add(path_id);

        int cp_size = path_point.Count;
        if (cp_size < 4) return;
        float arc_length = 0.0f;
        float percent = 1.0f / DIVIDE_LINE;
        Vector3 cross_t;
        Vector3 orient_t = new Vector3(0, 1, 0);
        MeshFilter mf = road_obj.AddComponent<MeshFilter>();
        MeshRenderer mr = road_obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        List<int> triangles = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        bool first_calc = true;
        Vector3 preQ = GMT(path_point[cp_size-1], path_point[0], path_point[1], path_point[2],2, 0.0f);
        float dis = 0.01f;
        
        for (int i = 0; i < cp_size-1; i++)
        {
            Vector3 p0 = path_point[(i - 1 + cp_size) % cp_size];
            Vector3 p1 = path_point[i % cp_size];
            Vector3 p2 = path_point[(i + 1) % cp_size];
            Vector3 p3 = path_point[(i + 2) % cp_size];
            float t = percent;
            for (int j = 1; j < DIVIDE_LINE; j++)
            {
                Vector3 Q = GMT(p0, p1, p2, p3, 2, t);
                Vector3 backward = Q -preQ;
                cross_t = Vector3.Cross((Q - preQ), orient_t).normalized;
                cross_t = cross_t * road_width;
                arc_length += vec_len(backward);

                if (arc_length >= dis) {
                    Vector3 right_pos = Q + cross_t;
                    vertices.Add(right_pos);

                    Vector3 left_pos = Q - cross_t;
                    vertices.Add(left_pos);

                    if (!first_calc)
                    {
                        int verts = vertices.Count - 4;
                        //第一個三角形
                        triangles.Add(verts);
                        triangles.Add(verts + 2);
                        triangles.Add(verts + 1);
                        //第二個三角形
                        triangles.Add(verts + 2);
                        triangles.Add(verts + 3);
                        triangles.Add(verts + 1);
                    }
                    first_calc = false;
                    arc_length -= dis;
                }
                preQ = Q;
                t += percent;
            }

        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mf.mesh = mesh;
        mr.material = roads_polygon_mat;
        road_obj.transform.parent = all_road_obj.transform;


        //GameObject instance_p = Instantiate(view_instance);
        //instance_p.GetComponent<ViewInstance>().cam = cam;
        //instance_p.GetComponent<ViewInstance>().points = mesh.vertices;
        //instance_p.GetComponent<ViewInstance>().instance = road_obj;
        //instance_p.GetComponent<ViewInstance>().setRoad(path_id, vertices, cam, GetComponent<RoadIntegration>());
        //instance_p.GetComponent<ViewInstance>().setup(false);
        //instance_p.AddComponent<MeshCollider>();
        //List<GameObject> path_objects = new List<GameObject>();
        //path_objects.Add(instance_p);
        //pathes_objects.Add(path_id, path_objects);
    }

    Mesh createRoadPolygon(int road_index, string road_name, float road_width) // generate a road
    {
        List<int> belong_to_hier_x = new List<int>();
        List<int> belong_to_hier_y = new List<int>();
        belong_to_hier_x.Clear();
        belong_to_hier_y.Clear();
        int belong_x = 0;
        int belong_y = 0;

        Mesh mesh = new Mesh();
        Vector3[] vertex = new Vector3[osm_reader.pathes[road_index].ref_node.Count * 2];
        Vector3 f = new Vector3();
        Vector3 up = new Vector3();
        Vector3 right = new Vector3();
        for (int index = 0; index < osm_reader.pathes[road_index].ref_node.Count - 1; index++)
        {
            f = osm_reader.points_lib[osm_reader.pathes[road_index].ref_node[index + 1]].position - osm_reader.points_lib[osm_reader.pathes[road_index].ref_node[index]].position;
            up = new Vector3(0, 1, 0);
            right = Vector3.Cross(f, up).normalized * road_width;
            vertex[index * 2] = osm_reader.points_lib[osm_reader.pathes[road_index].ref_node[index]].position - right;
            vertex[index * 2 + 1] = osm_reader.points_lib[osm_reader.pathes[road_index].ref_node[index]].position + right;
        }
        vertex[(osm_reader.pathes[road_index].ref_node.Count - 1) * 2] = osm_reader.points_lib[osm_reader.pathes[road_index].ref_node[osm_reader.pathes[road_index].ref_node.Count - 1]].position - right;
        vertex[(osm_reader.pathes[road_index].ref_node.Count - 1) * 2 + 1] = osm_reader.points_lib[osm_reader.pathes[road_index].ref_node[osm_reader.pathes[road_index].ref_node.Count - 1]].position + right;

        for (int index = 0; index < vertex.Length; index++)
        {
            hierarchy_c.calcLocation(vertex[index].x, vertex[index].z, ref belong_x, ref belong_y);
            belong_to_hier_x.Add(belong_x);
            belong_to_hier_y.Add(belong_y);
        }

        //Vector2[] uv = new Vector2[osm_reader.pathes[road_index].Count];
        //for (int index = 0; index < osm_reader.pathes[road_index].Count; index++)
        //{
        //    if (index % 2 == 0)
        //        uv[index] = new Vector2(0, 0);
        //    else
        //        uv[index] = new Vector2(1, 1);
        //}

        int[] indice = new int[6 * (osm_reader.pathes[road_index].ref_node.Count - 1)];
        int point_index = 0;
        for (int index = 0; index < indice.Length; index += 6)
        {
            indice[index] = point_index;
            indice[index + 1] = point_index + 1;
            indice[index + 2] = point_index + 3;

            indice[index + 3] = point_index;
            indice[index + 4] = point_index + 3;
            indice[index + 5] = point_index + 2;
            point_index += 2;
        }

        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.triangles = indice;

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //Name the mesh
        mesh.name = road_name;

        //Return the mesh
        return mesh;
    }

    void createRoadPolygons(List<Vector3> road, string road_id, float road_width, int layer) // generate pieces of road
    {
        List<GameObject> path_objects = new List<GameObject>();
        Transform[] trans = new Transform[road.Count];
        GameObject road_obj =  new GameObject(road_id);
        PathCreator pc = road_obj.AddComponent<PathCreator>();
        road_obj.transform.parent = all_road_obj.transform;
        for (int i = 0; i < road.Count; i++)
        {
            GameObject road_point_obj = new GameObject("road_point_" + i.ToString());
            road_point_obj.transform.parent = road_obj.transform;
            trans[i] = road_point_obj.transform;
            trans[i].position = new Vector3(road[i].x, road[i].y, road[i].z);
        }
        pc.bezierPath = new BezierPath(trans, false, PathSpace.xyz);
        all_pc.Add(pc);

        //RoadMeshCreator rm = road_obj.AddComponent<RoadMeshCreator>();
        //rm.pathCreator = pc;
        //rm.roadWidth = 6.0f;
        //rm.flattenSurface = true;
        //rm.roadMaterial = roads_polygon_mat;
        //rm.undersideMaterial = roads_polygon_mat;
        //rm.TriggerUpdate();


        //smooth road
        GameObject instance_s = Instantiate(view_instance);
        instance_s.GetComponent<ViewInstance>().cam = cam;
        instance_s.GetComponent<ViewInstance>().points = road.ToArray();
        instance_s.GetComponent<ViewInstance>().instance = road_obj;
        instance_s.GetComponent<ViewInstance>().setRoad(road_id, road, cam, GetComponent<RoadIntegration>());
        instance_s.GetComponent<ViewInstance>().setup(false);
        instance_s.AddComponent<MeshCollider>();
        instance_s.transform.parent = all_road_obj.transform;

        road_id_list.Add(road_id);
        //rm.PathUpdated();
        //rm.CreateRoadMesh();
        //road.Clear();
        //for (float dis = 0.0f; dis <= 1.0f; dis += 0.05f)
        //{
        //    Vector3 point = pc.path.GetPointAtDistance(dis);
        //    road.Add(point);
        //}


        List<int> belong_to_hier_x = new List<int>();
        List<int> belong_to_hier_y = new List<int>();
        belong_to_hier_x.Clear();
        belong_to_hier_y.Clear();
        int belong_x = 0;
        int belong_y = 0;

        Vector3[][] vertex = new Vector3[(road.Count - 1) * 2][];
        Vector3 f = new Vector3();
        Vector3 up = new Vector3();
        Vector3 right = new Vector3();
        Vector3[] road_point = new Vector3[road.Count * 2];
        for (int road_point_index = 0; road_point_index < road.Count - 1; road_point_index++)
        {
            f = road[road_point_index + 1] - road[road_point_index];
            up = new Vector3(0, 1, 0);
            right = Vector3.Cross(f, up).normalized * road_width;
            road_point[road_point_index * 2] = road[road_point_index] - right + layer * up * 10;
            road_point[road_point_index * 2 + 1] = road[road_point_index] + right + layer * up * 10;
        }
        road_point[(road.Count - 1) * 2] = road[road.Count - 1] - right + layer * up * 10;
        road_point[(road.Count - 1) * 2 + 1] = road[road.Count - 1] + right + layer * up * 10;

        for (int road_point_index = 0; road_point_index < road.Count - 1; road_point_index++)
        {
            vertex[road_point_index * 2] = new Vector3[3];
            vertex[road_point_index * 2][0] = road_point[road_point_index * 2];
            vertex[road_point_index * 2][1] = road_point[road_point_index * 2 + 1];
            vertex[road_point_index * 2][2] = road_point[road_point_index * 2 + 3];

            vertex[road_point_index * 2 + 1] = new Vector3[3];
            vertex[road_point_index * 2 + 1][0] = road_point[road_point_index * 2];
            vertex[road_point_index * 2 + 1][1] = road_point[road_point_index * 2 + 3];
            vertex[road_point_index * 2 + 1][2] = road_point[road_point_index * 2 + 2];
        }


        for (int piece_index = 0; piece_index < vertex.Length; piece_index++)
        {
            belong_to_hier_x.Clear();
            belong_to_hier_y.Clear();

            Mesh mesh = new Mesh();

            for (int vertex_indice = 0; vertex_indice < 3; vertex_indice++)
            {
                hierarchy_c.calcLocation(vertex[piece_index][vertex_indice].x, vertex[piece_index][vertex_indice].z, ref belong_x, ref belong_y);
                belong_to_hier_x.Add(belong_x);
                belong_to_hier_y.Add(belong_y);
            }

            int[] indice = new int[3];
            indice[0] = 0;
            indice[1] = 1;
            indice[2] = 2;

            //Assign data to mesh
            mesh.vertices = vertex[piece_index];
            mesh.triangles = indice;

            //Recalculations
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            //Name the mesh
            mesh.name = road_id;

            GameObject road_peice = new GameObject();
            road_peice.name = "instance_" + road_id + "_" + piece_index;
            MeshFilter mf = road_peice.AddComponent<MeshFilter>();
            MeshRenderer mr = road_peice.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = roads_polygon_mat;
            road_peice.transform.parent = road_manager.transform;

            //road_peice.AddComponent<ViewInstance>();
            //road_peice.GetComponent<ViewInstance>().cam = cam;
            //road_peice.GetComponent<ViewInstance>().points = vertex[piece_index];
            //road_peice.GetComponent<ViewInstance>().instance = road_peice;
            //road_peice.GetComponent<ViewInstance>().setup(false, false);
            //road_peice.transform.parent = road_manager.transform;
            GameObject instance_p = Instantiate(view_instance);
            instance_p.GetComponent<ViewInstance>().cam = cam;
            instance_p.GetComponent<ViewInstance>().points = vertex[piece_index];
            instance_p.GetComponent<ViewInstance>().instance = road_peice;
            instance_p.GetComponent<ViewInstance>().setRoad(road_id, new List<Vector3>(vertex[piece_index]), cam, GetComponent<RoadIntegration>());
            instance_p.GetComponent<ViewInstance>().setup(false);
            instance_p.AddComponent<MeshCollider>();



            if (road_id == "330745386")
            {
                Debug.Log("road_" + road_id + "_" + piece_index);
                Debug.Log(road_point[piece_index]);
            }
            //instance_p.GetComponent<MeshCollider>().sharedMesh = mesh;
            instance_p.transform.parent = road_manager.transform;
            //if (road_id == "407209896" && piece_index == 52) // 其中一片
            //instance_p.GetComponent<ViewInstance>().finish_instance = true;
            instance_p.name = "road_" + road_id + "_" + piece_index;

            for (int belong_index = 0; belong_index < belong_to_hier_x.Count; belong_index++)
            {
                hierarchy_c.heirarchy_master[belong_to_hier_x[belong_index], belong_to_hier_y[belong_index]].objects.Add(instance_p);
            }

            path_objects.Add(instance_p);
        }

        pathes_objects.Add(road_id, path_objects);
    }

    Mesh createHousePolygon(List<string> house_point_ids, int house_index, string house_id) // generate a house polygon
    {
        List<int> belong_to_hier_x = new List<int>();
        List<int> belong_to_hier_y = new List<int>();
        belong_to_hier_x.Clear();
        belong_to_hier_y.Clear();
        int belong_x = 0;
        int belong_y = 0;

        Mesh mesh = new Mesh();
        // generate polygon vertex
        Vector3[] vertex = new Vector3[house_point_ids.Count - 1];
        // classification hierarchy area
        for (int index = 0; index < vertex.Length; index++)
        {
            hierarchy_c.calcLocation(vertex[index].x, vertex[index].z, ref belong_x, ref belong_y);
            belong_to_hier_x.Add(belong_x);
            belong_to_hier_y.Add(belong_y);
        }

        // for shape grammar
        Vector2[] vertex2D = new Vector2[house_point_ids.Count - 1];
        for (int index = 0; index < house_point_ids.Count - 1; index++)
        {
            vertex2D[index] = new Vector2(osm_reader.points_lib[house_point_ids[index]].position.x, osm_reader.points_lib[house_point_ids[index]].position.z);
        }

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertex2D);
        int[] indices = tr.Triangulate();

        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.triangles = indices;

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //Name the mesh
        mesh.name = house_id;

        // create a gameobject to scene
        houses_polygon[house_index] = new GameObject();
        houses_polygon[house_index].name = "instance_" + house_id;
        houses_polygon[house_index].transform.parent = house_polygon_manager.transform;
        MeshFilter mf = houses_polygon[house_index].AddComponent<MeshFilter>();
        MeshRenderer mr = houses_polygon[house_index].AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = houses_polygon_mat;

        // managed by heirarchy
        GameObject instance_h = Instantiate(view_instance);
        instance_h.GetComponent<ViewInstance>().cam = cam;
        instance_h.GetComponent<ViewInstance>().points = vertex;
        instance_h.GetComponent<ViewInstance>().instance = houses_polygon[house_index];
        instance_h.GetComponent<ViewInstance>().setup(false);
        instance_h.name = "housePolygon_" + house_id;
        instance_h.transform.parent = house_polygon_manager.transform;

        // add to heirarchy system
        for (int belong_index = 0; belong_index < belong_to_hier_x.Count; belong_index++)
        {
            hierarchy_c.heirarchy_master[belong_to_hier_x[belong_index], belong_to_hier_y[belong_index]].objects.Add(instance_h);
        }



        //Return the points
        return mesh;
    }

    IEnumerator createHousePolygon(List<string> house_point_ids, int house_index, string house_id, int context_id) // generate a house polygon
    {
        List<int> belong_to_hier_x = new List<int>();
        List<int> belong_to_hier_y = new List<int>();
        belong_to_hier_x.Clear();
        belong_to_hier_y.Clear();
        int belong_x = 0;
        int belong_y = 0;

        Mesh mesh = new Mesh();
        // generate polygon vertex
        Vector3[] vertex = new Vector3[house_point_ids.Count - 1];
        float ele_min = 100000.0f;
        for (int index = 0; index < house_point_ids.Count - 1; index++)
        {
            vertex[index] = osm_reader.points_lib[house_point_ids[index]].position;
            ele_min = Mathf.Min(ele_min, vertex[index].y);
        }

        // classification hierarchy area
        for (int index = 0; index < vertex.Length; index++)
        {
            hierarchy_c.calcLocation(vertex[index].x, vertex[index].z, ref belong_x, ref belong_y);
            belong_to_hier_x.Add(belong_x);
            belong_to_hier_y.Add(belong_y);
        }

        // for shape grammar
        Vector2[] vertex2D = new Vector2[house_point_ids.Count - 1];
        for (int index = 0; index < house_point_ids.Count - 1; index++)
        {
            vertex2D[index] = new Vector2(osm_reader.points_lib[house_point_ids[index]].position.x, osm_reader.points_lib[house_point_ids[index]].position.z);
        }

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertex2D);
        int[] indices = tr.Triangulate();

        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.triangles = indices;

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //Name the mesh
        mesh.name = house_id;

        // create a gameobject to scene
        houses_polygon[house_index] = new GameObject();
        houses_polygon[house_index].name = "instance_" + house_id;
        houses_polygon[house_index].transform.parent = house_polygon_manager.transform;
        MeshFilter mf = houses_polygon[house_index].AddComponent<MeshFilter>();
        MeshRenderer mr = houses_polygon[house_index].AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = houses_polygon_mat;

        // ======================================================
        // Procedural Modeling of house
        List<Vector2> polygon = new List<Vector2>(vertex2D);
        Vector2 maxLen = Vector2.zero;
        Vector2 minLen = polygon[0];

        // bound record
        for (int i = 0; i < polygon.Count; i++)
        {
            if (polygon[i].x > maxLen.x)
                maxLen.x = polygon[i].x;
            if (polygon[i].y > maxLen.y)
                maxLen.y = polygon[i].y;
            if (polygon[i].x < minLen.x)
                minLen.x = polygon[i].x;
            if (polygon[i].y < minLen.y)
                minLen.y = polygon[i].y;
        }
        Vector2 maxSize = maxLen - minLen;
        Vector2 center2d = (maxLen + minLen) / 2;

        //for (int i = 0; i < vertex2D.Length; i++)
        //{
        //    center2d += vertex2D[i];
        //}
        //center2d /= vertex2D.Length;
        float building_height = Random.Range(10, 20);

        Vector3 center = new Vector3(center2d.x, ele_min, center2d.y);

        // Init house and set parameters
        ShapeGrammarBuilder.InitObject(context_id);
        ShapeGrammarBuilder.setIdLength(3, context_id);
        ShapeGrammarBuilder.AddPolygon("001", polygon, context_id);
        string filename = ".\\grammars\\hello_house.shp";
        ShapeGrammarBuilder.loadShape(filename, context_id);
        ShapeGrammarBuilder.setParam("height", building_height.ToString(), context_id);
        ShapeGrammarBuilder.setParam("splitfacade", Random.Range(2, 6).ToString(), context_id);
        ShapeGrammarBuilder.setParam("maxSize", Mathf.Max(Mathf.Abs(maxSize.x), Mathf.Abs(maxSize.y)).ToString(), context_id);
        
        yield return null;
        // build the house mesh
        bool done = false;
        //Debug.Log("Start Thread: " + id.ToString());
        //System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //{
        //    ShapeGrammarBuilder.buildShape(context_id);
        //    done = true;
        //});


        BuildingCreationJob job = new BuildingCreationJob {
            _context_id = context_id
        };

        var jobHandle = job.Schedule();

        for (int i = 0; i < house_index / 3; i++)
        {
            yield return null;
        }
        jobHandle.Complete();
        /*
        Thread thread = new Thread(() =>
        {
            ShapeGrammarBuilder.buildShape(context_id);
            done = true;
        });
        thread.Start();

        // wait until function finish
        while (!done)
        {
            Debug.Log("wait for creation");
            yield return null;
        }*/

        Debug.Log("Finished Thread: " + house_index.ToString());

        // record the mesh in obj and mtl format
        string obj = "", mtl = "";
        ShapeGrammarBuilder.buildMesh(ref obj, ref mtl, context_id);
        ShapeGrammarBuilder.destroyContext(context_id);
        // Procedural Modeling of house
        // ======================================================

        // managed by heirarchy
        GameObject instance_h = Instantiate(view_instance);
        instance_h.GetComponent<ViewInstance>().cam = cam;
        instance_h.GetComponent<ViewInstance>().points = vertex;
        instance_h.GetComponent<ViewInstance>().instance = houses_polygon[house_index];
        instance_h.GetComponent<ViewInstance>().setup(false);
        instance_h.GetComponent<ViewInstance>().building_height = building_height;


        // bind the house information to ViewInstance
        instance_h.GetComponent<ViewInstance>().setHouse(house_id, obj, mtl, center);

        instance_h.name = "housePolygon_" + house_id;
        instance_h.transform.parent = house_polygon_manager.transform;
        instance_h.transform.position = new Vector3(instance_h.transform.position.x, ele_min, instance_h.transform.position.z);

        // add to heirarchy system
        for (int belong_index = 0; belong_index < belong_to_hier_x.Count; belong_index++)
        {
            hierarchy_c.heirarchy_master[belong_to_hier_x[belong_index], belong_to_hier_y[belong_index]].objects.Add(instance_h);
        }

        Debug.Log("A building creation finished");
        yield return null;
    }

    IEnumerator getIndexCreateMesh(int index)
    {
        Debug.Log("A building creation Init-----");
        // wait for the free builder to build mesh
        int context_index = ShapeGrammarBuilder.getFreeStack();
        while (context_index == -1)
        {
            context_index = ShapeGrammarBuilder.getFreeStack();
            yield return null;
        }
        Debug.Log("A building creation start");
        StartCoroutine(createHousePolygon(osm_reader.houses[index].ref_node, index, osm_reader.houses[index].id, context_index));
    }

    //void createVirtualCam()
    //{
    //    // virtual camera
    //    virtualCamera = UnityEngine.Object.Instantiate(virtualCameraPrefab);
    //    CinemachineTrackedDolly trackedDolly = virtualCamera.AddCinemachineComponent<CinemachineTrackedDolly>();
    //    CinemachineSmoothPath smoothPath = new GameObject("DollyTrack").AddComponent<CinemachineSmoothPath>(); //camera_path.GetComponent<CinemachineSmoothPath>();
    //    List<Vector3> way_points = new List<Vector3>();
    //    for (int index = 0; index < osm_reader.pathes.Count; index++)
    //    {
    //        if (osm_reader.pathes[index].id == "689317173") //28492749 689317173竹子湖路 226830312環金路 23241821基隆路四段 45263678_226830312環金合體 308724899公館圓環 238689696NTUST前基隆高架橋 286408368_774696439關聯大路
    //        {
    //            smoothPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[osm_reader.pathes[index].ref_node.Count];
    //            for (int point_index = 0; point_index < osm_reader.pathes[index].ref_node.Count; point_index++)
    //            {
    //                smoothPath.m_Waypoints[point_index] = new CinemachineSmoothPath.Waypoint();
    //                smoothPath.m_Waypoints[point_index].position = osm_reader.points_lib[osm_reader.pathes[index].ref_node[point_index]].position + new Vector3(0, 1, 0) * osm_reader.pathes[index].layer * 10 + new Vector3(0, 3.5f, 0);
    //            }
    //            camera_path_index = index;
    //        }
    //    }
    //    trackedDolly.m_Path = smoothPath;
    //    trackedDolly.m_PathPosition = 0;
    //}

    void setCam()
    {
        cam.transform.position = osm_reader.points_lib[set_camera_to_point_id].position + new Vector3(0, 3.5f, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        ShapeGrammarBuilder.InitClass();
        osm_reader = new OSMReader();
        osm_reader.readOSM(Application.streamingAssetsPath + "//" + file_name, need_write_osm3d, Application.streamingAssetsPath + "//" + osm3d_file_name);
        if (!need_write_osm3d)
        {
            hierarchy_c = new HierarchyControl();
            hierarchy_c.setup((int)(osm_reader.boundary_max.x - osm_reader.boundary_min.x) / 200, (int)(osm_reader.boundary_max.y - osm_reader.boundary_min.y) / 200, osm_reader.boundary_max.x, osm_reader.boundary_max.y);

            house_polygon_manager = new GameObject("House Polygon Manager");
            road_manager = new GameObject("Road Manager");
            tree_manager = new GameObject("Tree Manager");

            // instance
            //roads_polygon = new GameObject[osm_reader.pathes.Count];

            if (show_osm_points)
            {
                point_manager = new GameObject("Point Manager");
                foreach (KeyValuePair<string, Node> nn in osm_reader.points_lib)
                {
                    GameObject bb = Instantiate(sphere_prefab, nn.Value.position, Quaternion.identity);
                    string ans = "connectway";
                    for (int i = 0; i < nn.Value.connect_way.Count; i++)
                        ans += " & " + nn.Value.connect_way[i];
                    bb.name = nn.Key + ans;
                    bb.transform.parent = point_manager.transform;
                }
            }
            all_road_obj = new GameObject("All_Roads");
            all_pc = new List<PathCreator>();
            road_id_list = new List<string>();
            pathes_objects = new Dictionary<string, List<GameObject>>();
            int road_index = 0;
            for (road_index = 0; road_index < osm_reader.pathes.Count; road_index++)
            {
                if (osm_reader.pathes[road_index].is_merged)
                    break;

                //if (osm_reader.pathes[road_index].highway != Highway.Primary && osm_reader.pathes[road_index].highway != Highway.Secondary && osm_reader.pathes[road_index].highway != Highway.Trunk && osm_reader.pathes[road_index].highway != Highway.Unclassified)
                //    continue;

                // roads
                createRoadPolygons(osm_reader.toPositions(osm_reader.pathes[road_index].ref_node), osm_reader.pathes[road_index].id, osm_reader.pathes[road_index].road_width, osm_reader.pathes[road_index].layer);
                //createSmoothRoadPolygons(osm_reader.toPositions(osm_reader.pathes[road_index].ref_node), osm_reader.pathes[road_index].id, osm_reader.pathes[road_index].road_width);
                if (osm_reader.pathes[road_index].layer != 0)
                    continue;

                // trees
                createArcTree(osm_reader.toPositions(osm_reader.pathes[road_index].ref_node), osm_reader.pathes[road_index].id, osm_reader.pathes[road_index].road_width + 2);
            }

            // new way
            for (; road_index < osm_reader.pathes.Count; road_index++)
            {
                createRoadPolygons(osm_reader.toPositions(osm_reader.pathes[road_index].ref_node), osm_reader.pathes[road_index].id, osm_reader.pathes[road_index].road_width, osm_reader.pathes[road_index].layer);
                //createSmoothRoadPolygons(osm_reader.toPositions(osm_reader.pathes[road_index].ref_node), osm_reader.pathes[road_index].id, osm_reader.pathes[road_index].road_width);
                if (osm_reader.pathes[road_index].layer != 0)
                    continue;

                // trees
                if (osm_reader.pathes[road_index].highway != Highway.CombineLink) // link road is in merged road
                    createArcTree(osm_reader.toPositions(osm_reader.pathes[road_index].ref_node), osm_reader.pathes[road_index].id, osm_reader.pathes[road_index].road_width + 2);
            }

            road_choose.AddOptions(road_id_list);

            Debug.Log("Tree amount: " + count); // trees amount

            houses_polygon = new GameObject[osm_reader.houses.Count];
            house_mesh = new GameObject[osm_reader.houses.Count];
            for (int index = 0; index < osm_reader.houses.Count; index++)
            {
                // build houses or polygon
                if (!build_house)
                    createHousePolygon(osm_reader.houses[index].ref_node, index, osm_reader.houses[index].id);
                else
                    StartCoroutine(getIndexCreateMesh(index));
            }

            //createVirtualCam();
            setCam();

            hierarchy_c.beginHierarchy();

            finish_create = true;
            Debug.Log("orm start finished");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (finish_create)
        {
            int x = 0, y = 0;
            hierarchy_c.calcLocation(cam.transform.position.x, cam.transform.position.z, ref x, ref y);
            hierarchy_c.lookHierarchy();
        }
        //createVisibleHouse();
    }

    //void createHouseMesh(string house_id, int index)
    //{
    //    List<Vector2> polygon = osm_reader.getHousePolygon(house_id);
    //    Vector2 maxLen = Vector2.zero;
    //    Vector2 minLen = polygon[0];
    //    for (int i = 0; i < polygon.Count; i++)
    //    {
    //        if (polygon[i].x > maxLen.x)
    //            maxLen.x = polygon[i].x;
    //        if (polygon[i].y > maxLen.y)
    //            maxLen.y = polygon[i].y;
    //        if (polygon[i].x < minLen.x)
    //            minLen.x = polygon[i].x;
    //        if (polygon[i].y < minLen.y)
    //            minLen.y = polygon[i].y;
    //    }
    //    Vector2 maxSize = maxLen - minLen;
    //    Vector2 center = (maxLen + minLen) / 2;

    //    ShapeGrammarBuilder.InitObject();
    //    ShapeGrammarBuilder.setIdLength(3);
    //    ShapeGrammarBuilder.AddPolygon("001", polygon);
    //    string filename = ".\\grammars\\hello_house2.shp";
    //    ShapeGrammarBuilder.loadShape(filename);
    //    ShapeGrammarBuilder.setParam("height", Random.Range(10, 20).ToString());
    //    ShapeGrammarBuilder.setParam("splitfacade", Random.Range(2, 6).ToString());
    //    ShapeGrammarBuilder.setParam("maxSize", Mathf.Max(Mathf.Abs(maxSize.x), Mathf.Abs(maxSize.y)).ToString());

    //    ShapeGrammarBuilder.buildShape();
    //    GameObject house_instance = ShapeGrammarBuilder.buildMesh();
    //    house_instance.name = "house_" + house_id;
    //    Mesh mesh = house_instance.GetComponentInChildren<MeshFilter>().mesh;
    //    //Recalculations
    //    mesh.RecalculateNormals();
    //    mesh.RecalculateBounds();
    //    mesh.Optimize();
    //    house_instance.GetComponentInChildren<MeshFilter>().mesh = mesh;
    //    Transform temp = house_instance.GetComponentsInChildren<Transform>()[1];
    //    //temp.transform.position = new Vector3(center.x, 0, center.y);
    //    Bounds bound = mesh.bounds;
    //    house_instance.transform.position = new Vector3(center.x - 31, -bound.center.y, center.y + 35);
    //    house_instance.transform.Rotate(new Vector3(0, 183, 0));
    //    house_mesh[index] = (house_instance);
    //}

    //void createVisibleHouse()
    //{
    //    for (int i = 0; i < osm_reader.houses.Count; i++)
    //    {
    //        if (osm_reader.houses_id[i] != "374912754") continue; // the house is on road 689317173
    //        if (houses_polygon[i].active && house_mesh[i] == null)
    //        {

    //            createHouseMesh(osm_reader.houses_id[i], i);
    //        }
    //        else
    //        {
    //            if (house_mesh[i] == null)
    //            {
    //                Destroy(house_mesh[i]);
    //                house_mesh[i] = null;
    //            }
    //        }
    //    }
    //    Resources.UnloadUnusedAssets();
    //}
}

[BurstCompile]
public struct BuildingCreationJob : IJob
{
    public int _context_id;
    public void Execute()
    {
        ShapeGrammarBuilder.buildShape(this._context_id);
    }
}