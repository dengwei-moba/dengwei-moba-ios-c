using System;
using UnityEngine;

[Serializable]
public struct FixedPointF
{
    /// <summary>
    /// 定点数小数位数
    /// </summary>
    private static int m_float_num = 8;
    [SerializeField]
    private long m_value;
    public float value
    {
        get
        {
            return (float)m_value / (1 << m_float_num);
        }
    }

    public FixedPointF(float t)
    {
        m_value = (long)(t * (1 << m_float_num));
    }
    public FixedPointF(int t)
    {
        m_value = ((long)t) << m_float_num;
    }
    public FixedPointF(double t)
    {
        m_value = (long)(t * (1 << m_float_num));
    }
    public FixedPointF(int t1, int t2)
    {
        m_value = ((long)t1 << m_float_num) / t2;
    }

    public static FixedPointF zero = new FixedPointF(0);

    public static FixedPointF operator +(FixedPointF t1, FixedPointF t2)
    {
        FixedPointF temp;
        temp.m_value = t1.m_value + t2.m_value;
        return temp;
    }
    public static FixedPointF operator -(FixedPointF t1, FixedPointF t2)
    {
        FixedPointF temp;
        temp.m_value = t1.m_value - t2.m_value;
        return temp;
    }
    public static FixedPointF operator -(FixedPointF t)
    {
        t.m_value = -t.m_value;
        return t;
    }
    public static FixedPointF operator *(FixedPointF t1, FixedPointF t2)
    {
        FixedPointF temp;
        temp.m_value = (t1.m_value * t2.m_value) >> m_float_num;
        return temp;
    }
    public static FixedPointF operator /(FixedPointF t1, FixedPointF t2)
    {
        FixedPointF temp;
        temp.m_value = (t1.m_value << m_float_num) / t2.m_value;
        return temp;
    }
    public static FixedPointF operator /(FixedPointF t1, int t2)
    {
        FixedPointF temp = t1 / new FixedPointF(t2);
        return temp;
    }
    public static bool operator ==(FixedPointF t1, FixedPointF t2)
    {
        return t1.m_value == t2.m_value;
    }
    public static bool operator !=(FixedPointF t1, FixedPointF t2)
    {
        return t1.m_value != t2.m_value;
    }
    public static bool operator >(FixedPointF t1, FixedPointF t2)
    {
        return t1.m_value > t2.m_value;
    }
    public static bool operator <(FixedPointF t1, FixedPointF t2)
    {
        return t1.m_value < t2.m_value;
    }
    public static bool operator >=(FixedPointF t1, FixedPointF t2)
    {
        return t1.m_value >= t2.m_value;
    }
    public static bool operator <=(FixedPointF t1, FixedPointF t2)
    {
        return t1.m_value <= t2.m_value;
    }

    public static FixedPointF Sqrt(FixedPointF t)
    {
        double v = (double)t.m_value / (1 << m_float_num);
        double d = Math.Sqrt(v);
        FixedPointF f = new FixedPointF(d);
        return f;
    }

    public override bool Equals(object obj)
    {
        if (obj is FixedPointF)
        {
            FixedPointF f = (FixedPointF)obj;
            return f.m_value == m_value;
        }
        return false;
    }
    public override string ToString()
    {
        return value.ToString();
    }
    public override int GetHashCode()
    {
        return 0;
    }
}