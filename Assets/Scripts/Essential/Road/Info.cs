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
}
