using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceController : MonoBehaviour
{
    public void Freeze()
    {
        var cat = FindObjectOfType<CatController>();
        cat.CheckForFreeze();
    }
}
