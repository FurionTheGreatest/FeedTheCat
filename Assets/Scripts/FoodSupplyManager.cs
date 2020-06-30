using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSupplyManager : MonoBehaviour
{
    public delegate void CheckCondition();
    public static event CheckCondition OnLose;
    public static event CheckCondition OnWin;
    
    public GameObject[] mealPrefabs;
    
    public int commonFoodSatiety;
    public int rareFoodSatiety;
    public int mythFoodSatiety;
    public int legendFoodSatiety;
    
    public GameObject badMealPrefab;
    public bool isBadMealNotEmpty = true;
    
    public int maxSatiety = 100;
    public int maxFoodMachineSatiety;
    
    [SerializeField]private int _currentSatiety;
    public int currentFoodMachineSatiety;
    
    private int _currentBadMealSatiety;
    private int _maxBadMealSatiety;
    
    private FoodSpawner[] _spawns;

    private bool _isFoodOnSceneEnd;
    private bool _isWon;

    private void Awake()
    {
        maxFoodMachineSatiety = maxSatiety + maxSatiety/2;
        currentFoodMachineSatiety = maxFoodMachineSatiety;
        _maxBadMealSatiety = maxSatiety / 2;
        _currentBadMealSatiety = _maxBadMealSatiety;
    }
    
    private void Start()
    {
        _spawns = FindObjectsOfType<FoodSpawner>();

        commonFoodSatiety = mealPrefabs[0].GetComponent<MealData>().mealStats.satiety;
        rareFoodSatiety = mealPrefabs[1].GetComponent<MealData>().mealStats.satiety;
        mythFoodSatiety = mealPrefabs[2].GetComponent<MealData>().mealStats.satiety;
        legendFoodSatiety = mealPrefabs[3].GetComponent<MealData>().mealStats.satiety;
    }

    private void AddCatSatiety(int satiety)
    {
        if(_currentSatiety == 0 && satiety < 0) return;
        if (_currentSatiety - satiety > 0 && satiety < 0)
        {
            _currentSatiety = 0;
            return;
        }
        _currentSatiety += satiety;
        CheckForWinCondition();
        if(!_isFoodOnSceneEnd  || _isWon) return;
        CheckForLoseCondition();
    }
    
    private void DecreaseFoodMachineSatiety(int satiety)
    {
        if(currentFoodMachineSatiety == 0 && satiety < 0) return;
        if (currentFoodMachineSatiety - satiety > 0 && satiety < 0)
        {
            currentFoodMachineSatiety = 0;
            return;
        }
        currentFoodMachineSatiety -= satiety;
        CheckForSatietyLimit();
        CheckForLoseCondition();
    }

    private void CheckForSatietyLimit()
    {
        if (currentFoodMachineSatiety - rareFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonMealChanceToSpawn);
        }
        else if (currentFoodMachineSatiety - mythFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.RareMealChanceToSpawn);
        } 
        else if (currentFoodMachineSatiety - legendFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.MythMealChanceToSpawn);
        }
    }
    
    private void LimitSatiety(int limit)
    {
        foreach (var spawn in _spawns)
        {
            spawn.maxRandomValue = limit;
        }
    }

    private void DecreaseBadMealSatiety(int satiety)
    {
        _currentBadMealSatiety += satiety;
        CheckForLoseCondition();
        if(_currentBadMealSatiety > 0) return;
        isBadMealNotEmpty = false;
    }

    private void CheckForWinCondition()
    {
        if (_currentSatiety < maxSatiety) return;
        StopSpawnFood();
        _isWon = true;
        OnWin?.Invoke();
        
        var otherFood = GameObject.FindGameObjectsWithTag("Meal");

        foreach (var food in otherFood)
        {
            food.GetComponent<CircleCollider2D>().enabled = false;
            food.GetComponent<OnTouch>().destroyFood = true;
            //Todo foodOnScreenLeft +add points
        }
    }

    private void CheckForLoseCondition()
    {
        if (currentFoodMachineSatiety > 0)
            return;
        StopSpawnFood();
        
        var otherFood = GameObject.FindGameObjectsWithTag("Meal");
        
        if (otherFood.Length - 1 <= 0)
        {
            OnLose?.Invoke();
        }
        else
        {
            _isFoodOnSceneEnd = true; 
        }
    }

    private void StopSpawnFood()
    {
        foreach (var spawn in _spawns)
        {
            spawn.StopSpawn();
        }
    }

    private void OnEnable()
    {
        OnTouch.UpdateStats += AddCatSatiety;
        OnTouch.CheckForLose += CheckForLoseCondition;
        FoodSpawner.OnMealSpawned += DecreaseFoodMachineSatiety;
        FoodSpawner.OnBadMealSpawned += DecreaseBadMealSatiety;
    }
    
    private void OnDisable()
    {
        OnTouch.UpdateStats -= AddCatSatiety;
        OnTouch.CheckForLose -= CheckForLoseCondition;
        FoodSpawner.OnMealSpawned -= DecreaseFoodMachineSatiety;
        FoodSpawner.OnBadMealSpawned -= DecreaseBadMealSatiety;
    }
}
