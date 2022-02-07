using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OSMTerrainRender : MonoBehaviour
{
    OSMEditor osm_editor;
    bool is_initial = false;
    int resolution = 128;
    public Material terrain_mat;
    public double[] center_pos = new double[2]; // 121.5402259 25.1677106 121.6414614 25.217501
    public GameObject test_ball;
    // Start is called before the first frame update
    void Start()
    {
        osm_editor = GetComponent<OSMEditor>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!is_initial && osm_editor.is_initial)
        {
            is_initial = true;
            double[,,] terrain_points = new double[resolution + 1, resolution + 1, 3];
            double[] lefttop_pos = new double[3] { MercatorProjection.xToLon(MercatorProjection.lonToX(center_pos[0]) - 4096),
                                                   0.0f,
                                                   MercatorProjection.yToLat(MercatorProjection.latToY(center_pos[1]) - 4096)}; //121.5094453f, 0.0f, 25.1079903f 
            double rightbottom_pos_x = MercatorProjection.xToLon(MercatorProjection.lonToX(lefttop_pos[0]) + 8192); // + 16384.0
            double rightbottom_pos_z = MercatorProjection.yToLat(MercatorProjection.latToY(lefttop_pos[2]) + 8192); // + 16384.0
            double[] terrain_size = new double[2] { rightbottom_pos_x - lefttop_pos[0], rightbottom_pos_z - lefttop_pos[2] };
            double dx = terrain_size[0] / resolution;
            double dz = terrain_size[1] / resolution;
            //////////////////////////////get elevations/////////////////////////////////////////
            List<float> all_elevations = new List<float>();
            List<EarthCoord> all_coords = new List<EarthCoord>();
            for (int i = 0; i < resolution + 1; i++)
            {
                for (int j = 0; j < resolution + 1; j++)
                {
                    terrain_points[i, j, 0] = lefttop_pos[0] + i * dx;
                    terrain_points[i, j, 1] = 0.0;
                    terrain_points[i, j, 2] = lefttop_pos[2] + j * dz;
                    all_coords.Add(new EarthCoord((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 2]));
                }
            }
            all_elevations = HgtReader.getElevations(all_coords);
            /////////////////////////////////////////////////////////////////////////////////////
            // set elevations
            Mesh mesh = new Mesh();
            Vector3[] vertice = new Vector3[(resolution + 1) * (resolution + 1)];
            Vector2[] vertice2D = new Vector2[(resolution + 1) * (resolution + 1)];
            int[] indices = new int[6 * resolution * resolution];
            int indices_index = 0;
            for (int i = 0; i < resolution + 1; i++)
            {
                for (int j = 0; j < resolution + 1; j++)
                {
                    //float pos_x, pos_z;
                    //osm_editor.osm_reader.toUnityLocation(terrain_points[i, j].x, terrain_points[i, j].z, out pos_x, out pos_z);
                    terrain_points[i, j, 0] = MercatorProjection.lonToX(terrain_points[i, j, 0]) - osm_editor.osm_reader.boundary_min.x;
                    terrain_points[i, j, 1] = all_elevations[i * (resolution + 1) + j];
                    terrain_points[i, j, 2] = MercatorProjection.latToY(terrain_points[i, j, 2]) - osm_editor.osm_reader.boundary_min.y;
                    //terrain_points[i, j].x = pos_x;
                    //terrain_points[i, j].y = all_elevations[i * (resolution + 1) + j];
                    //terrain_points[i, j].z = pos_z;
                    vertice[i * (resolution + 1) + j] = new Vector3((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 1], (float)terrain_points[i, j, 2]);
                    //vertice2D[i * (resolution + 1) + j] = new Vector2(terrain_points[i, j].x, terrain_points[i, j].z);
                }
            }
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    // counter-clockwise
                    indices[indices_index++] = i * (resolution + 1) + j;
                    indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
                    indices[indices_index++] = (i + 1) * (resolution + 1) + j;
                    indices[indices_index++] = i * (resolution + 1) + j;
                    indices[indices_index++] = i * (resolution + 1) + j + 1;
                    indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
                }
            }
            //for (int i = 0; i < resolution * resolution * 6; i++)
            //{
            //    if (i % 6 == 0)
            //        Debug.Log("==============");
            //    Debug.Log(indices[i]);
            //}
            // Use the triangulator to get indices for creating triangles
            //Triangulator tr = new Triangulator(vertice2D);
            //int[] indices = tr.Triangulate();
            //Assign data to mesh
            mesh.vertices = vertice;
            mesh.triangles = indices;
            //Recalculations
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            //Name the mesh
            mesh.name = "terrain_mesh";
            GameObject terrain = new GameObject("terrain");
            MeshFilter mf = terrain.AddComponent<MeshFilter>();
            MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = terrain_mat;

            // test board in DouglasPeucker performance
            //List<Vector3> test_profile = new List<Vector3>();
            //for (int i = 0; i < resolution + 1; i++)
            //{
            //    Vector3 point = new Vector3((float)terrain_points[i, 0, 0], (float)terrain_points[i, 0, 1], (float)terrain_points[i, 0, 2]);
            //    test_profile.Add(point);
            //    Instantiate(test_ball, point, Quaternion.identity);
            //}
            //List<Vector3> profile = DouglasPeuckerAlgorithm.DouglasPeucker(test_profile, 1.0f);
            //foreach (Vector3 point in profile)
            //{
            //    Instantiate(test_ball, point - new Vector3(0, 0, 10), Quaternion.identity);
            //}
        }
    }
}