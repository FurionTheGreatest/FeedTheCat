using System;
using System.Collections;
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
        var time = DateTime.Now; 
        Debug.Log("freezed");
        isFreezed = true;
        catIceShell.SetActive(true);
        OnFreeze?.Invoke(isFreezed);
        yield return Yielders.Get(freezeTime);
        var elapsed = DateTime.Now;
        var difference = elapsed - time;
        Debug.Log(difference + " not freezed");
        isFreezed = false;
        catIceShell.SetActive(false);
        OnFreeze?.Invoke(isFreezed);
    }
}
