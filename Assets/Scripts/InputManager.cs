using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    // Delegate and event to notify the end of a drag.
    // Defines a delegate for the DragEnded event.
    public delegate void DragEnded(float powerRatio);
    // Static event that other scripts can subscribe to.
    public static event DragEnded OnDragEnded;
    
    // Header for organizing references in the Inspector.
    [Header("References")]
    // Reference to the slider for showing shot power.
    [SerializeField] private Slider powerSlider;
    // Reference to the GameController script.
    [SerializeField] private GameController controller;

    // Header for organizing debug settings.
    [Header("Debug")]
    // Toggles the drawing of the ball's trajectory for debugging.
    [SerializeField] private bool debugPath = true;

    // Header for organizing input parameters.
    [Header("Parameters")]
    // The maximum vertical distance of a drag gesture.
    [SerializeField] private float maxDelta = 300f;
    // Maximum drag duration allowed.
    [SerializeField] private float maxDragTime = 0.6f;
    
    // Variables for the drag state.
    // Indicates if the application is running on a mobile platform.
    private bool isMobile;
    // Stores the starting position of the drag gesture.
    private Vector2 startPos;
    // Flag to track if a drag is currently in progress.
    private bool isDragging;
    // Stores the time when the drag started.
    private float dragStartTime;
    
    // Public property to control whether dragging is allowed.
    public bool CanDrag { get; set; }

    void Start()
    {
        // Checks if the platform is mobile at the start of the game.
        isMobile = Application.isMobilePlatform;
    }
    
    void Update()
    {
        // Only handles input if dragging is currently allowed.
        if (CanDrag)
        {
            HandleInput();
        }

        // Draws the path for debugging if currently dragging.
        if (debugPath && isDragging)
        {
            controller.DrawPath();
        }
    }

    // Handles input from mouse or touch.
    private void HandleInput()
    {
        Vector2 currentPosition;
        bool inputBegan, inputHeld, inputEnded;

        // Determines input based on platform (mobile or desktop).
        if (isMobile)
        {
            if (Input.touchCount == 0) return;
            Touch touch = Input.GetTouch(0);
            currentPosition = touch.position;
            inputBegan = touch.phase == TouchPhase.Began;
            inputHeld = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            inputEnded = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }
        else // Desktop
        {
            currentPosition = Input.mousePosition;
            inputBegan = Input.GetMouseButtonDown(0);
            inputHeld = Input.GetMouseButton(0);
            inputEnded = Input.GetMouseButtonUp(0);
        }

        // Starts the drag when input begins.
        if (inputBegan)
        {
            StartDrag(currentPosition);
        }
        // Updates the drag if input is held.
        else if (isDragging && inputHeld)
        {
            UpdateDrag(currentPosition);
            // Ends the drag if the maximum time has been exceeded.
            if (Time.time - dragStartTime > maxDragTime)
            {
                EndDrag(currentPosition);
            }
        }
        // Ends the drag when input is released.
        else if (isDragging && inputEnded)
        {
            EndDrag(currentPosition);
        }
    }

    // Initializes a drag gesture.
    private void StartDrag(Vector2 pos)
    {
        // Stores the start position and sets the dragging state.
        startPos = pos;
        isDragging = true;
        // Resets the power slider and records the start time.
        powerSlider.value = 0f;
        dragStartTime = Time.time;
    }

    // Updates the power slider's value during the drag.
    private void UpdateDrag(Vector2 inputPos)
    {
        // Updates the slider based on the calculated power ratio.
        powerSlider.value = CalculatePowerRatio(inputPos);
    }

    // Ends the drag gesture and triggers the event.
    private void EndDrag(Vector2 endPos)
    {
        // Resets the dragging state.
        isDragging = false;

        // Calculates the final power ratio.
        float powerRatio = CalculatePowerRatio(endPos);
        powerSlider.value = powerRatio;
        
        // Invokes the OnDragEnded event if the power ratio is valid.
        if (powerRatio > 0)
        {
            OnDragEnded?.Invoke(powerRatio);
        }
    }
    
    // Calculates the power ratio based on the drag distance.
    private float CalculatePowerRatio(Vector2 inputPos)
    {
        // Calculates the vertical distance of the drag.
        float deltaY = inputPos.y - startPos.y;
        
        // Returns 0 if the drag is not a forward motion.
        if (deltaY <= 0) return 0f;
        
        // Clamps the drag distance and calculates the ratio.
        float clampedDelta = Mathf.Clamp(deltaY, 0f, maxDelta);
        return clampedDelta / maxDelta;
    }

    // Resets the power slider to its initial state.
    public void ResetSlider()
    {
        powerSlider.value = 0f;
    }
}