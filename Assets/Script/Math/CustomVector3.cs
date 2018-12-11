using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CustomVector3
{
    public FixedPointF x;
    public FixedPointF y;
    public FixedPointF z;

    public CustomVector3(FixedPointF tX, FixedPointF tY, FixedPointF tZ)
    {
        x = tX;
        y = tY;
        z = tZ;
    }

    public CustomVector3(int tX, int tY, int tZ)
    {
        x = new FixedPointF(tX);
        y = new FixedPointF(tY);
        z = new FixedPointF(tZ);
    }

    public static CustomVector3 operator +(CustomVector3 a, CustomVector3 b)
    {
        a.x += b.x;
        a.y += b.y;
        a.z += b.z;
        return a;
    }
    public static CustomVector3 operator -(CustomVector3 a, CustomVector3 b)
    {
        a.x -= b.x;
        a.y -= b.y;
        a.z -= b.z;
        return a;
    }
    public static CustomVector3 operator -(CustomVector3 a)
    {
        a.x = -a.x;
        a.y = -a.y;
        a.z = -a.z;
        return a;
    }
    public static bool operator ==(CustomVector3 t1, CustomVector3 t2)
    {
        return t1.x == t2.x && t1.y == t2.y && t1.z == t2.z;
    }
    public static bool operator !=(CustomVector3 t1, CustomVector3 t2)
    {
        return t1.x != t2.x || t1.y != t2.y || t1.z != t2.z;
    }

    public static CustomVector3 GetCustomVector3ByVector3(Vector3 a)
    {
        CustomVector3 mCustomPos;
        mCustomPos.x = new FixedPointF(a.x);
        mCustomPos.y = new FixedPointF(a.y);
        mCustomPos.z = new FixedPointF(a.z);
        return mCustomPos;
    }

    public Vector3 value
    {
        get
        {
            return new Vector3(x.value, y.value, z.value);
        }
    }

    public override string ToString()
    {
        return string.Format("({0},{1},{2})", x, y, z);
    }
}
