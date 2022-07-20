using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    public string house_info_file = "";
    public string house_show_file = "";
    GameObject buildings;
    List<List<Vector3>> house_polygons;
    List<List<int>> house_show;

    Dictionary<int,GameObject> exist_buildings;
    HashSet<int> now_shows;

    int local_segment_index = 0;
    Queue<int> exist_segments; 

    void get_house_polygons() {
        string[] house_info_lines = System.IO.File.ReadAllLines(house_info_file);
        house_polygons = new List<List<Vector3>>(house_info_lines.Length);
        for (int i = 0; i < house_info_lines.Length; i++)
        {
            string[] polygon_line = house_info_lines[i].Split(' ');
            int polygon_size = int.Parse(polygon_line[0]);
            int coord_index = 1;
            List<Vector3> polygon = new List<Vector3>(polygon_size);
            for (int j = 0; j < polygon_size; j++)
            {
                Vector3 point = new Vector3(float.Parse(polygon_line[coord_index]), float.Parse(polygon_line[coord_index + 1]), float.Parse(polygon_line[coord_index + 2]));
                coord_index += 3;
                polygon.Add(point);
            }
            house_polygons.Add(polygon);
        }
    }

    void get_house_shows()
    {
        string[] house_show_lines = System.IO.File.ReadAllLines(house_show_file);
        house_show = new List<List<int>>(house_show_lines.Length);
        for (int i = 0; i < house_show_lines.Length; i++)
        {
            string[] numbers_str = house_show_lines[i].Split(' ');
            List<int> numbers = new List<int>(numbers_str.Length);
            for (int j = 0; j < numbers_str.Length; j++) {
                if (numbers_str[j] == string.Empty) break;
                numbers.Add(int.Parse(numbers_str[j]));
            }
            house_show.Add(numbers);
        }
    }

    void Start()
    {
        get_house_polygons();
        get_house_shows();
        buildings = new GameObject("buildings");
    }

    public void generate_next_segment(int count = 1) {
        for (int i = 0; i < count; i++)
        {
            exist_segments.Enqueue(local_segment_index);
            generate_segment(local_segment_index);
            local_segment_index++;
        }
    }

    public void destroy_previous_segment(int count = 1) {
        for (int i = 0; i < count; i++)
        {
            int seg = exist_segments.Dequeue();
            destroy_segment(seg);
        }
    }

    public void generate_segment(int index) {
        now_shows.Clear();
        List<int> shows = house_show[index];
        for (int i = 0; i < shows.Count; i++) {
            if (exist_buildings.ContainsKey(shows[i])) continue;
            List<Vector3> polygon = house_polygons[shows[i]];
            float min_y = float.MaxValue;
            Vector3 total = new Vector3();
            for (int j = 0; j < polygon.Count; j++) {
                Vector3 point = polygon[j];
                if (TerrainGenerator.is_initial)
                {
                    point.y = TerrainGenerator.getHeightWithBais(point.x, point.z);
                }
                if (point.y < min_y) min_y = point.y;
                total.x += point.x;
                total.y += point.y;
                total.z += point.z;
                polygon[j] = point;
            }

            Vector3 averge = total / polygon.Count;
            averge.y = min_y;
            for (int j = 0; j < polygon.Count; j++)
            {
                polygon[j] = new Vector3(polygon[j].x - averge.x, 0, polygon[j].z - averge.z);
            }
            GameObject gobj = HouseGenerator.build_polygon_house(polygon, 4);
            gobj.transform.position = averge;
            gobj.transform.parent = buildings.transform;

            exist_buildings.Add(shows[i],gobj);
            now_shows.Add(shows[i]);
        }
    }

    bool need_destroy(int index) {
        if (!exist_buildings.ContainsKey(index)) return false;
        Queue<int> _exist_segments = exist_segments;
        while (_exist_segments.Count > 0) {
            List<int> shows = house_show[_exist_segments.Dequeue()];
            if (shows.Contains(index)) return false;
        }
        return true;
    }

    public void destroy_segment(int index)
    {
        List<int> shows = house_show[index];
        for (int i = 0; i < shows.Count; i++)
        {
            if (need_destroy(shows[i])) {
                Destroy(exist_buildings[shows[i]]);
                exist_buildings.Remove(shows[i]);
            }
        }
    }
}