using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter instance;
    public static CoroutineStarter Instance
    {
        get => instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public void StartCoroutine(Coroutine coroutine)
    {
        StartCoroutine(coroutine);
    }
}
