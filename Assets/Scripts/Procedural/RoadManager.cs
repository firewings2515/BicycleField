using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PathCreation;
using UnityEditor;

public class RoadManager : MonoBehaviour
{
    private int current_segment = 2;
    private int current_loaded_segment = 0;
    private int current_running_segment = 0;
    private int house_id = 0;

    private StreamReader reader;

    public string file_name;
    public PathCreator path_creator;
    public bool path_loop = false;
    private bool update_mesh = false;

    Vector3 last_segment = new Vector3(0,0,0);

    private void Start()
    {
        reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name);

        //remove first default segment
        removeEarliestRoad(false);

        while (path_creator.bezierPath.NumSegments < Info.MAX_LOADED_SEGMENT)
        {
            getAndSetNextSegment();
        }

        //remove second default segment
        getAndSetNextSegment();

        //remove last default segment
        removeEarliestRoad(false);

        path_creator.bezierPath.NotifyPathModified();
    }

    // Update is called once per frame
    void Update()
    {
        if (Info.MAX_LOADED_SEGMENT - current_segment <= Info.PRELOAD_SEGMENT)
        {
            getAndSetNextSegment();

            path_creator.bezierPath = path_creator.bezierPath; //force update
            update_mesh = true;
        }
        else if (update_mesh)
        {
            update_mesh = false;
            GetComponent<PathCreation.Examples.MyRoadMeshCreator>().CreateRoadMesh();
        }
    }

    private void getAndSetNextSegment()
    {
        if (getNextSegment(out string str_point))
        {
            Vector3 vec3_point = Functions.StrToVec3(str_point) + new Vector3(-200, 0, -200);
            vec3_point.y = 0.0f;
            last_segment = vec3_point;

            spawnAnchorCheckpoint(vec3_point);

            generateRoad(vec3_point);
            if (path_creator.bezierPath.NumSegments > Info.MAX_LOADED_SEGMENT) removeEarliestRoad();
        }
    }

    private bool getNextSegment(out string point_data)
    {
        current_loaded_segment++;
        house_id = 0;
        //
        List<int> segment_id_list = new List<int>();
        List< int > house_id_list = new List<int>();
        List<string> info_list = new List<string>();
        //
        point_data = reader.ReadLine();

        if (path_loop)
        {
            if (point_data == null)
            {
                reader.Close();
                reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name);
                point_data = reader.ReadLine();
            }
        }

        while (point_data != null && point_data[0] == 'H')
        {
            segment_id_list.Add(current_loaded_segment);
            house_id_list.Add(house_id);
            info_list.Add(point_data);
            
            house_id++;

            //GetComponent<HouseManager>().addToBuffer(point_data);
            point_data = reader.ReadLine();
        }
        StartCoroutine(HouseGenerator.generateHouses(segment_id_list, house_id_list, info_list));
        return point_data != null;
    }

    private void generateRoad(Vector3 road)
    {
        //terrain
        TerrainGenerator.generateTerrain(road);
        //BezierPath new_bezier = new BezierPath(path_creator.bezierPath[0]);
        //new_bezier = path_creator.bezierPath;
        //path_creator.bezierPath = new_bezier;

        path_creator.bezierPath.AddSegmentToEnd(road);
        //force display
        Selection.activeGameObject = this.gameObject;
    }

    private void removeEarliestRoad(bool destroy = true)
    {
        if (destroy) HouseGenerator.destroySegment(current_running_segment - (Info.MAX_LOADED_SEGMENT / 2) + 2);
        path_creator.bezierPath.DeleteSegment(0);
        current_segment--;
    }

    private void spawnAnchorCheckpoint(Vector3 position)
    {
        GameObject prefab = new GameObject();
        //position.y = TerrainGenerator.getIDWHeightWithBais(position.x, position.z);
        prefab.transform.position = position;
        prefab.AddComponent<SphereCollider>();
        prefab.GetComponent<SphereCollider>().isTrigger = true;
        prefab.GetComponent<SphereCollider>().transform.localScale *= Info.CHECKPOINT_SIZE;
        prefab.AddComponent<AnchorCheckpoint>();

    }

    public void incrementCurrentSegment()
    {
        current_segment++;
        current_running_segment++;
    }
}
