using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FoodSupplyManager : MonoBehaviour
{
    public delegate void CheckLoseCondition(bool isFoodOver);
    public delegate void CheckWinCondition();
    public static event CheckLoseCondition OnLose;
    public static event CheckWinCondition OnWin;
    
    public List<AssetReference> mealPrefabs;
    public int rareFoodSatiety;
    [HideInInspector] public int mythFoodSatiety;
    [HideInInspector] public int legendFoodSatiety;
    
    public List<AssetReference> badMealPrefabs;

    [HideInInspector] public int rareBadFoodSatiety;
    [HideInInspector] public int mythBadFoodSatiety;
    
    public bool isBadMealNotEmpty = true;
    
    public int maxCatSatiety = 100;
    public int maxFoodMachineSatiety;
    
    private int _currentCatSatiety;
    public int currentFoodMachineSatiety;
    
    private int _currentBadMealSatiety;
    private int _maxBadMealSatiety;
    
    private FoodSpawner[] _spawns;

    private bool _isFoodOnSceneEnd;
    private bool _isWon;

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

        LoadFoodSatiety();
    }

    private void LoadFoodSatiety()
    {
        mealPrefabs[1].LoadAssetAsync<GameObject>().Completed +=
            (res) =>
            {
                rareFoodSatiety = res.Result.GetComponent<MealData>().mealStats.satiety;
                Addressables.Release(res);
            };
        mealPrefabs[2].LoadAssetAsync<GameObject>().Completed +=
            (res) =>
            {
                mythFoodSatiety = res.Result.GetComponent<MealData>().mealStats.satiety;
                Addressables.Release(res);
            };
        mealPrefabs[3].LoadAssetAsync<GameObject>().Completed +=
            (res) =>
            {
                legendFoodSatiety = res.Result.GetComponent<MealData>().mealStats.satiety;
                Addressables.Release(res);
            };

        badMealPrefabs[1].LoadAssetAsync<GameObject>().Completed +=
            (res) =>
            {
                rareBadFoodSatiety = res.Result.GetComponent<MealData>().mealStats.satiety;
                Addressables.Release(res);
            };
        badMealPrefabs[2].LoadAssetAsync<GameObject>().Completed +=
            (res) =>
            {
                mythBadFoodSatiety = res.Result.GetComponent<MealData>().mealStats.satiety;
                Addressables.Release(res);
            };
    }

    private void AddCatSatiety(int satiety)
    {
        if(_currentCatSatiety == 0 && satiety < 0) return;
        if (_currentCatSatiety + satiety < 0 && satiety < 0)
        {
            _currentCatSatiety = 0;
            return;
        }
        _currentCatSatiety += satiety;
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
        if (_currentCatSatiety < maxCatSatiety) return;
        StopSpawnFood();
        _isWon = true;
        OnWin?.Invoke();
        
        var otherFood = new List<GameObject>(GameObject.FindGameObjectsWithTag("Meal"));
        var bombs = GameObject.FindGameObjectsWithTag("BadMeal");
        otherFood.AddRange(bombs);
        foreach (var food in otherFood)
        {
            food.GetComponent<CircleCollider2D>().enabled = false;
            food.GetComponent<OnTouch>().destroyFood = true;
            //Todo foodOnScreenLeft +add points
        }
    }

    private void CheckForLoseCondition()
    {
        var otherFood = new List<GameObject>(GameObject.FindGameObjectsWithTag("Meal"));
        int sceneSatiety = 0;
        foreach (var food in otherFood)
        {
            sceneSatiety += food.GetComponent<MealData>().mealStats.satiety;
        }
        if (_currentCatSatiety + currentFoodMachineSatiety + sceneSatiety < maxCatSatiety)
        {
            var bombs = GameObject.FindGameObjectsWithTag("BadMeal");
            otherFood.AddRange(bombs);
            foreach (var food in otherFood)
            {
                food.GetComponent<CircleCollider2D>().enabled = false;
                food.GetComponent<OnTouch>().destroyFood = true;
            }
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
