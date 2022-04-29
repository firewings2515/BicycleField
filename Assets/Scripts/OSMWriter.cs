using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class OSMWriter : MonoBehaviour
{
    public OSMReader osm_reader;
    public string file_name = "map.osm";
    public string output_name = "mapLimit.osm";
    public bool write_osm_file = false;
    public float lon_max;
    public float lon_min;
    public float lat_max;
    public float lat_min;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (write_osm_file)
        {
            write_osm_file = false;
            osm_reader = new OSMReader();
            osm_reader.readOSM(Application.streamingAssetsPath + "//" + file_name, false, Application.streamingAssetsPath + "//" + file_name, true, lon_max, lon_min, lat_max, lat_min, true);
            writeOSM(Application.streamingAssetsPath + "//" + output_name);
        }
    }

    private void writeOSM(string file_path)
    {
        Debug.Log("Writing " + file_path);
        using (StreamWriter sw = new StreamWriter(file_path))
        {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<osm version=\"0.6\" generator=\"osmium/1.8.0\">");
            sw.WriteLine($"  <bounds minlat=\"{osm_reader.boundary_min.y}\" minlon=\"{osm_reader.boundary_min.x}\" maxlat=\"{osm_reader.boundary_max.y}\" maxlon=\"{osm_reader.boundary_max.x}\"/>");
            foreach (KeyValuePair<string, Node> point in osm_reader.points_lib)
            {
                sw.Write(point.Value.writeNode(point.Key));
            }

            foreach (Way path in osm_reader.pathes)
            {
                sw.Write(path.writeWay());
            }

            foreach (House house in osm_reader.houses)
            {
                sw.Write(house.writeWay());
            }
            sw.WriteLine("</osm>");
        }
        Debug.Log("Write Successfully!");
    }
}