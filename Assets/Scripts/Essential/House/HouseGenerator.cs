using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class HouseGenerator
{
    static string[] component_names = new string[] { 
        "house1","floors_base","hello_house","polygon_house1"
    };

    static string[] facade_names = new string[] {
        "hello_facade2","hello_facade","hello_facade3","hello_facade4","hello_facade5"
    };

    static string[] grammar_files_path = new string[] {
        @"Assets\Grammars\test.txt",
        @"Assets\Grammars\house1.txt",
        @"Assets\Grammars\backgrounds.txt"
    };
    static public bgBuilder builder = new bgBuilder(grammar_files_path);
    static Dictionary<int, Dictionary<int, GameObject>> gobj_db = new Dictionary<int, Dictionary<int, GameObject>>();
    static List<int> segment_id_q = new List<int>();

    static public Queue<List<int>> queue_segment_id = new Queue<List<int>>();
    static public Queue<List<int>> queue_house_id = new Queue<List<int>>();
    static public Queue<List<string>> queue_info = new Queue<List<string>>();

    static public GameObject house_manager;

    static public void init() {
        //builder = new bgBuilder(grammar_files_path);
        //gobj_db = new Dictionary<int, Dictionary<int, GameObject>>();
        //segment_id_q = new List<int>();
    }
    static public void generateHouses(List<int> segment_id,List<int> house_id,List<string> info) {
        queue_segment_id.Enqueue(segment_id);
        queue_house_id.Enqueue(house_id);
        queue_info.Enqueue(info);

    }

    static public IEnumerator generateHouse(List<int> segment_id, List<int> house_id, List<string> info)
    {
        int count = segment_id.Count;
        for (int i = 0; i < count; i++)
        {
            generateHouse(segment_id[i], house_id[i], info[i]);
            //break;
            yield return new WaitForSeconds(0.3f);
        }
        yield return null;
    }

    static public void generateHouse(int segment_id, int house_id, string info)
    {
        //demo code
        string[] house_infos = info.Split(' ');
        float polygon_count = float.Parse(house_infos[1]);
        List<Vector3> points = new List<Vector3>();
        int coord_index = 2;
        float min_y = float.MaxValue;
        Vector3 single_point = new Vector3(float.Parse(house_infos[coord_index]), float.Parse(house_infos[coord_index + 1]), float.Parse(house_infos[coord_index + 2]));
        Vector3 total = new Vector3();
        //if (TerrainGenerator.is_initial)
        //{
        single_point.y = TerrainGenerator.getHeightWithBais(single_point.x, single_point.z);
        //}
        for (int i = 0; i < polygon_count; i++) {
            Vector3 point = new Vector3(float.Parse(house_infos[coord_index]),0, float.Parse(house_infos[coord_index + 2]));
            //if (TerrainGenerator.is_initial)
            //{
            point.y = TerrainGenerator.getHeightWithBais(point.x, point.z);
            //}
            if (point.y < min_y) min_y = point.y;
            points.Add(point);
            coord_index += 3;
            total.x += point.x;
            total.y += point.y;
            total.z += point.z;
        }
        Vector3 averge = total / polygon_count;
        averge.y = min_y;
        for (int i = 0; i < polygon_count; i++)
        {
            points[i] = new Vector3(points[i].x - averge.x, 0, points[i].z - averge.z);
        }


        GameObject gobj;
        //gobj = builder.build(component_names[Random.Range(0, component_names.Length)]);
        //gobj.transform.position = single_point;
        gobj = build_polygon_house(points,4);
        gobj.transform.position = averge;

        //gobj.transform.Translate(0, single_point.y,0);

        //gobj.transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
        if (!gobj_db.ContainsKey(segment_id)) {
            gobj_db.Add(segment_id, new Dictionary<int, GameObject>());
            segment_id_q.Add(segment_id);
        }
        gobj_db[segment_id].Add(house_id, gobj);
        gobj.transform.parent = house_manager.transform;
    }

    static public void destroyEarliestSegment()
    {
        //destroy all houses in segment with lowest id
        if (segment_id_q.Count > 0)
        {
            destroySegment(segment_id_q[0]);
        }
    }

    static public void destroySegment(int segment_id)
    {
        if (!gobj_db.ContainsKey(segment_id)) return;
        //destroy all houses in segment_id
        foreach (var item in gobj_db[segment_id])
        {
            Object.Destroy(item.Value);
        }
        gobj_db.Remove(segment_id);

        segment_id_q.Remove(segment_id);
    }

    static public void destroyHouse(int segment_id, int house_id)
    {
        //destroy house_id in segment_id
        Object.Destroy(gobj_db[segment_id][house_id]);
        gobj_db[segment_id].Remove(house_id);
    }

    static public GameObject build_polygon_house(List<Vector3> vertexs,float width_per_facade)
    {
        if (!determine_clock_wise(vertexs))
        {
            vertexs.Reverse();
        }
        List<List<string>> component_names = new List<List<string>>();
        for (int i = 0; i < vertexs.Count; i++)
        {
            component_names.Add(new List<string>());
            //int facade_count = Random.Range(1,3);
            float dis;
            if (i != vertexs.Count - 1) {
                dis = Vector3.Distance(vertexs[i], vertexs[i + 1]);
            }
            else { 
                dis = Vector3.Distance(vertexs[i], vertexs[0]);
            }
            int facade_count = (int)Mathf.Floor(dis / width_per_facade);
            if (facade_count == 0)
            {
                component_names[i].Add("nothing_facade");
                continue;
            }
            for (int j = 0; j < facade_count; j++)
            {
                component_names[i].Add(facade_names[Random.Range(0, facade_names.Length)]);
            }
            //component_names[i].Add("nothing_facade");
        }
        builder.load_base_coords("runtime_base", vertexs);
        builder.load_base_facades("runtime_base", component_names);
        return builder.build("runtime_base");
    }

    static bool determine_clock_wise(List<Vector3> points)
    { //§PÂ_¶¶®É°wOR°f®É°w
        //https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        int coords_size = points.Count;
        float result = 0.0f;
        for (int index = 0; index < coords_size; index++)
        {
            Vector3 current = points[index];
            Vector3 next = points[(index + 1) % coords_size];
            result += (next.x - current.x) * (next.z + current.z);
        }
        return result >= 0.0f;
    }
}
