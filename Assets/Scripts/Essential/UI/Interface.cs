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

        if (Input.GetKeyDown(KeyCode.Alpha9)) slope_display.SetActive(!slope_display.activeSelf);

        if (Input.GetKeyDown(KeyCode.P)) play = !play;
        if (play)
        {
            Debug.Log("playing");
            if (Input.GetKeyDown(KeyCode.Space)) speedUp();
            else
            {
                speedDown();
            }
        }
    }

    public void speedUp()
    {
        cyclist.GetComponent<PathCreation.Examples.PathFollower>().accelerate(1.0f);
    }

    public void speedDown()
    {
        cyclist.GetComponent<PathCreation.Examples.PathFollower>().decelerate(0.1f);
    }

    public void changeSpeed()
    {
        cyclist.GetComponent<PathCreation.Examples.PathFollower>().speed = slider.value;
    }
}
