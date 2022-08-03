using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class EvaluateImages : MonoBehaviour
{
    [SerializeField]
    private Texture2D image_a;
    [SerializeField]
    private Texture2D image_b;
    [SerializeField]
    private bool evaluate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (evaluate)
        {
            evaluate = false;
            exportTexture("diff", diffImages());
        }
    }

    Texture2D diffImages()
    {
        Texture2D texture2D = new Texture2D(image_a.width, image_a.height, TextureFormat.RGBA32, false);
        float max_diff = float.MinValue;
        float min_diff = float.MaxValue;
        for (int i = 0; i < image_a.height; i++)
        {
            for (int j = 0; j < image_a.width; j++)
            {
                float diff = image_a.GetPixel(i, j).r - image_b.GetPixel(i, j).r;
                max_diff = Mathf.Max(diff, max_diff);
                min_diff = Mathf.Min(diff, min_diff);
            }
        }
        for (int i = 0; i < image_a.height; i++)
        {
            for (int j = 0; j < image_a.width; j++)
            {
                float diff = image_a.GetPixel(i, j).r - image_b.GetPixel(i, j).r;
                if (diff >= 0)
                    texture2D.SetPixel(i, j, new Color(diff / max_diff, 0, 0));
                else
                    texture2D.SetPixel(i, j, new Color(0, 0, diff / min_diff));
            }
        }
        return texture2D;
    }

    void exportTexture(string tag, Texture2D texture2D)
    {
        //then Save To Disk as PNG
        byte[] bytes = texture2D.EncodeToPNG();
        var dirPath = Application.dataPath + "/Resources/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string file_path = dirPath + tag + ".png";
        File.WriteAllBytes(file_path, bytes);
        Debug.Log($"Store {file_path} successfully!");
    }
}
