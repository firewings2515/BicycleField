using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Material material;
    // Start is called before the first frame update
    void Start()
    {
        TerrainGenerator.terrain_mat = material;
        //terrain
        TerrainGenerator.loadTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
