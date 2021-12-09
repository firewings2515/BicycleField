using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSMHousePolygonRender : MonoBehaviour
{
    OSMEditor osm_editor;
    public bool is_initial = false;
    public Material house_polygon_mat;
    public Dictionary<string, GameObject> house_polygons_objects;
    GameObject house_polygons_manager;

    // Start is called before the first frame update
    void Start()
    {
        osm_editor = GetComponent<OSMEditor>();
        house_polygons_manager = new GameObject("house_polygons_manager");
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_initial && osm_editor.is_initial)
        {
            // OSMHousePolygonRender is initail
            is_initial = true;

            // manage all houses view_instance
            house_polygons_objects = new Dictionary<string, GameObject>();

            // process houses render
            for (int house_index = 0; house_index < osm_editor.osm_reader.houses.Count; house_index++)
            {
                createHousePolygon(osm_editor.osm_reader.houses[house_index]);
            }
        }
    }

    Mesh createHousePolygon(House house) // generate a house polygon
    {
        List<int> belong_to_hier_x = new List<int>();
        List<int> belong_to_hier_y = new List<int>();
        belong_to_hier_x.Clear();
        belong_to_hier_y.Clear();
        int belong_x = 0;
        int belong_y = 0;

        Mesh mesh = new Mesh();
        // generate polygon vertex
        Vector3[] vertex = new Vector3[house.ref_node.Count - 1];
        float ele_min = 100000.0f;
        Vector2 max_len = Vector2.negativeInfinity;
        Vector2 min_len = Vector2.positiveInfinity;
        for (int index = 0; index < house.ref_node.Count - 1; index++)
        {
            vertex[index] = osm_editor.osm_reader.points_lib[house.ref_node[index]].position;
            ele_min = Mathf.Min(ele_min, vertex[index].y);

            // bound record
            if (vertex[index].x > min_len.x)
                max_len.x = vertex[index].x;
            if (vertex[index].y > min_len.y)
                max_len.y = vertex[index].y;
            if (vertex[index].x < min_len.x)
                max_len.x = vertex[index].x;
            if (vertex[index].y < min_len.y)
                max_len.y = vertex[index].y;
        }
        Vector2 maxSize = max_len - min_len;
        Vector2 center2d = (max_len + min_len) / 2;
        Vector3 center = new Vector3(center2d.x, ele_min, center2d.y);

        // classification hierarchy area
        for (int index = 0; index < vertex.Length; index++)
        {
            osm_editor.hierarchy_c.calcLocation(vertex[index].x, vertex[index].z, ref belong_x, ref belong_y);
            belong_to_hier_x.Add(belong_x);
            belong_to_hier_y.Add(belong_y);
        }

        // for shape grammar
        Vector2[] vertex2D = new Vector2[house.ref_node.Count - 1];
        for (int index = 0; index < house.ref_node.Count - 1; index++)
        {
            vertex2D[index] = new Vector2(osm_editor.osm_reader.points_lib[house.ref_node[index]].position.x, osm_editor.osm_reader.points_lib[house.ref_node[index]].position.z);
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
        mesh.name = house.id;

        // create a gameobject to scene
        GameObject house_polygon = new GameObject();
        house_polygon.name = "instance_" + house.id;
        MeshFilter mf = house_polygon.AddComponent<MeshFilter>();
        MeshRenderer mr = house_polygon.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = house_polygon_mat;
        house_polygon.transform.parent = house_polygons_manager.transform;

        // managed by heirarchy
        GameObject instance_h = Instantiate(osm_editor.view_instance);
        instance_h.GetComponent<ViewInstance>().instance = house_polygon;
        instance_h.GetComponent<ViewInstance>().setHouse(house.id, vertex, center, GetComponent<OSMEditor>().cam, GetComponent<RoadIntegration>());
        instance_h.name = "housePolygon_" + house.id;
        instance_h.transform.parent = house_polygons_manager.transform;

        // add to heirarchy system
        for (int belong_index = 0; belong_index < belong_to_hier_x.Count; belong_index++)
        {
            osm_editor.hierarchy_c.heirarchy_master[belong_to_hier_x[belong_index], belong_to_hier_y[belong_index]].objects.Add(instance_h);
        }

        house_polygons_objects.Add(house.id, instance_h);

        //Return the points
        return mesh;
    }
}
