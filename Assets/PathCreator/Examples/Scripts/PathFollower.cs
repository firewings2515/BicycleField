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

        private float cam_y_offset = 1.0f;

        void Start() {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                transform.position = new Vector3(transform.position.x, TerrainGenerator.getIDWHeightWithBais(transform.position.x, transform.position.z) + cam_y_offset, transform.position.z);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }
        }

        void Update()
        {
            Vector3 tempGPA = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            tempGPA.y = TerrainGenerator.getIDWHeightWithBais(tempGPA.x, tempGPA.z);
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
            if (speed < 0) speed = 0;
            speed_display.GetComponent<Text>().text = "Speed: " + speed.ToString("0")  + " (O to speed up, P to speed down, L to hide)";
            if (pathCreator != null && run)
            {
                distanceTravelled += speed * Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, tempGPA + Vector3.up * cam_y_offset, 0.1f);
                transform.rotation = Quaternion.Lerp(transform.rotation, pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction), 0.02f);
            }
            Vector3 here = tempGPA;
            Vector3 there = pathCreator.path.GetPointAtDistance(distanceTravelled + 0.01f, endOfPathInstruction);
            there.y = TerrainGenerator.getIDWHeightWithBais(there.x, there.z);
            float slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
            slope_display.GetComponent<Text>().text = "Slope: " + slope.ToString();
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