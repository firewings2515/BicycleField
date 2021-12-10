using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    private List<string> house_buffer = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (house_buffer.Count != 0)
        {
            generateHouse();
        }
    }

    public void addToBuffer(string house)
    {
        house_buffer.Add(house);
    }

    private void generateHouse()
    {
        string house_info = house_buffer[0];
        house_buffer.RemoveAt(0);

        string[] house_infos = house_info.Split(' ');
        Vector3[] points = new Vector3[int.Parse(house_infos[1]) + 1];
        for (int point = 2; point < house_infos.Length; point += 3)
        {
            Debug.Log(point / 3);
            points[point / 3] = new Vector3(float.Parse(house_infos[point]), float.Parse(house_infos[point + 1]), float.Parse(house_infos[point + 2])); 
        }

        Mesh mesh = new Mesh();
        mesh.vertices = points;

        GameObject house_base = new GameObject();
        house_base.AddComponent<MeshCollider>();
        house_base.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
