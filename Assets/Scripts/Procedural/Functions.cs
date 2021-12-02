using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Functions
{
    public static Vector3 StrToVec3(string str)
    {
        string[] str_arr = str.Split(' ');

        return new Vector3(
            float.Parse(str_arr[0]),
            float.Parse(str_arr[1]),
            float.Parse(str_arr[2]));
    }
}
