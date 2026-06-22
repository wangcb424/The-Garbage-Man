using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalRunManager : MonoBehaviour
{
    public static GlobalRunManager Instance { get; private set; }

    [Header("Base Player Stats")]
    public int baseMaxHealth = 100;
    public float baseMoveSpeed = 8f;
    public float baseAttackSpeedMultiplier = 1f;
    public float baseCritChance = 0.05f;
    public float baseBlockDamageReduction = 0.5f;
    public int baseStartingHealPotionCount = 0;
    public float maxBlockDamageReduction = 0.9f;

    [Header("Current Player Stats")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public float moveSpeed = 8f;
    public float attackSpeedMultiplier = 1f;
    public float critChance = 0.05f;
    public float blockDamageReduction = 0.5f;

    [Header("Weapon")]
    public GameObject currentWeaponPrefab;
    public string currentWeaponID = "";

    [Header("Permanent Upgrade Values")]
    public int permanentMaxHealthBonus = 0;
    public float permanentMoveSpeedBonus = 0f;
    public float permanentAttackSpeedBonus = 0f;
    public float permanentCritChanceBonus = 0f;
    public float permanentBlockReductionBonus = 0f;
    public int permanentStartingHealPotionBonus = 0;

    [Header("Run Items")]
    public int startingHealPotionCount = 0;
    public int healPotionCount = 0;
    public int itemHealAmount = 1;

    [Header("Run Resources")]
    public int runGold = 0;
    public int runTrash = 0;

    [Header("Saved Resources")]
    public int totalGold = 0;

    [Header("Run Checkpoint")]
    public bool hasRunCheckpoint = false;
    public int checkpointDungeonSceneBuildIndex = 2;
    public int checkpointSmallLevel = 1;
    public int checkpointRunGold = 0;
    public int checkpointRunTrash = 0;
    public int checkpointHealth = 100;
    public int checkpointHealPotionCount = 0;

    [Header("Checkpoint Stats")]
    public int checkpointMaxHealth = 10;
    public float checkpointMoveSpeed = 8f;
    public float checkpointAttackSpeedMultiplier = 1f;
    public float checkpointCritChance = 0.05f;
    public float checkpointBlockDamageReduction = 0.5f;

    [Header("Dungeon Progress")]
    public bool loopOnlyFirstDungeonScene = true;
    public int hubSceneBuildIndex = 1;
    public int firstDungeonSceneBuildIndex = 2;
    public int currentDungeonSceneBuildIndex = 2;
    public int finalDungeonSceneBuildIndex = 4;
    public int buffChoiceSceneBuildIndex = 5;
    public int currentSmallLevel = 1;
    public int smallLevelsPerDungeonScene = 3;

    [Header("Player Upgrades")]
    public int permanentPotionHealBonus = 0;

    [Header("ATM Progress")]
    public int atmTotalDeposited = 0;
    public int milestonesReached = 0;
    public int atmGoal = 5000;
    public int weapon2UnlockMilestone = 3;
    public int weapon3UnlockMilestone = 6;
    public bool weapon2Unlocked = false;
    public bool weapon3Unlocked = false;
    public GameObject weapon2Prefab;
    public GameObject weapon3Prefab;

    private const string TotalGoldKey = "TotalGold";

    private const string PermanentMaxHealthBonusKey = "PermanentMaxHealthBonus";
    private const string PermanentMoveSpeedBonusKey = "PermanentMoveSpeedBonus";
    private const string PermanentAttackSpeedBonusKey = "PermanentAttackSpeedBonus";
    private const string PermanentCritChanceBonusKey = "PermanentCritChanceBonus";
    private const string PermanentBlockReductionBonusKey = "PermanentBlockReductionBonus";
    private const string PermanentStartingHealPotionBonusKey = "PermanentStartingHealPotionBonus";

    private const string CurrentWeaponIDKey = "CurrentWeaponID";

    private const string HasRunCheckpointKey = "HasRunCheckpoint";
    private const string CheckpointDungeonSceneBuildIndexKey = "CheckpointDungeonSceneBuildIndex";
    private const string CheckpointSmallLevelKey = "CheckpointSmallLevel";
    private const string CheckpointRunGoldKey = "CheckpointRunGold";
    private const string CheckpointRunTrashKey = "CheckpointRunTrash";
    private const string CheckpointHealthKey = "CheckpointHealth";
    private const string CheckpointHealPotionCountKey = "CheckpointHealPotionCount";

    private const string CheckpointMaxHealthKey = "CheckpointMaxHealth";
    private const string CheckpointMoveSpeedKey = "CheckpointMoveSpeed";
    private const string CheckpointAttackSpeedMultiplierKey = "CheckpointAttackSpeedMultiplier";
    private const string CheckpointCritChanceKey = "CheckpointCritChance";
    private const string CheckpointBlockDamageReductionKey = "CheckpointBlockDamageReduction";

    private const string AtmTotalDepositedKey = "AtmTotalDeposited";
    private const string MilestonesReachedKey = "MilestonesReached";
    private const string Weapon2UnlockedKey = "Weapon2Unlocked";
    private const string Weapon3UnlockedKey = "Weapon3Unlocked";
    private const string PermanentPotionHealBonusKey = "PermanentPotionHealBonus";

        private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPermanentData();
        LoadRunCheckpoint();
        ApplyPermanentUpgrades(true);

        // Do not force-load Hub here.
        // IntroScene is build index 0, and IntroSequenceManager controls when to load HubScene.
        // This avoids the first Hub load being redirected back to IntroScene.
    }

    private void OnApplicationQuit()
    {
        SavePermanentData();
        SaveRunCheckpointDataOnly();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePermanentData();
            SaveRunCheckpointDataOnly();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SavePermanentData();
            SaveRunCheckpointDataOnly();
        }
    }

    public void StartNewRun()
    {
        runGold = 0;
        runTrash = 0;

        ApplyPermanentUpgrades(true);

        healPotionCount = startingHealPotionCount;

        currentDungeonSceneBuildIndex = firstDungeonSceneBuildIndex;
        currentSmallLevel = 1;

        SaveRunCheckpoint();

        Debug.Log("New run started.");
        Debug.Log("Checkpoint saved at Level " + GetCurrentLevelText());
    }

    public void StartOrContinueRunFromHub()
    {
        if (hasRunCheckpoint)
        {
            LoadCheckpointIntoCurrentRun();
            Debug.Log("Continue run from checkpoint: " + GetCurrentLevelText());
            SceneManager.LoadScene(currentDungeonSceneBuildIndex);
            return;
        }

        StartNewRun();
        SceneManager.LoadScene(currentDungeonSceneBuildIndex);
    }

    public void LoadFirstDungeonScene()
    {
        StartOrContinueRunFromHub();
    }

    public void GoToBuffChoiceScene()
    {
        SceneManager.LoadScene(buffChoiceSceneBuildIndex);
    }

    public void ContinueAfterBuffChoice()
    {
        if (!loopOnlyFirstDungeonScene)
        {
            bool isFinalDungeonScene = currentDungeonSceneBuildIndex >= finalDungeonSceneBuildIndex;
            bool isFinalSmallLevel = currentSmallLevel >= smallLevelsPerDungeonScene;

            if (isFinalDungeonScene && isFinalSmallLevel)
            {
                CompleteDungeonRun();
                RestoreHealthToFull();
                SceneManager.LoadScene(hubSceneBuildIndex);
                return;
            }
        }

        MoveToNextSmallLevel();
        SaveRunCheckpoint();

        Debug.Log("Continue to next level: " + GetCurrentLevelText());

        SceneManager.LoadScene(currentDungeonSceneBuildIndex);
    }

    private void MoveToNextSmallLevel()
    {
        if (loopOnlyFirstDungeonScene)
        {
            currentDungeonSceneBuildIndex = firstDungeonSceneBuildIndex;

            if (currentSmallLevel < smallLevelsPerDungeonScene)
            {
                currentSmallLevel++;
            }
            else
            {
                currentSmallLevel = 1;
            }

            return;
        }

        if (currentSmallLevel < smallLevelsPerDungeonScene)
        {
            currentSmallLevel++;
            return;
        }

        if (currentDungeonSceneBuildIndex < finalDungeonSceneBuildIndex)
        {
            currentDungeonSceneBuildIndex++;
            currentSmallLevel = 1;
            return;
        }
    }

    public int GetGlobalSmallLevelNumber()
    {
        int dungeonSceneOffset = currentDungeonSceneBuildIndex - firstDungeonSceneBuildIndex;
        int globalLevel = dungeonSceneOffset * smallLevelsPerDungeonScene + currentSmallLevel;

        if (globalLevel < 1)
        {
            globalLevel = 1;
        }

        return globalLevel;
    }

    public string GetCurrentLevelText()
    {
        int dungeonSceneNumber =
            currentDungeonSceneBuildIndex -
            firstDungeonSceneBuildIndex +
            1;

        if (dungeonSceneNumber < 1)
        {
            dungeonSceneNumber = 1;
        }

        return dungeonSceneNumber + "-" + currentSmallLevel;
    }

    public void AddRunGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        runGold += amount;

        Debug.Log("Run Gold: " + runGold);
    }

    public void AddRunTrash(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        runTrash += amount;

        Debug.Log("Run Trash: " + runTrash);
    }

    public bool SpendRunGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (runGold < amount)
        {
            Debug.Log("Not enough run gold.");
            return false;
        }

        runGold -= amount;

        Debug.Log("Run Gold: " + runGold);
        return true;
    }

    public bool SpendRunTrash(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (runTrash < amount)
        {
            Debug.Log("Not enough trash.");
            return false;
        }

        runTrash -= amount;

        Debug.Log("Run Trash: " + runTrash);
        return true;
    }

    public void AddTotalGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        totalGold += amount;
        SavePermanentData();

        Debug.Log("Total Gold: " + totalGold);
    }

    public bool SpendTotalGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (totalGold < amount)
        {
            Debug.Log("Not enough total gold.");
            return false;
        }

        totalGold -= amount;
        SavePermanentData();

        Debug.Log("Total Gold: " + totalGold);
        return true;
    }

    public void AddHealPotion(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        healPotionCount += amount;

        Debug.Log("Heal Potion Count: " + healPotionCount);
    }

    public bool UseHealPotion()
    {
        if (healPotionCount <= 0)
        {
            Debug.Log("No heal potion left.");
            return false;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("Health is already full.");
            return false;
        }

        healPotionCount--;
        HealPlayer(itemHealAmount);

        Debug.Log("Used heal potion. Remaining: " + healPotionCount);

        return true;
    }

    public void HealPlayer(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);
    }

    public void DamagePlayer(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);

            HandlePlayerDeath();
            return;
        }

        Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        maxHealth += amount;
        currentHealth += amount;

        Debug.Log("Temporary Max Health: " + maxHealth);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;

        if (moveSpeed < 1f)
        {
            moveSpeed = 1f;
        }

        Debug.Log("Temporary Move Speed: " + moveSpeed);
    }

    public void IncreaseAttackSpeedMultiplier(float amount)
    {
        attackSpeedMultiplier += amount;

        if (attackSpeedMultiplier < 0.1f)
        {
            attackSpeedMultiplier = 0.1f;
        }

        Debug.Log("Temporary Attack Speed Multiplier: " + attackSpeedMultiplier);
    }

    public void IncreaseCritChance(float amount)
    {
        critChance += amount;

        if (critChance < 0f)
        {
            critChance = 0f;
        }

        if (critChance > 1f)
        {
            critChance = 1f;
        }

        Debug.Log("Temporary Crit Chance: " + critChance);
    }

    public void IncreaseBlockDamageReduction(float amount)
    {
        blockDamageReduction += amount;

        if (blockDamageReduction > maxBlockDamageReduction)
        {
            blockDamageReduction = maxBlockDamageReduction;
        }

        if (blockDamageReduction < 0f)
        {
            blockDamageReduction = 0f;
        }

        Debug.Log("Temporary Block Damage Reduction: " + blockDamageReduction);
    }

    public void SetWeapon(GameObject weaponPrefab)
    {
        currentWeaponPrefab = weaponPrefab;

        if (weaponPrefab != null)
        {
            currentWeaponID = weaponPrefab.name;
            SavePermanentData();

            Debug.Log("Current Weapon: " + weaponPrefab.name);
        }
    }

    public void CompleteDungeonRun()
    {
        if (runGold > 0)
        {
            totalGold += runGold;
        }

        Debug.Log("Dungeon run completed. Cashed out gold: " + runGold);

        runGold = 0;
        runTrash = 0;
        healPotionCount = 0;

        ClearRunCheckpoint();
        ApplyPermanentUpgrades(true);
        SavePermanentData();

        Debug.Log("Total Gold: " + totalGold);
    }

    public void RetreatAndCashOutRun()
    {
        Time.timeScale = 1f;

        if (runGold > 0)
        {
            totalGold += runGold;
        }

        Debug.Log("Retreated from run. Cashed out gold: " + runGold);

        runGold = 0;
        runTrash = 0;
        healPotionCount = 0;

        ClearRunCheckpoint();
        ApplyPermanentUpgrades(true);

        currentDungeonSceneBuildIndex = firstDungeonSceneBuildIndex;
        currentSmallLevel = 1;

        SavePermanentData();

        Debug.Log("Total Gold: " + totalGold);

        SceneManager.LoadScene(hubSceneBuildIndex);
    }

    public void QuitCurrentRunToHub()
    {
        Time.timeScale = 1f;

        // Quit To Hub is not cash out.
        // 只是暂时离开 run，不结算 runGold
        if (hasRunCheckpoint)
        {
            LoadCheckpointIntoCurrentRun();
        }
        else
        {
            SaveRunCheckpoint();
        }

        SavePermanentData();
        SaveRunCheckpointDataOnly();

        Debug.Log("Quit to Hub. Checkpoint remains at: " + GetCurrentLevelText());

        SceneManager.LoadScene(hubSceneBuildIndex);
    }

    public void SaveAndQuitGame()
    {
        Time.timeScale = 1f;

        // Quit Game saves checkpoint and exits.
        // 退出游戏保存 checkpoint，下次先进 Hub
        if (hasRunCheckpoint)
        {
            LoadCheckpointIntoCurrentRun();
        }
        else
        {
            SaveRunCheckpoint();
        }

        SavePermanentData();
        SaveRunCheckpointDataOnly();

        Debug.Log("Save and quit game. Checkpoint remains at: " + GetCurrentLevelText());

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void HandlePlayerDeath()
    {
        runGold = Mathf.FloorToInt(runGold * 0.5f);
        runTrash = 0;
        currentDungeonSceneBuildIndex = firstDungeonSceneBuildIndex;
        currentSmallLevel = 1;
    }

    public void LoadHubScene()
    {
        RestoreHealthToFull();
        SceneManager.LoadScene(hubSceneBuildIndex);
    }

    public void RestoreHealthToFull()
    {
        currentHealth = maxHealth;
    }

    private void ApplyPermanentUpgrades(bool healToFull)
    {
        maxHealth = baseMaxHealth + permanentMaxHealthBonus;
        moveSpeed = baseMoveSpeed + permanentMoveSpeedBonus;
        attackSpeedMultiplier = baseAttackSpeedMultiplier + permanentAttackSpeedBonus;
        critChance = baseCritChance + permanentCritChanceBonus;
        blockDamageReduction = baseBlockDamageReduction + permanentBlockReductionBonus;
        startingHealPotionCount = baseStartingHealPotionCount + permanentStartingHealPotionBonus;
        itemHealAmount = 5 + permanentPotionHealBonus;

        if (critChance < 0f)
        {
            critChance = 0f;
        }

        if (critChance > 1f)
        {
            critChance = 1f;
        }

        if (blockDamageReduction < 0f)
        {
            blockDamageReduction = 0f;
        }

        if (blockDamageReduction > maxBlockDamageReduction)
        {
            blockDamageReduction = maxBlockDamageReduction;
        }

        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }

    public void SaveRunCheckpoint()
    {
        hasRunCheckpoint = true;

        checkpointDungeonSceneBuildIndex = currentDungeonSceneBuildIndex;
        checkpointSmallLevel = currentSmallLevel;
        checkpointRunGold = runGold;
        checkpointRunTrash = runTrash;
        checkpointHealth = currentHealth;
        checkpointHealPotionCount = healPotionCount;

        checkpointMaxHealth = maxHealth;
        checkpointMoveSpeed = moveSpeed;
        checkpointAttackSpeedMultiplier = attackSpeedMultiplier;
        checkpointCritChance = critChance;
        checkpointBlockDamageReduction = blockDamageReduction;

        SaveRunCheckpointDataOnly();

        Debug.Log("Run checkpoint saved at Level " + GetCurrentLevelText());
    }

    private void LoadCheckpointIntoCurrentRun()
    {
        if (!hasRunCheckpoint)
        {
            return;
        }

        currentDungeonSceneBuildIndex = checkpointDungeonSceneBuildIndex;
        currentSmallLevel = checkpointSmallLevel;

        runGold = checkpointRunGold;
        runTrash = checkpointRunTrash;

        ApplyPermanentUpgrades(false);

        maxHealth = checkpointMaxHealth;
        moveSpeed = checkpointMoveSpeed;
        attackSpeedMultiplier = checkpointAttackSpeedMultiplier;
        critChance = checkpointCritChance;
        blockDamageReduction = checkpointBlockDamageReduction;

        if (critChance < 0f)
        {
            critChance = 0f;
        }

        if (critChance > 1f)
        {
            critChance = 1f;
        }

        if (blockDamageReduction < 0f)
        {
            blockDamageReduction = 0f;
        }

        if (blockDamageReduction > maxBlockDamageReduction)
        {
            blockDamageReduction = maxBlockDamageReduction;
        }

        currentHealth = checkpointHealth;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        healPotionCount = checkpointHealPotionCount;
    }

    public void ClearRunCheckpoint()
    {
        hasRunCheckpoint = false;

        PlayerPrefs.DeleteKey(HasRunCheckpointKey);
        PlayerPrefs.DeleteKey(CheckpointDungeonSceneBuildIndexKey);
        PlayerPrefs.DeleteKey(CheckpointSmallLevelKey);
        PlayerPrefs.DeleteKey(CheckpointRunGoldKey);
        PlayerPrefs.DeleteKey(CheckpointRunTrashKey);
        PlayerPrefs.DeleteKey(CheckpointHealthKey);
        PlayerPrefs.DeleteKey(CheckpointHealPotionCountKey);

        PlayerPrefs.DeleteKey(CheckpointMaxHealthKey);
        PlayerPrefs.DeleteKey(CheckpointMoveSpeedKey);
        PlayerPrefs.DeleteKey(CheckpointAttackSpeedMultiplierKey);
        PlayerPrefs.DeleteKey(CheckpointCritChanceKey);
        PlayerPrefs.DeleteKey(CheckpointBlockDamageReductionKey);

        PlayerPrefs.Save();

        Debug.Log("Run checkpoint cleared.");
    }

    private void SaveRunCheckpointDataOnly()
    {
        PlayerPrefs.SetInt(HasRunCheckpointKey, hasRunCheckpoint ? 1 : 0);

        PlayerPrefs.SetInt(CheckpointDungeonSceneBuildIndexKey, checkpointDungeonSceneBuildIndex);
        PlayerPrefs.SetInt(CheckpointSmallLevelKey, checkpointSmallLevel);
        PlayerPrefs.SetInt(CheckpointRunGoldKey, checkpointRunGold);
        PlayerPrefs.SetInt(CheckpointRunTrashKey, checkpointRunTrash);
        PlayerPrefs.SetInt(CheckpointHealthKey, checkpointHealth);
        PlayerPrefs.SetInt(CheckpointHealPotionCountKey, checkpointHealPotionCount);

        PlayerPrefs.SetInt(CheckpointMaxHealthKey, checkpointMaxHealth);
        PlayerPrefs.SetFloat(CheckpointMoveSpeedKey, checkpointMoveSpeed);
        PlayerPrefs.SetFloat(CheckpointAttackSpeedMultiplierKey, checkpointAttackSpeedMultiplier);
        PlayerPrefs.SetFloat(CheckpointCritChanceKey, checkpointCritChance);
        PlayerPrefs.SetFloat(CheckpointBlockDamageReductionKey, checkpointBlockDamageReduction);

        PlayerPrefs.Save();
    }

    private void LoadRunCheckpoint()
    {
        hasRunCheckpoint = PlayerPrefs.GetInt(HasRunCheckpointKey, 0) == 1;

        checkpointDungeonSceneBuildIndex = PlayerPrefs.GetInt(
            CheckpointDungeonSceneBuildIndexKey,
            firstDungeonSceneBuildIndex
        );

        checkpointSmallLevel = PlayerPrefs.GetInt(
            CheckpointSmallLevelKey,
            1
        );

        checkpointRunGold = PlayerPrefs.GetInt(
            CheckpointRunGoldKey,
            0
        );

        checkpointRunTrash = PlayerPrefs.GetInt(
            CheckpointRunTrashKey,
            0
        );

        checkpointHealth = PlayerPrefs.GetInt(
            CheckpointHealthKey,
            baseMaxHealth
        );

        checkpointHealPotionCount = PlayerPrefs.GetInt(
            CheckpointHealPotionCountKey,
            baseStartingHealPotionCount
        );

        checkpointMaxHealth = PlayerPrefs.GetInt(
            CheckpointMaxHealthKey,
            baseMaxHealth
        );

        checkpointMoveSpeed = PlayerPrefs.GetFloat(
            CheckpointMoveSpeedKey,
            baseMoveSpeed
        );

        checkpointAttackSpeedMultiplier = PlayerPrefs.GetFloat(
            CheckpointAttackSpeedMultiplierKey,
            baseAttackSpeedMultiplier
        );

        checkpointCritChance = PlayerPrefs.GetFloat(
            CheckpointCritChanceKey,
            baseCritChance
        );

        checkpointBlockDamageReduction = PlayerPrefs.GetFloat(
            CheckpointBlockDamageReductionKey,
            baseBlockDamageReduction
        );

        if (hasRunCheckpoint)
        {
            bool checkpointSceneIsInvalid =
                checkpointDungeonSceneBuildIndex < firstDungeonSceneBuildIndex ||
                checkpointDungeonSceneBuildIndex > finalDungeonSceneBuildIndex;

            if (checkpointSceneIsInvalid)
            {
                Debug.Log("Old run checkpoint used an invalid scene index. Clearing checkpoint.");
                ClearRunCheckpoint();
                checkpointDungeonSceneBuildIndex = firstDungeonSceneBuildIndex;
                checkpointSmallLevel = 1;
            }
        }
    }

    public void SavePermanentData()
    {
        PlayerPrefs.SetInt(TotalGoldKey, totalGold);

        PlayerPrefs.SetInt(PermanentMaxHealthBonusKey, permanentMaxHealthBonus);
        PlayerPrefs.SetFloat(PermanentMoveSpeedBonusKey, permanentMoveSpeedBonus);
        PlayerPrefs.SetFloat(PermanentAttackSpeedBonusKey, permanentAttackSpeedBonus);
        PlayerPrefs.SetFloat(PermanentCritChanceBonusKey, permanentCritChanceBonus);
        PlayerPrefs.SetFloat(PermanentBlockReductionBonusKey, permanentBlockReductionBonus);
        PlayerPrefs.SetInt(PermanentStartingHealPotionBonusKey, permanentStartingHealPotionBonus);
        PlayerPrefs.SetInt(PermanentPotionHealBonusKey, permanentPotionHealBonus);
        PlayerPrefs.SetInt(AtmTotalDepositedKey, atmTotalDeposited);
        PlayerPrefs.SetInt(MilestonesReachedKey, milestonesReached);
        PlayerPrefs.SetInt(Weapon2UnlockedKey, weapon2Unlocked ? 1 : 0);
        PlayerPrefs.SetInt(Weapon3UnlockedKey, weapon3Unlocked ? 1 : 0);

        PlayerPrefs.SetString(CurrentWeaponIDKey, currentWeaponID);

        PlayerPrefs.Save();

        Debug.Log("Permanent data saved.");
    }

    public void LoadPermanentData()
    {
        totalGold = PlayerPrefs.GetInt(TotalGoldKey, 0);

        permanentMaxHealthBonus = PlayerPrefs.GetInt(PermanentMaxHealthBonusKey, 0);
        permanentMoveSpeedBonus = PlayerPrefs.GetFloat(PermanentMoveSpeedBonusKey, 0f);
        permanentAttackSpeedBonus = PlayerPrefs.GetFloat(PermanentAttackSpeedBonusKey, 0f);
        permanentCritChanceBonus = PlayerPrefs.GetFloat(PermanentCritChanceBonusKey, 0f);
        permanentBlockReductionBonus = PlayerPrefs.GetFloat(PermanentBlockReductionBonusKey, 0f);
        permanentStartingHealPotionBonus = PlayerPrefs.GetInt(PermanentStartingHealPotionBonusKey, 0);
        permanentPotionHealBonus = PlayerPrefs.GetInt(PermanentPotionHealBonusKey, 0);

        atmTotalDeposited = PlayerPrefs.GetInt(AtmTotalDepositedKey, 0);
        milestonesReached = PlayerPrefs.GetInt(MilestonesReachedKey, 0);
        weapon2Unlocked = PlayerPrefs.GetInt(Weapon2UnlockedKey, 0) == 1;
        weapon3Unlocked = PlayerPrefs.GetInt(Weapon3UnlockedKey, 0) == 1;

        currentWeaponID = PlayerPrefs.GetString(CurrentWeaponIDKey, "");

        Debug.Log("Permanent data loaded.");
        Debug.Log("Total Gold: " + totalGold);
    }

    [ContextMenu("Debug Add 500 Total Gold")]
    private void DebugAdd500TotalGold()
    {
        AddTotalGold(500);
    }

    [ContextMenu("Debug Add 100 Run Gold")]
    private void DebugAdd100RunGold()
    {
        AddRunGold(100);
    }

    [ContextMenu("Reset All Save Data")]
    public void ResetAllSaveData()
    {
        PlayerPrefs.DeleteKey(TotalGoldKey);

        PlayerPrefs.DeleteKey(PermanentMaxHealthBonusKey);
        PlayerPrefs.DeleteKey(PermanentMoveSpeedBonusKey);
        PlayerPrefs.DeleteKey(PermanentAttackSpeedBonusKey);
        PlayerPrefs.DeleteKey(PermanentCritChanceBonusKey);
        PlayerPrefs.DeleteKey(PermanentBlockReductionBonusKey);
        PlayerPrefs.DeleteKey(PermanentStartingHealPotionBonusKey);

        PlayerPrefs.DeleteKey(CurrentWeaponIDKey);

        totalGold = 0;

        permanentMaxHealthBonus = 0;
        permanentMoveSpeedBonus = 0f;
        permanentAttackSpeedBonus = 0f;
        permanentCritChanceBonus = 0f;
        permanentBlockReductionBonus = 0f;
        permanentStartingHealPotionBonus = 0;

        currentWeaponID = "";

        runGold = 0;
        runTrash = 0;
        healPotionCount = 0;

        currentDungeonSceneBuildIndex = firstDungeonSceneBuildIndex;
        currentSmallLevel = 1;

        atmTotalDeposited = 0;
        milestonesReached = 0;
        weapon2Unlocked = false;
        weapon3Unlocked = false;
        permanentPotionHealBonus = 0;
        PlayerPrefs.DeleteKey(AtmTotalDepositedKey);
        PlayerPrefs.DeleteKey(MilestonesReachedKey);
        PlayerPrefs.DeleteKey(Weapon2UnlockedKey);
        PlayerPrefs.DeleteKey(Weapon3UnlockedKey);
        PlayerPrefs.DeleteKey(PermanentPotionHealBonusKey);

        ClearRunCheckpoint();
        ApplyPermanentUpgrades(true);

        PlayerPrefs.Save();

        Debug.Log("All save data reset.");
    }

    public void DepositAllRunGold()
    {
        if (totalGold <= 0)
        {
            return;
        }

        atmTotalDeposited += totalGold;
        totalGold = 0;

        CheckMilestones();
        SavePermanentData();

        Debug.Log("ATM total deposited: " + atmTotalDeposited);
    }

    private void CheckMilestones()
    {
        int milestoneGoldInterval = atmGoal / 10;
        int newMilestonesReached = Mathf.Min(atmTotalDeposited / milestoneGoldInterval, 10);

        while (milestonesReached < newMilestonesReached)
        {
            milestonesReached++;
            ApplyMilestone(milestonesReached);
        }
    }

    private void ApplyMilestone(int milestoneIndex)
    {
        Debug.Log("Milestone reached: " + milestoneIndex);

        if (milestoneIndex == 1)
        {
            permanentStartingHealPotionBonus += 1;
        }
        else if (milestoneIndex == 2)
        {
            permanentPotionHealBonus += 5;
        }
        else if (milestoneIndex == 3)
        {
            weapon2Unlocked = true;
        }
        else if (milestoneIndex == 4)
        {
            permanentStartingHealPotionBonus += 1;
        }
        else if (milestoneIndex == 5)
        {
            permanentPotionHealBonus += 5;
        }
        else if (milestoneIndex == 6)
        {
            weapon3Unlocked = true;
        }
        else if (milestoneIndex == 7)
        {
            permanentMoveSpeedBonus += 0.2f;
        }
        else if (milestoneIndex == 8)
        {
            permanentMoveSpeedBonus += 0.2f;
        }
        else if (milestoneIndex == 9)
        {
            permanentMoveSpeedBonus += 0.2f;
        }
        else if (milestoneIndex == 10)
        {
            OnGameWin?.Invoke();
        }

        ApplyPermanentUpgrades(false);

        OnMilestoneReached?.Invoke(milestoneIndex);
    }
    public static event System.Action<int> OnMilestoneReached;
    public static event System.Action OnGameWin;
}