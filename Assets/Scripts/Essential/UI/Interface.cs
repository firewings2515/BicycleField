using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interface : MonoBehaviour
{
    public GameObject fps_display;
    private double recorded_time;
    private double worst_time;
    private double second_counter = 1.0;

    public GameObject cyclist;
    public Slider slider;

    public GameObject slope_display;
    public GameObject end_display;
    public GameObject minimap_display;

    public FTMS_show ftms_show;

    private bool play = false;
    // Start is called before the first frame update
    void Start()
    {
        recorded_time = Time.realtimeSinceStartupAsDouble;
    }

    // Update is called once per frame
    void Update()
    {
        second_counter -= Time.realtimeSinceStartupAsDouble - recorded_time;
        //FPS
        if (second_counter <= 0.0 || (int)(1.0/worst_time) >= (int)(1.0/(Time.realtimeSinceStartupAsDouble - recorded_time)))
        {
            worst_time = Time.realtimeSinceStartupAsDouble - recorded_time;
            second_counter = 1.0;
        }
        fps_display.GetComponent<Text>().text = "FPS: " + (int)(1.0/worst_time);
        recorded_time = Time.realtimeSinceStartupAsDouble;

        if (Input.GetKeyDown(KeyCode.Alpha1)) toggleSlope();
        if (Input.GetKeyDown(KeyCode.Alpha2)) toggleEnd();
        if (Input.GetKeyDown(KeyCode.Alpha3)) toggleMinimap();

        if (Input.GetKeyDown(KeyCode.P)) play = !play;
        if (play)
        {
            if (Input.GetKeyDown(KeyCode.Space)) speedUp();
            else speedDown();
        }
        if (ftms_show.connect) {
            if (ftms_show.connector.has_speed)
            {
                cyclist.GetComponent<PathCreation.Examples.PathFollower>().speed = ftms_show.connector.speed;
            }
        }
    }

    public void speedUp()
    {
        cyclist.GetComponent<PathCreation.Examples.PathFollower>().accelerate(20.0f);
    }

    public void speedDown()
    {
        cyclist.GetComponent<PathCreation.Examples.PathFollower>().decelerate(1f);
    }

    public void changeSpeed()
    {
        cyclist.GetComponent<PathCreation.Examples.PathFollower>().speed = slider.value;
    }

    public void toggleSlope()
    {
        slope_display.SetActive(!slope_display.activeSelf);
    }

    public void toggleEnd()
    {
        end_display.SetActive(!end_display.activeSelf);
    }

    public void toggleMinimap()
    {
        minimap_display.SetActive(!minimap_display.activeSelf);
    }
}
