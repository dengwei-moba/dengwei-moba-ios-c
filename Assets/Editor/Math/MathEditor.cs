using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

public class MathEditor : Editor
{
    [MenuItem("设置数学基础数据/设置sin和cos表")]
    static void SetSinAndCos()
    {
        SetMathSin();
        SetMathCos();
        AssetDatabase.Refresh();
    }
    
    static void SetMathSin()
    {
        string sinPath = Application.dataPath + "/Config/Sin.txt";
        if (!File.Exists(sinPath))
            return;
        string dir = Application.dataPath + "/Script/Math/";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string sinScriptPath = dir + "CustomMathSin.cs";

        string fileContent = File.ReadAllText(sinPath);
        string pattern = @"sin([0-9]+)=([0-9.]+)";
        List<string> sins = new List<string>();
        MatchCollection regex = Regex.Matches(fileContent, pattern);

        StringBuilder sinDicContent = new StringBuilder();
        sinDicContent.Append("using System.Collections;\r\n" +
            "using System.Collections.Generic;\r\n" +
            "using UnityEngine;\r\n" +
            "public partial class CustomMath\r\n" +
            "{\r\n" +
            "\tprivate static Dictionary<int, CustomFraction> Sin = new Dictionary<int, CustomFraction>()\r\n" +
            "\t{\r\n");
        for (int i = 0; i < regex.Count; i++)
        {
            string key = regex[i].Groups[1].Value;
            string value = regex[i].Groups[2].Value;
            int valuenum = (int)(float.Parse(value) * 1000);
            string str = "\t\t{0} {1},new CustomFraction({2}, 1000){3},\r\n";
            sinDicContent.Append(string.Format(str, "{", key, valuenum, "}"));
        }
        sinDicContent.Append("\t};\r\n");
        sinDicContent.Append("}");
        File.WriteAllText(sinScriptPath, sinDicContent.ToString());        
    }
    
    static void SetMathCos()
    {
        string sinPath = Application.dataPath + "/Config/Cos.txt";
        if (!File.Exists(sinPath))
            return;
        string dir = Application.dataPath + "/Script/Math/";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        string sinScriptPath = dir + "CustomMathCos.cs";

        string fileContent = File.ReadAllText(sinPath);
        string pattern = @"cos([0-9]+)=([0-9.]+)";
        List<string> sins = new List<string>();
        MatchCollection regex = Regex.Matches(fileContent, pattern);

        StringBuilder sinDicContent = new StringBuilder();
        sinDicContent.Append("using System.Collections;\r\n" +
            "using System.Collections.Generic;\r\n" +
            "using UnityEngine;\r\n" +
            "public partial class CustomMath\r\n" +
            "{\r\n" +
            "\tprivate static Dictionary<int, CustomFraction> Cos = new Dictionary<int, CustomFraction>()\r\n" +
            "\t{\r\n");
        for (int i = 0; i < regex.Count; i++)
        {
            string key = regex[i].Groups[1].Value;
            string value = regex[i].Groups[2].Value;
            int valuenum = (int)(float.Parse(value) * 1000);
            string str = "\t\t{0} {1},new CustomFraction({2}, 1000){3},\r\n";
            sinDicContent.Append(string.Format(str, "{", key, valuenum, "}"));
        }
        sinDicContent.Append("\t};\r\n");
        sinDicContent.Append("}");
        File.WriteAllText(sinScriptPath, sinDicContent.ToString());
    }
}
