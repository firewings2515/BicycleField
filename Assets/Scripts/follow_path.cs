using UnityEngine;
using PathCreation;
using PathCreation.Examples;

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

    void Start()
    {
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
        }
        else {
            OSMReaderManager orm = GameObject.Find("OSMReader").GetComponent<OSMReaderManager>();
            //pathCreator = orm.all_pc[0];
            //path_change(0);
        }
    }

    void Update()
    {
        if (pathCreator == null) {
            Debug.Log("no path creator");
            OSMReaderManager orm = GameObject.Find("OSMReader").GetComponent<OSMReaderManager>();
            //pathCreator = orm.all_pc[0];
        }
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

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    public void path_change(int path_index) {
        path_index--;
        GameObject road_obj = GameObject.Find("road" + path_index.ToString());
        pathCreator = road_obj.GetComponent<PathCreator>();
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
