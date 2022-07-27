using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class calcHouseShow : EditorWindow
{
    string assets_path = System.IO.Directory.GetCurrentDirectory() + @"\Assets\";
    string default_path = System.IO.Directory.GetCurrentDirectory() + @"\Assets\StreamingAssets\";
    string road_point_file_path;
    string house_info_file_path;
    float max_distance = 100.0f;
    void Awake()
    {

    }

    Vector3 str2vec3(string line)
    {
        string[] xyz = line.Split(' ');
        return new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2]));
    }
    Vector3[] get_road_points() {
        string[] road_point_lines = System.IO.File.ReadAllLines(road_point_file_path);
        Vector3[] road_points = new Vector3[road_point_lines.Length];
        for (int i = 0; i < road_point_lines.Length; i++) {
            Vector3 point = str2vec3(road_point_lines[i]);
            road_points[i] = point;
        }
        return road_points;
    }

    Vector3[] get_house_centers()
    {
        string[] house_info_lines = System.IO.File.ReadAllLines(house_info_file_path);
        Vector3[] house_centers = new Vector3[house_info_lines.Length];
        for (int i = 0; i < house_info_lines.Length; i++){
            string[] polygon_line = house_info_lines[i].Split(' ');
            int polygon_size = int.Parse(polygon_line[0]);
            int coord_index = 1;
            Vector3 total = new Vector3();
            for (int j = 0; j < polygon_size; j++)
            {         
                total.x += float.Parse(polygon_line[coord_index]);
                total.y += float.Parse(polygon_line[coord_index + 1]);
                total.z += float.Parse(polygon_line[coord_index + 2]);
                coord_index += 3;
                
            }
            house_centers[i] = (total / polygon_size);
        }
        return house_centers;
    }

    void calc(string output_filename) {
        Vector3[] road_points = get_road_points();
        Vector3[] house_centers = get_house_centers();
        bool[] house_showing = new bool[house_centers.Length];
        StreamWriter writer = new StreamWriter(output_filename);
        for (int i = 0; i < road_points.Length; i++) {
            Vector3 road_point = road_points[i];
            string line = "";
            for (int j = 0; j < house_centers.Length; j++) {
                if (Vector3.Distance(road_point, house_centers[j]) < max_distance)
                {
                    if (!house_showing[j])
                    {
                        line += (j.ToString() + " ");
                        house_showing[j] = true;
                    }
                }
                else {
                    if (house_showing[j])
                    {
                        line += (j.ToString() + " ");
                        house_showing[j] = false;
                    }
                }
            }
            writer.WriteLine(line);
        }
        writer.Close();
    }

    [MenuItem("Preprocessing/calcHouseShow")]
    public static void ModelRebuild_Open()
    {
        EditorWindow.GetWindow(typeof(calcHouseShow), false, "Calculate House Show", true);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        default_path = EditorGUILayout.TextField("Default Path:", default_path);
        if (GUILayout.Button("Brower Path"))
        {
            string path = EditorUtility.OpenFolderPanel("Default Path:", assets_path, "");
            if (path.Length > 0)
            {
                default_path = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        road_point_file_path = EditorGUILayout.TextField("Road Point File:", road_point_file_path);
        if (GUILayout.Button("Brower File"))
        {
            string path = EditorUtility.OpenFilePanel("Road Point File:", default_path, "");
            if (path.Length > 0)
            {
                road_point_file_path = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        house_info_file_path = EditorGUILayout.TextField("House Info File:", house_info_file_path);
        if (GUILayout.Button("Brower File"))
        {
            string path = EditorUtility.OpenFilePanel("House Info File:", default_path, "");
            if (path.Length > 0)
            {
                house_info_file_path = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        float tmp_val = EditorGUILayout.FloatField("Max Distance:" , max_distance);
        if (tmp_val > 0.0f) {
            max_distance = tmp_val;
        }

        if (GUILayout.Button("Start Calculate") && road_point_file_path != string.Empty && house_info_file_path != string.Empty)
        {
            string path = EditorUtility.SaveFilePanel("Save File to:", default_path, "", "bpf");
            if (path.Length > 0)
            {
                calc(path);
            }
        }
    }
}