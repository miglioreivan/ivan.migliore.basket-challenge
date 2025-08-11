using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MenuScene";

    void Start()
    {
        // Check if the FlowManager instance exists.
        if (!FlowManager.Instance)
        {
            Debug.LogError("FlowManager not found in the scene! Make sure an instance exists and uses DontDestroyOnLoad.");
            return;
        }

        // 1. Display the final score.
        if (finalScoreText)
        {
            finalScoreText.text = FlowManager.Instance.finalScore + " pt";
        }

        // 2. Programmatically assign functions to the buttons.
        if (restartButton)
        {
            restartButton.onClick.AddListener(OnRestartButtonClick);
        }

        if (menuButton)
        {
            menuButton.onClick.AddListener(OnMenuButtonClick);
        }
    }

    // Method called when the restart button is pressed.
    private void OnRestartButtonClick()
    {
        if (FlowManager.Instance)
        {
            FlowManager.Instance.LoadScene(gameSceneName);
        }
    }

    // Method called when the menu button is pressed.
    private void OnMenuButtonClick()
    {
        if (FlowManager.Instance)
        {
            FlowManager.Instance.LoadScene(menuSceneName);
        }
    }
}