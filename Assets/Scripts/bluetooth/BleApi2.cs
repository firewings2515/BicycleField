using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;


namespace ble_api2
{
    public class pcBleApi
    {
        // dll calls
        public enum ScanStatus { PROCESSING, AVAILABLE, FINISHED };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DeviceUpdate
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string id;
            [MarshalAs(UnmanagedType.I1)]
            public bool isConnectable;
            [MarshalAs(UnmanagedType.I1)]
            public bool isConnectableUpdated;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string name;
            [MarshalAs(UnmanagedType.I1)]
            public bool nameUpdated;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string uuid;
        }

        [DllImport("BleWinrtDll2.dll", EntryPoint = "StartDeviceScan")]
        public static extern void StartDeviceScan();

        [DllImport("BleWinrtDll2.dll", EntryPoint = "PollDevice")]
        public static extern ScanStatus PollDevice(ref DeviceUpdate device, bool block);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "StopDeviceScan")]
        public static extern void StopDeviceScan();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Service
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string uuid;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string deviceId;
        };

        [DllImport("BleWinrtDll2.dll", EntryPoint = "ScanServices", CharSet = CharSet.Unicode)]
        public static extern void ScanServices(string deviceId);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "PollService")]
        public static extern ScanStatus PollService(out Service service, bool block);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Characteristic
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string uuid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
            public string userDescription;
        };

        [DllImport("BleWinrtDll2.dll", EntryPoint = "ScanCharacteristics", CharSet = CharSet.Unicode)]
        public static extern void ScanCharacteristics(string deviceId, string serviceId);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "PollCharacteristic")]
        public static extern ScanStatus PollCharacteristic(out Characteristic characteristic, bool block);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "SubscribeCharacteristic", CharSet = CharSet.Unicode)]
        public static extern bool SubscribeCharacteristic(string deviceId, string serviceId, string characteristicId, bool block);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BLEData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] buf;
            [MarshalAs(UnmanagedType.I2)]
            public short size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string deviceId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string serviceUuid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string characteristicUuid;
        };

        [DllImport("BleWinrtDll2.dll", EntryPoint = "PollData")]
        public static extern bool PollData(out BLEData data, bool block);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "SendData")]
        public static extern bool SendData(in BLEData data, bool block, bool isWithResponse);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "Quit")]
        public static extern void Quit();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ErrorMessage
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string msg;
        };

        [DllImport("BleWinrtDll2.dll", EntryPoint = "GetError")]
        public static extern void GetError(out ErrorMessage buf);




        ////////////////new things////////////////////

        [DllImport("BleWinrtDll2.dll", EntryPoint = "Connect")]
        public static extern void Connect(string deviceId);

        [DllImport("BleWinrtDll2.dll", EntryPoint = "DisConnect")]
        public static extern void DisConnect(string deviceId);


        [DllImport("BleWinrtDll2.dll", EntryPoint = "PollReadData")]
        public static extern void PollReadData(out BLEData data, bool block);


        [DllImport("BleWinrtDll2.dll", EntryPoint = "StartAdvertisementScan")]
        public static extern void StartAdvertisementScan();

        [DllImport("BleWinrtDll2.dll", EntryPoint = "StopAdvertisementScan")]
        public static extern void StopAdvertisementScan();
    }
};