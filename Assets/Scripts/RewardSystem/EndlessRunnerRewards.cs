using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudentRecruitment.EndlessRunner;

public class EndlessRunnerRewards : MonoBehaviour
{
    [SerializeField] private string canvasSceneName = "CanvasTestScene"; // Reference scene name but don't auto-transition

    private EndlessRunnerManager runnerManager;
    private List<int> availablePageIndices = new List<int>();
    private bool rewardsProcessed = false;

    private void Awake()
    {
        // Find the EndlessRunnerManager
        runnerManager = GetComponent<EndlessRunnerManager>();
        if (runnerManager == null)
            runnerManager = FindObjectOfType<EndlessRunnerManager>();

        // Register for events
        if (runnerManager != null)
        {
            // Subscribe to game state change events
            EndlessRunnerManager.OnGameStateChanged += HandleGameStateChanged;
        }
        else
        {
            Debug.LogError("Could not find EndlessRunnerManager!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (runnerManager != null)
        {
            EndlessRunnerManager.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    // Called when game state changes
    private void HandleGameStateChanged(GameState state)
    {
        // Process rewards when game is won
        if (state == GameState.Win && !rewardsProcessed)
        {
            ProcessWinRewards();
        }
        // Process rewards on game over
        else if (state == GameState.GameOver && !rewardsProcessed)
        {
            ProcessLossRewards();
        }
    }

    // Process rewards when player wins
    private void ProcessWinRewards()
    {
        if (rewardsProcessed) return;
        rewardsProcessed = true;

        // Calculate rewards (no longer triggering auto-return)
        int coinsEarned = CalculateCoinsReward();
        int pageIndex = SelectRandomPageReward();

        // Save rewards to be picked up by BookManager
        SaveRewards(coinsEarned, pageIndex);
        
        Debug.Log($"WIN REWARDS: {coinsEarned} coins and page #{pageIndex} ready for collection");
    }

    // Process minimal rewards when player loses
    private void ProcessLossRewards()
    {
        if (rewardsProcessed) return;
        rewardsProcessed = true;

        // Give coins based on score earned before losing
        int coinsEarned = 0;
        if (runnerManager != null)
        {
            coinsEarned = Mathf.RoundToInt(runnerManager.CurrentScore);
            Debug.Log($"Player lost but still earned {coinsEarned} coins based on score");
        }
        
        SaveRewards(coinsEarned, -1); // -1 means no page earned
        
        Debug.Log($"LOSS REWARDS: {coinsEarned} coins ready for collection");
    }

    // Calculate how many coins to award
    private int CalculateCoinsReward()
    {
        // Use only the score from the current session as the coin reward
        int coins = 0;
        if (runnerManager != null)
        {
            // Get the current score directly from the EndlessRunnerManager
            coins = Mathf.RoundToInt(runnerManager.CurrentScore);
            Debug.Log($"Using exact score value as coin reward: {coins} coins");
        }
        else
        {
            Debug.LogWarning("RunnerManager not found when calculating rewards");
        }

        return coins;
    }

    // Select a random page from available pages
    private int SelectRandomPageReward()
    {
        // If we have no available pages stored, check if BookManager is accessible
        if (availablePageIndices.Count == 0)
        {
            // For demo purposes, just return a random index between 0-9
            return Random.Range(0, 10);
        }
        
        // If we have available pages, select one randomly
        if (availablePageIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePageIndices.Count);
            return availablePageIndices[randomIndex];
        }
        
        // Fallback - return a random page index
        return Random.Range(0, 10);
    }

    // Save rewards to PlayerPrefs for BookManager to pick up
    private void SaveRewards(int coins, int pageIndex)
    {
        PlayerPrefs.SetInt("PendingRewardCoins", coins);
        PlayerPrefs.SetInt("PendingRewardPage", pageIndex);
        PlayerPrefs.SetInt("HasPendingRewards", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved rewards: {coins} coins and page #{pageIndex}");
    }

    // Set available page indices from BookManager
    public void SetAvailablePageIndices(List<int> pageIndices)
    {
        availablePageIndices = new List<int>(pageIndices);
    }
    
    // Get the Canvas scene name - can be used by UI Manager
    public string GetCanvasSceneName()
    {
        return canvasSceneName;
    }
} 