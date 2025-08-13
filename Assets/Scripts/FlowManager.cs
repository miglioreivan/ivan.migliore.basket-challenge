using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FlowManager : MonoBehaviour
{
    // Singleton instance to ensure there is only one FlowManager.
    public static FlowManager Instance;
    
    // UI references for the loading screen.
    [SerializeField] private GameObject loadingScreen;
    // Reference to the slider that shows loading progress.
    [SerializeField] private Slider loadingSlider;
    // Reference to the text component for displaying the score.
    [SerializeField] private TMP_Text scoreText;

    // The variable that holds the timer value for the game.
    public int timerValue;
    // The variable that holds the final score.
    public int finalScore;

    private void Awake()
    {
        // Implementation of the Singleton pattern for this script.
        // If no instance exists, this one becomes the instance.
        if (Instance == null)
        {
            Instance = this;
            // Prevents this object from being destroyed when a new scene loads.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another instance already exists, destroy this one.
            Destroy(gameObject);
        }
    }

    // Public method to start scene loading, linkable to UI buttons.
    public void LoadScene(string sceneName)
    {
        // Starts the coroutine to load the scene asynchronously.
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    // Coroutine for asynchronous scene loading.
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Activates the loading screen UI before starting the load operation.
        if (loadingScreen)
        {
            loadingScreen.SetActive(true);
        }
        
        // Starts the asynchronous scene loading process.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        // Loop runs until the scene loading is complete.
        while (!operation.isDone)
        {
            // Calculates the loading progress and clamps it to a 0-1 range.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Updates the loading slider's value to reflect the current progress.
            if (loadingSlider)
            {
                loadingSlider.value = progress;
            }
            
            // Pauses the coroutine for one frame.
            yield return null;
        }

        // Once the scene is loaded, deactivates the loading screen UI.
        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }
    }

    // Method to show the score and load the reward scene.
    // Called by BallCollisionHandler at the end of the game.
    public IEnumerator ShowScore(int score, string rewardSceneName)
    {
        // Stores the final score.
        finalScore = score;
        
        // Waits for a brief period before proceeding to the next scene.
        yield return new WaitForSeconds(1.5f);
        
        // Loads the specified reward scene.
        LoadScene(rewardSceneName);
    }
    
    // Method to be called in the RewardScene's Start event to display the score.
    public void DisplayFinalScore(TMP_Text textElement)
    {
        // Sets the provided text element to display the final score.
        if (textElement)
        {
            textElement.text = (finalScore + " pt");
        }
    }
    
    // Method to restart the game, linkable to a button in the RewardScene.
    public void RestartGame()
    {
        // Resets the final score and loads the game scene.
        finalScore = 0;
        LoadScene("GameScene"); // Replace with the name of your game scene.
    }

    public void QuitGame()
    {
        // Closes the application.
        Application.Quit();
    }
}