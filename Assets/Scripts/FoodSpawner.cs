using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodSpawner : MonoBehaviour
{
    public delegate void OnMealTouched(int satiety);
    public delegate void BadMealCalculation(int satiety);
    public static event OnMealTouched OnMealSpawned;
    public static event BadMealCalculation OnBadMealSpawned;

    private FoodSupplyManager _foodSupplyManager;
    private GameObject _mealParent;
    private const float BaseForceMultiplier = 4f;

    [SerializeField] private Vector2 forceVector = new Vector2(.2f,3f);
    private const float XLowerLimit = 0f;
    private const float XUpperLimit = 1.5f;
    
    private const float YLowerLimit = 2f;
    private const float YUpperLimit = 4f;

    public bool isOppositeSpawn;

    private const float DelayForSpawn = 1f;
    private const float RepeatRate = 1.5f;

    public const int CommonMealChanceToSpawn = 40;
    public const int RareMealChanceToSpawn = 60;
    public const int MythMealChanceToSpawn = 75;
    private const int LegendMealChanceToSpawn = 80;

    public int minRandomValue = -20;
    public int maxRandomValue;
    
    private void Start()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        maxRandomValue = LegendMealChanceToSpawn;
        _mealParent = transform.GetChild(0).gameObject;
        InvokeRepeating(nameof(SpawnFood),DelayForSpawn, RepeatRate);
    }

    private void SpawnFood()
    {
        var randomMealValue = Random.Range(minRandomValue, maxRandomValue);
        Debug.Log(randomMealValue);
        if (randomMealValue >= 0 && randomMealValue <= CommonMealChanceToSpawn )
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[0]);
        }
        else if ( randomMealValue > 0 && randomMealValue <= RareMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[1]);
        }
        else if (randomMealValue > 0 && randomMealValue <= MythMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[2]);
        }
        else if (randomMealValue > 0 && randomMealValue <= LegendMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[3]);
        }
        else
        {
            if (!_foodSupplyManager.isBadMealNotEmpty)
            {
                minRandomValue = 0;
                return;
            }
            FoodInstantiate(_foodSupplyManager.badMealPrefab);
        }
    }

    private int CheckFoodMachineSatiety(int indexOfObjectToSpawn, int foodSatiety)
    {
        Debug.Log(_foodSupplyManager.currentFoodMachineSatiety +">=" + foodSatiety);
        if (_foodSupplyManager.currentFoodMachineSatiety >= foodSatiety) return indexOfObjectToSpawn;
        
        return indexOfObjectToSpawn;
    }

    private void FoodInstantiate(GameObject prefab)
    {
        var meal = Instantiate(prefab, _mealParent.transform, false);

        var rb = meal.GetComponent<Rigidbody2D>();
        
        var randomXValue = UnityEngine.Random.Range(XLowerLimit, XUpperLimit);
        var randomYValue = UnityEngine.Random.Range(YLowerLimit, YUpperLimit);
        forceVector.x = isOppositeSpawn? -randomXValue : randomXValue;
        forceVector.y = randomYValue;
        
        rb.AddForce(forceVector * BaseForceMultiplier,ForceMode2D.Impulse);
        
        var satiety = meal.GetComponent<MealData>().mealStats.satiety;
        if (satiety > 0)
        {
            OnMealSpawned?.Invoke(satiety); 
        }
        else
        {
            OnBadMealSpawned?.Invoke(satiety);
        }
    }

    public void StopSpawn()
    {
        CancelInvoke(nameof(SpawnFood));
    }
}
