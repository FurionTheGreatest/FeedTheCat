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

    public bool destroyFood = false;
    [SerializeField] private float nextTimeToUpdate = 0.3f;
    
    public GameObject floatingNumberPrefab;
    public AsyncOperationHandle<GameObject> handler;
    
    public bool isUsingParticle = true;
    public bool isFood = true;

    public bool isAddressablesInstance = false;
    [Header("SpecialOptions")]
    public GameObject[] sausagePrefs;
    public Collectible.MealDataStats stats;
    public Transform mealParent;
    public ParticleSystem _parentParticleSystem;
    public bool isTrippleSausage;
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

    private GameObject _particleSystem;
    private bool _isParticleSystemNotNull;

    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        _touchHandler = FindObjectOfType<TouchHandler>();
    }

    private void Start()
    {
        if (isUsingParticle)
        {
            _particleSystem = gameObject.GetComponentInChildren<ParticleSystem>(true).gameObject;
            _isParticleSystemNotNull = _particleSystem != null;
        }
        _catMouth = GameObject.Find("Mouth").transform;
        _step = Speed * Time.deltaTime;
        
        _parentParticleSystem = GetComponentInChildren<ParticleSystem>();
    }
    
    public void Collect()
    {
        if (isTrippleSausage)
        {
            for (var i = 0; i < sausagePrefs.Length; i++)
            {
                var obj = sausagePrefs[i];
                obj.GetComponent<Collectible>().mealStats = stats;
                FallApart(obj, 5f, 1.5f, i);
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
        if(_isParticleSystemNotNull)
            _particleSystem.SetActive(false);
        
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
    
    private void FallApart(GameObject prefab, float heightForce, float widthForce, int indexOfPrefab)
    {
        GameObject instantiatedObj = Instantiate(prefab, gameObject.transform.position, Quaternion.identity, mealParent);//instantiatedObj.GetComponentInChildren<ParticleSystem>();
        OnTrippleSpawn?.Invoke(instantiatedObj);
        var particle = instantiatedObj.GetComponentInChildren<ParticleSystem>();
        
        var shapeOfParticle = particle.shape;
        shapeOfParticle.sprite = instantiatedObj.GetComponent<SpriteRenderer>().sprite;
        
        var color = particle.main;
        color.startColor = _parentParticleSystem.main.startColor;
        
        var emitter = particle.emission;
        emitter.enabled = true;
        
        var rb = instantiatedObj.GetComponent<Rigidbody2D>();

        Vector2 forceVector;
        forceVector.x = widthForce;
        switch (indexOfPrefab)
        {
            case 0:
                forceVector.x = -widthForce;
                break;
            case 1:
                forceVector.x = 0;
                break;
            case 2:
                forceVector.x = widthForce;
                break;
        }
        forceVector.y = heightForce;

        rb.AddForce(forceVector, ForceMode2D.Impulse);// * BaseForceMultiplier
    }

    private void DestroyObject()
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
