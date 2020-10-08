using System;
using System.Collections;
using System.Threading.Tasks;
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
    private RandomObjectSelector _selector;
    
    [Header("Food sprites")]
    public AssetReference spriteAtlas;
    public Sprite[] foodSprites;
    public static event Action<int> OnMealSpawned;
    public static event Action<GameObject> OnMealAddOnScene;
    public static event Action<int> OnBadMealSpawned;

    [Header("Broken machine event")]
    public bool isBrokenMachineEventEnabled = true;
    public bool _isBroken;
    public Material litDefaultMaterial;
    public Material brokenRedMaterial;

    public int minMachineEventBeginningTime = 10;
    public int maxMachineEventBeginningTime = 20;
    
    [Header("Special options")]
    public GameObject[] sausages;
    [Header("Boost options")]
    public bool isDoubleSatiety;
    public AssetReference doubleSatietyAura;

    private float _doubleSatietyTime = 7f;
    private Coroutine _doubleSatietyRoutine;
    
    private FoodSupplyManager _foodSupplyManager;
    private GameObject _mealParent;

    private const float MinDelayForSpawn = 1f;
    private const float MaxDelayForSpawn = 3f;
    private const float RepeatRate = 2f;

    private SpriteRenderer _machineSpriteRenderer;
    private SpriteAtlas _foodAtlas;
    private AsyncOperationHandle<SpriteAtlas> _foodHandler;
    private const string TripleSausageName = "tripple_sausage(Clone)";
    private GameObject _lastSpawnedGameObject;

    private void Awake()
    {
        _selector = GetComponent<RandomObjectSelector>();
    }

    private IEnumerator Start()
    {
        _foodSupplyManager = FindObjectOfType<FoodSupplyManager>();
        _machineSpriteRenderer = GetComponent<SpriteRenderer>();

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

    public void EnableDoubleSatiety()
    {
        if(_doubleSatietyRoutine != null)
            StopCoroutine(_doubleSatietyRoutine);
        _doubleSatietyRoutine = StartCoroutine(DoubleSatiety());
    }
    private IEnumerator DoubleSatiety()
    {
        isDoubleSatiety = true;
        yield return Yielders.Get(_doubleSatietyTime);
        isDoubleSatiety = false;
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

    public void OnTouch()
    {
        _isBroken = false;
        _machineSpriteRenderer.sharedMaterial = litDefaultMaterial;
    }
    #endregion

    #region SpawnFood
    private void SpawnFood()
    {
        if (_selector.IsGoodObject())
        {
            var index = _selector.SelectObjectFromTable(_selector.goodObjectPercentTable);
            FoodInstantiate(_foodSupplyManager.mealPrefabs[index], _foodSupplyManager.mealSatietyList[index]);
        }
        else
        {
            if (!_foodSupplyManager.isBadMealNotEmpty)
            {
                return;
            }
            var index = _selector.SelectObjectFromTable(_selector.badObjectPercentTable);
            FoodInstantiate(_foodSupplyManager.badMealPrefabs[index], _foodSupplyManager.badMealSatietyList[index]);
        }
    }

    private void FoodInstantiate(AssetReference prefab, int satietyOfFood)
    {
        Addressables.LoadAssetAsync<GameObject>(prefab).Completed += async handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (!_isBroken)
                {
                    PlaySpawnEffect();
                    var instanceHandler = Addressables.InstantiateAsync(prefab, _mealParent.transform);
                    var go = await instanceHandler.Task;

                    _lastSpawnedGameObject = go;
                    OnMealAddOnScene?.Invoke(_lastSpawnedGameObject);

                    _lastSpawnedGameObject.GetComponent<OnTouch>().handler = instanceHandler;
                    if(_lastSpawnedGameObject.GetComponent<Collectible>() != null)
                        _lastSpawnedGameObject.GetComponent<Collectible>().mealStats.satiety = isDoubleSatiety ? satietyOfFood * 2 : satietyOfFood;

                    if (_lastSpawnedGameObject.GetComponent<Collectible>().mealStats.isFood)
                    {
                        SetRandomSprite(_lastSpawnedGameObject,_foodAtlas, foodSprites);
                        
                        if(isDoubleSatiety)
                            SetFoodEffect(doubleSatietyAura,_lastSpawnedGameObject.transform);
                    }

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
                if (spawnEffectHandler.Status != AsyncOperationStatus.Succeeded) return;
                spawnParticle.InstantiateAsync(_mealParent.transform.position, Quaternion.identity);
                Addressables.ReleaseInstance(spawnEffectHandler);
            };
        }
        else
        {
            spawnParticle.InstantiateAsync(_mealParent.transform.position, Quaternion.identity);
        }
    }

    private void SetFoodEffect(AssetReference prefab, Transform parent)
    {
        Addressables.LoadAssetAsync<GameObject>(prefab).Completed += async handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instanceHandler = Addressables.InstantiateAsync(prefab, parent);
                await instanceHandler.Task;
                var aura = instanceHandler.Result;
                var ps = aura.GetComponent<ParticleSystem>();
                var color = ps.main;
                await Task.Delay(1/10);
                var currentColor = aura.GetComponentInParent<VFX>().particleFx.GetComponent<ParticleSystem>().main
                    .startColor.color;
                var alpha = currentColor.a;
                alpha = 1;
                currentColor.a = alpha;

                color.startColor = currentColor;

            }
            Addressables.Release(handle);
        };
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
