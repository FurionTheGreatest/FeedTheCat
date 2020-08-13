using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR.WSA;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

public class OnTouch : MonoBehaviour
{
    private TouchHandler _touchHandler;
    public delegate void OnMealTouched(int satiety);
    public delegate void OnDelete();
    public static event OnMealTouched UpdateStats;
    public static event OnDelete CheckForLose;

    public bool destroyFood = false;
    [SerializeField] private float nextTimeToUpdate = 0.3f;
    
    public GameObject floatingNumberPrefab;
    public AsyncOperationHandle<GameObject> handler;
    
    public bool isUsingParticle = true;
    public bool isFood = true;
    private int _satiety;
    private Transform _catMouth;
    private bool _isClicked;
    private float _step;
    private const float Speed = 4f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private CircleCollider2D _collider2D;

    private const float SpeedOfFadeOut = 3f;

    private Vector3 _screenPos;

    private const float LowerBound = -60f;

    private float _currentTime = 0f;

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
        _satiety = GetComponent<MealData>().mealStats.satiety;
    }

    public void OnMouseDown()
    {
        _isClicked = true;
        StartCoroutine(BeforeDestroy(0.3f));
        UpdateStats?.Invoke(_satiety);
        if(floatingNumberPrefab != null)
            ActivatePointText();
    }
    
    private void TranslateMealToMouth()
    {
        if(!isFood) return;
        _collider2D.enabled = false;
        _rb.bodyType = RigidbodyType2D.Static;
        transform.position = Vector3.MoveTowards(transform.position, _catMouth.position, _step);
        
        if(Vector3.Distance(transform.position, _catMouth.position) > 0.3f) return;

        Destroy(gameObject);
    }

    private void Update()
    {
        UpdateScreenPosition();

        if (_screenPos.y <= LowerBound)
        {
            Destroy(gameObject);
            CheckForLose?.Invoke();
        }
        if (destroyFood && !_isClicked)
            StartCoroutine(BeforeDestroy(0f));
        
        if(!_isClicked) return;
        TranslateMealToMouth();
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
        if(alpha.a <= 0)
            Destroy(gameObject);
    }

    private void ActivatePointText()
    {
        var prefab = Instantiate(floatingNumberPrefab, transform.position, Quaternion.identity, transform);
        var text = prefab.GetComponentInChildren<TMP_Text>(true);
        text.color = new Color(UnityEngine.Random.Range(0,1f),UnityEngine.Random.Range(0,1f),UnityEngine.Random.Range(0,1f));
        
        if(_satiety > 0)
            text.text = "+" + _satiety;
        else
            text.text = _satiety.ToString();
    }

    private void OnDestroy()
    {
        if(handler.IsValid())
            Addressables.ReleaseInstance(handler);
    }
}
