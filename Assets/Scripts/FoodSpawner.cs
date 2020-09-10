using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food machine options")]
    
    public bool isOppositeSpawn;
    public AssetReference spawnParticle;
    
    [Header("Food sprites")]
    
    public AssetReference spriteAtlas;
    public Sprite[] foodSprites;
    public static event Action<int> OnMealSpawned;
    public static event Action<GameObject> OnMealAddOnScene;
    public static event Action<int> OnBadMealSpawned;
    
    [Header("Random spawn rarity values")]
    
    public int minRandomValue;
    public int maxRandomValue;
    
    [Header("Broken machine event")]
    
    public bool isBrokenMachineEventEnabled = true;
    public bool _isBroken;
    public Material litDefaultMaterial;
    public Material brokenRedMaterial;

    public int minMachineEventBeginningTime = 10;
    public int maxMachineEventBeginningTime = 20;
    [Header("Special options")]
    public GameObject[] sausages;

    private FoodSupplyManager _foodSupplyManager;
    private GameObject _mealParent;

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

    private SpriteRenderer _machineSpriteRenderer;
    //public AssetReference bombAtlas;
    private SpriteAtlas _foodAtlas;
    //private SpriteAtlas _bombAtlas;
    //public Sprite[] bombSprites;
    private AsyncOperationHandle<SpriteAtlas> _foodHandler;
    private const string TripleSausageName = "tripple_sausage(Clone)";
    private GameObject _lastSpawnedGameObject;

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
        var randomTimeValue = Random.Range(minMachineEventBeginningTime, maxMachineEventBeginningTime);
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
            FoodInstantiate(_foodSupplyManager.mealPrefabs[0], _foodSupplyManager.CommonFoodSatiety);
        }
        else if (randomMealValue > 0 && randomMealValue <= RareMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[1],_foodSupplyManager.RareFoodSatiety);
        }
        else if (randomMealValue > 0 && randomMealValue <= MythMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[2], _foodSupplyManager.MythFoodSatiety);
        }
        else if (randomMealValue > 0 && randomMealValue <= LegendMealChanceToSpawn)
        {
            FoodInstantiate(_foodSupplyManager.mealPrefabs[3],_foodSupplyManager.LegendFoodSatiety);
        }
        else
        {
            if (!_foodSupplyManager.isBadMealNotEmpty)
            {
                minRandomValue = 0;
                return;
            }

            if (randomMealValue < 0 && randomMealValue >= CommonBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[0],_foodSupplyManager.CommonBadFoodSatiety,false);
            else if (randomMealValue < 0 && randomMealValue >= RareBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[1],_foodSupplyManager.RareBadFoodSatiety,false);
            else if (randomMealValue < 0 && randomMealValue >= BombBadMealChanceToSpawn)
                FoodInstantiate(_foodSupplyManager.badMealPrefabs[2],_foodSupplyManager.MythBadFoodSatiety,false);
        }
    }

    private void FoodInstantiate(AssetReference prefab, int satietyOfFood, bool isFood = true)
    {
        Addressables.LoadAssetAsync<GameObject>(prefab).Completed += async handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (!_isBroken)
                {
                    var instanceHandler = Addressables.InstantiateAsync(prefab, _mealParent.transform);
                    var go = await instanceHandler.Task;

                    PlaySpawnEffect();
                    
                    _lastSpawnedGameObject = go;
                    OnMealAddOnScene?.Invoke(_lastSpawnedGameObject);

                    _lastSpawnedGameObject.GetComponent<OnTouch>().handler = instanceHandler;
                    _lastSpawnedGameObject.GetComponent<Collectible>().mealStats.satiety = satietyOfFood;
                    
                    if(isFood)
                        SetRandomSprite(_lastSpawnedGameObject,_foodAtlas, foodSprites);

                    CheckForTripleSausage(_lastSpawnedGameObject);

                }

                var satiety = _lastSpawnedGameObject.GetComponent<Collectible>().mealStats.satiety;
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

    private void PlaySpawnEffect()
    {
        if (!spawnParticle.IsValid())
        {
            spawnParticle.LoadAssetAsync<GameObject>().Completed += spawnEffectHandler =>
            {
                if(spawnEffectHandler.Status == AsyncOperationStatus.Succeeded)
                {
                    spawnParticle.InstantiateAsync(_mealParent.transform.position, Quaternion.identity);
                    Addressables.ReleaseInstance(spawnEffectHandler);
                }
            };
        }
        else
        {
            spawnParticle.InstantiateAsync(_mealParent.transform.position, Quaternion.identity);
        }
    }

    private void CheckForTripleSausage(GameObject prefab)
    {
        if (!prefab.GetComponent<SpriteRenderer>().sprite.name.Equals(TripleSausageName)) return;
        
        Collectible.MealDataStats stats = prefab.GetComponent<Collectible>().mealStats;
        prefab.GetComponent<Collectible>().mealStats.satiety = 0;
        
        OnTouch mealStats = prefab.GetComponent<OnTouch>();
        mealStats.isTripleSausage = true;
        mealStats.stats = stats;
        mealStats.mealParent = _mealParent.transform;
        mealStats.sausagePrefs = sausages;
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
        if(spriteAtlas.IsValid())
            Addressables.Release(spriteAtlas);
    }
    #endregion
}
