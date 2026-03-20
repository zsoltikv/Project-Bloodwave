using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class MatchSaveManager
{
    public static async Task TryAutoSaveMatchAsync(PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("[MatchSaveManager] PlayerStats is null, skipping match save.");
            return;
        }

        if (AuthManager.Instance == null)
        {
            Debug.LogWarning("[MatchSaveManager] AuthManager instance not found, skipping match save.");
            return;
        }

        if (!AuthManager.Instance.IsLoggedIn())
        {
            Debug.LogWarning("[MatchSaveManager] User is not logged in, skipping match save.");
            return;
        }

        int runTime = RunTimer.instance != null ? Mathf.RoundToInt(RunTimer.instance.timeElapsed * 1000f) : 0;
        int level = playerStats.Level;
        int damageDealtThisRun = GameManagerScript.instance != null ? GameManagerScript.instance.damageDealtThisRun : 0;
        int damageTakenThisRun = Mathf.RoundToInt(playerStats.damageTakenThisRun);
        int enemiesKilledThisRun = GameManagerScript.instance != null ? GameManagerScript.instance.enemiesKilledThisRun : 0;
        int coinsCollectedThisRun = GameManagerScript.instance != null ? GameManagerScript.instance.coinsCollectedThisRun : 0;
        int maxHealth = Mathf.RoundToInt(playerStats.MaxHealth);

        List<int> itemIds = new List<int>();
        if (PlayerInventory.instance != null)
        {
            itemIds = PlayerInventory.instance.GetOwnedItemIds();
        }

        List<int> weaponIds = new List<int>();
        WeaponController weaponController = playerStats.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponIds = weaponController.GetOwnedWeaponIds();
        }

        MatchCreateRequest request = new MatchCreateRequest
        {
            time = runTime,
            level = level,
            maxHealth = maxHealth,
            damageDealt = damageDealtThisRun,
            damageTaken = damageTakenThisRun,
            enemiesKilled = enemiesKilledThisRun,
            coinsCollected = coinsCollectedThisRun,
            itemIds = itemIds,
            weaponIds = weaponIds
        };

        Debug.Log($"{request.itemIds.Count} and {request.weaponIds.Count}");

        try
        {
            string response = await AuthManager.Instance.CreateMatchAsync(request);
            Debug.Log($"[MatchSaveManager] End-of-game match save sent successfully. Response: {response}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MatchSaveManager] End-of-game match save failed: {e.Message}");
        }
    }
}
