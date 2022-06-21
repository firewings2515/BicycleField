using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorBike_FTMS : MonoBehaviour
{
    public bool connect_device = true;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();
    string selectedDeviceId = "";
    string selectedServiceId = "";
    string selectedCharacteristicId = "";

    bool isSubscribed = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!connect_device) return;
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
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (selectedDeviceId == "") {
                    Debug.LogError("device APXPRO 46080 not found!");
                    return;
                }
            }
        } while (status == BleApi.ScanStatus.AVAILABLE);
        BleApi.StopDeviceScan();

        Debug.Log("connecting service...");
        BleApi.ScanServices(selectedDeviceId);
        BleApi.Service service_res = new BleApi.Service();
        do
        {
            status = BleApi.PollService(out service_res, false);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                if (service_res.uuid == "{00001826-0000-1000-8000-00805f9b34fb}") {
                    selectedServiceId = service_res.uuid;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (selectedServiceId == "")
                {
                    Debug.LogError("service {00001826-0000-1000-8000-00805f9b34fb} not found!");
                    return;
                }
            }
        } while (status == BleApi.ScanStatus.AVAILABLE);


        Debug.Log("connecting characteristic...");
        BleApi.ScanCharacteristics(selectedDeviceId, selectedServiceId);
        BleApi.Characteristic characteristics_res = new BleApi.Characteristic();

        do
        {
            status = BleApi.PollCharacteristic(out characteristics_res, false);
            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                if (characteristics_res.uuid == "{00002ad2-0000-1000-8000-00805f9b34fb}") {
                    selectedCharacteristicId = characteristics_res.uuid;
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                if (selectedCharacteristicId == "")
                {
                    Debug.LogError("characteristic {00002ad2-0000-1000-8000-00805f9b34fb} not found!");
                    return;
                }
            }
        } while (status == BleApi.ScanStatus.AVAILABLE);

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
        if (isSubscribed) {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                //subcribeText.text = BitConverter.ToString(res.buf, 0, res.size) + "\n";
                int index = 0;
                int flags = BitConverter.ToUInt16(res.buf, index);
                index += 2;
                if ((flags & 0) == 0)
                {
                    float value = (float)BitConverter.ToUInt16(res.buf, index);
                    float f = (value * 1.0f) / 100.0f;
                    Debug.Log("Speed: " + f + "\n");
                    index += 2;
                }
                if ((flags & 2) > 0)
                {
                    //??
                    float f = BitConverter.ToUInt16(res.buf, index);
                    Debug.Log("Average Speed: " + f + "\n");
                    index += 2;
                }
                if ((flags & 4) > 0)
                {
                    float f = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;
                    Debug.Log("RPM: (rev/min): " + f + "\n");
                    index += 2;
                }
                if ((flags & 8) > 0)
                {
                    float f = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;
                    Debug.Log("Average RPM: " + f + "\n");
                    index += 2;
                }
                if ((flags & 16) > 0)
                {
                    float f = BitConverter.ToUInt16(res.buf, index); // ?????s
                    Debug.Log("Distance (meter): " + f + "\n");
                    index += 2;
                }
                if ((flags & 32) > 0)
                {
                    float f = BitConverter.ToInt16(res.buf, index);
                    Debug.Log("Resistance: " + f + "\n");
                    index += 2;
                }
                if ((flags & 64) > 0)
                {
                    float f = BitConverter.ToInt16(res.buf, index);
                    Debug.Log("Power (Watt): " + f + "\n");
                    index += 2;
                }
                if ((flags & 128) > 0)
                {
                    float f = BitConverter.ToInt16(res.buf, index);
                    Debug.Log("AveragePower: " + f + "\n");
                    index += 2;
                }
                if ((flags & 256) > 0)
                {
                    float f = BitConverter.ToUInt16(res.buf, index);
                    Debug.Log("ExpendedEnergy: " + f + "\n");
                    index += 2;
                }
                // subcribeText.text = Encoding.ASCII.GetString(res.buf, 0, res.size);
            }
        }
    }
}
