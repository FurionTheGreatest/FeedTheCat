using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FoodSupplyManager : MonoBehaviour
{
    public static FoodSupplyManager Instance;
    public static event Action<bool> OnLose;
    public static event Action OnWin;
    [Header("Prefabs")]
    public List<AssetReference> mealPrefabs;

    public int[] mealSatietyList = {1, 2, 5, 10};

    public List<AssetReference> badMealPrefabs;
    
    public int[] badMealSatietyList = { -1, -2, -5};
    [Header("Level Values")]
    public int maxCatSatiety = 100;
    public int additionalPartCatSatiety = 4;
    public int additionalPartBadSatiety = 3;
    public int maxFoodMachineSatiety;
    
    private int _currentCatSatiety;
    private int _currentFoodMachineSatiety;
    [Header("States")]
    public bool isBadMealNotEmpty = true;
    public bool isWon;
    [Header("Food On Scene")]
    public List<GameObject> foodOnScene;
    public List<GameObject> bombsOnScene;
    
    private int _currentBadMealSatiety;
    private int _maxBadMealSatiety;
    
    private FoodSpawner[] _spawns;
    private bool _isFoodOnSceneEnd;

    private void Awake()
    {
        maxFoodMachineSatiety = maxCatSatiety + maxCatSatiety/additionalPartCatSatiety;
        _currentFoodMachineSatiety = maxFoodMachineSatiety;
        _maxBadMealSatiety = maxCatSatiety / additionalPartBadSatiety;
        _currentBadMealSatiety = _maxBadMealSatiety;
    }
    
    private void Start()
    {
        _spawns = FindObjectsOfType<FoodSpawner>();
        if (Instance == null)
        {
            Instance = this;
        }
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
        if(_currentFoodMachineSatiety == 0 && satiety < 0) return;

        _currentFoodMachineSatiety -= satiety;
        //CheckForSatietyLimit();
        CheckForLoseCondition();
    }

   /* private void CheckForSatietyLimit()
    {
        if (_currentFoodMachineSatiety - RareFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonMealChanceToSpawn, false);
        }
        else if (_currentFoodMachineSatiety - MythFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.RareMealChanceToSpawn, false);
        } 
        else if (_currentFoodMachineSatiety - LegendFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.MythMealChanceToSpawn, false);
        }
    }*/
    /*private void CheckForBadSatietyLimit()
    {
        if (_currentBadMealSatiety - RareBadFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.CommonBadMealChanceToSpawn, true);
        }
        else if (_currentFoodMachineSatiety - MythBadFoodSatiety < 0)
        {
            LimitSatiety(FoodSpawner.RareBadMealChanceToSpawn, true);
        }
    }*/
    
    /*private void LimitSatiety(int limit, bool isLowerLimit)
    {
        foreach (var spawn in _spawns)
        {
            if (isLowerLimit)
                spawn.minRandomValue = limit;
            else spawn.maxRandomValue = limit;
        }
    }*/

    private void DecreaseBadMealSatiety(int satiety)
    {
        if (_currentBadMealSatiety + satiety < 0)
        {
            _currentBadMealSatiety = 0;
            return;
        }
        _currentBadMealSatiety += satiety;
        //CheckForBadSatietyLimit();
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
        FindObjectOfType<PauseController>().enabled = false;
        ClearFoodTable();
    }

    private void ClearFoodTable()
    {
        var objectsOnTable = foodOnScene;
        objectsOnTable.AddRange(bombsOnScene);
        foreach (var obj in objectsOnTable.Where(obj => obj != null))
        {
            obj.GetComponent<CircleCollider2D>().enabled = false;
            obj.GetComponent<OnTouch>().destroyFood = true;
        }
        foodOnScene.Clear();
        bombsOnScene.Clear();
        if (foodOnScene.Count == 0 && bombsOnScene.Count == 0) return;
        foodOnScene.Clear();
        bombsOnScene.Clear();
    }

    private void CheckForLoseCondition()
    {
        var sceneSatiety = foodOnScene.Where(food => food != null).Sum(food => food.GetComponent<Collectible>().mealStats.satiety);

        if (_currentCatSatiety + _currentFoodMachineSatiety + sceneSatiety < maxCatSatiety)
        {
            ClearFoodTable();
            StopSpawnFood();
            OnLose?.Invoke(true);
            FindObjectOfType<PauseController>().enabled = false;
        }
        
        if (_currentFoodMachineSatiety > 0)
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
            FindObjectOfType<PauseController>().enabled = false;
        }
        else
        {
            _isFoodOnSceneEnd = true;
        }
    }

    private void AddFoodToList(GameObject food)
    {
        if(food.GetComponent<Collectible>().mealStats.isFood)
            foodOnScene.Add(food);
        else
            bombsOnScene.Add(food);
    }
    private void RemoveFoodFromList(GameObject food)
    {
        if (food.GetComponent<Collectible>().mealStats.isFood)
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
        OnTouch.OnCollect += AddCatSatiety;
        OnTouch.OnCollect += RemoveFoodFromList;
        OnTouch.CheckForLose += CheckForLoseCondition;
        OnTouch.OnDestroyObject += RemoveFoodFromList;
        OnTouch.OnTrippleSpawn += AddFoodToList;
        FoodSpawner.OnMealSpawned += DecreaseFoodMachineSatiety;
        FoodSpawner.OnMealAddOnScene += AddFoodToList;
        FoodSpawner.OnBadMealSpawned += DecreaseBadMealSatiety;
    }
    
    private void OnDisable()
    {
        OnTouch.OnCollect -= AddCatSatiety;
        OnTouch.OnCollect -= RemoveFoodFromList;
        OnTouch.CheckForLose -= CheckForLoseCondition;
        OnTouch.OnDestroyObject -= RemoveFoodFromList;
        OnTouch.OnTrippleSpawn -= AddFoodToList;
        FoodSpawner.OnMealSpawned -= DecreaseFoodMachineSatiety;
        FoodSpawner.OnMealAddOnScene -= AddFoodToList;
        FoodSpawner.OnBadMealSpawned -= DecreaseBadMealSatiety;
    }
}
