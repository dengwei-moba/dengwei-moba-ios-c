using UnityEngine;
using System.Collections;
using System;

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
                _instance.Init();
            }

            return _instance;
        }

        private set { }
    }

    public Singleton()
    {

    }

    public virtual void Init()
    {

    }

    public virtual void Destroy()
    {
        _instance = null;
    }
}