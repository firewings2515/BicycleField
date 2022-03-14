using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestKDTree : MonoBehaviour
{
    public bool generateKDTree;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (generateKDTree)
        {
            generateKDTree = false;
            KDTree kdtree = new KDTree();
            Vector3[] points = new Vector3[] { new Vector3(2, 0, 3), new Vector3(5, 0, 4), new Vector3(9, 0, 6), new Vector3(4, 0, 7), new Vector3(8, 0, 1), new Vector3(7, 0, 2) };
            kdtree.buildKDTree(points);
            for (int index = 0; index < points.Length; index++)
            {
                Debug.Log(kdtree.nodes[index].ToString() + ": parent: " + kdtree.parent[index].ToString() + " left: " + kdtree.left[index].ToString() + " right: " + kdtree.right[index].ToString());
            }

            int[] area_points = kdtree.getAreaPoints(4, 1, 9, 5);
            for (int index = 0; index < area_points.Length; index++)
            {
                Debug.Log(kdtree.nodes[area_points[index]].ToString());
            }
        }
    }
}