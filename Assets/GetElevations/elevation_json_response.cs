using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EarthCoord
{
    [SerializeField] public float latitude;
    [SerializeField] public float longitude;
    public EarthCoord(float _longitude, float _latitude) {
        latitude = _latitude;
        longitude = _longitude;
    }
}

public class Results
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public int elevation { get; set; }
}

public class elevation_json_response
{
    public List<Results> results { get; set; }
}

