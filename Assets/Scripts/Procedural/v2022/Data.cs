using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

static public class Data
{
    public const int POINTS_PER_SEGMENT = 10;
    public const int CHECKPOINT_RADIUS = 50;

    static public List<Vector3> points = new List<Vector3>() {};

    static private StreamReader reader;
    static public void loadFile(string filename)
    {
        reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + filename);

        string str_point = reader.ReadLine();
        while (str_point != null)
        {
            if (str_point[0] == 'H')
            {
                //add to house list
            }
            else
            {
                Data.points.Add(Functions.StrToVec3(str_point));
            }
            str_point = reader.ReadLine();
        }
    }
}
