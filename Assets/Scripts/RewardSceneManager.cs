using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSceneManager : MonoBehaviour
{
    // Header for organizing UI references in the Inspector.
    [Header("UI References")]
    // Reference to the text component that displays the final score.
    [SerializeField] private TMP_Text finalScoreText;
    // Reference to the button that restarts the game.
    [SerializeField] private Button restartButton;
    // Reference to the button that returns to the main menu.
    [SerializeField] private Button menuButton;
    
    // Header for organizing scene names in the Inspector.
    [Header("Scene Names")]
    // The name of the scene to load when restarting the game.
    [SerializeField] private string gameSceneName = "GameScene";
    // The name of the scene to load when returning to the menu.
    [SerializeField] private string menuSceneName = "MenuScene";

    void Start()
    {
        // Checks if the FlowManager instance exists.
        if (!FlowManager.Instance)
        {
            Debug.LogError("FlowManager not found in the scene! Make sure an instance exists and uses DontDestroyOnLoad.");
            return;
        }

        // Displays the final score retrieved from the FlowManager.
        if (finalScoreText)
        {
            finalScoreText.text = FlowManager.Instance.finalScore + " pt";
        }

        // Assigns listeners to the buttons for their respective actions.
        if (restartButton)
        {
            restartButton.onClick.AddListener(OnRestartButtonClick);
        }

        // Assigns listeners to the buttons for their respective actions.
        if (menuButton)
        {
            menuButton.onClick.AddListener(OnMenuButtonClick);
        }
    }

    // Loads the game scene when the restart button is pressed.
    private void OnRestartButtonClick()
    {
        if (FlowManager.Instance)
        {
            FlowManager.Instance.LoadScene(gameSceneName);
        }
    }

    // Loads the menu scene when the menu button is pressed.
    private void OnMenuButtonClick()
    {
        if (FlowManager.Instance)
        {
            FlowManager.Instance.LoadScene(menuSceneName);
        }
    }
}