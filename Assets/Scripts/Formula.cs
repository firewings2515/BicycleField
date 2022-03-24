using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Formula
{
    // url: https://stackoverflow.com/questions/12132352/distance-from-a-point-to-a-line-segment
    static public void getLine(float x1, float y1, float x2, float y2, out float a, out float b, out float c)
    {
        // (x- p1X) / (p2X - p1X) = (y - p1Y) / (p2Y - p1Y) 
        a = y1 - y2; // Note: this was incorrectly "y2 - y1" in the original answer
        b = x2 - x1;
        c = x1 * y2 - x2 * y1;
    }

    static public double getPointToLineDist(float pointX, float pointY, float LineStartX, float LineStartY, float LineEndX, float LineEndY)
    {
        float a, b, c;
        getLine(LineStartX, LineStartY, LineEndX, LineEndY, out a, out b, out c);
        return Mathf.Abs(a * pointX + b * pointY + c) / Mathf.Sqrt(a * a + b * b);
    }
}