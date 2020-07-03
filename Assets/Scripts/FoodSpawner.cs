using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodSpawner : MonoBehaviour
{
    public delegate void OnMealTouched(int satiety);
    public delegate void BadMealCalculation(int satiety);
    public static event OnMealTouched OnMealSpawned;
    public static event BadMealCalculation OnBadMealSpawned;

    private FoodSupplyManager _foodSupplyManager;
    private GameObject _mealParent;
    private const float BaseForceMultiplier = 4f;

    [SerializeField] private Vector2 forceVector = new Vector2(.2f,3f);
    private const float XLowerLimit = 0f;
    private const float XUpperLimit = 1.5f;
    
    private const float YLowerLimit = 2f;
    private const float YUpperLimit = 4f;

    public bool isOppositeSpawn;

    private const float DelayForSpawn = 1f;
    private const float RepeatRate = 1.5f;

    public const int CommonMealChanceToSpawn = 40;
    public const int RareMealChanceToSpawn = 60;
    public const int MythMealChanceToSpawn = 75;
    private const int LegendMealChanceToSpawn = 80;
    
    public const int CommonBadMealChanceToSpawn = -12;
    public const int RareBadMealChanceToSpawn = -18;
    private const int BombBadMealChanceToSpawn = -20;

    public int minRandomValue;
    public int maxRandomValue;

    [SerializeField] private bool _isBroken;
    public Material litDefaultMaterial;
    public Material brokenRedMaterial;
    private SpriteRenderer _machineSpriteRenderer;

    public int minMachineEventRandomValue = 10;
    public int maxMachineEventRandomValue = 20;
    
    private void Start()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        _machineSpriteRenderer = GetComponent<SpriteRenderer>();
        
        minRandomValue = BombBadMealChanceToSpawn;
        maxRandomValue = LegendMealChanceToSpawn;

        _mealParent = transform.GetChild(0).gameObject;
        InvokeRepeating(nameof(SpawnFood),DelayForSpawn, RepeatRate);
        InvokeRepeating(nameof(BrokenMachineEvent), RandomTimeForMachineEvent(), RandomTimeForMachineEvent());
    }

    #region BrokenMachineEvent
    private float RandomTimeForMachineEvent()
    {
        var randomTimeValue = Random.Range(minMachineEventRandomValue, maxMachineEventRandomValue);
        return randomTimeValue;
    }

    private void BrokenMachineEvent()
    {
        _isBroken = true;
        _machineSpriteRenderer.sharedMaterial = brokenRedMaterial;
    }

    private void OnMouseDown()
    {
        _isBroken = false;
        _machineSpriteRenderer.sharedMaterial = litDefaultMaterial;
    }
    #endregion

    #region SpawnFood

private void SpawnFood()
    {
        var randomMealValue = Random.Range(minRandomValue, maxRandomValue);
        if (randomMealValue >= 0 && randomMealValue <= CommonMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[0]);
        }
        else if (randomMealValue > 0 && randomMealValue <= RareMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[1]);
        }
        else if (randomMealValue > 0 && randomMealValue <= MythMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[2]);
        }
        else if (randomMealValue > 0 && randomMealValue <= LegendMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[3]);
        }
        else
        {
            if (!_foodSupplyManager.isBadMealNotEmpty)
            {
                minRandomValue = 0;
                return;
            }

            if (randomMealValue < 0 && randomMealValue >= CommonBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[0]);
            else if (randomMealValue < 0 && randomMealValue >= RareBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[1]);
            else if (randomMealValue < 0 && randomMealValue >= BombBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[2]);
        }
    }

    private void FoodInstantiate(GameObject prefab)
    {
        if (!_isBroken)
        {
            var meal = Instantiate(prefab, _mealParent.transform, false);

            var rb = meal.GetComponent<Rigidbody2D>();

            var randomXValue = Random.Range(XLowerLimit, XUpperLimit);
            var randomYValue = Random.Range(YLowerLimit, YUpperLimit);
            forceVector.x = isOppositeSpawn ? -randomXValue : randomXValue;
            forceVector.y = randomYValue;

            rb.AddForce(forceVector * BaseForceMultiplier, ForceMode2D.Impulse);
        }

        var satiety = prefab.GetComponent<MealData>().mealStats.satiety;
        if (satiety > 0)
        {
            OnMealSpawned?.Invoke(satiety);
        }
        else
        {
            OnBadMealSpawned?.Invoke(satiety);
        }
    }

    public void StopSpawn()
    {
        CancelInvoke(nameof(SpawnFood));
    }
    #endregion
    
}
