using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorBike_FTMS : MonoBehaviour
{
    public bool want_connect = true;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();
    string selectedDeviceId = "";
    string selectedServiceId = "";
    string selectedCharacteristicId = "";

    bool isSubscribed = false;

    public static string FTMS_output;
    public static float speed;              public static bool has_speed = false;
    public static float average_speed;      public static bool has_average_speed = false;
    public static float rpm;                public static bool has_rpm = false;
    public static float average_rpm;        public static bool has_average_rpm = false;
    public static float distance;           public static bool has_distance = false;
    public static float resistance;         public static bool has_resistance = false;
    public static float power;              public static bool has_power = false;
    public static float average_power;      public static bool has_average_power = false;
    public static float expended_energy;    public static bool has_expended_energy = false;
    void Start()
    {
        if (want_connect)
        {
            StartCoroutine(connect());
        }
    }

    // Start is called before the first frame update
    IEnumerator connect()
    {
        if (!want_connect) yield break;

        yield return StartCoroutine(connect_device());
        if (selectedDeviceId.Length == 0) yield break;

        yield return StartCoroutine(connect_service());
        if (selectedServiceId.Length == 0) yield break;

        yield return StartCoroutine(connect_characteristic());
        if (selectedCharacteristicId.Length == 0) yield break;


        subscribe();
    }

    IEnumerator connect_device() {
        Debug.Log("connecting device...");
        BleApi.StartDeviceScan();
        BleApi.ScanStatus status;


        BleApi.DeviceUpdate device_res = new BleApi.DeviceUpdate();
        do
        {

            status = BleApi.PollDevice(ref device_res, false);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                if (!devices.ContainsKey(device_res.id))
                    devices[device_res.id] = new Dictionary<string, string>() {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                if (device_res.nameUpdated)
                    devices[device_res.id]["name"] = device_res.name;
                if (device_res.isConnectableUpdated)
                    devices[device_res.id]["isConnectable"] = device_res.isConnectable.ToString();
                // consider only devices which have a name and which are connectable
                if (devices[device_res.id]["name"] == "APXPRO 46080" && devices[device_res.id]["isConnectable"] == "True")
                {
                    selectedDeviceId = device_res.id;
                    break;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {

                if (selectedDeviceId.Length == 0)
                {
                    Debug.LogError("device APXPRO 46080 not found!");
                }
            }
            yield return 0;
        } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
    }

    IEnumerator connect_service()
    {
        Debug.Log("connecting service...");
        BleApi.ScanServices(selectedDeviceId);
        BleApi.ScanStatus status;
        BleApi.Service service_res = new BleApi.Service();
        do
        {
            status = BleApi.PollService(out service_res, false);
            Debug.Log(service_res.uuid);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                Debug.Log(service_res.uuid);
                if (service_res.uuid == "{00001826-0000-1000-8000-00805f9b34fb}")
                {
                    selectedServiceId = service_res.uuid;
                    break;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (selectedServiceId.Length == 0)
                {
                    Debug.LogError("service {00001826-0000-1000-8000-00805f9b34fb} not found!");
                }
            }
            yield return 0;
        } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
    }

    IEnumerator connect_characteristic()
    {
        Debug.Log("connecting characteristic...");
        BleApi.ScanCharacteristics(selectedDeviceId, selectedServiceId);
        BleApi.ScanStatus status;
        BleApi.Characteristic characteristics_res = new BleApi.Characteristic();

        do
        {
            status = BleApi.PollCharacteristic(out characteristics_res, false);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                if (characteristics_res.uuid == "{00002ad2-0000-1000-8000-00805f9b34fb}")
                {
                    selectedCharacteristicId = characteristics_res.uuid;
                    break;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (selectedCharacteristicId.Length == 0)
                {
                    Debug.LogError("characteristic {00002ad2-0000-1000-8000-00805f9b34fb} not found!");
                }
            }
            yield return 0;
        } while (status == BleApi.ScanStatus.AVAILABLE || status == BleApi.ScanStatus.PROCESSING);
    }

    void subscribe() {
        Debug.Log("Subscribe...");
        BleApi.SubscribeCharacteristic(selectedDeviceId, selectedServiceId, selectedCharacteristicId, false);
        isSubscribed = true;
    }


    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }


    // Update is called once per frame
    void Update()
    {


        if (isSubscribed)
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                {
                    has_speed = false;
                    has_average_speed = false;
                    has_rpm = false;
                    has_average_rpm = false;
                    has_distance = false;
                    has_resistance = false;
                    has_power = false;
                    has_average_power = false;
                    has_expended_energy = false;
                }

                FTMS_output = String.Empty;
                //subcribeText.text = BitConverter.ToString(res.buf, 0, res.size) + "\n";
                int index = 0;
                int flags = BitConverter.ToUInt16(res.buf, index);
                index += 2;
                if ((flags & 0) == 0)
                {
                    has_speed = true;
                    float value = (float)BitConverter.ToUInt16(res.buf, index);
                    speed = (value * 1.0f) / 100.0f;
                    FTMS_output += "Speed: " + speed + "\n";
                    index += 2;
                }
                if ((flags & 2) > 0)
                {
                    //??
                    has_average_speed = true;
                    average_speed = BitConverter.ToUInt16(res.buf, index);
                    FTMS_output += "Average Speed: " + average_speed + "\n";
                    index += 2;
                }
                if ((flags & 4) > 0)
                {
                    rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;
                    FTMS_output += "RPM: (rev/min): " + rpm + "\n";
                    index += 2;
                }
                if ((flags & 8) > 0)
                {
                    average_rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;
                    FTMS_output += "Average RPM: " + average_rpm + "\n";
                    index += 2;
                }
                if ((flags & 16) > 0)
                {
                    distance = BitConverter.ToUInt16(res.buf, index); // ?????s
                    FTMS_output += "Distance (meter): " + distance + "\n";
                    index += 2;
                }
                if ((flags & 32) > 0)
                {
                    resistance = BitConverter.ToInt16(res.buf, index);
                    FTMS_output += "Resistance: " + resistance + "\n";
                    index += 2;
                }
                if ((flags & 64) > 0)
                {
                    power = BitConverter.ToInt16(res.buf, index);
                    FTMS_output += "Power (Watt): " + power + "\n";
                    index += 2;
                }
                if ((flags & 128) > 0)
                {
                    average_power = BitConverter.ToInt16(res.buf, index);
                    FTMS_output += "AveragePower: " + average_power + "\n";
                    index += 2;
                }
                if ((flags & 256) > 0)
                {
                    expended_energy = BitConverter.ToUInt16(res.buf, index);
                    FTMS_output += "ExpendedEnergy: " + expended_energy + "\n";
                    index += 2;
                }
                // subcribeText.text = Encoding.ASCII.GetString(res.buf, 0, res.size);
            }
        }
    }
}
