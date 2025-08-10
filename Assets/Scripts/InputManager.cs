using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private LaunchSystem launchSystem;

    [Header("Debug")] [SerializeField] private bool debugPath = true;

    private bool isMobile;
    private Vector2 startPos;
    private bool dragging;
    private float dragStartTime;
    private float lastPowerRatio;
    private float maxDelta = 300f;
    
    void Start()
    {
        launchSystem.InitLaunch();
    }

    void Update()
    {
        if (isMobile)
            TouchInput();
        else
            MouseInput();

        if (debugPath)
            launchSystem.DrawPath();
    }

    private void MouseInput()
    {
        if(Input.GetMouseButtonDown(0))
            StartDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0) && dragging)
            UpdateDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            EndDrag(Input.mousePosition);
    }

    private void TouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if(touch.phase == TouchPhase.Began)
            StartDrag(touch.position);
        else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && dragging)
            UpdateDrag(touch.position);
        else if (touch.phase == TouchPhase.Ended && dragging)
            EndDrag(touch.position);
    }

    private void StartDrag(Vector2 pos)
    {
        startPos = pos;
        dragStartTime = Time.time;
        dragging = true;
        powerSlider.value = 0f;
    }

    private void UpdateDrag(Vector2 inputPos)
    {
        if (!dragging) return;
        
        float deltaY = inputPos.y - startPos.y;
        if (deltaY <= 0)
        {
            powerSlider.value = 0f;
            return;
        }
        
        float clampedDelta = Mathf.Clamp(deltaY, 0f, maxDelta);
        float powerRatio = clampedDelta / maxDelta;
        
        powerSlider.value = powerRatio;

    }

    private void EndDrag(Vector2 endPos)
    {
        dragging = false;
        float deltaY = endPos.y - startPos.y;
        if (deltaY <= 0)
        {
            powerSlider.value = 0f;
            return;
        }
        
        float clampedDelta = Mathf.Clamp(deltaY, 0f, maxDelta);
        float powerRatio = clampedDelta / maxDelta;
        lastPowerRatio = powerRatio;
        powerSlider.value = lastPowerRatio;

        launchSystem.CheckZone(lastPowerRatio);
    }
    
}