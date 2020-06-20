using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text uICounter;
    private const string CounterText = "Food counter: ";
    private int _score = 0;
    
    private void UpdateCounter()
    {
        _score ++;
        uICounter.text = CounterText + _score;
    }
    
    private void OnEnable()
    {
        OnTouch.UpdateUi += UpdateCounter;
    }
    
    private void OnDisable()
    {
        OnTouch.UpdateUi -= UpdateCounter;
    }
}
