using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSupplyManager : MonoBehaviour
{
    public delegate void CheckLoseCondition(bool isFoodOver);
    public delegate void CheckWinCondition();
    public static event CheckLoseCondition OnLose;
    public static event CheckWinCondition OnWin;
    
    public GameObject[] mealPrefabs;
    
    [HideInInspector] public int rareFoodSatiety;
    [HideInInspector] public int mythFoodSatiety;
    [HideInInspector] public int legendFoodSatiety;
    
    public GameObject[] badMealPrefabs;
    
    [HideInInspector] public int rareBadFoodSatiety;
    [HideInInspector] public int mythBadFoodSatiety;
    
    public bool isBadMealNotEmpty = true;
    
    public int maxSatiety = 100;
    public int maxFoodMachineSatiety;
    
    [SerializeField]private int _currentSatiety;
    public int currentFoodMachineSatiety;
    
    [SerializeField]private int _currentBadMealSatiety;
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

        rareFoodSatiety = mealPrefabs[1].GetComponent<MealData>().mealStats.satiety;
        mythFoodSatiety = mealPrefabs[2].GetComponent<MealData>().mealStats.satiety;
        legendFoodSatiety = mealPrefabs[3].GetComponent<MealData>().mealStats.satiety;
        
        rareBadFoodSatiety = badMealPrefabs[1].GetComponent<MealData>().mealStats.satiety;
        mythBadFoodSatiety = badMealPrefabs[2].GetComponent<MealData>().mealStats.satiety;
    }

    private void AddCatSatiety(int satiety)
    {
        if(_currentSatiety == 0 && satiety < 0) return;
        if (_currentSatiety + satiety < 0 && satiety < 0)
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

        currentFoodMachineSatiety -= satiety;
        CheckForSatietyLimit();
        CheckForLoseCondition();
    }

    private void CheckForSatietyLimit()
    {
        if (currentFoodMachineSatiety - rareFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonMealChanceToSpawn, false);
        }
        else if (currentFoodMachineSatiety - mythFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.RareMealChanceToSpawn, false);
        } 
        else if (currentFoodMachineSatiety - legendFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.MythMealChanceToSpawn, false);
        }
    }
    
    private void CheckForBadSatietyLimit()
    {
        if (_currentBadMealSatiety - rareBadFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonBadMealChanceToSpawn, true);
        }
        else if (currentFoodMachineSatiety - mythBadFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.RareBadMealChanceToSpawn, true);
        }
    }
    
    private void LimitSatiety(int limit, bool isLowerLimit)
    {
        foreach (var spawn in _spawns)
        {
            if (isLowerLimit)
                spawn.minRandomValue = limit;
            else spawn.maxRandomValue = limit;
        }
    }

    private void DecreaseBadMealSatiety(int satiety)
    {
        if (_currentBadMealSatiety + satiety < 0)
        {
            _currentBadMealSatiety = 0;
            return;
        }
        _currentBadMealSatiety += satiety;
        CheckForBadSatietyLimit();
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
        if (currentFoodMachineSatiety - (maxSatiety - _currentSatiety) < 0)
        {
            StopSpawnFood();
            OnLose?.Invoke(true);  
        }
        
        if (currentFoodMachineSatiety > 0)
            return;
        
        StopSpawnFood();
        CheckForFoodOnScene();
    }

    private void StopSpawnFood()
    {
        foreach (var spawn in _spawns)
        {
            spawn.StopSpawn();
        }
    }

    private void CheckForFoodOnScene()
    {
        var otherFood = GameObject.FindGameObjectsWithTag("Meal");
        
        if (otherFood.Length - 1 <= 0)
        {
            OnLose?.Invoke(false);
        }
        else
        {
            _isFoodOnSceneEnd = true; 
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
