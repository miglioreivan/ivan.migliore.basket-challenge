using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float maxTime = 50.0f;
    [SerializeField] private TMP_Text timerText;

    private float currentTime;
    public bool IsTimerOver { get; private set; }

    public delegate void TimerOver();
    public static event TimerOver OnTimerOver;

    void Start()
    {
        currentTime = maxTime;
        IsTimerOver = false;
    }

    void Update()
    {
        if (!IsTimerOver)
        {
            if (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                currentTime = 0f;
                IsTimerOver = true;
                OnTimerOver?.Invoke(); // Notify the end of the timer
            }
        }
    }

    // Updates the UI text with the current time
    private void UpdateTimerDisplay()
    {
        if (timerText) 
            timerText.SetText(currentTime.ToString("F") + " s");
    }
}