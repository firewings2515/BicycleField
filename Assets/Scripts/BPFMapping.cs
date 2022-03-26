using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class BPFMapping : MonoBehaviour
{
    public string bpf_file_path = "arclength_100.bpf"; // _150_32
    public string output_bpf_file_path = "arclength_100_fix.bpf"; // _150_32
    public string feature_file_path = "YangJing1/features_100_16_big.f"; // _150_32
    public string output_feature_file_path = "YangJing1/features_100_16_fix.f"; // _150_32
    public bool correct;
    // Start is called before the first frame update
    void Start()
    {
        TerrainGenerator.file_path = feature_file_path;
        TerrainGenerator.loadTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        if (TerrainGenerator.is_initial && correct)
        {
            correct = false;
            float old_base;
            using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "//" + bpf_file_path))
            {
                string[] inputs = sr.ReadLine().Split(' ');
                float x = float.Parse(inputs[0]);
                old_base = TerrainGenerator.min_y; // (-0.5)
                float y = old_base;
                float z = float.Parse(inputs[2]);
                y = TerrainGenerator.getIDWHeight(x, z); // remove min_y in getIDW
                TerrainGenerator.min_y = -y;
            }

            TerrainGenerator.fixHeight(Application.streamingAssetsPath + "//" + output_feature_file_path, old_base);

            using (StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "//" + output_bpf_file_path))
            {
                using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "//" + bpf_file_path))
                {
                    string[] read_to_end = sr.ReadToEnd().Split('\n');
                    for (int line_idnex = 0; line_idnex < read_to_end.Length; line_idnex++)
                    {
                        string[] inputs = read_to_end[line_idnex].Split(' ');
                        float x, y, z;
                        if (inputs[0] == "H")
                        {
                            int n = int.Parse(inputs[1]);
                            sw.Write($"H {n}");
                            for (int vertex_index = 0; vertex_index < n; vertex_index++)
                            {
                                x = float.Parse(inputs[2]);
                                y = float.Parse(inputs[3]);
                                z = float.Parse(inputs[4]);
                                y = TerrainGenerator.getIDWHeight(x, z) + TerrainGenerator.min_y;
                                sw.Write($" {x} {y} {z}");
                            }
                            sw.WriteLine("");
                        }
                        else
                        {
                            x = float.Parse(inputs[0]);
                            y = float.Parse(inputs[1]);
                            z = float.Parse(inputs[2]);
                            y = TerrainGenerator.getIDWHeight(x, z) + TerrainGenerator.min_y;
                            sw.WriteLine($"{x} {y} {z}");
                        }
                    }
                }
            }

        }
    }
}