using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public static event Action<bool> OnFreeze;
    public GameObject catIceShell;
    public float freezeTime = 5f;
    public bool isFreezed;
    
    private Coroutine _showFreezingCoroutine;
    public void CheckForFreeze()
    {
        //if (!isFreezed) return;
        if (_showFreezingCoroutine != null)
        {
            StopCoroutine(_showFreezingCoroutine);
        }
        _showFreezingCoroutine = StartCoroutine(FreezeEvent());
    }
    
    private IEnumerator FreezeEvent()
    {
        isFreezed = true;
        catIceShell.SetActive(true);
        OnFreeze?.Invoke(isFreezed);
        yield return Yielders.Get(freezeTime);
        isFreezed = false;
        catIceShell.SetActive(false);
        OnFreeze?.Invoke(isFreezed);
    }
}
