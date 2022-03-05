using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class HouseGenerator
{
    static public void generateHouse(int segment_id, int house_id, string info)
    {
        //demo code
        string[] house_infos = info.Split(' ');
        Vector3 single_point = new Vector3(int.Parse(house_infos[2]), int.Parse(house_infos[3]), int.Parse(house_infos[4]));
    }

    static public void destroyEarliestSegment()
    {
        //destroy all houses in segment with lowest id
    }

    static public void destroySegment(int segment_id)
    {
        //destroy all houses in segment_id
    }

    static public void destroyHouse(int segment_id, int house_id)
    {
        //destroy house_id in segment_id
    }
}
