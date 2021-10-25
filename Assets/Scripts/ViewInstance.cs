using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewInstance : MonoBehaviour
{
    public GameObject prefab;
    public GameObject instance;
    bool in_view = false;
    public GameObject cam;
    public Vector3[] points;
    public bool finish_instance = false;

    // house information
    private bool is_house = false;
    private string obj;
    private string mtl;
    private string house_id;
    private Vector3 center;
    GameObject house_mesh;

    // Start is called before the first frame update
    void Start()
    {
        //InvokeRepeating("viewerUpdate", 0f, 0.02f);
    }

    public void setup(bool setup_prefab)
    {
        if (setup_prefab)
            instance = Instantiate(prefab, gameObject.transform);

        instance.SetActive(false);

        finish_instance = true;
    }

    // set the house information if the instance is house building
    public void setHouse(string house_id, string obj, string mtl, Vector3 center)
    {
        is_house = true;
        this.house_id = house_id;
        this.obj = obj;
        this.mtl = mtl;
        this.center = center;
    }

    private void Update()
    {
        if (finish_instance)
        {
            Vector2 f = new Vector2(cam.transform.forward.x, cam.transform.forward.z).normalized;

            foreach (Vector3 point in points)
            {
                //yield return new WaitForEndOfFrame();

                Vector2 v = new Vector2(point.x, point.z) - new Vector2(cam.transform.position.x, cam.transform.position.z);
                float dist = v.magnitude;
                float angle = Vector2.Angle(f, v);

                if ((angle > 65 && dist > 150) || dist > 100)
                {
                    in_view = false;
                }
                else if (angle <= 65 && dist <= 150)
                {
                    in_view = true;
                    break;
                }
            }

            if (is_house)
            {
                // if instance is house and in view
                if (in_view)
                {
                    if (house_mesh == null)
                    {
                        // build the mesh of building
                        house_mesh = ShapeGrammarBuilder.StringToGameobject(ref obj, ref mtl);
                        house_mesh.name = "house_" + house_id;
                        Mesh mesh = house_mesh.GetComponentInChildren<MeshFilter>().mesh;
                        
                        //Recalculations
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        mesh.Optimize();

                        // set transform
                        house_mesh.GetComponentInChildren<MeshFilter>().mesh = mesh;
                        Transform temp = house_mesh.GetComponentsInChildren<Transform>()[1];
                        //temp.transform.position = new Vector3(center.x, 0, center.y);
                        Bounds bound = mesh.bounds;
                        house_mesh.transform.position = new Vector3(center.x - 31, -bound.center.y, center.y + 35);
                        house_mesh.transform.Rotate(new Vector3(0, 183, 0));
                    }
                }
                else
                {
                    // destroy if not in view
                    if (house_mesh != null)
                    {
                        Destroy(house_mesh);
                        house_mesh = null;
                    }
                }
            }

            instance.SetActive(in_view);
        }
    }
}