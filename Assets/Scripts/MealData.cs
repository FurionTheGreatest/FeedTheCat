using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MealDataStats
{
    public int satiety;

    public MealDataStats(int satiety)
    {
        this.satiety = satiety;
    }
}
public class MealData : MonoBehaviour
{
    public MealDataStats mealStats;

}
