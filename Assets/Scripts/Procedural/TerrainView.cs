using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainView : MonoBehaviour
{
    //public GameObject cam;
    //float radius = 16 * 2 * 32;
    // Start is called before the first frame update
    bool is_update_mesh = false;
    public int x_index;
    public int z_index;
    public int x_piece_num;
    public int z_piece_num;
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
        if (!is_update_mesh)
        {
            int[] progress = new int[1];
            TerrainGenerator.progress_buffer[x_index * TerrainGenerator.z_patch_num + z_index].GetData(progress);
            if (progress[0] == 1)
            {
                is_update_mesh = true;
                StartCoroutine(TerrainGenerator.generateTerrainPatchWithTex(x_index, z_index, x_piece_num, z_piece_num));
            }
            else
            {
                Debug.LogError("Fence not working");
            }
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
}