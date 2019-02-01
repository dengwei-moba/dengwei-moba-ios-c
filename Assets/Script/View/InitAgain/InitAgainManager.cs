using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class InitAgainManager : ScriptBase
{
    

    void Start()
    {
        _ViewManager.ClearView();
		_ViewManager.LoadView("prefab/ui/matchview_prefab");
    }
}
