using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class HouseIntegration
{
    static public int bindHouses(OSMReader osm_reader, List<string> bicycle_points_list, HierarchyControl hierarchy_c, Dictionary<string, GameObject> house_polygons_objects)
    {
        List<string> house_polygons_object_ids = new List<string>();

        //foreach (KeyValuePair<string, GameObject> house_polygon_info in house_polygons_objects)
        //    house_polygons_object_ids.Add(house_polygon_info.Key);

        for (int bicycle_points_list_index = 0; bicycle_points_list_index < bicycle_points_list.Count - 1; bicycle_points_list_index++)
        {
            Vector3 point_pos = osm_reader.points_lib[bicycle_points_list[bicycle_points_list_index]].position;
            int at_x = 0;
            int at_y = 0;
            hierarchy_c.calcLocation(point_pos.x, point_pos.z, ref at_x, ref at_y);
            HashSet<string> hierarchy_house_ids_t = new HashSet<string>();
            for (int i = -1; i <= 1; i++)
            {
                if (at_x + i < 0 || at_x + i > hierarchy_c.split_x) continue;
                for (int j = -1; j <= 1; j++)
                {
                    if (at_y + j < 0 || at_y + j > hierarchy_c.split_y) continue;

                    List<string> hierarchy_house_ids_list = hierarchy_c.getHousesInArea(at_x + i, at_y + j);
                    foreach (string hierarchy_house_id in hierarchy_house_ids_list)
                        hierarchy_house_ids_t.Add(hierarchy_house_id);
                }
            }
        }

        return house_polygons_object_ids.Count;
    }
}