using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private Vector3 touchPosWorld;

    TouchPhase touchPhase = TouchPhase.Began;

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == touchPhase)
        {
            Debug.Log("Touch");

            //We transform the touch position into word space from screen space and store it.
            touchPosWorld = _camera.ScreenToWorldPoint(Input.GetTouch(0).position);

            Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);

            //We now raycast with this information. If we have hit something we can process it.
            RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, transform.forward);

            if (hitInformation.collider == null) return;
            GameObject touchedObject = hitInformation.transform.gameObject;
            Debug.Log("Touched " + touchedObject.transform.name);
        }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("ClickMouse");

            //We transform the touch position into word space from screen space and store it.
            touchPosWorld = _camera.ScreenToWorldPoint(Input.mousePosition);

            Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);

            //We now raycast with this information. If we have hit something we can process it.
            RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, transform.forward);

            if (hitInformation.collider == null) return;
            GameObject touchedObject = hitInformation.transform.gameObject;
            Debug.Log("Touched " + touchedObject.transform.name);
        } 
#endif
    }
}
