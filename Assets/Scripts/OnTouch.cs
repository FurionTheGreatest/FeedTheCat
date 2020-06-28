using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class OnTouch : MonoBehaviour
{
    private TouchHandler _touchHandler;
    public delegate void OnMealTouched(int satiety);
    public delegate void OnDestroy();
    public static event OnMealTouched UpdateStats;
    public static event OnDestroy CheckForLose;

    public bool destroyFood = false;
    [SerializeField] private float nextTimeToUpdate = 1f;
    
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

    private void Start()
    {
        _catMouth = GameObject.Find("Mouth").transform;
        _step = Speed * Time.deltaTime;
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        _satiety = GetComponent<MealData>().mealStats.satiety;
        _touchHandler = FindObjectOfType<TouchHandler>();
    }

    public void OnMouseDown()
    {
        _isClicked = true;
        StartCoroutine(ChangeMealAlpha(0.3f));
        UpdateStats?.Invoke(_satiety);
    }
    
    private void TranslateMealToMouth()
    {
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
            StartCoroutine(ChangeMealAlpha(0f));
        
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

    private IEnumerator ChangeMealAlpha(float alphaOfMeal)
    {
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
}
