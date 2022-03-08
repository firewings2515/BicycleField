using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class HouseGenerator
{
    static string component_name = "floors_base";
    static string[] grammar_files_path = new string[] {
        @"Assets\Grammars\test.txt"
    };
    static public bgBuilder builder;
    static Dictionary<int, Dictionary<int, GameObject>> gobj_db;
    static List<int> segment_id_q;

    static public void init() {
        builder = new bgBuilder(grammar_files_path);
        gobj_db = new Dictionary<int, Dictionary<int, GameObject>>();
        segment_id_q = new List<int>();
    }

    static public void generateHouse(int segment_id, int house_id, string info)
    {
        //demo code
        string[] house_infos = info.Split(' ');
        Vector3 single_point = new Vector3(int.Parse(house_infos[2]), int.Parse(house_infos[3]), int.Parse(house_infos[4]));
        GameObject gobj = builder.build(component_name);
        gobj.transform.position = single_point;
        if (!gobj_db.ContainsKey(segment_id)) {
            gobj_db.Add(segment_id, new Dictionary<int, GameObject>());
            segment_id_q.Add(segment_id);
        }
        gobj_db[segment_id].Add(house_id, gobj);
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
}
