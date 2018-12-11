using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CustomVector2
{
    public FixedPointF x;
    public FixedPointF y;

    public CustomVector2(FixedPointF tX, FixedPointF tY)
    {
        x = tX;
        y = tY;
    }

    public CustomVector2(int tX, int tY)
    {
        x = new FixedPointF(tX);
        y = new FixedPointF(tY);
    }

    public CustomVector2(int tX, FixedPointF tY)
    {
        x = new FixedPointF(tX);
        y = tY;
    }

    public CustomVector2(FixedPointF tX, int tY)
    {
        x = tX;
        y = new FixedPointF(tY);
    }

    public static FixedPointF Dot(CustomVector2 line1, CustomVector2 line2)
    {
        return line1.x * line2.x + line1.y * line2.y;
    }
}
