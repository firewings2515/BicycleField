using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class DouglasPeuckerAlgorithm
{
    static public List<Vector3> DouglasPeucker(List<Vector3> points, float epsilon)
    {
        int select_index = 0;
        float d = 0, dmax = 0;
        Vector3 point_tail = points[points.Count - 1];
        for (int point_index = 2; point_index < points.Count; point_index++)
        {
            d = perpendicularDistance(points[point_index], points[0], point_tail);
            if (d > dmax)
            {
                dmax = d;
                select_index = point_index;
            }
        }

        if (dmax > epsilon)
        {
            List<Vector3> results1 = DouglasPeucker(points.GetRange(0, select_index + 1), epsilon);
            List<Vector3> results2 = DouglasPeucker(points.GetRange(select_index, points.Count - select_index), epsilon);
            results2.RemoveAt(0);
            results1.AddRange(results2);
            return results1;
        }
        else
        {
            List<Vector3> results = new List<Vector3>();
            results.Add(points[0]);
            results.Add(points[points.Count - 1]);
            return results;
        }
    }

    // https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
    static public float perpendicularDistance(Vector3 p, Vector3 p1, Vector3 p2)
    {
        float A = p.x - p1.x;
        float B = p.y - p1.y;
        float C = p2.x - p1.x;
        float D = p2.y - p1.y;

        float dot = A * C + B * D;
        float len_sq = C * C + D * D;
        float param = -1;
        if (len_sq != 0) //in case of 0 length line
            param = dot / len_sq;

        Vector3 point_t;

        if (param < 0)
        {
            point_t = p1;
        }
        else if (param > 1)
        {
            point_t = p2;
        }
        else
        {
            point_t.x = p1.x + param * C;
            point_t.y = p1.y + param * D;
        }

        var dx = p.x - point_t.x;
        var dy = p.y - point_t.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
        //Vector3 v1 = p - p1;
        //Vector3 v2 = p2 - p1;
        //return fabs(Vector3.Cross(v1, v2)) / v2.magnitude;
    }
}
