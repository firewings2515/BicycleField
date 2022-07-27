using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    public string house_info_file = "";
    public string house_show_file = "";
    GameObject buildings_parent;
    Vector3[][] house_polygons;
    int[][] show_infos;
    bool[] house_showing;
    Dictionary<int,GameObject> buildings = new Dictionary<int, GameObject>();
    int local_seg_index = 0;
    void get_house_polygons() {
        string[] house_info_lines = System.IO.File.ReadAllLines(house_info_file);
        house_polygons = new Vector3[house_info_lines.Length][];
        for (int i = 0; i < house_info_lines.Length; i++)
        {
            string[] polygon_line = house_info_lines[i].Split(' ');
            int polygon_size = int.Parse(polygon_line[0]);
            int coord_index = 1;
            Vector3[] polygon = new Vector3[polygon_size];
            for (int j = 0; j < polygon_size; j++)
            {
                Vector3 point = new Vector3(float.Parse(polygon_line[coord_index]), float.Parse(polygon_line[coord_index + 1]), float.Parse(polygon_line[coord_index + 2]));
                coord_index += 3;
                polygon[j] = point;
            }
            house_polygons[i] = polygon;
        }
        house_showing = new bool[house_polygons.Length];
    }

    void get_house_shows()
    {
        string[] house_show_lines = System.IO.File.ReadAllLines(house_show_file);
        show_infos = new int[house_show_lines.Length][];
        for (int i = 0; i < house_show_lines.Length; i++)
        {
            string[] numbers_str = house_show_lines[i].Split(' ');
            List<int> numbers = new List<int>();
            for (int j = 0; j < numbers_str.Length; j++) {
                if (numbers_str[j] == string.Empty) break;
                numbers.Add(int.Parse(numbers_str[j]));
            }
            show_infos[i] = numbers.ToArray();
        }
    }

    void Start()
    {
        get_house_polygons();
        get_house_shows();
        buildings_parent = new GameObject("all_buildings");
    }

    public IEnumerator go_next_segment() {
        Debug.Log("generate_segment :");
        int[] change_houses = show_infos[local_seg_index];
        local_seg_index++;
        Debug.Log("change_houses :"+ change_houses.Length);
        for (int i = 0; i < change_houses.Length; i++) {
            int house_index = change_houses[i];
            if (house_showing[house_index] == false)
            {
                house_showing[house_index] = true;
                Debug.Log("\tgenerate_house " + house_index);
                generate_house(house_index);                
                yield return 0;
            }
            else if (house_showing[house_index] == true){
                house_showing[house_index] = false;
                Debug.Log("\tdestroy_house " + house_index);
                Destroy(buildings[house_index]);
                buildings.Remove(house_index);
                yield return 0;
            }
        }
        
    }


    void generate_house(int house_index) {
        Vector3[] polygon = house_polygons[house_index];
        float min_y = float.MaxValue;
        Vector3 total = new Vector3();
        for (int j = 0; j < polygon.Length; j++)
        {
            Vector3 point = polygon[j];
            if (point.y < min_y) min_y = point.y;
            total += point;
        }
        Vector3 averge = total / polygon.Length;
        averge.y = min_y;
        for (int j = 0; j < polygon.Length; j++)
        {
            polygon[j] = new Vector3(polygon[j].x - averge.x, 0, polygon[j].z - averge.z);
        }
        GameObject gobj = HouseGenerator.build_polygon_house(polygon, Random.Range(3, 6));
        gobj.transform.position = averge;
        gobj.transform.parent = buildings_parent.transform;
        buildings.Add(house_index, gobj);
    }
}