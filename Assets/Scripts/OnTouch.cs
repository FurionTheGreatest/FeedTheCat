using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Vector3 = UnityEngine.Vector3;

public class OnTouch : MonoBehaviour
{
    private TouchHandler _touchHandler;
    public static event Action<GameObject> OnTrippleSpawn;
    public static event Action CheckForLose;
    public static event Action<GameObject> OnDestroyObject;
    public static event Action<GameObject> OnCollect;

    public bool destroyFood;
    [SerializeField] private float nextTimeToUpdate = 0.3f;
    
    public GameObject floatingNumberPrefab;
    public AsyncOperationHandle<GameObject> handler;
    
    public bool isFood = true;

    public bool isAddressablesInstance = false;
    [Header("SpecialOptions")]
    public GameObject[] sausagePrefs;
    public Collectible.MealDataStats stats;
    public Transform mealParent;
    public bool isTripleSausage;
    private Transform _catMouth;
    private float _step;
    private const float Speed = 4f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private CircleCollider2D _collider2D;

    private const float SpeedOfFadeOut = 3f;

    private Vector3 _screenPos;

    private const float LowerBound = -60f;

    private float _currentTime;
    private VFX _vfx;

    private void Awake()
    {
        _vfx = GetComponent<VFX>();
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        _touchHandler = FindObjectOfType<TouchHandler>();
    }

    private void Start()
    {
        _catMouth = GameObject.Find("Mouth").transform;
        _step = Speed * Time.deltaTime;
    }
    
    public void Collect()
    {
        if (isTripleSausage)
        {
            foreach (var obj in sausagePrefs)
            {
                obj.GetComponent<Collectible>().mealStats = stats;
                FallApart(obj);
            }
            DestroyObject();
            return;
        }
        gameObject.tag = "Untagged";
        StartCoroutine(BeforeDestroy(0.3f));
        StartCoroutine(TranslateMealToMouth());
        if(floatingNumberPrefab != null)
            ActivatePointText();
    }
    
    private IEnumerator TranslateMealToMouth()
    {
        if(!GetComponent<Collectible>().mealStats.isFood) yield break;
        _collider2D.enabled = false;
        _rb.bodyType = RigidbodyType2D.Static;

        while(Vector3.Distance(transform.position, _catMouth.position) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _catMouth.position, _step);
            yield return Yielders.FixedUpdate;
        }
        OnCollect?.Invoke(gameObject);

        DestroyObject();
    }

    private void Update()
    {
        UpdateScreenPosition();

        if (_screenPos.y <= LowerBound)
        {
            DestroyObject();
            CheckForLose?.Invoke();
        }

        if (destroyFood)
        {
            StartCoroutine(BeforeDestroy(0f));
        }
    }

    private void UpdateScreenPosition()
    {
        if (nextTimeToUpdate > _currentTime)
        {
            _currentTime += Time.deltaTime;
        }
        else
        {
            _screenPos = _touchHandler.sceneCamera.WorldToScreenPoint(gameObject.transform.position);
            _currentTime = 0;
        }
    }

    private IEnumerator BeforeDestroy(float alphaOfMeal)
    {
        if(_vfx.IsVfxEnabled())
            _vfx.DisableParticleSystem();
        
        var alpha = _sprite.color;
        while (alpha.a >= alphaOfMeal)
        {
            alpha.a -= Time.deltaTime * SpeedOfFadeOut;
            _sprite.color = alpha;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (alpha.a <= 0)
            DestroyObject();
    }

    private void ActivatePointText()
    {
        var prefab = Instantiate(floatingNumberPrefab, transform.position, Quaternion.identity, transform);
        var text = prefab.GetComponentInChildren<TMP_Text>(true);
        text.color = new Color(UnityEngine.Random.Range(0,1f),UnityEngine.Random.Range(0,1f),UnityEngine.Random.Range(0,1f));
        var satiety = GetComponent<Collectible>().mealStats.satiety;
        if(satiety > 0)
            text.text = "+" + satiety;
        else
            text.text = satiety.ToString();
    }
    
    private void FallApart(GameObject prefab)
    {
        GameObject instantiatedObj = Instantiate(prefab, gameObject.transform.position, Quaternion.identity, mealParent);
        instantiatedObj.GetComponent<SpawnedObjectPhysics>().isSpawnedFromAnotherObject = true;
        OnTrippleSpawn?.Invoke(instantiatedObj);
        var particle = instantiatedObj.GetComponentInChildren<ParticleSystem>();
        
        var shapeOfParticle = particle.shape;
        shapeOfParticle.sprite = instantiatedObj.GetComponent<SpriteRenderer>().sprite;
        
        var color = particle.main;
        color.startColor = _vfx.particleFx.GetComponent<ParticleSystem>().main.startColor;
        
        var emitter = particle.emission;
        emitter.enabled = true;
    }

    public void DestroyObject()
    {
        OnDestroyObject?.Invoke(gameObject);
        
        if(isAddressablesInstance)
            Addressables.ReleaseInstance(gameObject);
        else
            Destroy(gameObject);
    }

    /*private void OnDestroy()
    {
        OnDestroyObject?.Invoke(gameObject);
        DestroyObject();
    }*/
}
