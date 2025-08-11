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
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text scoreText;

    // The variable that holds the score.
    public int finalScore;

    private void Awake()
    {
        // Implementation of the Singleton pattern for this script.
        if (Instance == null)
        {
            Instance = this;
            // Prevents this object from being destroyed on scene changes.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a FlowManager already exists, destroy this new object.
            Destroy(gameObject);
        }
    }

    // Public method to start scene loading, linkable to UI buttons.
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    // Coroutine for asynchronous scene loading.
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Activate the loading screen, if it exists.
        if (loadingScreen)
        {
            loadingScreen.SetActive(true);
        }
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!operation.isDone)
        {
            // Calculate progress and update the slider
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (loadingSlider)
            {
                loadingSlider.value = progress;
            }
            
            yield return null;
        }

        // Once the scene is loaded, deactivate the loading screen.
        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }
    }

    // Method to show the score and load the reward scene.
    // Called by BallCollisionHandler at the end of the game.
    public IEnumerator ShowScore(int score, string rewardSceneName)
    {
        finalScore = score;
        
        // Wait a moment before showing the score to allow for animations.
        yield return new WaitForSeconds(1.5f);
        
        // Load the reward scene.
        LoadScene(rewardSceneName);
    }
    
    // Method to be called in the RewardScene's Start event to display the score.
    public void DisplayFinalScore(TMP_Text textElement)
    {
        if (textElement)
        {
            textElement.text = (finalScore + " pt");
        }
    }
    
    // Method to restart the game, linkable to a button in the RewardScene.
    public void RestartGame()
    {
        finalScore = 0;
        LoadScene("GameScene"); // Replace with the name of your game scene.
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}