using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FoodSupplyManager : MonoBehaviour
{
    public static event Action<bool> OnLose;
    public static event Action OnWin;
    
    public List<AssetReference> mealPrefabs;
    public readonly int CommonFoodSatiety = 1;
    public readonly int RareFoodSatiety = 2;
    public readonly int MythFoodSatiety = 5;
    public readonly int LegendFoodSatiety = 10;
    
    public List<AssetReference> badMealPrefabs;

    public readonly int CommonBadFoodSatiety = -1;
    public readonly int RareBadFoodSatiety = -2;
    public readonly int MythBadFoodSatiety = -5;
    
    public bool isBadMealNotEmpty = true;
    
    public int maxCatSatiety = 100;
    public int maxFoodMachineSatiety;
    
    private int _currentCatSatiety;
    public int currentFoodMachineSatiety;
    
    public bool isWon;
    
    public List<GameObject> foodOnScene;
    public List<GameObject> bombsOnScene;
    
    private int _currentBadMealSatiety;
    private int _maxBadMealSatiety;
    
    private FoodSpawner[] _spawns;

    private bool _isFoodOnSceneEnd;


    private void Awake()
    {
        maxFoodMachineSatiety = maxCatSatiety + maxCatSatiety/2;
        currentFoodMachineSatiety = maxFoodMachineSatiety;
        _maxBadMealSatiety = maxCatSatiety / 2;
        _currentBadMealSatiety = _maxBadMealSatiety;
    }
    
    private void Start()
    {
        _spawns = FindObjectsOfType<FoodSpawner>();
    }
    

    private void AddCatSatiety(GameObject obj)
    {
        var satiety = obj.GetComponent<Collectible>().mealStats.satiety;

        if(_currentCatSatiety == 0 && satiety < 0) return;
        if (_currentCatSatiety + satiety < 0 && satiety < 0)
        {
            _currentCatSatiety = 0;
            return;
        }
        _currentCatSatiety += satiety;
        CheckForWinCondition();
        if(!_isFoodOnSceneEnd  || isWon) return;
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
        if (currentFoodMachineSatiety - RareFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonMealChanceToSpawn, false);
        }
        else if (currentFoodMachineSatiety - MythFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.RareMealChanceToSpawn, false);
        } 
        else if (currentFoodMachineSatiety - LegendFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.MythMealChanceToSpawn, false);
        }
    }
    
    private void CheckForBadSatietyLimit()
    {
        if (_currentBadMealSatiety - RareBadFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonBadMealChanceToSpawn, true);
        }
        else if (currentFoodMachineSatiety - MythBadFoodSatiety < 0)
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
        if (_currentCatSatiety < maxCatSatiety) return;
        StopSpawnFood();
        isWon = true;
        OnWin?.Invoke();
        
        ClearFoodTable();
    }

    private void ClearFoodTable()
    {
        var objectsOnTable = foodOnScene;
        objectsOnTable.AddRange(bombsOnScene);
        foreach (var obj in objectsOnTable)
        {
            obj.GetComponent<CircleCollider2D>().enabled = false;
            obj.GetComponent<OnTouch>().destroyFood = true;
        }
        foodOnScene.Clear();
        bombsOnScene.Clear();
    }

    private void CheckForLoseCondition()
    {
        var sceneSatiety = foodOnScene.Sum(food => food.GetComponent<Collectible>().mealStats.satiety);
        if (_currentCatSatiety + currentFoodMachineSatiety + sceneSatiety < maxCatSatiety)
        {
            ClearFoodTable();
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
        if (foodOnScene.Count == 0)
        {
            OnLose?.Invoke(false);
        }
        else
        {
            _isFoodOnSceneEnd = true; 
        }
    }

    private void AddFoodToList(GameObject food)
    {
        if(food.GetComponent<OnTouch>().isFood)
            foodOnScene.Add(food);
        else
            bombsOnScene.Add(food);
    }
    private void RemoveFoodFromList(GameObject food)
    {
        if (food.GetComponent<OnTouch>().isFood)
        {
            if (foodOnScene.Contains(food))
            {
                foodOnScene.Remove(food);
            }
        }
        else
        {
            if (bombsOnScene.Contains(food))
            {
                bombsOnScene.Remove(food);
            }
        }
    }

    private void OnEnable()
    {
        Collectible.OnCollect += AddCatSatiety;
        Collectible.OnCollect += RemoveFoodFromList;
        OnTouch.CheckForLose += CheckForLoseCondition;
        OnTouch.OnDestroyObject += RemoveFoodFromList;
        OnTouch.OnTrippleSpawn += AddFoodToList;
        FoodSpawner.OnMealSpawned += DecreaseFoodMachineSatiety;
        FoodSpawner.OnMealAddOnScene += AddFoodToList;
        FoodSpawner.OnBadMealSpawned += DecreaseBadMealSatiety;
    }
    
    private void OnDisable()
    {
        Collectible.OnCollect -= AddCatSatiety;
        Collectible.OnCollect -= RemoveFoodFromList;
        OnTouch.CheckForLose -= CheckForLoseCondition;
        OnTouch.OnDestroyObject -= RemoveFoodFromList;
        OnTouch.OnTrippleSpawn -= AddFoodToList;
        FoodSpawner.OnMealSpawned -= DecreaseFoodMachineSatiety;
        FoodSpawner.OnMealAddOnScene -= AddFoodToList;
        FoodSpawner.OnBadMealSpawned -= DecreaseBadMealSatiety;
    }
}
