using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public void Initialize()
    {
        DontDestroyOnLoad(this);
    }

    public void ChangeScene(int index)
    {
        if(index == 1)
        {
            SessionData.instance.ClearData();
        }

        SceneManager.LoadScene(index);
    }

    public void ChangeSceneNetwork(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void SubscribeOnSceneLoad(UnityAction<Scene, LoadSceneMode> action)
    {
        SceneManager.sceneLoaded += action;
    }

    public void UnsubscribeOnSceneLoad(UnityAction<Scene, LoadSceneMode> action)
    {
        SceneManager.sceneLoaded -=  action;
    }

}
