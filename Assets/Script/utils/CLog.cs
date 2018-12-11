using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using Google.Protobuf;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using DMC;

namespace DMC
{
   /* public partial class AsyncLog
    {
    }*/
}
public class CLog : MonoSingleton<CLog>
{
    struct LogObject
    {
        public string context;
        public LogType logType;
        public LogObject(string ctx, LogType type) : this()
        {
            context = ctx;
            logType = type;
        }
    }
    private static Queue<LogObject> logQ = new Queue<LogObject>();
    private static int mainThreadID = 0;
    static StringBuilder sb = null;
    //public static void Record(PB_FrameInfo pb)
    //{
    //    if (AsyncLog.logSwitch)
    //    {
    //        if (null == sb)
    //            sb = new StringBuilder();
    //        sb.Length = 0;
    //        string pbinfo = string.Format("inputes count {0}", pb.inputs.Count);
    //        sb.Append(pbinfo);
    //        sb.Append("\n");
    //        for (int i = 0; i < pb.inputs.Count; i++)
    //        {
    //            sb.AppendFormat("player {0}:key {1},move {2},state {3}\n", pb.inputs[i].index, (ControlCmd)pb.inputs[i].input.key, (ControlCmd)pb.inputs[i].input.move, (ButtonStateType)pb.inputs[i].input.state);
    //        }
    //        AsyncLog.LogFormatWithoutConsole(sb.ToString());
    //    }
    //}

    public static void LogFormat(string format, params object[] args)
    {
        LogObject logObject = new LogObject(string.Format(format, args), LogType.Log);
        logQ.Enqueue(logObject);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        LogObject logObject = new LogObject(string.Format(format, args), LogType.Warning);
        logQ.Enqueue(logObject);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        LogObject logObject = new LogObject(string.Format(format, args), LogType.Error);
        logQ.Enqueue(logObject);
    }

    void Awake()
    {
        mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    void Update()
    {
        while (logQ.Count > 0)
        {
            LogObject logObject = logQ.Dequeue();
            switch (logObject.logType)
            {
                case LogType.Error:
                    Debug.LogError(logObject.context);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(logObject.context);
                    break;
                case LogType.Log:
                    Debug.Log(logObject.context);
                    break;
                default:
                    break;

            }
        }
    }
}
