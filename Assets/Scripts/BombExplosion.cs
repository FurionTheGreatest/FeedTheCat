using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BombExplosion : MonoBehaviour
{
    public CircleCollider2D areaToForce;

    public PointEffector2D forceField;

    public GameObject explosionPrefab;

    private Quaternion _explosionRotation;
    private float _maxRotationValue = 60f;

    private void OnMouseDown()
    {
        areaToForce.enabled = true;
        forceField.enabled = true;
        
        _explosionRotation = Quaternion.Euler(0,0,Random.Range(-_maxRotationValue,_maxRotationValue));
        Instantiate(explosionPrefab, transform.position, _explosionRotation);
        
        Destroy(gameObject, 0.3f);
    }
}
