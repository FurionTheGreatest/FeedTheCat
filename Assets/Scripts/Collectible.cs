using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    public bool isCollected;
    public static event Action<GameObject> OnCollect;
    public UnityEvent onCollectEvent;
    public int touchCounter = 0;
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
            touchCounter--;
        if (touchCounter != 0) return;
        isCollected = true;
        OnCollect?.Invoke(gameObject);
        onCollectEvent.Invoke();
    }
}
