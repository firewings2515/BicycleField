using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEMFetcher : MonoBehaviour
{
    MeshFilter mf;
    bool is_initial = false;
    bool is_done = false;
    // Start is called before the first frame update
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        is_initial = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] vertices = mf.mesh.vertices;
        if (is_initial && !is_done)
        {
            is_done = true;
            float[] xs = new float[vertices.Length];
            float[] zs = new float[vertices.Length];
            float find_x_min = float.MaxValue;
            float find_z_min = float.MaxValue;
            int center_index = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                xs[i] = transform.position.x + vertices[i].x * transform.localScale.x;
                zs[i] = transform.position.z + vertices[i].z * transform.localScale.z;
                if (xs[i] > find_x_min)
                {
                    find_x_min = xs[i];
                    center_index = 0;
                }
                if (zs[i] > find_z_min)
                {
                    find_z_min = zs[i];
                    center_index = 0;
                }
            }
            float[] ys = TerrainGenerator.getDEMHeights(xs, zs);

            transform.position = new Vector3(transform.position.x, ys[center_index], transform.position.z);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y = ys[i] - transform.position.y;
            }
            mf.mesh.vertices = vertices;
            //Recalculations
            mf.mesh.RecalculateNormals();
            mf.mesh.RecalculateBounds();
            mf.mesh.Optimize();
        }
    }
}