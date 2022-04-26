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
    static Dictionary<string, FileStream> hgt_file_map = new Dictionary<string, FileStream>();
    static int m_floor(float input)
    {
        return (int)Math.Floor(input);
    }
    static string getHgtFileName(float longitude, float latitude)
    {
        int lat_int = m_floor(latitude);
        int lon_int = m_floor(longitude);
        char EW = longitude > 0.0f ? 'E' : 'W';
        char NS = latitude > 0.0f ? 'N' : 'S';
        return NS + Math.Abs(lat_int).ToString() + EW + Math.Abs(lon_int).ToString() + ".hgt";
    }

    static string getHgtPath(float longitude, float latitude)
    {
        return hgt_file_location + getHgtFileName(longitude, latitude);
    }
    static int hgtSeekPos(int lat_row, int lon_row)
    {
        return ((resolution - 1 - lat_row) * resolution + lon_row) * 2;
    }
    static void hgtSeek(FileStream hgt_file, int lat_row, int lon_row)
    {
        hgt_file.Seek(hgtSeekPos(lat_row, lon_row), SeekOrigin.Begin);
    }

    static public float getElevation(float longitude, float latitude)
    {
        int lat_int = m_floor(latitude);
        int lon_int = m_floor(longitude);
        FileStream hgt_file;
        if (System.IO.File.Exists(getHgtPath(longitude, latitude)))
        {
            hgt_file = new FileStream(getHgtPath(longitude, latitude), FileMode.Open);
        }
        else
        {
            return 0.0f;
        }
        int lat_row = (int)Math.Round((latitude - lat_int) * (resolution - 1));
        int lon_row = (int)Math.Round((longitude - lon_int) * (resolution - 1));
        hgtSeek(hgt_file, lat_row, lon_row);
        int byte1 = hgt_file.ReadByte();
        int byte2 = hgt_file.ReadByte();
        int result = byte1 << 8 | byte2;
        return (float)result;
    }

    static public List<float> getElevations(List<EarthCoord> all_coords, bool interpolation = false)
    {
        List<KeyValuePair<string, int>> rows = new List<KeyValuePair<string, int>>();
        List<float> results = new List<float>();
        List<double> ratio_lats = new List<double>();
        List<double> ratio_lons = new List<double>();
        for (int i = 0; i < all_coords.Count; i++)
        {
            string hgt_file_name = all_coords[i].getHgtFileName();
            if (!hgt_file_map.ContainsKey(hgt_file_name))
            {
                if (System.IO.File.Exists(hgt_file_location + hgt_file_name))
                {
                    hgt_file_map.Add(hgt_file_name, new FileStream(hgt_file_location + hgt_file_name, FileMode.Open));
                }
                else
                {
                    //insert empty string if coord not found in files
                    rows.Add(new KeyValuePair<string, int>("", 0));
                    continue;
                }
            }
            float latitude = all_coords[i].latitude;
            float longitude = all_coords[i].longitude;
            int lat_int = m_floor(latitude);
            int lon_int = m_floor(longitude);
            if (interpolation)
            {
                int lat_row_low = (int)Math.Floor((latitude - lat_int) * (resolution - 1));
                int lon_row_low = (int)Math.Floor((longitude - lon_int) * (resolution - 1));
                int lat_row_high = (int)Math.Ceiling((latitude - lat_int) * (resolution - 1));
                int lon_row_high = (int)Math.Ceiling((longitude - lon_int) * (resolution - 1));
                double lat_row = (latitude - lat_int) * (resolution - 1);
                ratio_lats.Add(1 - (lat_row - lat_row_low));
                double lon_row = (longitude - lon_int) * (resolution - 1);
                ratio_lons.Add(1 - (lon_row - lon_row_low));
                int position_a = hgtSeekPos(lat_row_low, lon_row_low);
                int position_b = hgtSeekPos(lat_row_low, lon_row_high);
                int position_c = hgtSeekPos(lat_row_high, lon_row_high);
                int position_d = hgtSeekPos(lat_row_high, lon_row_low);

                rows.Add(new KeyValuePair<string, int>(hgt_file_name, position_a));
                rows.Add(new KeyValuePair<string, int>(hgt_file_name, position_b));
                rows.Add(new KeyValuePair<string, int>(hgt_file_name, position_c));
                rows.Add(new KeyValuePair<string, int>(hgt_file_name, position_d));
            }
            else
            {
                int lat_row = (int)Math.Round((latitude - lat_int) * (resolution - 1));
                int lon_row = (int)Math.Round((longitude - lon_int) * (resolution - 1));
                int position = hgtSeekPos(lat_row, lon_row);

                rows.Add(new KeyValuePair<string, int>(hgt_file_name, position));
            }
        }

        List<float> results_x = new List<float>();
        for (int i = 0; i < rows.Count; i++)
        {
            if (string.IsNullOrEmpty(rows[i].Key))
            {
                results.Add(0.0f);//return 0 if not found
                continue;
            }
            hgt_file_map[rows[i].Key].Seek(rows[i].Value, SeekOrigin.Begin);
            int byte1 = hgt_file_map[rows[i].Key].ReadByte();
            int byte2 = hgt_file_map[rows[i].Key].ReadByte();
            int result = byte1 << 8 | byte2;
            if (result > 9000.0f || result < 0.0f)
            { //if strange value shows
                results.Add(0.0f);
                continue;
            }
            if (interpolation)
                results_x.Add((float)result);
            else
                results.Add((float)result);
        }

        if (interpolation)
        {
            for (int i = 0; i < rows.Count / 4; i++)
            {
                double result = results_x[i * 4] * ratio_lats[i] * ratio_lons[i] + results_x[i * 4 + 1] * ratio_lats[i] * (1 - ratio_lons[i]) + results_x[i * 4 + 2] * (1 - ratio_lats[i]) * (1 - ratio_lons[i]) + results_x[i * 4 + 3] * (1 - ratio_lats[i]) * ratio_lons[i];
                results.Add((float)result);
            }
        }

        return results;
    }
}