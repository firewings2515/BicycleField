using UnityEngine;
using UnityEngine.UI;
namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        float distanceTravelled;
        private bool run = false;

        public GameObject speed_display;
        public GameObject slope_display;

        private float cam_y_offset = 2.5f;
        private bool is_started = false;

        private float last_speed = 0.0f;

        void Update()
        {
            if (TerrainGenerator.is_initial)
            {
                if (!is_started)
                {
                    if (pathCreator != null)
                    {
                        // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                        pathCreator.pathUpdated += OnPathChanged;
                        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                        transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y,TerrainGenerator.getHeightWithBais(transform.position.x, transform.position.z), 0.1f) + cam_y_offset, transform.position.z);
                        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                    }
                    is_started = true;
                }

                Vector3 tempGPA = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                tempGPA.y = TerrainGenerator.getHeightWithBais(tempGPA.x, tempGPA.z);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    run = !run;
                    transform.position = tempGPA;
                    transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                }
                float add_speed = 0.05f;
                if (Input.GetKey(KeyCode.O)) if (speed < Info.CHECKPOINT_SIZE - add_speed) speed += add_speed;
                if (Input.GetKey(KeyCode.P)) if (speed > 0) speed -= add_speed;
                if (Input.GetKeyDown(KeyCode.L))
                {
                    speed_display.SetActive(!speed_display.activeSelf);
                    slope_display.SetActive(!slope_display.activeSelf);
                }


                Vector3 here = tempGPA;
                Vector3 there = pathCreator.path.GetPointAtDistance(distanceTravelled + 1f, endOfPathInstruction);
                there.y = TerrainGenerator.getHeightWithBais(there.x, there.z);
                float slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
                slope_display.GetComponent<Text>().text = "Slope: " + slope.ToString();
                if (speed < 0) speed = 0;

                last_speed = Mathf.Lerp(last_speed, speed * (1 - slope), 0.1f);
                if (last_speed < 1f && speed > 0) last_speed = 1f;
                speed_display.GetComponent<Text>().text = "Speed: " + ((int)(last_speed * 3.6f)).ToString("0") + " km/hr (base: " + (int)(speed * 3.6f) + ")\n(P/O (de/ac)celerate, L to hide)";
                
                if (pathCreator != null && run)
                {
                    distanceTravelled += last_speed * Time.deltaTime;
                    transform.position = Vector3.Lerp(transform.position, tempGPA + Vector3.up * cam_y_offset, 0.1f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction), 0.02f);
                }
            }
        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        public float nearestDistance()
        {
            return pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        public void setDistance(float value)
        {
            distanceTravelled = value;
        }
    }
}