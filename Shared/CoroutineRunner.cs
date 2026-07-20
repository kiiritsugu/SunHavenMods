using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    
    private static MonoBehaviour _host;

    public static void SetHost(MonoBehaviour host)
    {
        _host = host;
    }

    public static void Run(IEnumerator routine)
    {
        if (_host != null)
        {
            _host.StartCoroutine(routine);
        }
        else
        {
            Instance.StartCoroutine(routine);
        }
    }

    public static void BatchProcess<T>(IEnumerable<T> collection, Action<T> action, int batchSize, Action onComplete = null)
    {
        Run(ExecuteBatch(collection, action, batchSize, onComplete));
    }

    private static IEnumerator ExecuteBatch<T>(IEnumerable<T> collection, Action<T> action, int batchSize, Action onComplete)
    {
        if (collection == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        var list = new List<T>(collection);
        if (batchSize <= 0) batchSize = 10;
        int count = 0;

        foreach (var item in list)
        {
            if (item == null) continue;

            try
            {
                action(item);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[CoroutineRunner] Error processing batch item: {ex}");
            }

            count++;
            if (count % batchSize == 0)
            {
                yield return null;
            }
        }
        onComplete?.Invoke();
    }

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