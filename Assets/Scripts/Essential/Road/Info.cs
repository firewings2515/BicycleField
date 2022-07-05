using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Info
{
    public const int MAX_LOADED_SEGMENT = 20;
    public const int PRELOAD_SEGMENT = 10;
    public const int CHECKPOINT_SIZE = 40;
    static public Vector3 end_point = new Vector3( 0, 0, 0 );
    static public float mapview_height = 20.0f;
    static public float slope = 0f;
    static public string getOutputSlope()
    {
        string value = "110000";
        if (slope > 0) value += "0A03";
        else value += "0000";
        /*
        if (slope > 1) slope = 1; //cap
        int int_value = (int)(slope * 1000);
        int mso = int_value / 256;
        int lso = int_value % 256;

        value += Add16(lso / 16);
        value += Add16(lso % 16);
        value += Add16(mso / 16);
        value += Add16(mso % 16);
        */
        value += "0000";
        return value;
    }

    static private string Add16(int num)
    {
        if (num == 1) return "1";
        if (num == 2) return "2";
        if (num == 3) return "3";
        if (num == 4) return "4";
        if (num == 5) return "5";
        if (num == 6) return "6";
        if (num == 7) return "7";
        if (num == 8) return "8";
        if (num == 9) return "9";
        if (num == 10) return "A";
        if (num == 11) return "B";
        if (num == 12) return "C";
        if (num == 13) return "D";
        if (num == 14) return "E";
        if (num == 15) return "F";
        return "0";
    }
}
