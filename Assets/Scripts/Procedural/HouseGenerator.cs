using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class HouseGenerator
{
    static string[] component_names = new string[] { 
        "house1","floors_base","hello_house"
    };
    static string[] grammar_files_path = new string[] {
        @"Assets\Grammars\test.txt",
        @"Assets\Grammars\house1.txt",
        @"Assets\Grammars\backgrounds.txt"
    };
    static public bgBuilder builder = new bgBuilder(grammar_files_path);
    static Dictionary<int, Dictionary<int, GameObject>> gobj_db = new Dictionary<int, Dictionary<int, GameObject>>();
    static List<int> segment_id_q = new List<int>();

    static public void init() {
        //builder = new bgBuilder(grammar_files_path);
        //gobj_db = new Dictionary<int, Dictionary<int, GameObject>>();
        //segment_id_q = new List<int>();
    }
    static public IEnumerator generateHouses(List<int> segment_id,List<int> house_id,List<string> info) {
        int count = segment_id.Count;
        for (int i = 0; i < count; i++) {
            generateHouse(segment_id[i],house_id[i],info[i]);
            //break;
            yield return new WaitForSeconds(0.3f);
        }
        yield return null;
    }
    static public void generateHouse(int segment_id, int house_id, string info)
    {
        //demo code
        string[] house_infos = info.Split(' ');
        Vector3 single_point = new Vector3(float.Parse(house_infos[2]), float.Parse(house_infos[3]), float.Parse(house_infos[4])) + new Vector3(-200, 0, -200);
        GameObject gobj;
        if (single_point.y < -0.5f)
        {
            gobj = builder.build("polygon_house1");
        }
        else {
            gobj = builder.build(component_names[Random.Range(0, component_names.Length)]);
        }
        gobj.transform.position = single_point;
        gobj.transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
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
}
