using UnityEngine;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        // Check if the FlowManager instance exists.
        if (!FlowManager.Instance)
        {
            Debug.LogError("FlowManager not found in the scene! Make sure an instance exists and uses DontDestroyOnLoad.");
            return;
        }

        // 2. Programmatically assign functions to the buttons.
        if (playButton)
        {
            playButton.onClick.AddListener(OnPlayButtonClick);
        }

        if (quitButton)
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
        }
    }

    // Method called when the play button is pressed.
    private void OnPlayButtonClick()
    {
        if (FlowManager.Instance)
        {
            FlowManager.Instance.LoadScene(gameSceneName);
        }
    }

    // Method called when the quit button is pressed.
    private void OnQuitButtonClick()
    {
        Application.Quit();
    }
}