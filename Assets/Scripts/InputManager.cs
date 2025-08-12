using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    // Delegate and event to notify the end of a drag.
    public delegate void DragEnded(float powerRatio);
    public static event DragEnded OnDragEnded;
    
    [Header("References")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private GameController controller;

    [Header("Debug")]
    [SerializeField] private bool debugPath = true;

    [Header("Parameters")]
    [SerializeField] private float maxDelta = 300f;
    // Maximum drag duration allowed.
    [SerializeField] private float maxDragTime = 0.6f;
    
    // Variables for the drag state.
    private bool isMobile;
    private Vector2 startPos;
    private bool isDragging;
    // Stores the time when the drag started.
    private float dragStartTime;
    
    public bool CanDrag { get; set; }

    void Start()
    {
        isMobile = Application.isMobilePlatform;
    }
    
    void Update()
    {
        if (CanDrag)
        {
            HandleInput();
        }

        // Draw the path for debugging if currently dragging
        if (debugPath && isDragging)
        {
            controller.DrawPath();
        }
    }

    // Handles input from mouse or touch
    private void HandleInput()
    {
        Debug.Log("Handle Input");
        Vector2 currentPosition;
        bool inputBegan, inputHeld, inputEnded;

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
            Debug.Log(currentPosition = Input.mousePosition);
            inputBegan = Input.GetMouseButtonDown(0);
            inputHeld = Input.GetMouseButton(0);
            inputEnded = Input.GetMouseButtonUp(0);
        }

        if (inputBegan)
        {
            StartDrag(currentPosition);
        }
        else if (isDragging && inputHeld)
        {
            UpdateDrag(currentPosition);
            // End the drag if the maximum time has been exceeded.
            if (Time.time - dragStartTime > maxDragTime)
            {
                EndDrag(currentPosition);
            }
        }
        else if (isDragging && inputEnded)
        {
            EndDrag(currentPosition);
        }
    }

    private void StartDrag(Vector2 pos)
    {
        startPos = pos;
        isDragging = true;
        powerSlider.value = 0f;
        dragStartTime = Time.time;
    }

    private void UpdateDrag(Vector2 inputPos)
    {
        powerSlider.value = CalculatePowerRatio(inputPos);
    }

    private void EndDrag(Vector2 endPos)
    {
        Debug.Log("End Drag");
        isDragging = false;

        float powerRatio = CalculatePowerRatio(endPos);
        powerSlider.value = powerRatio;
        
        // Invoke the event if a valid power ratio was calculated
        if (powerRatio > 0)
        {
            OnDragEnded?.Invoke(powerRatio);
        }
    }
    
    // Calculates the power ratio based on the drag distance
    private float CalculatePowerRatio(Vector2 inputPos)
    {
        float deltaY = inputPos.y - startPos.y;
        
        if (deltaY <= 0) return 0f;
        
        float clampedDelta = Mathf.Clamp(deltaY, 0f, maxDelta);
        return clampedDelta / maxDelta;
    }

    public void ResetSlider()
    {
        powerSlider.value = 0f;
    }
}