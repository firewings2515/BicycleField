#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ScreenshotGrabber
{
    [MenuItem("Screenshot/Grab")]
    public static void Grab()
    {
        string file_path = Application.dataPath + "/Resources/" + "Screenshot.png";
        ScreenCapture.CaptureScreenshot(file_path, 1);
        Debug.Log($"Screenshot store to {Application.dataPath + "/Resources/" + "Screenshot.png"}");
    }
}
#endif