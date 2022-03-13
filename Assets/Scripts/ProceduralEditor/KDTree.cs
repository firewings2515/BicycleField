using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDTree
{
    public Vector3[] nodes;
    public int[] parent;
    public int[] left;
    public int[] right;
    static int nodes_length;

    public void buildKDTree(Vector3[] points)
    {
        nodes = new Vector3[points.Length];
        parent = new int[points.Length];
        left = new int[points.Length];
        right = new int[points.Length];
        nodes_length = 0;
        insertPoint(ref points, 0, points.Length, true, -1);
    }

    int insertPoint(ref Vector3[] points, int x, int y, bool is_x, int the_parent)
    {
        mergeSortPoints(ref points, x, y, new Vector3[points.Length], is_x);
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

    void mergeSortPoints(ref Vector3[] points, int x, int y, Vector3[] points_t, bool is_x)
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

    public void getNearpoints(out Vector3[] points)
    {
        List<int> nearpoints = new List<int>();

        points = new Vector3[nearpoints.Count];
    }
}