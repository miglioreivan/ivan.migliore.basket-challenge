using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class LaunchSystem : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float h = 25;
    [SerializeField] private float gravity = -18;
    [SerializeField] private bool debugPath;
    [SerializeField] private float maxDelta = 200f;
    
    [Header("References")]
    [SerializeField] private Rigidbody ball;
    [SerializeField] private Transform target;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private List<Transform> shotPositions;

    private bool isMobile;
    private Vector2 startPos;
    private bool dragging;
    private int posIndex;
    private float dragStartTime;
    private int totalPositions;
    private float lastPowerRatio;
    
    private void Awake()
    {
        isMobile = Application.isMobilePlatform;
        ball.useGravity = false;
        totalPositions = shotPositions.Count;
        posIndex = 0;
    }

    void Update()
    {
        if (isMobile)
            TouchInput();
        else
            MouseInput();

        if (debugPath)
            DrawPath();
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

        Launch();
    }

    void Launch()
    {
        Physics.gravity = Vector3.up * gravity;
        ball.useGravity = true;
        ball.linearVelocity = CalculateLaunchData().initialVelocity;
    }
    
    LaunchData CalculateLaunchData()
    {
        float displacementY = target.position.y - ball.position.y; // Displacement of the ball from its initial position to the final position.
        Vector3 displacementXZ = new Vector3(target.position.x - ball.position.x, 0, target.position.z - ball.position.z);
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;
        Vector3 initialVelocity = velocityXZ + velocityY;

        initialVelocity *= lastPowerRatio;

        return new LaunchData(initialVelocity * -Mathf.Sign(gravity), time);
    }

    void DrawPath()
    {
        LaunchData launchData = CalculateLaunchData();
        Vector3 prevDrawPoint = ball.position;

        int resolution = 30;
        for (int i = 0; i <= resolution; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * (gravity * simulationTime * simulationTime) / 2f;
            Vector3 drawPoint = ball.position + displacement;
            Debug.DrawLine(prevDrawPoint, drawPoint, Color.green);
            prevDrawPoint = drawPoint;
        }
    }
    
    public void SetSpherePosition()
    {
        ball.useGravity = false;

        if (posIndex < totalPositions - 1)
        {
            posIndex++;
        }
        else
        {
            posIndex = 0;
        }

        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.position = shotPositions[posIndex].position;
    }
    
    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }
    }
    
}