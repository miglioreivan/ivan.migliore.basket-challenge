using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    // Component references
    // Header for organizing Ball-related fields in the Inspector.
    [Header("Ball References")]
    [SerializeField] private Rigidbody ball;
    
    // Header for organizing Target-related fields in the Inspector.
    [Header("Target References")]
    [SerializeField] private Transform hoop;
    [SerializeField] private Transform backHoop;
    [SerializeField] private Transform backboard;
    [SerializeField] private Transform frontHoop;

    // Header for organizing UI-related fields in the Inspector.
    [Header("UI Elements")]
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text pointsTextFeedback;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image perfectZone;
    [SerializeField] private Image backboardZone;
    [SerializeField] private Image yellowZoneBottom;
    [SerializeField] private Image yellowZoneTop;
    [SerializeField] private TMP_Text bonusText;

    // Header for organizing general settings in the Inspector.
    [Header("Settings")]
    [SerializeField] private float h = 2.8f;
    [SerializeField] private float gravity = -18f;

    // References to other scripts
    // Header for organizing dependency fields in the Inspector.
    [Header("Dependencies")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private BallCollisionHandler collisionHandler;

    // Public property to get the hoop transform.
    public Transform GetHoop => hoop;
    // Public property to get the current target for the ball.
    public Transform CurrentTarget { get; private set; }
    
    // Variables for defining the perfect shot zone on the slider.
    private float minPerfect, maxPerfect;
    // Variables for defining the backboard shot zone on the slider.
    private float minBackboard, maxBackboard;
    // Variables for defining the yellow shot zones on the slider.
    private float minYellow, maxYellow;
    
    // Public property to get the current score.
    public int Score { get; private set; }

    // Public property to check if a bonus is active.
    public bool BonusActive { get; private set; }
    // Public property to get the number of bonus points.
    public int BonusPoints { get; private set; }

    void Start()
    {
        // Initializes the score and sets the initial target.
        Score = 0;
        CurrentTarget = hoop;
        // Hides the points feedback text at the start.
        pointsTextFeedback.gameObject.SetActive(false);
        
        // Subscribes to the OnDragEnded event from the InputManager.
        InputManager.OnDragEnded += CheckZone;
        // Hides the bonus text if it exists.
        if (bonusText)
        {
            bonusText.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Unsubscribes from the event to prevent memory leaks.
        InputManager.OnDragEnded -= CheckZone;
    }

    // Sets the ball's position and adjusts the slider's colored zones based on distance.
    public void SetBallAndZones(Transform pos)
    {
        // Resets the input slider.
        inputManager.ResetSlider();
        // Hides the points feedback.
        pointsTextFeedback.gameObject.SetActive(false);

        // Calculates the horizontal distance to the hoop and normalizes it.
        Vector3 distXZ = new Vector3(CurrentTarget.position.x - pos.position.x, 0, CurrentTarget.position.z - pos.position.z);
        float distanceMagnitude = Mathf.Clamp01(distXZ.magnitude / 10f);

        // Sets the ball's position.
        ball.position = pos.position;
        
        // Defines the perfect zone on the slider and sets its anchor positions.
        minPerfect = Mathf.Clamp01(distanceMagnitude - 0.07f);
        maxPerfect = Mathf.Clamp01(distanceMagnitude + 0.07f);
        perfectZone.rectTransform.anchorMin = new Vector2(0f, minPerfect);
        perfectZone.rectTransform.anchorMax = new Vector2(1f, maxPerfect);
        perfectZone.rectTransform.sizeDelta = new Vector2(0f, 0f);

        // Defines the backboard zone on the slider and sets its anchor positions.
        minBackboard = (maxPerfect + 0.1f);
        maxBackboard = (minBackboard + 0.1f);
        backboardZone.rectTransform.anchorMin = new Vector2(0f, minBackboard);
        backboardZone.rectTransform.anchorMax = new Vector2(1f, maxBackboard);
        backboardZone.rectTransform.sizeDelta = new Vector2(0f, 0f);

        // Defines the yellow zones on the slider and sets their anchor positions.
        minYellow = Mathf.Clamp01(distanceMagnitude - 0.12f);
        maxYellow = Mathf.Clamp01(distanceMagnitude + 0.12f);
        yellowZoneBottom.rectTransform.anchorMin = new Vector2(0f, minYellow);
        yellowZoneBottom.rectTransform.anchorMax = new Vector2(1f, minPerfect);
        yellowZoneBottom.rectTransform.sizeDelta = new Vector2(0f, 0f);
        yellowZoneTop.rectTransform.anchorMin = new Vector2(0f, maxPerfect);
        yellowZoneTop.rectTransform.anchorMax = new Vector2(1f, maxYellow);
        yellowZoneTop.rectTransform.sizeDelta = new Vector2(0f, 0f);
        
        // Logs a warning if the slider isn't set up correctly.
        if (minYellow == 0.0f)
        {
            Debug.LogWarning("The slider was not set up correctly.");
        }

        // Enables dragging for the next shot.
        inputManager.CanDrag = true;

        // Activates a random bonus at the beginning of a new shot if one isn't already active.
        if (!BonusActive)
        {
            CheckAndActivateRandomBonus();
        }
    }

    // Checks the slider value to determine the shot type and launches the ball.
    public void CheckZone(float t)
    {
        // Disables dragging while the ball is being launched.
        inputManager.CanDrag = false;

        // Adjusts the height parameter based on the slider value.
        h = (t < 0.30f) ? 2f :
            (t < 0.50f) ? 2.1f   :
            (t < 0.59f) ? 2.5f : 2.9f;
        
        // Boolean checks to determine which zone the shot falls into.
        bool isPerfectRange = t >= minPerfect && t <= maxPerfect;
        bool isFrontRange   = t >= minYellow && t < minPerfect;
        bool isBackRange    = t > maxPerfect && t <= maxYellow;
        bool isBoardRange   = t >= minBackboard && t <= maxBackboard;

        // Sets the target and launches the ball based on the selected zone.
        if (isPerfectRange)
        {
            CurrentTarget = hoop;
            LaunchBall();
        }
        else if (isFrontRange || isBackRange || isBoardRange)
        {
            // Sets the target and shot type for rim or backboard shots.
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
            // Resets height parameter for specific shots.
            h = 0f;
        }
        else
        {
            // Applies a height parameter and error for out-of-zone shots.
            if (t > maxBackboard) h = 1.5f;
            LaunchBall();
            
            Vector3 error = (t < minPerfect) ? AddError(-ball.velocity.normalized) : AddError(ball.velocity.normalized);
            ball.velocity += error;
        }
    }
    
    // Launches the ball by setting its velocity and enabling gravity.
    public void LaunchBall()
    {
        // Sets the global physics gravity and enables gravity for the ball.
        Physics.gravity = Vector3.up * gravity;
        ball.useGravity = true;
        // Calculates and retrieves the initial velocity.
        Vector3 vel = CalculateLaunchData().initialVelocity;

        // Prevents the ball from being launched if velocity is invalid.
        if (float.IsNaN(vel.x) || float.IsNaN(vel.y) || float.IsNaN(vel.z))
        {
            Debug.LogWarning("Calculated velocity is NaN. Launch cancelled.");
            return;
        }
        
        // Applies the calculated velocity to the ball.
        ball.velocity = vel;
    }
    
    // Adds a random directional error to the ball's velocity.
    public Vector3 AddError(Vector3 dir)
    {
        // Generates a random angle for error.
        float angleError = Random.Range(-5f, 5f);
        // Rotates the direction vector by the random angle.
        return Quaternion.Euler(0, angleError, 0) * dir;
    }

    // Calculates the initial launch velocity and time to hit the target.
    LaunchData CalculateLaunchData()
    {
        // Calculates the vertical and horizontal displacement to the target.
        float displacementY = CurrentTarget.position.y - ball.position.y;
        Vector3 displacementXZ = new Vector3(CurrentTarget.position.x - ball.position.x, 0, CurrentTarget.position.z - ball.position.z);
        // Calculates the total time of flight using projectile motion formulas.
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        // Calculates the vertical and horizontal components of the initial velocity.
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;
        // Combines the components to get the total initial velocity.
        Vector3 initialVelocity = velocityXZ + velocityY;
    
        // Returns the calculated launch data.
        return new LaunchData(initialVelocity * -Mathf.Sign(gravity), time);
    }
    
    // Adds points to the score and updates the UI.
    public void AddPoints(int p, ShotType shotType)
    {
        int pointsToAdd;
        // Checks if a bonus is active and the shot type is a backboard shot.
        if (BonusActive && shotType == ShotType.Backboard)
        {
            // Adds bonus points and deactivates the bonus.
            pointsToAdd = BonusPoints;
            DeactivateBonus();
        } else pointsToAdd = p;
        
        // Increments the score.
        Score += pointsToAdd;
        
        // Updates the points feedback text and makes it visible.
        pointsTextFeedback.SetText("+"+pointsToAdd);
        pointsTextFeedback.gameObject.SetActive(true);
        // Updates the main score text.
        pointsText.SetText(Score + " pt");
    }
    
    // Randomly activates a bonus for a backboard shot.
    private void CheckAndActivateRandomBonus()
    {
        // Activates the bonus with a 25% chance.
        if (Random.Range(0, 100) < 25)
        {
            BonusActive = true;
            // Defines the possible bonus point values.
            int[] bonusOptions = { 4, 6, 8 };
            // Selects a bonus rarity based on a random number.
            int rarity = Random.Range(0, 100);
            // Sets the bonus points based on rarity.
            BonusPoints = (rarity < 50) ? bonusOptions[0] :
                (rarity < 80) ? bonusOptions[1] : bonusOptions[2]; 
            
            // Updates and displays the bonus text if it's available.
            if (bonusText != null)
            {
                bonusText.gameObject.SetActive(true);
                bonusText.text = "+" + BonusPoints + " pt!";
            }
        }
    }
    
    // Deactivates the current bonus and hides the UI text.
    private void DeactivateBonus()
    {
        // Resets bonus-related variables.
        BonusActive = false;
        BonusPoints = 0;
        // Hides the bonus text.
        if (bonusText != null)
        {
            bonusText.gameObject.SetActive(false);
        }
    }

    // Draws the projected trajectory of the ball.
    public void DrawPath()
    {
        // Calculates the launch data for the trajectory.
        LaunchData launchData = CalculateLaunchData();
        Vector3 prevDrawPoint = ball.position;
        int resolution = 30;
        
        // Loops to calculate and draw a series of points along the trajectory.
        for (int i = 0; i <= resolution; i++)
        {
            // Calculates the position of the ball at a specific simulation time.
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * (gravity * simulationTime * simulationTime) / 2f;
            Vector3 drawPoint = ball.position + displacement;
            // Draws a line segment for the trajectory.
            Debug.DrawLine(prevDrawPoint, drawPoint, Color.green);
            prevDrawPoint = drawPoint;
        }
    }

    // Struct to hold the initial velocity and time for a launched object.
    struct LaunchData
    {
        // The initial velocity vector.
        public readonly Vector3 initialVelocity;
        // The time it takes to reach the target.
        public readonly float timeToTarget;

        // Constructor for the LaunchData struct.
        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }
    }
}