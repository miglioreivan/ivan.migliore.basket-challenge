using System.Collections.Generic;
using UnityEngine;

// Defines possible shot types for cleaner logic
// An enumeration of different shot types.
public enum ShotType { None, Backboard, FrontRim, BackRim }

public class BallCollisionHandler : MonoBehaviour
{
    // Delegate and static event to notify the end of a shot.
    // A delegate for the shot ended event.
    public delegate void ShotEnded();
    // A static event that other scripts can subscribe to.
    public static event ShotEnded OnShotEnded;
    
    // Reference to the GameController script.
    [SerializeField] private GameController controller;
    // Reference to the ball's Rigidbody component.
    [SerializeField] private Rigidbody rb;
    // A list of positions where the ball can be spawned.
    [SerializeField] private List<Transform> positions;
    // Reference to the InputManager script.
    [SerializeField] private InputManager inputManager;
    // Reference to the FlowManager for scene transitions.
    [SerializeField] private FlowManager flowManager;

    // Flag to check if the ball has gone through the basket.
    private bool ballInBasket = false;
    // Flag to check if the shot was a perfect shot.
    private bool isPerfectShot = true;
    // The current type of shot being performed.
    private ShotType shotType = ShotType.None;
    // The index of the current ball spawn position.
    private int posIndex = 0;
    
    // Reference to the GameTimer script.
    private GameTimer gameTimer;
    // Flag to check if the game is over.
    private bool gameOverTriggered = false;

    void Start()
    {
        // Finds and assigns references to the FlowManager and GameTimer.
        flowManager = FindObjectOfType<FlowManager>();
        gameTimer = FindObjectOfType<GameTimer>();
        
        // Subscribes to the OnTimerOver event if the timer exists.
        if (gameTimer)
        {
            GameTimer.OnTimerOver += OnTimerOver;
        }
        
        // Sets the ball to the initial spawn position.
        SetSpherePosition();
    }
    
    void OnDestroy()
    {
        // Unsubscribes from the timer event to prevent memory leaks.
        if (gameTimer)
        {
            GameTimer.OnTimerOver -= OnTimerOver;
        }
    }

    // Sets the gameOverTriggered flag when the timer runs out.
    private void OnTimerOver()
    {
        gameOverTriggered = true;
    }
    
    // Sets the shot type based on the launch zone.
    public void SetShotType(ShotType type)
    {
        // Sets the shot type and marks it as not perfect.
        shotType = type;
        isPerfectShot = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        // Gets the tag of the object the ball collided with.
        string tag = other.gameObject.tag;
        
        // Calls HandleEndOfShot if the ball hits the floor.
        if (tag == "Floor")
        {
            HandleEndOfShot();
        }
        // If the ball hits the backboard for a backboard shot, it's no longer a perfect shot.
        else if (tag == "Backboard" && shotType == ShotType.Backboard)
        {
            isPerfectShot = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Gets the tag of the object the ball entered.
        string tag = other.gameObject.tag;

        // If the ball enters the "BALL-IN-BASKET" trigger, points are awarded.
        if (tag == "BALL-IN-BASKET")
        {
            // Awards points based on whether it was a perfect shot or not.
            controller.AddPoints(isPerfectShot ? 3 : 2, shotType);
            ballInBasket = true;
        }
    }
    
    // Handles the actions to take when a shot is completed.
    private void HandleEndOfShot()
    {
        // Invokes the OnShotEnded event.
        OnShotEnded?.Invoke();
        
        // Checks if the game is over.
        if (gameOverTriggered)
        {
            // Prepares for the next shot and starts the coroutine to show the final score.
            SetSpherePosition();
            StartCoroutine(flowManager.ShowScore(controller.Score, "RewardScene"));
        }
        else
        {
            // Resets the ball to the next position for a new shot.
            SetSpherePosition();
        }
    }
    
    // Resets the ball's position and state for a new shot.
    public void SetSpherePosition()
    {
        // Resets the shot flags.
        isPerfectShot = true;
        shotType = ShotType.None;
        
        // Moves to the next position index if the ball went in the basket.
        if (ballInBasket)
        {
            posIndex++;
            ballInBasket = false;
        }
        
        // Wraps the position index around to the beginning if it exceeds the list count.
        if (posIndex >= positions.Count) posIndex = 0;

        // Tells the GameController to set the ball's position and update the zones.
        controller.SetBallAndZones(positions[posIndex]);
        
        // Resets the ball's physics state.
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}