using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class TerrainGenerator
{
    static public void generateTerrain(Vector3 position)
    {
        //generate terrain near position

        removeTerrain(position);
    }

    static public void removeTerrain(Vector3 position)
    {
        //remove terrain not near position
    }
}
