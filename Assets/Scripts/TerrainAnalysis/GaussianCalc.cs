using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GaussianCalc : MonoBehaviour
{
    public bool get_gaussian_coeffs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (get_gaussian_coeffs)
        {
            get_gaussian_coeffs = false;
            string ans = "";
            for (int i = PublicOutputInfo.gaussian_m; i > 0; i--)
            {
                ans += getGaussian(i, 10).ToString() + ",\n";
            }
            for (int i = 0; i <= PublicOutputInfo.gaussian_m; i++)
            {
                ans += getGaussian(i, 10).ToString() + ",\n";
            }
            Debug.Log(ans);
        }
    }

    float getGaussian(float x, float sigma)
    {
        return Mathf.Exp(-(x * x) / (2 * sigma * sigma)) / (Mathf.Sqrt(2 * Mathf.PI) * sigma);
    }
}