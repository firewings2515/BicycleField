using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PublicOutputInfo
{
    public static Vector3 origin_pos;
    static public int piece_num = 64;                                                                   // The number of piece in a patch    ver2 = 8 ver3 = 64
    public static float piece_length = 2.0f;
    public static float patch_length = piece_num * piece_length;                                        // 128 is well
    public static int piece_num_in_chunk = 192;
    public static float editor_chunk_piece_length = 16.0f;
    public static float editor_chunk_patch_length = piece_num_in_chunk * editor_chunk_piece_length;
    public static Vector2 boundary_min;                                                                 // for terrain to getDEMHeight
    public static int gaussian_m = 16;
    public static int pregaussian_tex_size = Mathf.FloorToInt(patch_length + 0.5f) + 2 * gaussian_m;    // 128 + 32 = 160
    public static int tex_size = 512;                                                                   
}