using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WVec3
{
    public float x;
    public float y;
    public float z;
    public float w;
}

public class KDTree
{
    public WVec3[] nodes;
    public int[] parent;
    public int[] left;
    public int[] right;
    public int[] weight;
    static int nodes_length;

    public void buildKDTree(WVec3[] points)
    {
        if (points.Length == 0)
            Debug.LogError("The number of points can not be 0!");
        nodes = new WVec3[points.Length];
        parent = new int[points.Length];
        left = new int[points.Length];
        right = new int[points.Length];
        weight = new int[points.Length];
        nodes_length = 0;
        insertPoint(ref points, 0, points.Length, true, -1);
    }

    int insertPoint(ref WVec3[] points, int x, int y, bool is_x, int the_parent)
    {
        mergeSortPoints(ref points, x, y, new WVec3[points.Length], is_x);
        int middle = x + (y - x) / 2;
        int nodes_index = nodes_length;
        nodes_length++;
        nodes[nodes_index] = points[middle];
        parent[nodes_index] = the_parent;
        if (y - x > 1)
        {
            if (middle - x > 0)
                left[nodes_index] = insertPoint(ref points, x, middle, !is_x, nodes_index);
            if (y - (middle + 1) > 0)
                right[nodes_index] = insertPoint(ref points, middle + 1, y, !is_x, nodes_index);
        }
        return nodes_index;
    }

    void mergeSortPoints(ref WVec3[] points, int x, int y, WVec3[] points_t, bool is_x)
    {
        if (y - x > 1)
        {
            int m = x + (y - x) / 2;
            mergeSortPoints(ref points, x, m, points_t, is_x);
            mergeSortPoints(ref points, m, y, points_t, is_x);
            int p = x, q = m;
            int index = x;
            while (p < m && q < y)
            {
                if ((is_x && points[p].x < points[q].x) ||
                    (!is_x && points[p].z < points[q].z))
                    points_t[index++] = points[p++];
                else
                    points_t[index++] = points[q++];
            }
            while (p < m)
                points_t[index++] = points[p++];
            while (q < y)
                points_t[index++] = points[q++];
            for (int i = x; i < y; i++)
                points[i] = points_t[i];
        }
    }

    public int[] getAreaPoints(float x_min, float z_min, float x_max, float z_max)
    {
        List<int> area_points = new List<int>();
        getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, 0, true);
        return area_points.ToArray();
    }

    void getAreaPointsRec(ref List<int> area_points, float x_min, float z_min, float x_max, float z_max, int head, bool is_x)
    {
        if (is_x)
        {
            if (nodes[head].x < x_min)
            {
                if (right[head] != 0)
                getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, right[head], !is_x);
            }
            else if (nodes[head].x > x_max)
            {
                if (left[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, left[head], !is_x);
            }
            else // x_min < nodes[head].x < x_max
            {
                if (z_min <= nodes[head].z && nodes[head].z <= z_max)
                    area_points.Add(head);
                if (right[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, right[head], !is_x);
                if (left[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, left[head], !is_x);
            }
        }
        else
        {
            if (nodes[head].z < z_min)
            {
                if (right[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, right[head], !is_x);
            }
            else if (nodes[head].z > z_max)
            {
                if (left[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, left[head], !is_x);
            }
            else // z_min < nodes[head].z < z_max
            {
                if (x_min <= nodes[head].x && nodes[head].x <= x_max)
                    area_points.Add(head);
                if (right[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, right[head], !is_x);
                if (left[head] != 0)
                    getAreaPointsRec(ref area_points, x_min, z_min, x_max, z_max, left[head], !is_x);
            }
        }
    }
}