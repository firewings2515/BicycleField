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
        public float speed = 5;
        float distanceTravelled;

        public GameObject speed_display;
        public GameObject slope_display;

        private float cam_y_offset = 5f;
        private bool is_started = false;

        private float add_speed = 0.05f;
        private float last_speed = 0.0f;

        public GameObject[] slope_displays;
        private float slope_diff = 0;

        public GameObject end_text;
        public GameObject compass;
        private List<float> directions = new List<float>() { };

        void Update()
        {
            if (TerrainGenerator.is_initial)
            {
                Vector3 tempGPA = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                tempGPA.y = TerrainGenerator.getHeightWithBais(tempGPA.x, tempGPA.z);

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

                    transform.position = tempGPA;
                    transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                    is_started = true;
                }

                Vector3 here = tempGPA;
                Vector3 there = pathCreator.path.GetPointAtDistance(distanceTravelled + 1f, endOfPathInstruction);
                there.y = TerrainGenerator.getHeightWithBais(there.x, there.z);
                float slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
                Info.slope = slope;
                //slope_display.GetComponent<Text>().text = "Slope: " + slope.ToString();
                if (speed < 0) speed = 0;

                last_speed = Mathf.Lerp(last_speed, speed * (1 - slope), 0.1f);
                if (last_speed < 1f && speed > 0) last_speed = 1f;
                speed_display.GetComponent<Text>().text = "Speed: " + ((int)(last_speed * 3.6f)).ToString("0") + " km/hr (base: " + (int)(speed * 3.6f) + ")";
                
                if (pathCreator != null)
                {
                    distanceTravelled += last_speed * Time.deltaTime;
                    Vector3 new_pos = Vector3.Lerp(transform.position, tempGPA + Vector3.up * cam_y_offset, 0.1f);
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

                    transform.position = new_pos;
                    transform.rotation = Quaternion.Lerp(transform.rotation, pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction), 0.02f);
                }

                manageSlopeDisplay();
                endDistanceDisplay();
            }
        }

        private void manageSlopeDisplay()
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

        private void endDistanceDisplay()
        {
            Vector3 end = Info.end_point;
            Vector3 here = transform.position;
            end_text.GetComponent<Text>().text = "終點: " + (int)((end - here).magnitude) + "公尺";
        }

        private float calculateSlopeHeightDiff(int id, int anchor)
        {
            Vector3 there = pathCreator.path.GetPointAtDistance(distanceTravelled + (id - anchor) * Info.mapview_height, endOfPathInstruction);
            float result = (TerrainGenerator.getHeightWithBais(transform.position.x, transform.position.z) - TerrainGenerator.getHeightWithBais(there.x, there.z)) * 5;
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
            speed -= slope_diff * 0.1f * Time.deltaTime;
            if (set_speed == 0) speed += add_speed * Time.deltaTime;
            else speed += set_speed * Time.deltaTime;
            slopeCelerate();
        }

        public void decelerate(float set_speed = 0)
        {
            speed -= slope_diff * 0.1f * Time.deltaTime;
            if (set_speed == 0) speed -= add_speed * Time.deltaTime;
            else speed -= set_speed * Time.deltaTime;
            slopeCelerate();
        }

        private void slopeCelerate()
        {
            speed -= slope_diff * 0.1f * Time.deltaTime;
            if (speed < 0) speed = 0;
            if (speed > Info.CHECKPOINT_SIZE) speed = Info.CHECKPOINT_SIZE;
        }
    }
}