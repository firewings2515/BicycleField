using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IDW
{
    static public float getWeight(float d)
    {
        float f = Mathf.Pow(d, 2);
        if (f < 1e-6)
            return 1.0f;
        return 1 / f;
    }

    static public float inverseDistanceWeighting(Vector3[] point_cloud, float x, float z)
    {
        float sum_up = 0.0f;
        float sum_down = 0.0f;
        float[] d = new float[point_cloud.Length];
        for (int point_index = 0; point_index < point_cloud.Length; point_index++)
        {
            d[point_index] = Mathf.Sqrt(Mathf.Pow(point_cloud[point_index].x - x, 2) + Mathf.Pow(point_cloud[point_index].z - z, 2));
        }
        for (int point_index = 0; point_index < point_cloud.Length; point_index++)
        {
            sum_up += getWeight(d[point_index]) * point_cloud[point_index].y;
            sum_down += getWeight(d[point_index]);
        }
        if (sum_down < 1e-6)
            sum_down = 1.0f;
        return sum_up / sum_down;
    }
}