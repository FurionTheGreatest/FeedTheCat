using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class OnTouch : MonoBehaviour
{
    public delegate void OnMealTouched();
    public static event OnMealTouched UpdateUi;

    private Transform _catMouth;
    private bool _isClicked;
    private float _step;
    private const float Speed = 4f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    private void Start()
    {
        _catMouth = GameObject.Find("Mouth").transform;
        _step = Speed * Time.deltaTime;
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        _isClicked = true;
        ChangeMealAlpha();
        UpdateUi?.Invoke();
    }

    private void TranslateMealToMouth()
    {
        _rb.bodyType = RigidbodyType2D.Static;
        transform.position = Vector3.MoveTowards(transform.position, _catMouth.position, _step);
        
        if(Vector3.Distance(transform.position, _catMouth.position) > 0.3f) return;

        Destroy(gameObject);
    }

    private void Update()
    {
        if(!_isClicked) return;
        TranslateMealToMouth();
    }

    private void ChangeMealAlpha()
    {
        var alpha = _sprite.color;
        alpha.a = 0.3f;
        _sprite.color = alpha;
    }
}
