using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testHgt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //List<EarthCoord> all_coords = new List<EarthCoord>();
        //all_coords.Add(new EarthCoord(121.58098f, 25.20202f));
        //all_coords.Add(new EarthCoord(121.33301f, 24.67198f));
        //List<float> result = HgtReader.getElevations(all_coords);
        //Debug.Log(result[0]);


        Debug.Log(HgtReader.getElevation(121.58098f, 25.20202f));
        Debug.Log(HgtReader.getElevation(121.33301f, 24.67198f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
