using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CustomMath
{
    // 遥感获得角度范围是-180到180
    public static FixedPointF GetCos(int tAngle)
    {
        // -180
        if (tAngle == -180)
        {
            return new FixedPointF(-1);
        }
        // -179和-89
        else if (tAngle < -90)
        {
            tAngle = -tAngle;
            tAngle = 180 - tAngle;
            FixedPointF cos = Cos[tAngle];
            cos = -cos;
            return cos;
        }
        // -90到-1
        else if (tAngle < 0)
        {
            tAngle = -tAngle;
            return Cos[tAngle];
        }
        // 0到89
        else if (tAngle < 90)
        {
            return Cos[tAngle];
        }
        // 90到179
        else if (tAngle < 180)
        {
            tAngle = 180 - tAngle;
            FixedPointF cos = Cos[tAngle];
            cos = -cos;
            return cos;
        }
        // 180
        else
        {
            return new FixedPointF(-1);
        }
    }

    // 遥感获得角度范围是-180到180
    public static FixedPointF GetSin(int tAngle)
    {
        // -180
        if (tAngle == -180)
        {
            return new FixedPointF(0);
        }
        // -179和-89
        else if (tAngle < -90)
        {
            tAngle = -tAngle;
            tAngle = 180 - tAngle;
            FixedPointF sin = Sin[tAngle];
            sin = -sin;
            return sin;
        }
        // -90到-1
        else if (tAngle < 0)
        {
            tAngle = -tAngle;
            FixedPointF sin = Sin[tAngle];
            sin = -sin;
            return sin;
        }
        // 0到89
        else if (tAngle < 90)
        {
            return Sin[tAngle];
        }
        // 90到179
        else if (tAngle < 180)
        {
            tAngle = 180 - tAngle;
            return Sin[tAngle];
        }
        // 180
        else
        {
            return new FixedPointF(0);
        }
    }
}
