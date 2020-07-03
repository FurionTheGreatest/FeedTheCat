using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text loseText;
    public TMP_Text loseFoodOverText;
    public TMP_Text winText;
    public Slider satietySlider;
    public Slider foodLeftSlider;

    public int maxSatietyUiValue;
    private int _score;
    private int _currentSatiety;
    private FoodSupplyManager _foodSupplyManager;
    private void Awake()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        maxSatietyUiValue = _foodSupplyManager.maxSatiety;
        
        satietySlider.maxValue = maxSatietyUiValue;
        satietySlider.value = 0;
        
        foodLeftSlider.maxValue = _foodSupplyManager.maxFoodMachineSatiety;
        foodLeftSlider.value = foodLeftSlider.maxValue;
    }

    private void UpdateSatietySliderValue(int satiety)
    {
        if(satietySlider.value == 0 && satiety < 0) return;
        if (satietySlider.value + satiety < 0 && satiety < 0)
        {
            satietySlider.value = 0;
            return;
        }
        satietySlider.value += satiety;
    }
    
    private void UpdateFoodMachineSatietySliderValue(int satiety)
    {
        if(foodLeftSlider.value == 0 && satiety < 0) return;
        if (foodLeftSlider.value - satiety > 0 && satiety < 0)
        {
            foodLeftSlider.value = 0;
            return;
        }
        foodLeftSlider.value -= satiety;
    }

    private void EnableLoseScreen(bool isFoodOver)
    {
        if(!isFoodOver)
            loseText.gameObject.SetActive(true);
        else
            loseFoodOverText.gameObject.SetActive(true);
    }
    
    private void EnableWinScreen()
    {
        winText.gameObject.SetActive(true);
    }
    
    private void OnEnable()
    {
        OnTouch.UpdateStats += UpdateSatietySliderValue;
        FoodSpawner.OnMealSpawned += UpdateFoodMachineSatietySliderValue;
        FoodSupplyManager.OnLose += EnableLoseScreen;
        FoodSupplyManager.OnWin += EnableWinScreen;
    }
    
    private void OnDisable()
    {
        OnTouch.UpdateStats -= UpdateSatietySliderValue;
        FoodSpawner.OnMealSpawned -= UpdateFoodMachineSatietySliderValue;
        FoodSupplyManager.OnLose -= EnableLoseScreen;
        FoodSupplyManager.OnWin -= EnableWinScreen;
    }
}
