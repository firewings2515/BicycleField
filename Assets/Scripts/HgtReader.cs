using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class HgtReader
{
    static int resolution = 3601;
    static string hgt_file_location = "Assets\\Resources\\hgt\\";

    static string getHgtFileName(float longitude, float latitude)
    {
        int lat_int = (int)latitude;
        int lon_int = (int)longitude;
        char EW = longitude > 0.0f ? 'E' : 'W';
        char NS = latitude > 0.0f ? 'N' : 'S';
        return NS + Math.Abs(lat_int).ToString() + EW + Math.Abs(lon_int).ToString() + ".hgt";
    }

    static string getHgtPath(float longitude, float latitude)
    {
        return hgt_file_location + getHgtFileName(longitude, latitude);
    }

    static public float getElevation(float longitude, float latitude)
    {
        int lat_int = (int)latitude;
        int lon_int = (int)longitude;
        FileStream hgt_file = new FileStream(getHgtPath(longitude, latitude), FileMode.Open);
        int lat_row = (int)Math.Round((latitude - lat_int) * (resolution - 1));
        int lon_row = (int)Math.Round((longitude - lon_int) * (resolution - 1));
        hgt_file.Seek(((resolution - 1 - lat_row) * resolution + lon_row) * 2, SeekOrigin.Begin);
        int byte1 = hgt_file.ReadByte();
        int byte2 = hgt_file.ReadByte();
        int result = byte1 << 8 | byte2;
        return (float)result;
    }

    static public List<float> getElevations(List<EarthCoord> all_coords)
    {

        Dictionary<string, FileStream> hgt_file_map = new Dictionary<string, FileStream>();
        List<KeyValuePair<string,int>> rows = new List<KeyValuePair<string, int>>();
        List<float> results = new List<float>();
        for (int i = 0; i < all_coords.Count; i++)
        {
            string hgt_file_name = all_coords[i].getHgtFileName();
            if (!hgt_file_map.ContainsKey(hgt_file_name))
            {
                hgt_file_map.Add(hgt_file_name, new FileStream(hgt_file_location + hgt_file_name, FileMode.Open));
            }
            float latitude = all_coords[i].latitude;
            float longitude = all_coords[i].longitude;
            int lat_int = (int)latitude;
            int lon_int = (int)longitude;
            int lat_row = (int)Math.Round((latitude - lat_int) * (resolution - 1));
            int lon_row = (int)Math.Round((longitude - lon_int) * (resolution - 1));
            int position = ((resolution - 1 - lat_row) * resolution + lon_row) * 2;
            rows.Add(new KeyValuePair<string, int>(hgt_file_name, position));
        }

        for (int i = 0; i < rows.Count; i++)
        {
            hgt_file_map[rows[i].Key].Seek(rows[i].Value, SeekOrigin.Begin);
            int byte1 = hgt_file_map[rows[i].Key].ReadByte();
            int byte2 = hgt_file_map[rows[i].Key].ReadByte();
            int result = byte1 << 8 | byte2;
            results.Add((float)result);
        }
        return results;
    }

}
