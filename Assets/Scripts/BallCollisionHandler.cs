using System.Collections.Generic;
using UnityEngine;

// Defines possible shot types for cleaner logic
public enum ShotType { None, Backboard, FrontRim, BackRim }

public class BallCollisionHandler : MonoBehaviour
{
    // Delegate and static event to notify the end of a shot.
    public delegate void ShotEnded();
    public static event ShotEnded OnShotEnded;
    
    [SerializeField] private GameController controller;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private List<Transform> positions;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private FlowManager flowManager;

    private bool ballInBasket = false;
    private bool isPerfectShot = true;
    private ShotType shotType = ShotType.None;
    private int posIndex = 0;
    
    private GameTimer gameTimer;
    private bool gameOverTriggered = false;

    void Start()
    {
        flowManager = FindObjectOfType<FlowManager>();
        gameTimer = FindObjectOfType<GameTimer>();
        
        if (gameTimer)
        {
            // Subscribe to the timer-over event
            GameTimer.OnTimerOver += OnTimerOver;
        }
        
        SetSpherePosition();
    }
    
    void OnDestroy()
    {
        if (gameTimer)
        {
            // Unsubscribe to prevent memory leaks
            GameTimer.OnTimerOver -= OnTimerOver;
        }
    }

    private void OnTimerOver()
    {
        gameOverTriggered = true;
    }
    
    // Sets the shot type based on the launch zone
    public void SetShotType(ShotType type)
    {
        shotType = type;
        isPerfectShot = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        string tag = other.gameObject.tag;
        
        if (tag == "Floor")
        {
            // The ball hit the floor, the shot is over.
            HandleEndOfShot();
        }
        else if (tag == "Backboard" && shotType == ShotType.Backboard)
        {
            isPerfectShot = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        string tag = other.gameObject.tag;

        if (tag == "BALL-IN-BASKET")
        {
            // Add points based on shot type (perfect or not)
            controller.AddPoints(isPerfectShot ? 3 : 2);
            ballInBasket = true;
        }
    }
    
    // Handles the logic at the end of a shot
    private void HandleEndOfShot()
    {
        // Notify the camera that the shot has ended.
        OnShotEnded?.Invoke();
        
        if (gameOverTriggered)
        {
            SetSpherePosition();
            StartCoroutine(flowManager.ShowScore(controller.Score, "RewardScene"));
        }
        else
        {
            SetSpherePosition();
        }
    }
    
    // Resets the ball's position and state for a new shot
    public void SetSpherePosition()
    {
        isPerfectShot = true;
        shotType = ShotType.None;
        
        if (ballInBasket)
        {
            posIndex++;
            ballInBasket = false;
        }
        
        // Loop back to the start if all positions have been used
        if (posIndex >= positions.Count) posIndex = 0;
        
        // Set the ball's position and the UI zones
        controller.SetBallAndZones(positions[posIndex]);
        
        // Reset ball physics
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}