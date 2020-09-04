using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class OnTouchSpecial : MonoBehaviour
{
    public MealDataStats stats;
    public GameObject[] sausagePrefs;
    public Transform mealParent;
    public bool destroyFood;
    
    private Vector3 _screenPos;
    [SerializeField] private float nextTimeToUpdate = 0.3f;
    private float _currentTime;
    private const float LowerBound = -60f;
    private TouchHandler _touchHandler;
    private SpriteRenderer _spriteRenderer;
    private const float SpeedOfFadeOut = 3f;
    public ParticleSystem _parentParticleSystem;

    private void Awake()
    {
        _touchHandler = FindObjectOfType<TouchHandler>();
    }
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _parentParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void OnMouseDown()
    {
        for (var i = 0; i < sausagePrefs.Length; i++)
        {
            var obj = sausagePrefs[i];
            obj.GetComponent<MealData>().mealStats = stats;
            Decay(obj, 5f, 1.5f, i);
        }
        Destroy(gameObject);
    }

    private void Decay(GameObject prefab, float heightForce, float widthForce, int indexOfPrefab)
    {
        GameObject instantiatedObj = Instantiate(prefab, gameObject.transform.position, Quaternion.identity, mealParent);//instantiatedObj.GetComponentInChildren<ParticleSystem>();
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
    
    private void Update()
    {
        UpdateScreenPosition();
        if (_screenPos.y <= LowerBound)
        {
            Addressables.ReleaseInstance(gameObject);
        }
        if (destroyFood)
            StartCoroutine(BeforeDestroy(0f));
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
        var alpha = _spriteRenderer.color;
        while (alpha.a >= alphaOfMeal)
        {
            alpha.a -= Time.deltaTime * SpeedOfFadeOut;
            _spriteRenderer.color = alpha;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if(alpha.a <= 0)
            Addressables.ReleaseInstance(gameObject);
    }
}
