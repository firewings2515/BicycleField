using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IDW
{
    static public float getWeight(float d, float w)
    {
        float f = Mathf.Pow(d, w);
        if (f < 1e-6)
            return 0.000001f;
        return 1 / f;
    }

    static public float inverseDistanceWeighting(Vector4[] point_cloud, float x, float z, float old_base = 0.0f)
    {
        float sum_up = 0.0f;
        float sum_down = 0.0f;
        for (int point_index = 0; point_index < point_cloud.Length; point_index++)
        {
            float dist = Mathf.Sqrt(Mathf.Pow(point_cloud[point_index].x - x, 2) + Mathf.Pow(point_cloud[point_index].z - z, 2));
            if (dist < 320.0)
            {
                if (point_cloud[point_index].w > 8)
                {
                    sum_up += getWeight(dist, 0.9f) * (point_cloud[point_index].y - old_base);
                    sum_down += getWeight(dist, 0.9f);
                }
                else
                {
                    sum_up += getWeight(dist, 1) * (point_cloud[point_index].y - old_base);
                    sum_down += getWeight(dist, 1);
                }
            }
        }
        if (sum_down < 1e-6)
            sum_down = 0.000001f;
        return sum_up / sum_down;
    }
}