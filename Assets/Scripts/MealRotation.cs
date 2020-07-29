using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MealRotation : MonoBehaviour
{
    private int _directionToRotate;
    private float _rotationSpeed;

    private const int MinRotationSpeed = 1;
    private const int MaxRotationSpeed = 2;

    private const float RepeatRateOfRotation = 0.03f;
    private void Start()
    {
        _directionToRotate = Random.Range(-1, 2);
        _rotationSpeed = Random.Range(MinRotationSpeed,MaxRotationSpeed);
        InvokeRepeating(nameof(RotateMeal),0,RepeatRateOfRotation);
    }

    private void RotateMeal()
    {
        gameObject.transform.Rotate(0,0,_directionToRotate * _rotationSpeed);
    }
}
