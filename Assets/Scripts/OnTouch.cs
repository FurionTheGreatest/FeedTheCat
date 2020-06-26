using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;


public class OnTouch : MonoBehaviour
{
    public delegate void OnMealTouched(int satiety);
    public static event OnMealTouched UpdateUi;

    private Transform _catMouth;
    private bool _isClicked;
    private float _step;
    private const float Speed = 4f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private CircleCollider2D _collider2D;

    public int satiety;
    public bool destroyFood = false;
    private const float SpeedOfFadeOut = 3f;

    private void Start()
    {
        _catMouth = GameObject.Find("Mouth").transform;
        _step = Speed * Time.deltaTime;
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<CircleCollider2D>();
        satiety = GetComponent<MealData>().mealStats.satiety;
    }

    public void OnMouseDown()
    {
        _isClicked = true;
        StartCoroutine(ChangeMealAlpha(0.3f));
        UpdateUi?.Invoke(satiety);
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
        if (destroyFood && !_isClicked)
            StartCoroutine(ChangeMealAlpha(0f));
        
        if(!_isClicked) return;
        TranslateMealToMouth();
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
