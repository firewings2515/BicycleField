using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testGetElevations : MonoBehaviour
{
    [SerializeField]
    public EarthCoord[] all_coords;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        List<EarthCoord> all_coords_list = new List<EarthCoord>(all_coords);
        GetElevations ele = gameObject.AddComponent<GetElevations>();
        yield return ele.get_elevation_list(all_coords_list);
        for (int i = 0; i < ele.elevations.Count; i++) {
            Debug.Log(
                "latitude: " + all_coords[i].latitude.ToString() + ' ' + 
                "longitude: " + all_coords[i].longitude.ToString()+ ' ' + 
                "elevation: " + ele.elevations[i].ToString() + ' '
                );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
