using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    // Sets the maximum time for the timer.
    [SerializeField] private float maxTime = 50.0f;
    // Reference to the UI text element for the timer.
    [SerializeField] private TMP_Text timerText;

    // The current time remaining on the timer.
    private float currentTime;
    // Public property to check if the timer has ended.
    public bool IsTimerOver { get; private set; }

    // Delegate and static event for when the timer is over.
    public delegate void TimerOver();
    public static event TimerOver OnTimerOver;

    void Start()
    {
        // Retrieves the timer value from the FlowManager instance if it exists.
        if (FlowManager.Instance)
        {
            maxTime = FlowManager.Instance.timerValue;
        }
        
        // Initializes the current time and the timer state.
        currentTime = maxTime;
        IsTimerOver = false;
    }

    void Update()
    {
        // Checks if the timer has not yet finished.
        if (!IsTimerOver)
        {
            // Decrements the timer if there is time remaining.
            if (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                // Sets the time to zero, marks the timer as over, and invokes the event.
                currentTime = 0f;
                IsTimerOver = true;
                OnTimerOver?.Invoke(); // Notify the end of the timer
            }
        }
    }

    // Updates the UI text with the current time.
    private void UpdateTimerDisplay()
    {
        // Formats and displays the current time in the UI.
        if (timerText) 
            timerText.SetText(currentTime.ToString("F") + " s");
    }
}