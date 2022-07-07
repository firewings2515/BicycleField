using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class HouseIntegration
{
    static float view_distance = 150.0f;
    static public Dictionary<string, KeyValuePair<int, float>> house_polygons_object_dists = new Dictionary<string, KeyValuePair<int, float>>();
    static public Dictionary<string, ViewInstance> house_polygons_view_instances = new Dictionary<string, ViewInstance>();
    static public List<List<string>> house_polygons_object_index = new List<List<string>>();

    static public int bindHouses(OSMReader osm_reader, List<string> bicycle_points_list, HierarchyControl hierarchy_c, Dictionary<string, GameObject> house_polygons_objects)
    {
        house_polygons_object_dists.Clear();
        house_polygons_object_index.Clear();

        //foreach (KeyValuePair<string, GameObject> house_polygon_info in house_polygons_objects)
        //    house_polygons_object_ids.Add(house_polygon_info.Key);

        HashSet<string> hierarchy_house_ids = new HashSet<string>();
        for (int bicycle_points_list_index = 0; bicycle_points_list_index < bicycle_points_list.Count - 1; bicycle_points_list_index++)
        {
            house_polygons_object_index.Add(new List<string>());

            Vector3 point_pos_s = osm_reader.points_lib[bicycle_points_list[bicycle_points_list_index]].position;
            Vector3 point_pos_e = osm_reader.points_lib[bicycle_points_list[bicycle_points_list_index + 1]].position;
            int at_x = 0;
            int at_y = 0;
            hierarchy_c.calcLocation(point_pos_s.x, point_pos_s.z, ref at_x, ref at_y);
            List<string> hierarchy_house_ids_list = new List<string>();
            for (int i = -1; i <= 1; i++)
            {
                if (at_x + i < 0 || at_x + i > hierarchy_c.split_x) continue;
                for (int j = -1; j <= 1; j++)
                {
                    if (at_y + j < 0 || at_y + j > hierarchy_c.split_y) continue;

                    hierarchy_house_ids_list = hierarchy_c.getHousesInArea(at_x + i, at_y + j);
                    foreach (string hierarchy_house_id in hierarchy_house_ids_list)
                    {
                        if (!hierarchy_house_ids.Contains(hierarchy_house_id))
                        {
                            hierarchy_house_ids.Add(hierarchy_house_id);
                        }
                    }
                }
            }

            foreach (string hierarchy_house_id in hierarchy_house_ids)
            {
                ViewInstance house_polygons_view_instance = house_polygons_objects[hierarchy_house_id].GetComponent<ViewInstance>();

                float instance_to_s = house_polygons_view_instance.getDistance(point_pos_s);
                float instance_to_e = house_polygons_view_instance.getDistance(point_pos_e);
                float min_dist = Mathf.Min(instance_to_s, instance_to_e);
                if (min_dist < view_distance)
                {
                    if (!house_polygons_object_dists.ContainsKey(hierarchy_house_id))
                    {
                        house_polygons_object_dists.Add(hierarchy_house_id, new KeyValuePair<int, float>(bicycle_points_list_index, min_dist));
                        house_polygons_object_index[bicycle_points_list_index].Add(hierarchy_house_id);
                    }
                    //else
                    //{
                    //    if (min_dist < house_polygons_object_dists[hierarchy_house_id].Value)
                    //    {
                    //        house_polygons_object_index[house_polygons_object_dists[hierarchy_house_id].Key].Remove(hierarchy_house_id);
                    //        house_polygons_object_dists[hierarchy_house_id] = new KeyValuePair<int, float>(bicycle_points_list_index, min_dist);
                    //        house_polygons_object_index[bicycle_points_list_index].Add(hierarchy_house_id);
                    //    }
                    //}

                    if (!house_polygons_view_instances.ContainsKey(house_polygons_view_instance.house_id))
                    {
                        house_polygons_view_instances.Add(house_polygons_view_instance.house_id, house_polygons_view_instance);
                    }
                    //else
                    //{
                    //    house_polygons_view_instances[house_polygons_view_instance.house_id] = house_polygons_view_instance;
                    //}
                }
            }
        }

        return house_polygons_object_dists.Count;
    }

    static public (List<Vector3>, List<int>) buildingPointsListToVec3()
    {
        List<Vector3> building_points_list = new List<Vector3>();
        List<int> building_point_count_list = new List<int>();
        for (int bicycle_points_list_index = 0; bicycle_points_list_index < house_polygons_object_index.Count; bicycle_points_list_index++)
        {
            List<string> house_polygon_ids = house_polygons_object_index[bicycle_points_list_index];
            for (int house_polygon_ids_index = 0; house_polygon_ids_index < house_polygon_ids.Count; house_polygon_ids_index++)
            {
                Vector3[] vertices = house_polygons_view_instances[house_polygon_ids[house_polygon_ids_index]].points;
                building_point_count_list.Add(vertices.Length);
                for (int vertices_index = 0; vertices_index < vertices.Length; vertices_index++)
                {
                    building_points_list.Add(vertices[vertices_index]);
                }
            }
        }
        return (building_points_list, building_point_count_list);
    }
}