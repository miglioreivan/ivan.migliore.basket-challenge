using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSceneManager : MonoBehaviour
{
    // Header to organize UI references in the Inspector.
    [Header("UI References")]
    // Reference to the play button in the UI.
    [SerializeField] private Button playButton;
    // Reference to the quit button in the UI.
    [SerializeField] private Button quitButton;
    // Reference to the slider for setting the game timer.
    [SerializeField] private Slider timerSlider; 
    // Reference to the text that displays the timer value.
    [SerializeField] private TextMeshProUGUI timerValueText; 
    
    // Header to organize scene names in the Inspector.
    [Header("Scene Names")]
    // Stores the name of the game scene to be loaded.
    [SerializeField] private string gameSceneName = "GameScene";

    // Header to organize timer settings in the Inspector.
    [Header("Timer Settings")]
    // The minimum value for the timer slider.
    [SerializeField] private int minTimerValue = 50;
    // The maximum value for the timer slider.
    [SerializeField] private int maxTimerValue = 120;

    void Start()
    {
        // Checks if the FlowManager instance exists.
        if (!FlowManager.Instance)
        {
            Debug.LogError("FlowManager not found in the scene! Make sure an instance exists and uses DontDestroyOnLoad.");
            return;
        }

        // Adds a listener to the play button to call the OnPlayButtonClick method.
        if (playButton)
        {
            playButton.onClick.AddListener(OnPlayButtonClick);
        }

        // Adds a listener to the quit button to call the OnQuitButtonClick method.
        if (quitButton)
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
        }

        // Sets up the slider's properties and event listeners.
        if (timerSlider)
        {
            // Set the minimum and maximum values for the slider.
            timerSlider.minValue = minTimerValue;
            timerSlider.maxValue = maxTimerValue;
            
            // Ensures the slider only produces whole number values.
            timerSlider.wholeNumbers = true; 
            
            // Registers the UpdateTimerDisplay method to the slider's value changed event.
            timerSlider.onValueChanged.AddListener(UpdateTimerDisplay);
            
            // Initializes the timer display text with the slider's starting value.
            UpdateTimerDisplay(timerSlider.value);
        }
    }

    // Updates the text to show the rounded integer value of the slider.
    private void UpdateTimerDisplay(float value)
    {
        if (timerValueText)
        {
            timerValueText.text = Mathf.RoundToInt(value).ToString();
        }
    }

    // Retrieves the integer value from the slider, saves it, and loads the game scene.
    private void OnPlayButtonClick()
    {
        int timerValue = (int)timerSlider.value;
        Debug.Log("Selected timer value: " + timerValue);

        if (FlowManager.Instance)
        {
            FlowManager.Instance.timerValue = timerValue;
            FlowManager.Instance.LoadScene(gameSceneName);
        }
    }

    // Quits the application.
    private void OnQuitButtonClick()
    {
        Application.Quit();
    }
}