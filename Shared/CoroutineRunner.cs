using System;
using UnityEngine;
using System.Collections;
using BepInEx;

namespace Shared;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("CoroutineRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<CoroutineRunner>();
            }
            return _instance;
        }
    }
    
    public static void Run(IEnumerator routine) => Instance.StartCoroutine(routine);

    // Add the helper method here
    public static void DelayedFunction(Action action, int frames = 1)
    {
        Run(ExecuteDelayed(action, frames));
    }

    private static IEnumerator ExecuteDelayed(Action action, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null; // Wait for the next frame
        }
        action?.Invoke();
    }
}