using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    public bool isCollected;
    public UnityEvent onCollectEvent;
    public int touchCounter;

    private int _iceBreakTrigger = Animator.StringToHash("IceBreak");
    public List<Animator> iceAnimators;

    public void Start()
    {
        touchCounter = 0;
    }

    [Serializable] public struct MealDataStats
    {
        //public string name;
        public int satiety;
        public bool isFood;

        public MealDataStats(int satiety, bool isFood)//string name,
        {
            //this.name = name;
            this.satiety = satiety;
            this.isFood = isFood;
        }
    }
    public MealDataStats mealStats;

    private void OnMouseDown()
    {
        if (touchCounter > 0)
        {
            touchCounter--;
            PlayIceBreakEffect();
            return;
        }
        if (touchCounter != 0) return;
        isCollected = true;
        onCollectEvent.Invoke();
    }

    private void PlayIceBreakEffect()
    {
        var animator = iceAnimators[0];
        animator.SetTrigger(_iceBreakTrigger);
        iceAnimators.RemoveAt(0);
    }
}
