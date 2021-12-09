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
    int in_angle = 65;
    int in_dist = 150;
    int mid_dist = 100;

    // house information
    public bool is_house = false;
    bool build_house = false;
    private string obj;
    private string mtl;
    public string house_id;
    public Vector3 center;
    GameObject house_mesh;
    public float building_height;

    // road information
    string road_id;
    RoadIntegration road_integration;
    public List<int> belong_to_hier_x;
    public List<int> belong_to_hier_y;

    // Start is called before the first frame update
    void Start()
    {
        //InvokeRepeating("viewerUpdate", 0f, 0.02f);
        belong_to_hier_x = new List<int>();
        belong_to_hier_y = new List<int>();
    }

    public void setup(bool setup_prefab, bool have_instance = true)
    {
        if (setup_prefab)
            instance = Instantiate(prefab, gameObject.transform);

        if (have_instance)
            instance.SetActive(false);

        finish_instance = true;
    }

    // set the house information if the instance is house building
    public void setHouse(string house_id, string obj, string mtl, Vector3 center)
    {
        is_house = true;
        build_house = true;
        this.house_id = house_id;
        this.obj = obj;
        this.mtl = mtl;
        this.center = center;
    }

    // set the house information if the instance is house polygon
    public void setHouse(string house_id, Vector3[] points, Vector3 center, GameObject cam, RoadIntegration road_integration)
    {
        is_house = true;
        this.house_id = house_id;
        this.cam = cam;
        this.road_integration = road_integration;
        this.points = points;
        this.center = center;
        setup(false);
        in_dist = 3200;
        mid_dist = 3200;
    }

    // set the road information if the instance is road
    public void setRoad(string path_id, Vector3[] points, GameObject cam, RoadIntegration road_integration)
    {
        this.road_id = path_id;
        this.cam = cam;
        this.road_integration = road_integration;
        this.points = points;
        setup(false);
        in_dist = 3200;
        mid_dist = 3200;
    }

    // set the road information if the instance is road
    public void setRoad(string path_id, List<Vector3> points, GameObject cam, RoadIntegration road_integration)
    {
        setRoad(path_id, points.ToArray(), cam, road_integration);
    }

    public float getDistance(Vector3 target)
    {
        float dist = float.MaxValue;
        foreach (Vector3 point in points)
        {
            dist = Mathf.Min(dist, Vector3.Distance(target, point));
        }
        return dist;
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

                if ((angle > in_angle && dist > mid_dist) || dist > in_dist)
                {
                    in_view = false;
                }
                else if (angle <= in_angle && dist <= mid_dist)
                {
                    in_view = true;
                    break;
                }
            }

            if (is_house && build_house)
            {
                // if instance is house and in view
                if (in_view)
                {
                    if (house_mesh == null)
                    {
                        //8603.1, 654.4, 10195.0
                        // build the mesh of building
                        house_mesh = ShapeGrammarBuilder.StringToGameobject(ref obj, ref mtl);
                        house_mesh.name = "house_" + house_id + center.ToString();
                        Mesh mesh = house_mesh.GetComponentInChildren<MeshFilter>().mesh;
                        
                        //Recalculations
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        mesh.Optimize();

                        //// set transform
                        //house_mesh.GetComponentInChildren<MeshFilter>().mesh = mesh;
                        //Transform temp = house_mesh.GetComponentsInChildren<Transform>()[1];
                        ////temp.transform.position = new Vector3(center.x, 0, center.y);
                        ////Bounds bound = mesh.bounds;
                        //house_mesh.transform.position = new Vector3(center.x, center.y, center.z); // - 11.2f   + 18.7f
                        //house_mesh.transform.Rotate(new Vector3(0, 183, 0));
                        //house_mesh.transform.localScale = new Vector3(house_mesh.transform.localScale.x * 0.8f, house_mesh.transform.localScale.y * 0.8f, house_mesh.transform.localScale.z * 0.8f);


                        house_mesh.GetComponentInChildren<MeshFilter>().mesh = mesh;
                        house_mesh.transform.position = center;
                        house_mesh.transform.rotation = Quaternion.Euler(0, 180, 0);
                        house_mesh.transform.localScale = new Vector3(house_mesh.transform.localScale.x * 0.8f, house_mesh.transform.localScale.y * 0.8f, house_mesh.transform.localScale.z * 0.8f);

                        //­×¥¿mesh¦ì²¾----
                        Bounds Bound = mesh.bounds;
                        Vector3 offset = house_mesh.transform.position - house_mesh.transform.TransformPoint(Bound.center);
                        house_mesh.transform.position += offset;
                        //----

                        float y_offset = Mathf.Abs(house_mesh.transform.position.y - center.y);
                        house_mesh.transform.Translate(0, y_offset, 0);
                        //house_mesh.transform.position = new Vector3(house_mesh.transform.position.x, center.y, house_mesh.transform.position.z); ;

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

    void OnMouseDown()
    {
        road_integration.selectPath(road_id);
    }
}