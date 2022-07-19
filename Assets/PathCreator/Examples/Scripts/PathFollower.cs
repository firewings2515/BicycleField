using System.Collections.Generic;
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
        float distanceTravelled;

        public GameObject speed_display;
        public GameObject slope_display;

        private float cam_y_offset = 5f;

        private float add_speed = 0.05f;

        public GameObject[] slope_displays;
        private float slope_diff = 0;

        public GameObject end_text;
        public GameObject compass;
        private List<float> directions = new List<float>() { };

        private void Start()
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;

            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        }

        void Update()
        {
            Vector3 here = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            Vector3 there = pathCreator.path.GetPointAtDistance(distanceTravelled + 1f, endOfPathInstruction);
            Info.slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
            
            if (Info.speed < 0) Info.speed = 0;
            speed_display.GetComponent<Text>().text = "Speed: " + ((int)(Info.speed * 3.6f)).ToString("0") + " km/hr";
               
            distanceTravelled += Info.speed * Time.deltaTime;
            Vector3 new_pos = Vector3.Lerp(transform.position, here + Vector3.up * cam_y_offset, 0.1f);

            transform.position = new_pos;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(((there - here).magnitude < 0.000001f) ? Vector3.forward : there - here, pathCreator.path.up) * Quaternion.AngleAxis(-90, Vector3.forward), 0.2f);//Quaternion.Lerp(transform.rotation, pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction), 0.02f);

            slopeDisplay();
            compassDisplay(new_pos);
            distanceDisplay();
        }

        private void compassDisplay(Vector3 new_pos)
        {
            Vector3 direction = new_pos - transform.position;
            direction.y = 0;
            float direction_angle = Vector3.Angle(direction, new Vector3(0, 0, 1));
            if (Vector3.Angle(direction, new Vector3(1, 0, 0)) > 90) direction_angle = -direction_angle + 360;
            if (Mathf.Abs(direction_angle - compass.transform.eulerAngles.z) < 30 || compass.transform.eulerAngles.z == 0) directions.Add(direction_angle);

            int segment = 5;
            if (directions.Count == segment || compass.transform.eulerAngles.z == 0)
            {
                float avg_angle = 0;
                for (int id = 0; id < directions.Count; id++)
                {
                    avg_angle += directions[id];
                }
                compass.transform.eulerAngles = new Vector3(compass.transform.eulerAngles.x, compass.transform.eulerAngles.y, avg_angle / directions.Count);
                directions.Clear();
            }
        }

        private void slopeDisplay()
        {
            int anchor = 4;
            for (int id = 0; id < slope_displays.Length; id++)
            {
                if (id == anchor) continue;
                Vector3 new_pos = slope_displays[id].transform.localPosition;
                new_pos.y = slope_displays[anchor].transform.localPosition.y + -calculateSlopeHeightDiff(id, anchor);
                slope_displays[id].transform.localPosition = new_pos;
            }

            float slope_buffer = 5f;
            slope_diff = slope_displays[anchor + 2].transform.localPosition.y - slope_displays[anchor - 2].transform.localPosition.y;
            if (slope_diff > slope_buffer) slope_display.GetComponent<Text>().text = "上坡";
            else if (slope_diff < -slope_buffer) slope_display.GetComponent<Text>().text = "下坡";
            else slope_display.GetComponent<Text>().text = "平坡";
        }

        private void distanceDisplay()
        {
            end_text.GetComponent<Text>().text = "終點: " + (int)Info.total_length + "公尺";
        }

        private float calculateSlopeHeightDiff(int id, int anchor)
        {
            Vector3 there = pathCreator.path.GetPointAtDistance(distanceTravelled + (id - anchor) * Info.mapview_height, endOfPathInstruction);
            //float result = (TerrainGenerator.getHeightWithBais(transform.position.x, transform.position.z) - TerrainGenerator.getHeightWithBais(there.x, there.z)) * 5;
            float result = (transform.position.y - there.y) * 5;
            if (result > 100) result = 100;
            if (result < -100) result = -100;
            return result;
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

        public void accelerate(float set_speed = 0)
        {
            Info.speed -= slope_diff * 0.1f * Time.deltaTime;
            if (set_speed == 0) Info.speed += add_speed * Time.deltaTime;
            else Info.speed += set_speed * Time.deltaTime;
            slopeCelerate();
        }

        public void decelerate(float set_speed = 0)
        {
            Info.speed -= slope_diff * 0.1f * Time.deltaTime;
            if (set_speed == 0) Info.speed -= add_speed * Time.deltaTime;
            else Info.speed -= set_speed * Time.deltaTime;
            slopeCelerate();
        }

        private void slopeCelerate()
        {
            Info.speed -= slope_diff * 0.1f * Time.deltaTime;
            if (Info.speed < 0) Info.speed = 0;
            if (Info.speed > Info.CHECKPOINT_SIZE) Info.speed = Info.CHECKPOINT_SIZE;
        }
    }
}