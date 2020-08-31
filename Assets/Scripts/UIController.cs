using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TMP_Text loseText;
    public TMP_Text winText;
    public Slider satietySlider;
    public Slider foodLeftSlider;
    public int maxSatietyUiValue;

    public Image frameImage;
    private float _yieldTime = 0.05f;
    private Coroutine showFrameCoroutine;
    private int _score;
    private int _currentSatiety;
    private FoodSupplyManager _foodSupplyManager;

    private string _gameOverText = "The food is over, GG";
    private string _foodOverText = "Not enough food in machine to finish level, GG";
    private void Start()
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
        
        if (satiety > 0)
        {
            if (showFrameCoroutine != null)
            {
                StopCoroutine(showFrameCoroutine);
            }
            showFrameCoroutine = StartCoroutine(ShowFrame(Color.green));
        }
        else
        {
            if (showFrameCoroutine != null)
            {
                StopCoroutine(showFrameCoroutine);
            }
            showFrameCoroutine = StartCoroutine(ShowFrame(Color.red));
        }
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
        if (!isFoodOver)
        {
            loseText.text = _gameOverText;
            loseText.gameObject.SetActive(true);
        }
        else
        {
            loseText.text = _foodOverText;
            loseText.gameObject.SetActive(true);
        }
    }
    
    private void EnableWinScreen()
    {
        winText.gameObject.SetActive(true);
    }

    private IEnumerator ShowFrame(Color color)
    {
        frameImage.color = color;
        var alpha = frameImage.color;
        while (frameImage.color.a < 1)
        {
            alpha.a += 0.1f;
            frameImage.color = alpha;
            yield return Yielders.Get(_yieldTime);
        }
        while (frameImage.color.a > 0)
        {
            alpha.a -= 0.1f;
            frameImage.color = alpha;
            yield return Yielders.Get(_yieldTime);
        }
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
