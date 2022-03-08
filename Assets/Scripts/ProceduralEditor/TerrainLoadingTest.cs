using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLoadingTest : MonoBehaviour
{
    public string file_path = "features.f";
    public Material terrain_mat;
    public bool generate;
    float t = 0.0f;
    bool[] flags;
    // Start is called before the first frame update
    void Start()
    {
        TerrainGenerator.file_path = file_path;
        TerrainGenerator.terrain_mat = terrain_mat;
        TerrainGenerator.loadTerrain();
        flags = new bool[13];
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            if (t > 0.0f)
                t -= Time.deltaTime;
            else
            {
                if (!flags[0])
                {
                    flags[0] = true;
                    TerrainGenerator.generateTerrain(new Vector3());
                    t = 2.0f;
                    Debug.Log("Animal!");
                }
                else if (!flags[1])
                {
                    flags[1] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-9.769531f, 0f, 1.969727f));
                    //TerrainGenerator.removeTerrain(new Vector3(), new Vector3(-9.769531f, 0f, 1.969727f));
                    t = 2.0f;
                    Debug.Log("Penguin!");
                }
                else if (!flags[2])
                {
                    flags[2] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-23.86914f, 0f, 6.070313f));
                    //TerrainGenerator.removeTerrain(new Vector3(-9.769531f, 0f, 1.969727f), new Vector3(-23.86914f, 0f, 6.070313f));
                    t = 2.0f;
                    Debug.Log("Tiger!");
                }
                else if (!flags[3])
                {
                    flags[3] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-45.86914f, 0f, 11.07031f));
                    //TerrainGenerator.removeTerrain(new Vector3(-23.86914f, 0f, 6.070313f), new Vector3(-45.86914f, 0f, 11.07031f));
                    t = 2.0f;
                    Debug.Log("Bird!");
                }
                else if (!flags[4])
                {
                    flags[4] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-79.91992f, 0.0f, 14.21973f));
                    //TerrainGenerator.removeTerrain(new Vector3(-45.86914f, 0f, 11.07031f), new Vector3(-79.91992f, 0.0f, 14.21973f));
                    t = 2.0f;
                    Debug.Log("Allegate!");
                }
                else if (!flags[5])
                {
                    flags[5] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-111.2109f, 0.0f, 12.71973f));
                    //TerrainGenerator.removeTerrain(new Vector3(-79.91992f, 0.0f, 14.21973f), new Vector3(-111.2109f, 0.0f, 12.71973f));
                    t = 2.0f;
                    Debug.Log("Elephant!");
                }
                else if (!flags[6])
                {
                    flags[6] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-140.25f, 0.0f, 5.120117f));
                    //TerrainGenerator.removeTerrain(new Vector3(-111.2109f, 0.0f, 12.71973f), new Vector3(-140.25f, 0.0f, 5.120117f));
                    t = 2.0f;
                    Debug.Log("Ant!");
                }
                else if (!flags[7])
                {
                    flags[7] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-205.4805f, 0.0f, -17.08008f));
                    //TerrainGenerator.removeTerrain(new Vector3(-140.25f, 0.0f, 5.120117f), new Vector3(-205.4805f, 0.0f, -17.08008f));
                    t = 2.0f;
                    Debug.Log("Cat!");
                }
                else if (!flags[8])
                {
                    flags[8] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-234.1699f, 0.0f, -27.2998f));
                    //TerrainGenerator.removeTerrain(new Vector3(-205.4805f, 0.0f, -17.08008f), new Vector3(-234.1699f, 0.0f, -27.2998f));
                    t = 2.0f;
                    Debug.Log("Dophin!");
                }
                else if (!flags[9])
                {
                    flags[9] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-265.5605f, 0.0f, -31.87988f));
                    //TerrainGenerator.removeTerrain(new Vector3(-234.1699f, 0.0f, -27.2998f), new Vector3(-265.5605f, 0.0f, -31.87988f));
                    t = 2.0f;
                    Debug.Log("Monkey!");
                }
                else if (!flags[10])
                {
                    flags[10] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-316.5f, 0.0f, -27.34961f));
                    //TerrainGenerator.removeTerrain(new Vector3(-265.5605f, 0.0f, -31.87988f), new Vector3(-316.5f, 0.0f, -27.34961f));
                    t = 2.0f;
                    Debug.Log("Snake!");
                }
                else if (!flags[11])
                {
                    flags[11] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-399.8691f, 0.0f, -12.54004f));
                    //TerrainGenerator.removeTerrain(new Vector3(-316.5f, 0.0f, -27.34961f), new Vector3(-399.8691f, 0.0f, -12.54004f));
                    t = 2.0f;
                    Debug.Log("Cow!");
                }
                else if (!flags[12])
                {
                    flags[12] = true;
                    TerrainGenerator.generateTerrain(new Vector3(-431.25f, 0.0f, -11.75f));
                    //TerrainGenerator.removeTerrain(new Vector3(-399.8691f, 0.0f, -12.54004f), new Vector3(-431.25f, 0.0f, -11.75f));
                    t = 2.0f;
                    Debug.Log("Mouse!");
                }
            }
        }
    }
}