using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    public CircleCollider2D areaToForce;

    public PointEffector2D forceField;

    private void OnMouseDown()
    {
        areaToForce.enabled = true;
        forceField.enabled = true;
        
        Destroy(gameObject, 0.1f);
    }
}
