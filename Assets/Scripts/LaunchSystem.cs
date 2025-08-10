using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaunchSystem : MonoBehaviour
{
    [Header("Ball References")]
    [SerializeField] private List<Transform> positions;
    [SerializeField] private Rigidbody ball;
    [SerializeField] private BallCollisionHandler collisionHandler;
    
    [Header("Target References")]
    [SerializeField] private Transform hoop;
    [SerializeField] private Transform backHoop;
    [SerializeField] private Transform backboard;
    [SerializeField] private Transform frontHoop;

    [Header("UI Elements")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image perfectZone;
    [SerializeField] private Image backboardZone;
    [SerializeField] private Image yellowZoneBottom;
    [SerializeField] private Image yellowZoneTop;

    [Header("Settings")]
    [SerializeField] private float h = 2.8f;
    [SerializeField] private float gravity = -18f;
    
    public Transform GetHoop { get { return hoop; } }

    // PERFECT ZONE VALUE (UI)
    private float minPerfect; // Minimum value on the slider
    private float maxPerfect; // Maximum value on the slider
    
    // BACKBOARD ZONE VALUE (UI)
    private float minBackboard; // Minimum value on the slider
    private float maxBackboard; // Maximum value on the slider
    
    // YELLOW ZONE VALUE (UI)
    private float minYellow;
    private float maxYellow;
    
    private float distanceMagnitude; // Distance betweet ball and basket
    private int posIndex = 0; // Shooting position index
    private bool isMobile; // Device check
    public Transform currentTarget;

    void Awake()
    {
        currentTarget = hoop;
    }

    public void InitLaunch() => SetSpherePosition();

    public void SetZone(Transform pos)
    {
        Vector3 distXZ = new Vector3(pos.position.x - ball.position.x, 0, pos.position.z - ball.position.z);
        distanceMagnitude = Mathf.Clamp01(distXZ.magnitude / 10f);
        
        Debug.Log("Name: " + pos.name + "Basket distance: " + distanceMagnitude);
        
        // Set the anchor for the Perfect Zone
        minPerfect = Mathf.Clamp01(distanceMagnitude-0.07f);
        maxPerfect = Mathf.Clamp01(distanceMagnitude+0.07f);
        perfectZone.rectTransform.anchorMin = new Vector2(0f, minPerfect);
        perfectZone.rectTransform.anchorMax = new Vector2(1f, maxPerfect);
        perfectZone.rectTransform.sizeDelta = new Vector2(0f, 0f);

        // Set the anchor for the Backboard Zone
        minBackboard = (maxPerfect + 0.1f);
        maxBackboard = (minBackboard+0.1f);
        backboardZone.rectTransform.anchorMin = new Vector2(0f, minBackboard);
        backboardZone.rectTransform.anchorMax = new Vector2(1f, maxBackboard);
        backboardZone.rectTransform.sizeDelta = new Vector2(0f,0f);
        
        // Set the anchor for the Yellow Zone
        minYellow = Mathf.Clamp01(distanceMagnitude - 0.12f);
        maxYellow = Mathf.Clamp01(distanceMagnitude + 0.12f);
        yellowZoneBottom.rectTransform.anchorMin = new Vector2(0f, minYellow);
        yellowZoneBottom.rectTransform.anchorMax = new Vector2(1f, minPerfect);
        yellowZoneBottom.rectTransform.sizeDelta = new Vector2(0f, 0f);
        yellowZoneTop.rectTransform.anchorMin = new Vector2(0f, maxPerfect);
        yellowZoneTop.rectTransform.anchorMax = new Vector2(1f, maxYellow);
        yellowZoneTop.rectTransform.sizeDelta = new Vector2(0f, 0f);
    }

    public void CheckZone(float t)
    {
        h = (t < 0.30f) ? 1.9f :
            (t < 0.50f) ? 2f   :
            (t < 0.59f) ? 2.4f : 2.8f;
        
        bool isPerfectRange = t >= minPerfect && t <= maxPerfect;
        bool isFrontRange   = t >= minYellow && t < minPerfect;
        bool isBackRange    = t > maxPerfect && t <= maxYellow;
        bool isBoardRange   = t >= minBackboard && t <= maxBackboard;

        if (isPerfectRange)
        {
            currentTarget = hoop;
            PerfectShot();
        }
        else if (isFrontRange || isBackRange || isBoardRange)
        {
            if (isFrontRange)
            {
                collisionHandler.isBasket = true;
                currentTarget = frontHoop;
            }
            else if (isBackRange)
            {
                collisionHandler.isBasket = true;
                currentTarget = backHoop;
            }
            else
            {
                collisionHandler.isBackboard = true;
                currentTarget = backboard;
            }
            
            PerfectShot();
            h = 0f;
        }
        else
        {
            PerfectShot();
            if (t < minPerfect)
                ball.linearVelocity -= AddError(ball.linearVelocity.normalized);
            else if (t > maxPerfect)
                ball.linearVelocity += AddError(ball.linearVelocity.normalized);
        }

    }

    public void PerfectShot()
    {
        Physics.gravity = Vector3.up * gravity;
        ball.useGravity = true;
        Debug.Log(ball.linearVelocity = CalculateLaunchData().initialVelocity);
    }

    public Vector3 AddError(Vector3 dir)
    {
        float angleError = Random.Range(-Mathf.PI, Mathf.PI);
        return Quaternion.Euler(0, angleError, 0) * dir;
    }
    
    public void SetSpherePosition()
    {
        ball.useGravity = false;

        if (posIndex >= positions.Count)
            posIndex = 0;

        SetZone(positions[posIndex]);
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.position = positions[posIndex].position;
        
        posIndex++;
    }
    
    LaunchData CalculateLaunchData()
    {
        float displacementY = currentTarget.position.y - ball.position.y;
        Vector3 displacementXZ = new Vector3(currentTarget.position.x - ball.position.x, 0, currentTarget.position.z - ball.position.z);
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;
        Vector3 initialVelocity = velocityXZ + velocityY;
    
        return new LaunchData(initialVelocity * -Mathf.Sign(gravity), time);
    }
    
    public void DrawPath()
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
