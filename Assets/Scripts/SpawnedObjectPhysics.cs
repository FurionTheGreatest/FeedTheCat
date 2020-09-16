using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnedObjectPhysics : MonoBehaviour
{
    [SerializeField] private Vector2 forceVector = new Vector2(.2f,3f);
    public bool isSpawnedFromAnotherObject;
    private float _baseForceMultiplier = 4f;

    private const float XLowerLimit = 0f;
    private const float XUpperLimit = 1.5f;
    
    private const float YLowerLimit = 2f;
    private const float YUpperLimit = 4f;
    
    private FoodSpawner _foodSpawner;
    private Rigidbody2D _objRigidBody;


    private void Awake()
    {
        _foodSpawner = GetComponentInParent<FoodSpawner>();
        _objRigidBody = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SpawnedObjectForce();
    }
    public void RejectFood()
    {
        SpawnedObjectForce(3f);
    }

    private void SpawnedObjectForce()
    {
        var randomXValue = Random.Range(XLowerLimit, XUpperLimit);
        var randomYValue = Random.Range(YLowerLimit, YUpperLimit);
        
        if(_foodSpawner != null && !isSpawnedFromAnotherObject)
        {
            forceVector.x = _foodSpawner.isOppositeSpawn ? -randomXValue : randomXValue;
        }
        forceVector.y = randomYValue;
        
        if (isSpawnedFromAnotherObject)
        {
            forceVector.x = Random.Range(-randomXValue ,randomXValue);

            _baseForceMultiplier /= 2f;
        }
        _objRigidBody.bodyType = RigidbodyType2D.Dynamic;

        _objRigidBody.AddForce(forceVector * _baseForceMultiplier, ForceMode2D.Impulse);
    }
    private void SpawnedObjectForce(float decreaseValue)
    {
        var randomXValue = Random.Range(XLowerLimit, XUpperLimit);
        var randomYValue = Random.Range(YLowerLimit, YUpperLimit);
        
        if(_foodSpawner != null && !isSpawnedFromAnotherObject)
        {
            forceVector.x = _foodSpawner.isOppositeSpawn ? -randomXValue : randomXValue;
        }
        forceVector.y = randomYValue;
        
        if (isSpawnedFromAnotherObject)
        {
            forceVector.x = Random.Range(-randomXValue ,randomXValue);

            _baseForceMultiplier /= 2f;
        }
        _objRigidBody.bodyType = RigidbodyType2D.Dynamic;

        _objRigidBody.AddForce(forceVector * _baseForceMultiplier / decreaseValue, ForceMode2D.Impulse);
    }
    
}
