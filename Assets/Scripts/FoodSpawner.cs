using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
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

    private const float MinDelayForSpawn = 1f;
    private const float MaxDelayForSpawn = 3f;
    private const float RepeatRate = 2f;

    public const int CommonMealChanceToSpawn = 40;
    public const int RareMealChanceToSpawn = 60;
    public const int MythMealChanceToSpawn = 75;
    private const int LegendMealChanceToSpawn = 80;
    
    public const int CommonBadMealChanceToSpawn = -12;
    public const int RareBadMealChanceToSpawn = -18;
    private const int BombBadMealChanceToSpawn = -20;

    public int minRandomValue;
    public int maxRandomValue;
    [Header("Broken MachineEvent")]
    public bool isBrokenMachineEventEnabled = true;
    public bool _isBroken;
    public Material litDefaultMaterial;
    public Material brokenRedMaterial;
    private SpriteRenderer _machineSpriteRenderer;

    public int minMachineEventRandomValue = 10;
    public int maxMachineEventRandomValue = 20;

    public AssetReference spriteAtlas;
    //public AssetReference bombAtlas;
    private SpriteAtlas _foodAtlas;
    //private SpriteAtlas _bombAtlas;
    public Sprite[] foodSprites;
    //public Sprite[] bombSprites;
    private AsyncOperationHandle<SpriteAtlas> _foodHandler;
    //private AsyncOperationHandle<SpriteAtlas> _bombHandler;

    private IEnumerator Start()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        _machineSpriteRenderer = GetComponent<SpriteRenderer>();
        
        minRandomValue = BombBadMealChanceToSpawn;
        maxRandomValue = LegendMealChanceToSpawn;

        _mealParent = transform.GetChild(0).gameObject;

        yield return LoadAtlas(out _foodHandler, spriteAtlas);
        LoadSpritesFromAtlas(_foodHandler, out _foodAtlas, out foodSprites);
        
        /*yield return LoadAtlas(out _bombHandler, bombAtlas);
        LoadSpritesFromAtlas(_bombHandler,out _bombAtlas, out bombSprites);*/

        InvokeRepeating(nameof(SpawnFood), RandomTimeForStartFoodSpawn(), RepeatRate);
        if(isBrokenMachineEventEnabled)
            InvokeRepeating(nameof(BrokenMachineEvent), RandomTimeForMachineEvent(), RandomTimeForMachineEvent());
    }

    private object LoadAtlas(out AsyncOperationHandle<SpriteAtlas> handler, AssetReference reference)
    {
        handler = Addressables.LoadAssetAsync<SpriteAtlas>(reference);
        
        return handler;
    }

    private void LoadSpritesFromAtlas(AsyncOperationHandle<SpriteAtlas> handler, out SpriteAtlas atlas, out Sprite[] sprites)
    {
        if (handler.Status == AsyncOperationStatus.Succeeded)
        {
            atlas = handler.Result;
        }
        else
        {
            Addressables.Release(handler);
            atlas = null;
            sprites = null;
            return;
        }
        sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);
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
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[0],false);
            else if (randomMealValue < 0 && randomMealValue >= RareBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[1],false);
            else if (randomMealValue < 0 && randomMealValue >= BombBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[2],false);
        }
    }

    private void FoodInstantiate(AssetReference prefab, bool isFood = true)
    {
        Addressables.LoadAssetAsync<GameObject>(prefab).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (!_isBroken)
                {
                    Addressables.InstantiateAsync(prefab, _mealParent.transform).Completed += instance =>
                    {
                        var foodGo = instance.Result; 
                        foodGo.GetComponent<OnTouch>().handler = instance;
                        
                        if(isFood)
                            SetRandomSprite(foodGo,_foodAtlas, foodSprites);

                        var rb = instance.Result.GetComponent<Rigidbody2D>();

                        var randomXValue = Random.Range(XLowerLimit, XUpperLimit);
                        var randomYValue = Random.Range(YLowerLimit, YUpperLimit);

                        forceVector.x = isOppositeSpawn ? -randomXValue : randomXValue;
                        forceVector.y = randomYValue;

                        rb.AddForce(forceVector * BaseForceMultiplier, ForceMode2D.Impulse);
                    };
                }

                var satiety = handle.Result.GetComponent<MealData>().mealStats.satiety;
                if (satiety > 0)
                {
                    OnMealSpawned?.Invoke(satiety);
                }
                else
                {
                    OnBadMealSpawned?.Invoke(satiety);
                }
            }
            else
            {
                Addressables.Release(handle);
            }
            Addressables.Release(handle);
        };
    }

    private static void SetRandomSprite(GameObject food, SpriteAtlas atlas, Sprite[] sprites)
    {
        if(atlas == null) return;
         
        var randomSpriteIndex = Random.Range(0, atlas.spriteCount - 1);
        var sprite = sprites[randomSpriteIndex];
        
        food.GetComponent<SpriteRenderer>().sprite = sprite;
        var particle = food.GetComponentInChildren<ParticleSystem>();
        var shapeOfParticle = particle.shape;
        shapeOfParticle.sprite = sprite;
    
        var emitter = particle.emission;
        emitter.enabled = true;
    }
    private static float RandomTimeForStartFoodSpawn()
    {
        var randomTimeValue = Random.Range(MinDelayForSpawn, MaxDelayForSpawn);
        return randomTimeValue;
    }

    public void StopSpawn()
    {
        CancelInvoke(nameof(SpawnFood));
        CancelInvoke(nameof(BrokenMachineEvent));
        
        if(_foodHandler.IsValid())
            Addressables.Release(_foodHandler);
    }
    #endregion
}
