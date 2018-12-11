using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PackagerEditor : EditorWindow
{
    private static string dealPath = Application.dataPath;
    private static string assetBundlePath = Application.dataPath + "/StreamingAssets/AssetBundle";
    public int type = 0;

    //忽略的打包的路径
    private static List<string> overLookFolders = new List<string>
    { "Editor", "Plugins", "Script", "StreamingAssets", "AssetBundle", "EasyTouchBundle", "NGUI" };

    //允许打包的文件类型
    private static List<string> fileExtList = new List<string>
    { "prefab", "png", "jpg", "json", "txt", "mat", "shader", "ttf", "fnt", "tga", "psd", "renderTexture","bytes" };

    private static BuildTarget bt = BuildTarget.StandaloneWindows;

    #region 设置资源名称
    [MenuItem("AssetsBundle打包/设置所有预制体打包名")]
    static void OnSetAssetBundleName()
    {
        CreateDirectoryAssetBundleName(dealPath);
        AssetDatabase.Refresh();
    }

    [MenuItem("AssetsBundle打包/删除所有预制体打包名", false)]
    public static void Removing()
    {
        DeleteDirectoryAssetBundleName(dealPath);
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 创建包名
    /// </summary>
    /// <param name="direcPath"></param>
    static void CreateDirectoryAssetBundleName(string direcPath)
    {
        string[] dirs = Directory.GetDirectories(direcPath, "*.*", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < dirs.Length; i++)
        {
            string s = dirs[i].Substring(dirs[i].Replace("\\", "/").LastIndexOf('/') + 1);
            if (!overLookFolders.Contains(s))
            {
                string[] files = Directory.GetFiles(dirs[i], "*.*", SearchOption.AllDirectories);
                for (int m = 0; m < files.Length; m++)
                    DoSetAssetBundleName(files[m], files[m]);
            }
        }
    }
    /// <summary>
    /// 删除包名
    /// </summary>
    /// <param name="direcPath"></param>
    static void DeleteDirectoryAssetBundleName(string direcPath)
    {
        string[] dirs = Directory.GetDirectories(direcPath, "*.*", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < dirs.Length; i++)
        {
            string s = dirs[i].Substring(dirs[i].Replace("\\", "/").LastIndexOf('/') + 1);
            if (!overLookFolders.Contains(s))
            {
                string[] files = Directory.GetFiles(dirs[i], "*.*", SearchOption.AllDirectories);
                for (int m = 0; m < files.Length; m++)
                    DoSetAssetBundleName(files[m], "");
            }
        }
    }
    private static void DoSetAssetBundleName(string path, string abName)
    {
        path = path.Replace("\\", "/");
        abName = abName.Replace("\\", "/");
        string ext = path.Substring(path.LastIndexOf(".") + 1);
        if (fileExtList.Contains(ext))
        {
            Debug.Log("当前文件是：" + path);
            path = path.Replace(dealPath, "Assets");
            AssetImporter ai = AssetImporter.GetAtPath(path);
            abName = abName.Replace(dealPath + "/", "");
            ai.assetBundleName = abName.Replace(".", "_");
        }
    }
    #endregion

    #region 打包资源
    [MenuItem("AssetsBundle打包/打包资源")]
    public static void CreateAllAssetsBundle()
    {
        PackagerEditor window = (PackagerEditor)EditorWindow.GetWindow(typeof(PackagerEditor));
        window.type = 1;
        window.Show();
    }
    /// <summary>
    /// 打包资源，模型，图片，材质，表
    /// </summary>
    /// <param name="teg"></param>
    private static void BuildAssetBundle(BuildTarget teg)
    {
        if (!Directory.Exists(assetBundlePath))
        {
            Directory.CreateDirectory(assetBundlePath);            
        }
        BuildPipeline.BuildAssetBundles(assetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle, teg);
        AssetDatabase.Refresh();
    }
    #endregion

    #region 打包场景
    [MenuItem("AssetsBundle打包/打包单个场景")]
    public static void OnBuildSingleScene()
    {
        PackagerEditor window = (PackagerEditor)EditorWindow.GetWindow(typeof(PackagerEditor));
        window.type = 2;
        window.Show();
    }
    [MenuItem("AssetsBundle打包/打包所有场景")]
    public static void OnBuildAllScene()
    {
        PackagerEditor window = (PackagerEditor)EditorWindow.GetWindow(typeof(PackagerEditor));
        window.type = 3;
        window.Show();
    }
    /// <summary>
    /// 打包单个场景
    /// </summary>
    private static void BuildSingleScene()
    {
        string file = EditorUtility.OpenFilePanel("Select file", "", "unity");
        BuildScene(file);
    }
    /// <summary>
    /// 打包所有场景
    /// </summary>
    private static void BuildAllScene()
    {
        string findpath = dealPath + "/Scene/";
        if (Directory.Exists(findpath))
        {
            string[] files = Directory.GetFiles(findpath.Replace("\\", "/"), "*.unity");
            for(int i = 0;i< files.Length;i++)
                BuildScene(files[i]);
        }        
    }
    private static void BuildScene(string tPath)
    {
        string[] files = new string[1] { tPath };
        files[0] = files[0].Replace("\\", "/");
        string name = files[0].Remove(0, files[0].LastIndexOf("/") + 1).Replace(".unity", "");
        string outpath = assetBundlePath + "/scene/";
        if (!Directory.Exists(outpath))
            Directory.CreateDirectory(outpath);
        BuildPipeline.BuildPlayer(files, outpath + name + ".unity3d", bt, BuildOptions.BuildAdditionalStreamedScenes);
        AssetDatabase.Refresh();
    }
    #endregion 打包场景


    void OnGUI()
    {
        switch (type)
        {
            case 1:
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("选择打包平台");
                bt = (BuildTarget)EditorGUILayout.EnumPopup("Select Platform", bt);
                if (GUILayout.Button("打包"))
                {
                    Close();
                    BuildAssetBundle(bt);
                }
                EditorGUILayout.EndVertical();
                break;
            case 2:
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("选择打包平台");
                bt = (BuildTarget)EditorGUILayout.EnumPopup("Select Platform", bt);
                if (GUILayout.Button("打包单个场景"))
                {
                    Close();
                    BuildSingleScene();
                }
                EditorGUILayout.EndVertical();
                break;
            case 3:
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("选择打包平台");
                bt = (BuildTarget)EditorGUILayout.EnumPopup("Select Platform", bt);
                if (GUILayout.Button("打包所有场景"))
                {
                    Close();
                    BuildAllScene();
                }
                EditorGUILayout.EndVertical();
                break;
        }

    }
}