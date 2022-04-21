using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    ControlPointManager controlpoint_manager;
    string point_id;
    Node node;
    Material spheres_select_mat;
    Material spheres_unselect_mat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setControlPoint(string id, Node node, ControlPointManager controlpoint_manager)
    {
        point_id = id;
        this.node = node;
        this.controlpoint_manager = controlpoint_manager;
        spheres_select_mat = controlpoint_manager.spheres_select_mat;
        spheres_unselect_mat = controlpoint_manager.spheres_unselect_mat;
    }

    void OnMouseDown()
    {
        if (controlpoint_manager != null)
        {
            controlpoint_manager.selectPoint(point_id);
            select();
        }
    }

    public void select()
    {
        GetComponent<MeshRenderer>().material = spheres_select_mat;
        Debug.Log(point_id);
    }

    public void unselect()
    {
        GetComponent<MeshRenderer>().material = spheres_unselect_mat;
    }
}
