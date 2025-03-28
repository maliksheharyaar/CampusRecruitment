using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    public static class GameProgress
    {
        // Program data
        private const string BUSINESS_PROGRAM = "Business";
        private const int BUSINESS_PAGES_COUNT = 5;

        // Keys for PlayerPrefs
        private const string COINS_KEY = "EndlessRunnerCoins";
        private const string BUSINESS_PAGES_KEY = "BusinessPagesCollected";
        private const string BUSINESS_BOOK_CRAFTED = "BusinessBookCrafted";
        private const string LAST_AWARDED_PAGE_KEY = "LastAwardedBusinessPage";
        private const string NEW_PAGE_AWARDED_KEY = "NewBusinessPageAwarded";
        
        // Crafting costs
        private const int BOOK_CRAFT_COST = 100;

        // Max number of pages per program
        public const int MAX_BUSINESS_PAGES = 5;

        // Events
        public static event Action<string, string> OnPageUnlocked;
        public static event Action<string> OnBookCrafted;

        // Cache the loaded data
        private static int coins = -1;
        private static Dictionary<int, bool> businessPages = new Dictionary<int, bool>();
        private static bool businessBookCrafted = false;
        private static int businessPagesCollected = -1;
        private static int lastAwardedPage = -1;
        private static bool newPageAwarded = false;

        // Load data if not loaded
        private static void EnsureDataLoaded()
        {
            if (coins == -1)
            {
                coins = PlayerPrefs.GetInt(COINS_KEY, 0);
            }

            if (businessPagesCollected == -1)
            {
                businessPagesCollected = PlayerPrefs.GetInt(BUSINESS_PAGES_KEY, 0);
            }

            if (lastAwardedPage == -1)
            {
                lastAwardedPage = PlayerPrefs.GetInt(LAST_AWARDED_PAGE_KEY, -1);
            }

            if (!PlayerPrefs.HasKey(NEW_PAGE_AWARDED_KEY))
            {
                newPageAwarded = false;
            }
            else
            {
                newPageAwarded = PlayerPrefs.GetInt(NEW_PAGE_AWARDED_KEY) == 1;
            }

            // Load business pages
            for (int i = 0; i < BUSINESS_PAGES_COUNT; i++)
            {
                businessPages[i] = PlayerPrefs.GetInt(BUSINESS_PAGES_KEY + i, 0) == 1;
            }

            // Load book crafted status
            businessBookCrafted = PlayerPrefs.GetInt(BUSINESS_BOOK_CRAFTED, 0) == 1;
            
            Debug.Log($"Game progress loaded. Coins: {coins}, Pages: {businessPagesCollected}/{BUSINESS_PAGES_COUNT}");
        }

        // Save current progress to PlayerPrefs
        private static void SaveProgress()
        {
            PlayerPrefs.SetInt(COINS_KEY, coins);
            PlayerPrefs.SetInt(BUSINESS_PAGES_KEY, businessPagesCollected);
            PlayerPrefs.SetInt(LAST_AWARDED_PAGE_KEY, lastAwardedPage);
            PlayerPrefs.SetInt(NEW_PAGE_AWARDED_KEY, newPageAwarded ? 1 : 0);

            // Save business pages
            foreach (var page in businessPages)
            {
                PlayerPrefs.SetInt(BUSINESS_PAGES_KEY + page.Key, page.Value ? 1 : 0);
            }

            // Save book crafted status
            PlayerPrefs.SetInt(BUSINESS_BOOK_CRAFTED, businessBookCrafted ? 1 : 0);

            PlayerPrefs.Save();
            
            Debug.Log("Game progress saved.");
        }

        // Get the current coin count
        public static int GetCoins()
        {
            EnsureDataLoaded();
            return coins;
        }

        // Add coins to the player's total
        public static void AddCoins(int amount)
        {
            EnsureDataLoaded();
            coins += amount;
            SaveProgress();
        }

        // Spend coins (returns true if successful)
        public static bool SpendCoins(int amount)
        {
            EnsureDataLoaded();
            if (coins >= amount)
            {
                coins -= amount;
                SaveProgress();
                return true;
            }
            return false;
        }

        // Get the number of pages collected for the business program
        public static int GetBusinessPagesCollected()
        {
            EnsureDataLoaded();
            return businessPagesCollected;
        }

        // Add an alias for GetBusinessPagesCollected to fix compilation error in ProgramBuildingUI
        public static int GetBusinessPagesCount()
        {
            return GetBusinessPagesCollected();
        }

        // Award a business program page (returns true if a new page was awarded)
        public static bool AwardBusinessPage()
        {
            EnsureDataLoaded();
            
            // Reset the flag
            newPageAwarded = false;
            
            // If all pages are already collected, don't award more
            if (businessPagesCollected >= MAX_BUSINESS_PAGES)
            {
                SaveProgress();
                return false;
            }

            // Award a new page and save
            businessPagesCollected++;
            newPageAwarded = true;
            lastAwardedPage = businessPagesCollected - 1;
            
            // Trigger the page unlock event
            OnPageUnlocked?.Invoke(BUSINESS_PROGRAM, $"Business Page {businessPagesCollected}");
            
            SaveProgress();
            return true;
        }

        // Check if a new page was awarded
        public static bool HasNewPageAwarded()
        {
            EnsureDataLoaded();
            return newPageAwarded;
        }

        // Get the index of the last awarded page
        public static int GetLastAwardedPageIndex()
        {
            EnsureDataLoaded();
            return lastAwardedPage;
        }

        // Check if player has all business pages
        public static bool HasAllBusinessPages()
        {
            EnsureDataLoaded();
            return businessPagesCollected >= BUSINESS_PAGES_COUNT;
        }

        // Check if a specific business page is collected
        public static bool HasBusinessPage(int pageIndex)
        {
            EnsureDataLoaded();
            return businessPages.ContainsKey(pageIndex) && businessPages[pageIndex];
        }

        // Check if business book is crafted
        public static bool IsBusinessBookCrafted()
        {
            EnsureDataLoaded();
            return businessBookCrafted;
        }

        // Try to craft the business book
        public static bool TryCraftBusinessBook()
        {
            EnsureDataLoaded();
            
            // Check requirements
            if (!HasAllBusinessPages())
            {
                Debug.Log("Cannot craft business book: missing pages");
                return false;
            }

            // Check if we have enough coins
            if (coins < BOOK_CRAFT_COST)
            {
                Debug.Log($"Cannot craft business book: insufficient coins ({coins}/{BOOK_CRAFT_COST})");
                return false;
            }

            // Craft the book
            coins -= BOOK_CRAFT_COST;
            businessBookCrafted = true;
            
            // Trigger the book crafted event
            OnBookCrafted?.Invoke(BUSINESS_PROGRAM);
            
            SaveProgress();
            Debug.Log("Business book crafted successfully!");
            return true;
        }

        // Get the requirements for crafting the business book
        public static (int requiredPages, int requiredCoins) GetBusinessBookRequirements()
        {
            return (BUSINESS_PAGES_COUNT, BOOK_CRAFT_COST);
        }

        // Reset all progress
        public static void ResetAllProgress()
        {
            // Reset all cached values
            coins = 0;
            businessPages.Clear();
            businessBookCrafted = false;
            businessPagesCollected = 0;
            lastAwardedPage = -1;
            newPageAwarded = false;

            // Clear all PlayerPrefs keys
            PlayerPrefs.DeleteKey(COINS_KEY);
            PlayerPrefs.DeleteKey(BUSINESS_PAGES_KEY);
            PlayerPrefs.DeleteKey(BUSINESS_BOOK_CRAFTED);
            PlayerPrefs.DeleteKey(LAST_AWARDED_PAGE_KEY);
            PlayerPrefs.DeleteKey(NEW_PAGE_AWARDED_KEY);

            // Clear individual page keys
            for (int i = 0; i < BUSINESS_PAGES_COUNT; i++)
            {
                PlayerPrefs.DeleteKey(BUSINESS_PAGES_KEY + i);
            }

            PlayerPrefs.Save();
            Debug.Log("All game progress has been reset.");
        }
    }
} 