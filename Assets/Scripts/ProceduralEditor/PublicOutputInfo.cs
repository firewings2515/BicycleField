using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PublicOutputInfo
{
    public static Vector3 origin_pos;
    public static float patch_length = 128.0f;   // 128.0f
    public static float piece_length = 2.0f;   // 128.0f 16.0f
    public static int piece_num_in_chunk = 192;
    public static float editor_chunk_piece_length = 16.0f;   // 128.0f
    public static float editor_chunk_patch_length = piece_num_in_chunk * editor_chunk_piece_length;   // 128.0f
    public static Vector2 boundary_min;         // for terrain to getDEMHeight
    public static int tex_size = 512;
}