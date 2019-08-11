using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Global
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnRuntimeInitialize()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private static void OnSceneChanged(Scene prevScene, Scene curScene)
    {
        Debug.LogFormat("Scene Is Changed from {0} to {1}", prevScene.name, curScene.name);

        if(curScene.name.Equals("Game"))
        {
            var obj = new GameObject("Client");
            obj.AddComponent<Game>();
        }

    }
}
