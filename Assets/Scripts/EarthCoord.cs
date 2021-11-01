using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EarthCoord
{
    [SerializeField] public float latitude;
    [SerializeField] public float longitude;
    public EarthCoord(float _longitude, float _latitude)
    {
        latitude = _latitude;
        longitude = _longitude;
    }

    public string getHgtFileName()
    {
        int lat_int = (int)latitude;
        int lon_int = (int)longitude;
        char EW = longitude > 0.0f ? 'E' : 'W';
        char NS = latitude > 0.0f ? 'N' : 'S';
        return NS + Math.Abs(lat_int).ToString() + EW + Math.Abs(lon_int).ToString() + ".hgt";
    }
}
