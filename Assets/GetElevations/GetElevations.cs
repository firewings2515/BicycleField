using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;

public class GetElevations : MonoBehaviour
{
    // Start is called before the first frame update
    string api_url;
    public List<float> elevations = new List<float>();
    void Start()
    {

    }


    public IEnumerator get_elevation_list(List<EarthCoord> coords) {
        if (elevations!=null) elevations.Clear();
        api_url = "https://api.open-elevation.com/api/v1/lookup?locations=";
        for (int i = 0; i < coords.Count-1; i++) {
            api_url += coords[i].latitude.ToString() + ',' + coords[i].longitude.ToString() + '|';
        }
        api_url += coords.Last().latitude.ToString() + ',' + coords.Last().longitude.ToString();
        Debug.Log(api_url);
        yield return StartCoroutine(make_request(api_url));
    }

    IEnumerator make_request(string url) {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("can't connect api");
            Debug.LogError(request.error);
        }
        else {
            var response = JsonConvert.DeserializeObject<elevation_json_response>(request.downloadHandler.text);
            foreach (Results i in response.results)
            {
                elevations.Add(i.elevation);
            }
        }
    }
}
