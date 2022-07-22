using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainView : MonoBehaviour
{
    //public GameObject cam;
    //float radius = 16 * 2 * 32;
    // Start is called before the first frame update
    bool is_update_mesh = false;
    bool is_idw_ok = false;
    public int x_index;
    public int z_index;
    public int x_piece_num;
    public int z_piece_num;
    public bool need_mse = false;
    public bool use_gaussian = false;
    public Terrain origin_terrain;
    void Start()
    {
        
    }

    private void OnBecameVisible()
    {
        //GetComponent<MeshRenderer>().isVisible = true;
    }

    private void OnBecameInvisible()
    {
        //GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_idw_ok)
        {
            int[] progress = new int[1];
            TerrainGenerator.progress_buffer[x_index * TerrainGenerator.z_patch_num + z_index].GetData(progress);
            if ((progress[0] == 1 && !use_gaussian) || (progress[0] == 2 && use_gaussian))
            {
                is_idw_ok = true;
                TerrainGenerator.progress_buffer[x_index * TerrainGenerator.z_patch_num + z_index].Release();
            }
            else
            {
                Debug.LogError("Fence not working");
            }
        }
        if (is_idw_ok && !is_update_mesh && TerrainGenerator.constraintsmap_generated[x_index * TerrainGenerator.z_patch_num + z_index])
        {
            is_update_mesh = true;
            if (need_mse)
            {
                StartCoroutine(IDWAnalyze(origin_terrain, x_index, z_index, x_piece_num, z_piece_num));
            }
            else
                StartCoroutine(TerrainGenerator.generateTerrainPatchWithTex(x_index, z_index, x_piece_num, z_piece_num));
        }
        //if (GetComponent<MeshRenderer>().isVisible)
        //{
        //    GetComponent<MeshRenderer>().enabled = true;
        //    //GetComponent<MeshRenderer>().forceRenderingOff = false;
        //}
        //else
        //{
        //    GetComponent<MeshRenderer>().enabled = false;
        //    //GetComponent<MeshRenderer>().forceRenderingOff = true;
        //    Debug.Log("030!!!");
        //}
        //Vector2 f = new Vector2(cam.transform.forward.x, cam.transform.forward.z).normalized;

        //foreach (Vector3 point in points)
        //{
        //    //yield return new WaitForEndOfFrame();

        //    Vector2 v = new Vector2(point.x, point.z) - new Vector2(cam.transform.position.x, cam.transform.position.z);
        //    float dist = v.magnitude;
        //    float angle = Vector2.Angle(f, v);

        //    if ((angle > in_angle && dist > mid_dist) || dist > in_dist)
        //    {
        //        in_view = false;
        //    }
        //    else if (angle <= in_angle && dist <= mid_dist)
        //    {1
        //        in_view = true;
        //        break;
        //    }
        //}
    }

    public IEnumerator IDWAnalyze(Terrain terrain, int x_index, int z_index, int x_piece_num, int z_piece_num)
    {
        float min_mse = float.MaxValue;
        float min_error_power = TerrainGenerator.power;
        for (; TerrainGenerator.power < 10.0f; TerrainGenerator.power += 0.1f)
        {
            yield return StartCoroutine(TerrainGenerator.generateTerrainPatchTex(x_index, z_index, x_piece_num, z_piece_num));
            yield return StartCoroutine(TerrainGenerator.generateTerrainPatchWithTex(x_index, z_index, x_piece_num, z_piece_num));
            float mse = TerrainGenerator.calcMSE(origin_terrain, x_index, z_index, x_piece_num, z_piece_num);
            if (mse < min_mse)
            {
                min_mse = mse;
                min_error_power = TerrainGenerator.power;
            }
            Debug.Log(TerrainGenerator.power + ": " + mse);
        }
        TerrainGenerator.power = min_error_power;
        TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
        yield return StartCoroutine(TerrainGenerator.generateTerrainPatchTex(x_index, z_index, x_piece_num, z_piece_num));
        yield return StartCoroutine(TerrainGenerator.generateTerrainPatchWithTex(x_index, z_index, x_piece_num, z_piece_num));
        TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
        TimeSpan ts = ts1.Subtract(ts2).Duration();
        Debug.Log("The best power: " + min_error_power + " MSE = " + min_mse + " time cost: " + ts.TotalMilliseconds + "ms");
        yield return null;
    }
}