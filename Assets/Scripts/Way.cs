using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Highway
{
    Motorway,
    Trunk,
    Primary,
    Secondary,
    Tertiary,
    Unclassified,
    Residential,
    CombineLink,
    None
}

public class Way
{
    // Only choose one of ref_node or node_position to create the way
    public List<string> ref_node = new List<string>();
    public string id = string.Empty;
    public string name = string.Empty;
    public string head_node = string.Empty;
    public string tail_node = string.Empty;
    public bool is_merged = false;
    public Highway highway = Highway.None;
    public int layer = 0;
    public Vector3 head_orient = Vector3.forward;
    public Vector3 tail_orient = Vector3.forward;
    public float road_width = 6; // 6 is default
    public List<string> tag_k = new List<string>();
    public List<string> tag_v = new List<string>();

    public void updateOrient(Dictionary<string, Node> points_lib)
    {
        //try
        //{
            head_node = ref_node[0];
            head_orient = (points_lib[ref_node[0]].position - points_lib[ref_node[1]].position).normalized;
            tail_node = ref_node[ref_node.Count - 1];
            tail_orient = (points_lib[ref_node[ref_node.Count - 1]].position - points_lib[ref_node[ref_node.Count - 2]].position).normalized;
        //}
        //catch
        //{
        //    Debug.Log(id + " : " + ref_node[ref_node.Count - 1]);
        //}
    }

    public Vector3 getOrient(string node_id, out bool is_head)
    {
        is_head = false;
        if (head_node == node_id)
        {
            is_head = true;
            return head_orient;
        }
        else if (tail_node == node_id)
        {
            return tail_orient;
        }
        Debug.Log("getOrint relatepath: " + id + " find: " + node_id + " {" + head_node + " , " + tail_node + "}");
        return Vector3.zero;
    }

    static public int findWayIndex(List<Way> ways, string way_id)
    {
        int index = 0;
        foreach(Way w in ways)
        {
            if (w.id == way_id)
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    // 要所有的路 更新到有Merge 舊的點砍掉
    static public void pathConnectUpdate(List<Way> ways, ref Dictionary<string, Node> points_lib, List<string> remove_pathes_id, string do_on_point_id, string new_point_id)
    {
        for (int remove_pathes_id_index = 0; remove_pathes_id_index < remove_pathes_id.Count; remove_pathes_id_index++)
        {
            for (int connect_way_index = 0; connect_way_index < points_lib[do_on_point_id].connect_way.Count; connect_way_index++)
            {
                if (points_lib[do_on_point_id].connect_way[connect_way_index] == remove_pathes_id[remove_pathes_id_index])
                {
                    points_lib[do_on_point_id].connect_way.RemoveAt(connect_way_index);
                    connect_way_index--;
                    continue;
                }
                points_lib[new_point_id].connect_way.Add(points_lib[do_on_point_id].connect_way[connect_way_index]);
            }
        }

        if (points_lib[do_on_point_id].connect_way.Count == 0)
            points_lib.Remove(do_on_point_id);
    }

    public string writeWay()
    {
        string output = string.Empty;
        output += $"  <way id=\"{id}\"";
        if (tag_k.Count > 0)
        {
            output += ">\n";
            for (int nd_index = 0; nd_index < ref_node.Count; nd_index++)
            {
                output += $"    <nd ref=\"{ref_node[nd_index]}\"/>\n";
            }
            for (int tag_index = 0; tag_index < tag_k.Count; tag_index++)
            {
                output += $"    <tag k=\"{tag_k[tag_index]}\" v=\"{tag_v[tag_index]}\"/>\n";
            }
            output += "  </way>\n";
        }
        else
        {
            output += "/>\n";
        }
        return output;
    }
}