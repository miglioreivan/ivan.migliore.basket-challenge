using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    // Component references
    [Header("Ball References")]
    [SerializeField] private Rigidbody ball;
    
    [Header("Target References")]
    [SerializeField] private Transform hoop;
    [SerializeField] private Transform backHoop;
    [SerializeField] private Transform backboard;
    [SerializeField] private Transform frontHoop;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image perfectZone;
    [SerializeField] private Image backboardZone;
    [SerializeField] private Image yellowZoneBottom;
    [SerializeField] private Image yellowZoneTop;

    [Header("Settings")]
    [SerializeField] private float h = 2.8f;
    [SerializeField] private float gravity = -18f;

    // References to other scripts
    [Header("Dependencies")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BallCollisionHandler collisionHandler;

    public Transform GetHoop => hoop;
    public Transform CurrentTarget { get; private set; }
    
    private float minPerfect, maxPerfect;
    private float minBackboard, maxBackboard;
    private float minYellow, maxYellow;
    
    public int Score { get; private set; }

    void Start()
    {
        Score = 0;
        CurrentTarget = hoop;
        
        // Subscribe to the event to notify the end of the drag.
        InputManager.OnDragEnded += CheckZone;
    }

    void OnDestroy()
    {
        // Remove the subscription to prevent memory leaks.
        InputManager.OnDragEnded -= CheckZone;
    }

    // Sets the ball's position and the launch zones on the UI.
    public void SetBallAndZones(Transform pos)
    {
        // Reset slider value
        inputManager.ResetSlider();
        
        // Calculate distance and set UI zones...
        Vector3 distXZ = new Vector3(pos.position.x - ball.position.x, 0, pos.position.z - ball.position.z);
        float distanceMagnitude = Mathf.Clamp01(distXZ.magnitude / 10f);
        
        // Logic for the zones...
        minPerfect = Mathf.Clamp01(distanceMagnitude - 0.07f);
        maxPerfect = Mathf.Clamp01(distanceMagnitude + 0.07f);
        perfectZone.rectTransform.anchorMin = new Vector2(0f, minPerfect);
        perfectZone.rectTransform.anchorMax = new Vector2(1f, maxPerfect);
        perfectZone.rectTransform.sizeDelta = new Vector2(0f, 0f);

        minBackboard = (maxPerfect + 0.1f);
        maxBackboard = (minBackboard+0.1f);
        backboardZone.rectTransform.anchorMin = new Vector2(0f, minBackboard);
        backboardZone.rectTransform.anchorMax = new Vector2(1f, maxBackboard);
        backboardZone.rectTransform.sizeDelta = new Vector2(0f,0f);
        
        minYellow = Mathf.Clamp01(distanceMagnitude - 0.12f);
        maxYellow = Mathf.Clamp01(distanceMagnitude + 0.12f);
        yellowZoneBottom.rectTransform.anchorMin = new Vector2(0f, minYellow);
        yellowZoneBottom.rectTransform.anchorMax = new Vector2(1f, minPerfect);
        yellowZoneBottom.rectTransform.sizeDelta = new Vector2(0f, 0f);
        yellowZoneTop.rectTransform.anchorMin = new Vector2(0f, maxPerfect);
        yellowZoneTop.rectTransform.anchorMax = new Vector2(1f, maxYellow);
        yellowZoneTop.rectTransform.sizeDelta = new Vector2(0f, 0f);
        
        // Move the ball and re-enable dragging.
        ball.position = pos.position;
        Debug.Log(inputManager.CanDrag = true);
    }

    // Handles the ball's launch
    public void CheckZone(float t)
    {
        // Immediately disable dragging to prevent multiple launches.
        inputManager.CanDrag = false;

        // Set the height 'h' based on the power ratio 't'
        h = (t < 0.30f) ? 2f :
            (t < 0.50f) ? 2.1f   :
            (t < 0.59f) ? 2.5f : 2.9f;
        
        // Check which launch zone the power ratio falls into
        bool isPerfectRange = t >= minPerfect && t <= maxPerfect;
        bool isFrontRange   = t >= minYellow && t < minPerfect;
        bool isBackRange    = t > maxPerfect && t <= maxYellow;
        bool isBoardRange   = t >= minBackboard && t <= maxBackboard;

        if (isPerfectRange)
        {
            CurrentTarget = hoop;
            LaunchBall();
        }
        else if (isFrontRange || isBackRange || isBoardRange)
        {
            if (isFrontRange)
            {
                collisionHandler.SetShotType(ShotType.FrontRim);
                CurrentTarget = frontHoop;
            }
            else if (isBackRange)
            {
                collisionHandler.SetShotType(ShotType.BackRim);
                CurrentTarget = backHoop;
            }
            else
            {
                collisionHandler.SetShotType(ShotType.Backboard);
                CurrentTarget = backboard;
            }
            
            LaunchBall();
            h = 0f;
        }
        else
        {
            if (t > maxBackboard) h = 1.5f;
            LaunchBall();
            
            // Add a small random error.
            Vector3 error = (t < minPerfect) ? AddError(-ball.velocity.normalized) : AddError(ball.velocity.normalized);
            ball.velocity += error;
        }
    }
    
    public void LaunchBall()
    {
        Physics.gravity = Vector3.up * gravity;
        ball.useGravity = true;
        Vector3 vel = CalculateLaunchData().initialVelocity;

        if (float.IsNaN(vel.x) || float.IsNaN(vel.y) || float.IsNaN(vel.z))
        {
            Debug.LogWarning("Calculated velocity is NaN. Launch cancelled.");
            return;
        }
        
        ball.velocity = vel;
    }
    
    public Vector3 AddError(Vector3 dir)
    {
        float angleError = Random.Range(-5f, 5f);
        return Quaternion.Euler(0, angleError, 0) * dir;
    }

    LaunchData CalculateLaunchData()
    {
        float displacementY = CurrentTarget.position.y - ball.position.y;
        Vector3 displacementXZ = new Vector3(CurrentTarget.position.x - ball.position.x, 0, CurrentTarget.position.z - ball.position.z);
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;
        Vector3 initialVelocity = velocityXZ + velocityY;
    
        return new LaunchData(initialVelocity * -Mathf.Sign(gravity), time);
    }
    
    public void AddPoints(int p)
    {
        Score += p;
        pointsText.SetText(Score + " pt");
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