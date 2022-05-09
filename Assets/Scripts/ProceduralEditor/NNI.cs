using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NNI
{
    static public float naturalNeighborInterpolation(Vector4[] point_cloud, float x, float z, float old_base = 0.0f)
    {
        float d_min = Mathf.Sqrt(Mathf.Pow(point_cloud[0].x - x, 2) + Mathf.Pow(point_cloud[0].z - z, 2));
        int p_index = 0;
        for (int point_index = 1; point_index < point_cloud.Length; point_index++)
        {
            if (Mathf.Abs(point_cloud[point_index].x - x) < 320.0 && Mathf.Abs(point_cloud[point_index].z - z) < 320.0)
            {
                float dist = Mathf.Sqrt(Mathf.Pow(point_cloud[point_index].x - x, 2) + Mathf.Pow(point_cloud[point_index].z - z, 2));
                if (d_min > dist)
                {
                    d_min = dist;
                    p_index = point_index;
                }
            }
        }
        return point_cloud[p_index].y;
    }
}