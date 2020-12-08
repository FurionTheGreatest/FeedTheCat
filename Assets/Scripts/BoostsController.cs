using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostsController : MonoBehaviour
{
    private Animator _animator;
    private readonly int _isOpenHash = Animator.StringToHash("isBoostOpen");
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void ChangeBoostsState(bool isOpen)
    {
        _animator.SetBool(_isOpenHash, isOpen);
    }

    public void DoubleSatiety()
    {
        var spawners = FindObjectsOfType<FoodSpawner>();
        foreach (var spawner in spawners)
        {
            spawner.EnableDoubleSatiety();
        }
    }
}
