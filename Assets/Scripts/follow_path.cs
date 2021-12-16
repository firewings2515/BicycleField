using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;

// Moves along a path at constant speed.
// Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
public class follow_path : MonoBehaviour
{
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 30;
    public bool reverse = false;
    float distanceTravelled;
    public bool pause = true;
    public OSMReaderManager orm;

    public float total_time = 30.0f;
    private float travel_time = 0.0f;
    private List<float> fit_speeds;
    void Start()
    {
        fit_speeds = new List<float>();
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
        }
        TextAsset fit_data = Resources.Load<TextAsset>("fit");
        string[] lines = fit_data.text.Split(new char[] { '\n' });
        Debug.Log(lines.Length);
        for (int i = 1; i < lines.Length - 1; i++) {
            string[] row = lines[i].Split(new char[] { ',' });
            if (row.Length > 22)
            {
                if (row[22].Length >= 2) { 
                    string remove_quote = row[22].Substring(1, row[22].Length - 2);
                    float reuslt = 0;
                    float.TryParse(remove_quote, out reuslt);
                    fit_speeds.Add(reuslt);
                    Debug.Log("csv" + reuslt.ToString());
                }
            }
        }
        Debug.Log("fit_speeds len:" + fit_speeds.Count.ToString());
    }

    void Update()
    {

        if (pathCreator != null && !pause)
        {
            if (!reverse)
            {
                distanceTravelled += speed * Time.deltaTime;
            }
            else {
                distanceTravelled -= speed * Time.deltaTime;
            }
            
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            //transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        }
        //Bounds a;
        //pathCreator.path.bounds
        //pathCreator.path.ge
    }

    void FixedUpdate()
    {
        if (!pause)
        {
            travel_time += Time.deltaTime;
            if (travel_time > total_time)
            {
                travel_time -= total_time;
            }
            int integer = (int)(travel_time / total_time * fit_speeds.Count);
            float m_decimal = (travel_time / total_time * fit_speeds.Count) - integer;
            //Debug.Log("integer:" + integer.ToString());
            //Debug.Log("m_decimal:" + m_decimal.ToString());
            //Debug.Log("travel_time:" + travel_time.ToString());
            speed = fit_speeds[integer] + m_decimal * (fit_speeds[(integer + 1) % fit_speeds.Count] - fit_speeds[integer]);
            //Debug.Log("speed:" + speed.ToString());
        }
    }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    public void path_change(int path_index) {
        GameObject road_obj = GameObject.Find("road" + path_index.ToString());
        pathCreator = orm.all_pc[path_index];
    }
    public void reverse_change(bool rev) {
        reverse = rev;
    }
    public void speed_change(float spd)
    {
        speed = spd;
    }
    public void to_start()
    {
        distanceTravelled = 0.0f;
    }
    public void to_end()
    {
        distanceTravelled = 1.0f;
    }
    public void to_keep()
    {
        pause = false;
    }
    public void to_pause()
    {
        pause = !pause;
    }
}
