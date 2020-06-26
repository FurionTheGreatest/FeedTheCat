using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text uICounter;
    public Slider satietySlider;
    public Slider foodLeftSlider;

    public int maxSatietyValue;
    private const string CounterText = "Food counter: ";
    private int _score = 0;
    private int _currentSatiety;
    private FoodSupplyManager _foodSupplyManager;
    private void Awake()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        maxSatietyValue = _foodSupplyManager.desiredSatiety;
        
        satietySlider.maxValue = maxSatietyValue;
        satietySlider.value = 0;
        foodLeftSlider.maxValue = maxSatietyValue + (maxSatietyValue/2);
        foodLeftSlider.value = foodLeftSlider.maxValue;
    }

    private void UpdateCounter()
    {
        _score ++;
        uICounter.text = CounterText + _score;
    }

    private void UpdateSatietySliderValue(int satiety)
    {
        satietySlider.value += satiety;
        foodLeftSlider.value -= satiety;
    }
    
    private void OnEnable()
    {
        OnTouch.UpdateUi += UpdateSatietySliderValue;
    }
    
    private void OnDisable()
    {
        OnTouch.UpdateUi -= UpdateSatietySliderValue;
    }
}
