using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject loseScreen;
    public GameObject winScreen;
    public Slider satietySlider;
    public Slider foodLeftSlider;
    public int maxSatietyUiValue;

    public Image frameImage;
    private float _yieldTime = 0.05f;
    private Coroutine showFrameCoroutine;
    private Coroutine winCoroutine;
    private int _score;
    private int _currentSatiety;
    private FoodSupplyManager _foodSupplyManager;

    private string _gameOverText = "The food is over, GG";
    private string _foodOverText = "Not enough food in machine to finish level";
    private float _spawnDelay = 0.2f;

    private void Start()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        maxSatietyUiValue = _foodSupplyManager.maxCatSatiety;
        
        satietySlider.maxValue = maxSatietyUiValue;
        satietySlider.value = 0;
        foodLeftSlider.maxValue = _foodSupplyManager.maxFoodMachineSatiety;
        foodLeftSlider.value = foodLeftSlider.maxValue;
    }    

    private void UpdateSatietySliderValue(GameObject obj)
    {
        var satiety = obj.GetComponent<Collectible>().mealStats.satiety;
        if(satietySlider.value == 0 && satiety < 0) return;
        if (satietySlider.value + satiety < 0 && satiety < 0)
        {
            satietySlider.value = 0;
            return;
        }
        satietySlider.value += satiety;
        
        if(satiety == 0) return;
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
            loseScreen.GetComponentInChildren<TMP_Text>().text = _gameOverText;
            loseScreen.SetActive(true);
        }
        else
        {
            loseScreen.GetComponentInChildren<TMP_Text>().text = _foodOverText;
            loseScreen.SetActive(true);
        }
    }
    
    private void EnableWinScreen()
    {
        Debug.Log("win");
        if (winCoroutine != null)
        {
            StopCoroutine(winCoroutine);
        }
        winCoroutine = StartCoroutine(ShowWinScreen(10, 1));
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

    private IEnumerator ShowWinScreen(int spawnCount, int index)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            FindObjectOfType<ParticleSpawner>().Spawn(index);
            yield return Yielders.Get(_spawnDelay);
        }
        yield return Yielders.EndOfFrame;
        winScreen.SetActive(true);
    }
    
    private void OnEnable()
    {
        OnTouch.OnCollect += UpdateSatietySliderValue;
        FoodSpawner.OnMealSpawned += UpdateFoodMachineSatietySliderValue;
        FoodSupplyManager.OnLose += EnableLoseScreen;
        FoodSupplyManager.OnWin += EnableWinScreen;
    }
    
    private void OnDisable()
    {
        OnTouch.OnCollect -= UpdateSatietySliderValue;
        FoodSpawner.OnMealSpawned -= UpdateFoodMachineSatietySliderValue;
        FoodSupplyManager.OnLose -= EnableLoseScreen;
        FoodSupplyManager.OnWin -= EnableWinScreen;
    }
}
