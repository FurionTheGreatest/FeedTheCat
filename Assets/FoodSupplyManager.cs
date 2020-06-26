using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSupplyManager : MonoBehaviour
{
    public GameObject mealPrefab;
    public int desiredSatiety;

    private int _currentSatiety;

    private void AddSatiety(int satiety)
    {
        _currentSatiety += satiety;
        CheckForEndCondition();
    }

    private void CheckForEndCondition()
    {
        if (_currentSatiety < desiredSatiety) return;
        var spawns = FindObjectsOfType<FoodSpawner>();
        foreach (var spawn in spawns)
        {
            spawn.StopSpawn();
        }

        var otherFood = GameObject.FindGameObjectsWithTag("Meal");

        foreach (var food in otherFood)
        {
            food.GetComponent<OnTouch>().destroyFood = true;
        }
    }
    
    private void OnEnable()
    {
        OnTouch.UpdateUi += AddSatiety;
    }
    
    private void OnDisable()
    {
        OnTouch.UpdateUi -= AddSatiety;
    }
}
