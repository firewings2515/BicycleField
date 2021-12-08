using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class OSMRoadRender : MonoBehaviour
{
    OSMEditor osm_editor;
    public bool is_initial = false;
    public Material roads_polygon_mat;
    public Dictionary<string, List<GameObject>> pathes_objects;
    GameObject roads_manager;
    public GameObject road_name_prefab;
    GameObject road_textes_manager;

    // Start is called before the first frame update
    void Start()
    {
        osm_editor = GetComponent<OSMEditor>();
        roads_manager = new GameObject("roads_manager");
        road_textes_manager = new GameObject("road_textes_manager");
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_initial && osm_editor.is_initial)
        {
            // OSMRoadRender is initail
            is_initial = true;

            // manage all pathes view_instance
            pathes_objects = new Dictionary<string, List<GameObject>>();

            // process roads render
            for (int road_index = 0; road_index < osm_editor.osm_reader.pathes.Count; road_index++)
            {
                // for conditional road
                //if (osm_editor.osm_reader.pathes[road_index].highway != Highway.Primary && osm_editor.osm_reader.pathes[road_index].highway != Highway.Secondary && osm_editor.osm_reader.pathes[road_index].highway != Highway.Trunk && osm_editor.osm_reader.pathes[road_index].highway != Highway.Unclassified)
                //    continue;

                // roads
                createRoadPolygons(osm_editor.osm_reader.pathes[road_index]);
            }
        }
    }

    void createRoadPolygons(Way path) // generate pieces of road
    {
        List<Vector3> path_points = osm_editor.osm_reader.toPositions(path.ref_node);
        GameObject road_manager = new GameObject(path.id);
        List<GameObject> path_objects = new List<GameObject>();
        List<int> belong_to_hier_x = new List<int>();
        List<int> belong_to_hier_y = new List<int>();
        int belong_x = 0;
        int belong_y = 0;

        // Bezier roads =============================================================
        //Mesh mesh = new Mesh();

        //Transform[] trans = new Transform[path_points.Count];
        //GameObject road_obj = new GameObject(path.id);
        //for (int i = 0; i < path_points.Count; i++)
        //{
        //    GameObject road_point_obj = new GameObject("road_point_" + i.ToString());
        //    road_point_obj.transform.parent = road_obj.transform;
        //    trans[i] = road_point_obj.transform;
        //    trans[i].position = new Vector3(path_points[i].x, path_points[i].y, path_points[i].z);

        //    hierarchy_c.calcLocation(path_points[i].x, path_points[i].z, ref belong_x, ref belong_y);
        //    belong_to_hier_x.Add(belong_x);
        //    belong_to_hier_y.Add(belong_y);
        //}
        //PathCreator pc = road_obj.AddComponent<PathCreator>();
        //pc.bezierPath = new BezierPath(trans, false, PathSpace.xyz);
        ////all_pc.Add(pc);
        //RoadMeshCreator rm = road_obj.AddComponent<RoadMeshCreator>();
        //rm.pathCreator = pc;
        //rm.roadWidth = 6.0f;
        //rm.flattenSurface = true;
        //rm.roadMaterial = roads_polygon_mat;
        //rm.undersideMaterial = roads_polygon_mat;
        //rm.TriggerUpdate();

        ////smooth road
        //GameObject instance_s = Instantiate(view_instance);
        //instance_s.GetComponent<ViewInstance>().instance = road_obj;
        //instance_s.GetComponent<ViewInstance>().setRoad(path.id, path_points, cam, GetComponent<RoadIntegration>());
        //instance_s.AddComponent<MeshCollider>();
        //instance_s.GetComponent<MeshCollider>().sharedMesh = road_obj.GetComponent<MeshFilter>().mesh;
        //road_obj.transform.parent = instance_s.transform;

        //for (int belong_index = 0; belong_index < belong_to_hier_x.Count; belong_index++)
        //{
        //    hierarchy_c.heirarchy_master[belong_to_hier_x[belong_index], belong_to_hier_y[belong_index]].objects.Add(instance_s);
        //}
        // ==========================================================================

        // linear roads =============================================================
        Vector3[][] vertex = new Vector3[(path_points.Count - 1) * 2][];
        Vector3 f = new Vector3();
        Vector3 up = new Vector3();
        Vector3 right = new Vector3();
        Vector3[] road_point = new Vector3[path_points.Count * 2];
        for (int road_point_index = 0; road_point_index < path_points.Count - 1; road_point_index++)
        {
            f = path_points[road_point_index + 1] - path_points[road_point_index];
            up = new Vector3(0, 1, 0);
            right = Vector3.Cross(f, up).normalized * path.road_width;
            road_point[road_point_index * 2] = path_points[road_point_index] - right + path.layer * up * 10;
            road_point[road_point_index * 2 + 1] = path_points[road_point_index] + right + path.layer * up * 10;
        }
        road_point[(path_points.Count - 1) * 2] = path_points[path_points.Count - 1] - right + path.layer * up * 10;
        road_point[(path_points.Count - 1) * 2 + 1] = path_points[path_points.Count - 1] + right + path.layer * up * 10;

        for (int road_point_index = 0; road_point_index < path_points.Count - 1; road_point_index++)
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
                osm_editor.hierarchy_c.calcLocation(vertex[piece_index][vertex_indice].x, vertex[piece_index][vertex_indice].z, ref belong_x, ref belong_y);
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
            mesh.name = path.id;

            // managed by view_instance
            GameObject road_peice = new GameObject();
            road_peice.name = "instance_" + path.id + "_" + piece_index;
            MeshFilter mf = road_peice.AddComponent<MeshFilter>();
            MeshRenderer mr = road_peice.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = roads_polygon_mat;
            road_peice.transform.parent = road_manager.transform;

            // view_instance
            GameObject instance_p = Instantiate(osm_editor.view_instance);
            instance_p.GetComponent<ViewInstance>().instance = road_peice;
            instance_p.GetComponent<ViewInstance>().setRoad(path.id, vertex[piece_index], GetComponent<OSMEditor>().cam, GetComponent<RoadIntegration>());

            // solve mesh failed and clean problem
            if (vertex[piece_index][0] != vertex[piece_index][1] && vertex[piece_index][1] != vertex[piece_index][2] && vertex[piece_index][2] != vertex[piece_index][0])
            {
                instance_p.AddComponent<MeshCollider>();
                instance_p.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
                instance_p.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
            instance_p.name = "road_" + path.id + "_" + piece_index;
            instance_p.transform.parent = road_manager.transform;

            // catalog hierarchy
            for (int belong_index = 0; belong_index < belong_to_hier_x.Count; belong_index++)
            {
                osm_editor.hierarchy_c.heirarchy_master[belong_to_hier_x[belong_index], belong_to_hier_y[belong_index]].objects.Add(instance_p);
            }
            instance_p.GetComponent<ViewInstance>().belong_to_hier_x = belong_to_hier_x;
            instance_p.GetComponent<ViewInstance>().belong_to_hier_y = belong_to_hier_y;

            path_objects.Add(instance_p);
        }

        pathes_objects.Add(path.id, path_objects);
        road_manager.transform.parent = roads_manager.transform;
        // ==========================================================================

        // show text on the road
        Vector3 text_center = path_points[path_points.Count / 2] + new Vector3(0, 10, 0);
        GameObject road_name = Instantiate(road_name_prefab);
        road_name.GetComponent<TMPro.TextMeshPro>().text = path.name;
        road_name.GetComponent<TMPro.TextMeshPro>().rectTransform.position = text_center;
        road_name.name = path.name;
        road_name.transform.parent = road_textes_manager.transform;
    }
}