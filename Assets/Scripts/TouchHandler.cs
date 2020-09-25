using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TouchHandler : MonoBehaviour
{
    public Camera sceneCamera;
    private Vector3 _touchPosWorld;

    private const TouchPhase BeganTouchPhase = TouchPhase.Began;

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == BeganTouchPhase)
        {
            //Debug.Log("Touch");

            //We transform the touch position into word space from screen space and store it.
            _touchPosWorld = sceneCamera.ScreenToWorldPoint(Input.GetTouch(0).position);

            Vector2 touchPosWorld2D = new Vector2(_touchPosWorld.x, _touchPosWorld.y);

            //We now raycast with this information. If we have hit something we can process it.
            RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, transform.forward);

            if (hitInformation.collider == null) return;
            GameObject touchedObject = hitInformation.transform.gameObject;
            touchedObject.GetComponent<Collectible>()?.OnTouch();
            touchedObject.GetComponent<FoodSpawner>()?.OnTouch();
            //Debug.Log("Touched " + touchedObject.transform.name);
        }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Debug.Log("ClickMouse");

            //We transform the touch position into word space from screen space and store it.
            _touchPosWorld = sceneCamera.ScreenToWorldPoint(Input.mousePosition);

            Vector2 touchPosWorld2D = new Vector2(_touchPosWorld.x, _touchPosWorld.y);

            //We now raycast with this information. If we have hit something we can process it.
            RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, transform.forward);

            if (hitInformation.collider == null) return;
            GameObject touchedObject = hitInformation.transform.gameObject;
            touchedObject.GetComponent<Collectible>()?.OnTouch();
            touchedObject.GetComponent<FoodSpawner>()?.OnTouch();
            //Debug.Log("Touched " + touchedObject.transform.name);
        } 
#endif
    }
}
