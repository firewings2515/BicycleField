using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

