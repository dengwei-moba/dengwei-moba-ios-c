using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//通过加载loadingScene跳转场景
public class LoadingManager : ScriptBase
{
    private static string mTargetSceneName;

    public Slider mSlider;
    public Text mSliderLab;

    private AssetBundle mSceneBundle;
    private AsyncOperation mAsyncOperation;

    void Start()
    {
        _ViewManager.ClearView();
        ProcessValue = 0;

        if (!string.IsNullOrEmpty(mTargetSceneName))
            StartCoroutine(StartLoadScene(mTargetSceneName));
    }

    float mActualProcess = 0;
    float mCurProcess = 0;

    void Update()
    {
        if (mAsyncOperation != null)
        {
            mActualProcess = mAsyncOperation.progress;
            if (mAsyncOperation.progress >= 0.9f)
                mActualProcess = 1;

            if (mCurProcess < mActualProcess)
            {
                mCurProcess += 0.01f;
            }
            mCurProcess = Mathf.Clamp(mCurProcess, 0, mActualProcess);

            ProcessValue = mCurProcess;

            if (mCurProcess == 1)
            {
                mAsyncOperation.allowSceneActivation = true;
                switch (mTargetSceneName)
                {
                }
                mTargetSceneName = "";
            }
        }
    }

    float ProcessValue
    {
        set
        {
            mSlider.value = value;
            mSliderLab.text = string.Format("{0}%", Mathf.CeilToInt(value * 100));
        }
    }

    IEnumerator StartLoadScene(string tTargetSceneName)
    {
        mSceneBundle = AssetBundle.LoadFromFile(AppConst.LoadRes_Root_Path + AppConst.mScene_Path + tTargetSceneName + ".unity3d");
        mAsyncOperation = SceneManager.LoadSceneAsync(tTargetSceneName);
        mAsyncOperation.allowSceneActivation = false;
        yield return null;
    }

    public static void LoadSceneAsync(string tTargetSceneName)
    {
        if (mTargetSceneName == tTargetSceneName)
            return;
        mTargetSceneName = tTargetSceneName;
        SceneManager.LoadScene(SceneConfig.Loading);
    }

    void OnDestroy()
    {
        if (mSceneBundle != null)
            mSceneBundle.Unload(false);
    }
}
