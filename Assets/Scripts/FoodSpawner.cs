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

    
    public FoodSupplyManager foodSupplyManager;
    private GameObject _mealParent;
    private readonly float _baseForceMultiplier = 4f;

    [SerializeField] private Vector2 forceVector = new Vector2(.2f,3f);
    private const float XLowerLimit = 0f;
    private const float XUpperLimit = 1.5f;
    
    private const float YLowerLimit = 2f;
    private const float YUpperLimit = 4f;

    public bool isOppositeSpawn;

    private const float DelayForSpawn = 1f;
    private const float RepeatRate = 1f;
    private const float BadMealChanceToSpawn = 0.5f;

    private void Start()
    {
        foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        _mealParent = transform.GetChild(0).gameObject;
        InvokeRepeating(nameof(SpawnFood),DelayForSpawn, RepeatRate);
    }

    private void SpawnFood()
    {
        if (foodSupplyManager.isBadMealNotEmpty)
        {
            var randomMealValue = Random.Range(0f, 1f);
            FoodInstantiate(randomMealValue < BadMealChanceToSpawn ? foodSupplyManager.mealPrefab : foodSupplyManager.badMealPrefab);
        }
        else
        {
            FoodInstantiate(foodSupplyManager.mealPrefab);
        }
    }

    private void FoodInstantiate(GameObject prefab)
    {
        var meal = Instantiate(prefab, _mealParent.transform, false);

        var rb = meal.GetComponent<Rigidbody2D>();
        
        var randomXValue = UnityEngine.Random.Range(XLowerLimit, XUpperLimit);
        var randomYValue = UnityEngine.Random.Range(YLowerLimit, YUpperLimit);
        forceVector.x = isOppositeSpawn? -randomXValue : randomXValue;
        forceVector.y = randomYValue;
        
        rb.AddForce(forceVector * _baseForceMultiplier,ForceMode2D.Impulse);
        
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
