using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testGetElevations : MonoBehaviour
{
    [SerializeField]
    public EarthCoord[] all_coords;
    // Start is called before the first frame update
    void Start()
    {

        List<EarthCoord> all_coords_list = new List<EarthCoord>(all_coords);
        List<float> all_ele = HgtReader.getElevations(all_coords_list);

        for (int i = 0; i < all_ele.Count; i++)
        {
            Debug.Log(
                "latitude: " + all_coords[i].latitude.ToString() + ' ' +
                "longitude: " + all_coords[i].longitude.ToString() + ' ' +
                "elevation: " + all_ele[i].ToString() + ' '
                );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
