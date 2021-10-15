using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 position = Vector3.zero;
    public List<string> connect_way = new List<string>();

    public bool includeAnotherWay(string way_id, out List<string> relation_ways)
    {
        relation_ways = new List<string>();
        bool result = false;
        for (int w = 0; w < connect_way.Count; w++)
        {
            if (connect_way[w] != way_id)
            {
                //if (connect_way[w] == "428385744")
                //    Debug.Log("---428385744");
                result = true;
                relation_ways.Add(connect_way[w]);
            }
        }
        return result;
    }

    public bool includePathID(string way_id)
    {
        return connect_way.IndexOf(way_id) != -1;
    }
}