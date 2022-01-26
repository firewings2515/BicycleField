using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSMTerrainProcedural : MonoBehaviour
{
    OSMEditor osm_editor;
    int resolution = 128; //128
    public Material terrain_mat;
    public double[] center_pos = new double[2];
    double[] center_xyz = new double[3];
    public GameObject cam;
    public bool is_initial = false;
    GameObject terrain;
    Mesh mesh;
    int[] current_loc = new int[2];
    double[] current_pos = new double[2];
    double center_ele = 13.5;
    Dictionary<Vector2, double> dp_ele = new Dictionary<Vector2, double>();
    double near;
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
            center_xyz[0] = MercatorProjection.lonToX(center_pos[0]);
            center_xyz[1] = center_ele;
            center_xyz[2] = MercatorProjection.latToY(center_pos[1]);
            double[] lefttop_pos = new double[3] { MercatorProjection.xToLon(center_xyz[0] + cam.transform.position.x - 2048),
                                                   0.0f,
                                                   MercatorProjection.yToLat(center_xyz[2] + cam.transform.position.z - 2048)}; //121.5094453f, 0.0f, 25.1079903f 
            double rightbottom_pos_x = MercatorProjection.xToLon(MercatorProjection.lonToX(lefttop_pos[0]) + 4096); // + 16384.0
            double rightbottom_pos_z = MercatorProjection.yToLat(MercatorProjection.latToY(lefttop_pos[2]) + 4096); // + 16384.0
            double[] terrain_size = new double[2] { rightbottom_pos_x - lefttop_pos[0], rightbottom_pos_z - lefttop_pos[2] };
            double dx = terrain_size[0] / resolution;
            double dz = terrain_size[1] / resolution;
            near = dx / 2;

            current_pos[0] = MercatorProjection.xToLon(center_xyz[0]);
            current_pos[1] = MercatorProjection.yToLat(center_xyz[2]);
            current_loc[0] = (int)(current_pos[0] / dx);
            current_loc[1] = (int)(current_pos[1] / dz);

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
            mesh = new Mesh();
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
                    terrain_points[i, j, 0] = MercatorProjection.lonToX(terrain_points[i, j, 0]) - center_xyz[0];
                    terrain_points[i, j, 1] = all_elevations[i * (resolution + 1) + j] - center_xyz[1];
                    terrain_points[i, j, 2] = MercatorProjection.latToY(terrain_points[i, j, 2]) - center_xyz[2];
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

            terrain = new GameObject("terrain");
            MeshFilter mf = terrain.AddComponent<MeshFilter>();
            MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = terrain_mat;
            mr.material.SetFloat("Vector1_1827e4cc820e4294b95207d8f2b4bed7", (float)center_xyz[1]);
        }
        else if (is_initial)
        {
            //double[,,] terrain_points = new double[resolution + 1, resolution + 1, 3];
            //center_xyz[0] = MercatorProjection.lonToX(center_pos[0]);
            //center_xyz[1] = center_ele;
            //center_xyz[2] = MercatorProjection.latToY(center_pos[1]);
            //double[] lefttop_pos = new double[3] { MercatorProjection.xToLon(center_xyz[0] + cam.transform.position.x - 512),
            //                                       0.0f,
            //                                       MercatorProjection.yToLat(center_xyz[2] + cam.transform.position.z - 512)}; //121.5094453f, 0.0f, 25.1079903f 
            //double rightbottom_pos_x = MercatorProjection.xToLon(MercatorProjection.lonToX(lefttop_pos[0]) + 1024); // + 16384.0
            //double rightbottom_pos_z = MercatorProjection.yToLat(MercatorProjection.latToY(lefttop_pos[2]) + 1024); // + 16384.0
            //double[] terrain_size = new double[2] { rightbottom_pos_x - lefttop_pos[0], rightbottom_pos_z - lefttop_pos[2] };
            //double dx = terrain_size[0] / resolution;
            //double dz = terrain_size[1] / resolution;

            //current_pos[0] = MercatorProjection.xToLon(center_xyz[0] + cam.transform.position.x);
            //current_pos[1] = MercatorProjection.yToLat(center_xyz[2] + cam.transform.position.z);
            //if ((int)(current_pos[0] / dx) == current_loc[0] && (int)(current_pos[1] / dz) == current_loc[1])
            //{
            //    return;
            //}
            //current_loc[0] = (int)(current_pos[0] / dx);
            //current_loc[1] = (int)(current_pos[1] / dz);

            ////////////////////////////////get elevations/////////////////////////////////////////
            //List<float> all_elevations = new List<float>();
            //List<EarthCoord> all_coords = new List<EarthCoord>();
            //for (int i = 0; i < resolution + 1; i++)
            //{
            //    for (int j = 0; j < resolution + 1; j++)
            //    {

            //        terrain_points[i, j, 0] = lefttop_pos[0] + i * dx;
            //        terrain_points[i, j, 1] = 0.0;
            //        terrain_points[i, j, 2] = lefttop_pos[2] + j * dz;
            //        all_coords.Add(new EarthCoord((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 2]));
            //    }
            //}
            //all_elevations = HgtReader.getElevations(all_coords);
            ///////////////////////////////////////////////////////////////////////////////////////

            //// set elevations
            //mesh = new Mesh();
            //Vector3[] vertice = new Vector3[(resolution + 1) * (resolution + 1)];
            //int[] indices = new int[6 * resolution * resolution];
            //int indices_index = 0;
            //for (int i = 0; i < resolution + 1; i++)
            //{
            //    for (int j = 0; j < resolution + 1; j++)
            //    {
            //        //float pos_x, pos_z;
            //        //osm_editor.osm_reader.toUnityLocation(terrain_points[i, j].x, terrain_points[i, j].z, out pos_x, out pos_z);
            //        terrain_points[i, j, 0] = MercatorProjection.lonToX(terrain_points[i, j, 0]) - center_xyz[0];
            //        double history_ele;
            //        if (searchClosing((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 2], out history_ele))
            //            terrain_points[i, j, 1] = history_ele;
            //        else
            //        {
            //            terrain_points[i, j, 1] = all_elevations[i * (resolution + 1) + j] - center_xyz[1];
            //            dp_ele.Add(new Vector2((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 2]), terrain_points[i, j, 1]);
            //        }
            //        terrain_points[i, j, 2] = MercatorProjection.latToY(terrain_points[i, j, 2]) - center_xyz[2];
            //        //terrain_points[i, j].x = pos_x;
            //        //terrain_points[i, j].y = all_elevations[i * (resolution + 1) + j];
            //        //terrain_points[i, j].z = pos_z;
            //        vertice[i * (resolution + 1) + j] = new Vector3((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 1], (float)terrain_points[i, j, 2]);
            //        //vertice2D[i * (resolution + 1) + j] = new Vector2(terrain_points[i, j].x, terrain_points[i, j].z);
            //    }
            //}

            //for (int i = 0; i < resolution; i++)
            //{
            //    for (int j = 0; j < resolution; j++)
            //    {
            //        // counter-clockwise
            //        indices[indices_index++] = i * (resolution + 1) + j;
            //        indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
            //        indices[indices_index++] = (i + 1) * (resolution + 1) + j;

            //        indices[indices_index++] = i * (resolution + 1) + j;
            //        indices[indices_index++] = i * (resolution + 1) + j + 1;
            //        indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
            //    }
            //}

            ////Assign data to mesh
            //mesh.vertices = vertice;
            //mesh.triangles = indices;

            ////Recalculations
            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
            //mesh.Optimize();

            ////Name the mesh
            //mesh.name = "terrain_mesh";

            //terrain.GetComponent<MeshFilter>().mesh = mesh;
        }

        bool searchClosing(float x, float z, out double y)
        {
            if (dp_ele.ContainsKey(new Vector2(x, z)))
            {
                y = dp_ele[new Vector2(x, z)];
                return true;
            }
            foreach (KeyValuePair<Vector2, double> point_info in dp_ele)
            {
                if (Vector2.Distance(point_info.Key, new Vector2(x, z)) < near)
                {
                    y = point_info.Value;
                    return true;
                }
            }
            y = 0;
            return false;
        }
    }
}