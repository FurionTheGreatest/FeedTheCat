using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFloatingText : MonoBehaviour
{
    [SerializeField] private float destroyTime;
    private Quaternion _rotation;
    
    private void Start()
    {
        _rotation = gameObject.transform.rotation;
        
        Destroy(gameObject, destroyTime);
    }

    private void LateUpdate()
    {
        transform.rotation = _rotation;
    }
}
