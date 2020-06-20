using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject mealPrefab;
    public GameObject _mealParent;
    private float _baseForceMultiplier = 4f;

    [SerializeField] private Vector2 forceVector = new Vector2(.2f,3f);
    private const float XLowerLimit = 0f;
    private const float XUpperLimit = 1.5f;
    
    private const float YLowerLimit = 2f;
    private const float YUpperLimit = 4f;

    public bool isOppositeSpawn = false;

    private const float DelayForSpawn = 0f;
    private const float RepeatRate = 1f;

    private void Start()
    {
        _mealParent = transform.GetChild(0).gameObject;
        InvokeRepeating(nameof(SpawnFood),DelayForSpawn, RepeatRate);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnFood();
        }
    }

    private void SpawnFood()
    {
        var meal = Instantiate(mealPrefab, _mealParent.transform, false);

        var rb = meal.GetComponent<Rigidbody2D>();
        
        var randomXValue = UnityEngine.Random.Range(XLowerLimit, XUpperLimit);
        var randomYValue = UnityEngine.Random.Range(YLowerLimit, YUpperLimit);
        forceVector.x = isOppositeSpawn? -randomXValue : randomXValue;
        forceVector.y = randomYValue;
        
        rb.AddForce(forceVector * _baseForceMultiplier,ForceMode2D.Impulse);
    }
}
