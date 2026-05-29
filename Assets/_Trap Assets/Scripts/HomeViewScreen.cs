using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeViewScreen : MonoBehaviour
{
    private const int MaxChestSlots = 4;

    public Button playButton;
    public List<ChestInfo> chestInfos;
    public ChestPreviewPanel chestPreviewPanel;
    public ChestConfig chestConfig;
    public ChestType wave4ChestType = ChestType.Common;
    public ChestType wave8ChestType = ChestType.Rare;
    public int rvTimeReductionSeconds = 300;

    private static HomeViewScreen instance;
    private SaveGameData saveData;
    private int selectedChestIndex = -1;

    private void Awake()
    {
        instance = this;

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() =>
            {
                UIManager.Instance.StartGame();
                SoundManager.Instance.PlayButtonClickSound();
            });
        }

        if (chestPreviewPanel == null)
        {
            ChestPreviewPanel[] previewPanels = FindObjectsByType<ChestPreviewPanel>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            chestPreviewPanel = previewPanels.Length > 0 ? previewPanels[0] : null;
        }

        chestPreviewPanel?.Hide();
        LoadAndRefresh();
    }

    private void OnEnable()
    {
        LoadAndRefresh();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        if (saveData == null || saveData.chestSlots == null)
        {
            return;
        }

        bool stateChanged = false;
        for (int i = 0; i < saveData.chestSlots.Length; i++)
        {
            ChestSlotSaveData slot = saveData.chestSlots[i];
            if (slot == null || !slot.hasChest)
            {
                continue;
            }

            if ((ChestSlotState)slot.state == ChestSlotState.Unlocking)
            {
                if (slot.unlockEndUtc <= GetUtcNowSeconds())
                {
                    slot.state = (int)ChestSlotState.ReadyToOpen;
                    stateChanged = true;
                }
                else if (chestInfos != null && i < chestInfos.Count && chestInfos[i] != null)
                {
                    chestInfos[i].RefreshTimer(slot);
                }
            }
        }

        if (stateChanged)
        {
            SaveCurrentData();
            RefreshChestSlots();
        }
    }

    public static void AwardChestForCompletedWave(int completedWave)
    {
        /* if (!IsChestRewardAvailableForWave(completedWave))
        {
            GameViewScreen.Instance.RefreshChestAvailabilityVisuals();
            return;
        } */

        bool chestAwarded = false;
        if (completedWave == 4)
        {
            chestAwarded = AwardChest(instance != null ? instance.wave4ChestType : ChestType.Common);
            if (chestAwarded)
            {
                GameViewScreen.Instance.wave4triggered = true;
            }
        }
        else if (completedWave == 8)
        {
            chestAwarded = AwardChest(instance != null ? instance.wave8ChestType : ChestType.Rare);
            if (chestAwarded)
            {
                GameViewScreen.Instance.wave8triggered = true;
            }
        }

        if (chestAwarded)
        {
            MarkChestRewardClaimed(completedWave);
            // GameViewScreen.Instance.RefreshChestAvailabilityVisuals();
        }
    }

    public static bool IsChestRewardAvailableForWave(int completedWave)
    {
        SaveGameData data = GameSaveSystem.Load();
        EnsureChestRewardLevel(data, GetCurrentLevelNumber());

        return completedWave switch
        {
            4 => !data.wave4ChestClaimed,
            8 => !data.wave8ChestClaimed,
            _ => false,
        };
    }

    public static int GetEmptyChestSlotCount()
    {
        SaveGameData data = GameSaveSystem.Load();
        EnsureChestSlots(data);

        int emptySlotCount = 0;
        for (int i = 0; i < data.chestSlots.Length; i++)
        {
            if (data.chestSlots[i] == null || !data.chestSlots[i].hasChest)
            {
                emptySlotCount++;
            }
        }

        return emptySlotCount;
    }

    public static void ResetChestRewardsForLevel(int levelNumber)
    {
        SaveGameData data = GameSaveSystem.Load();
        int currentLevel = Mathf.Max(1, levelNumber);
        data.chestRewardLevel = currentLevel;
        data.wave4ChestClaimed = false;
        data.wave8ChestClaimed = false;
        GameSaveSystem.Save(data);
        GameViewScreen.Instance.RefreshChestAvailabilityVisuals();
    }

    public static bool AwardChest(ChestType chestType)
    {
        SaveGameData data = GameSaveSystem.Load();
        EnsureChestSlots(data);

        int slotIndex = FindEmptyChestSlot(data.chestSlots);
        if (slotIndex < 0)
        {
            Debug.Log("Chest reward skipped because all chest slots are full.");
            return false;
        }

        ChestDefinition definition = instance != null
            ? instance.GetChestDefinition(chestType)
            : ChestDefinition.CreateFallback(chestType);

        data.chestSlots[slotIndex] = CreateChestSlot(chestType, definition);
        GameSaveSystem.Save(data);

        if (instance != null)
        {
            instance.LoadAndRefresh();
        }

        return true;
    }

    public static long GetUtcNowSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static string FormatDuration(float durationSeconds)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, durationSeconds));
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        return hours > 0
            ? $"{hours:00}:{minutes:00}:{seconds:00}"
            : $"{minutes:00}:{seconds:00}";
    }

    private void LoadAndRefresh()
    {
        saveData = GameSaveSystem.Load();
        EnsureChestSlots(saveData);
        ResolveCompletedTimers();
        SaveCurrentData();
        RefreshChestSlots();
    }

    private void RefreshChestSlots()
    {
        if (chestInfos == null)
        {
            return;
        }

        for (int i = 0; i < chestInfos.Count; i++)
        {
            ChestInfo chestInfo = chestInfos[i];
            if (chestInfo == null)
            {
                continue;
            }

            ChestSlotSaveData slot = i < saveData.chestSlots.Length ? saveData.chestSlots[i] : null;
            ChestDefinition definition = slot != null
                ? GetChestDefinition((ChestType)slot.chestType)
                : null;
            int capturedIndex = i;

            chestInfo.Bind(
                slot,
                definition,
                () => ShowChestPreview(capturedIndex),
                () => OpenChest(capturedIndex),
                () => ReduceChestTimer(capturedIndex));
        }
    }

    private void ShowChestPreview(int chestIndex)
    {
        if (!IsValidChestIndex(chestIndex) || chestPreviewPanel == null)
        {
            return;
        }
        SoundManager.Instance.PlayButtonClickSound();
        ResolveCompletedTimers();
        selectedChestIndex = chestIndex;
        ChestSlotSaveData slot = saveData.chestSlots[chestIndex];
        ChestDefinition definition = GetChestDefinition((ChestType)slot.chestType);
        chestPreviewPanel.Show(
            definition,
            slot,
            !HasActiveTimer(),
            UnlockSelectedChestWithTime,
            UnlockSelectedChestWithGems);
    }

    private void UnlockSelectedChestWithTime()
    {
        SoundManager.Instance.PlayButtonClickSound();

        if (!IsValidChestIndex(selectedChestIndex))
        {
            return;
        }

        ChestSlotSaveData slot = saveData.chestSlots[selectedChestIndex];
        ChestDefinition definition = GetChestDefinition((ChestType)slot.chestType);

        ResolveCompletedTimers();
        if (HasActiveTimer())
        {
            Debug.Log("Only one chest can unlock with time at once.");
            return;
        }

        slot.state = (int)ChestSlotState.Unlocking;
        slot.unlockEndUtc = GetUtcNowSeconds() + Mathf.CeilToInt(Mathf.Max(0f, definition.unlockDurationSeconds));
        SaveCurrentData();

        chestPreviewPanel?.Hide();
        RefreshChestSlots();
    }

    private void ReduceChestTimer(int chestIndex)
    {
        SoundManager.Instance.PlayButtonClickSound();
        if (!IsValidChestIndex(chestIndex))
        {
            return;
        }

        ChestSlotSaveData slot = saveData.chestSlots[chestIndex];
        if ((ChestSlotState)slot.state != ChestSlotState.Unlocking)
        {
            return;
        }

        long reductionSeconds = Mathf.Max(0, rvTimeReductionSeconds);
        slot.unlockEndUtc = Math.Max(GetUtcNowSeconds(), slot.unlockEndUtc - reductionSeconds);

        if (slot.unlockEndUtc <= GetUtcNowSeconds())
        {
            slot.state = (int)ChestSlotState.ReadyToOpen;
            slot.unlockEndUtc = 0;
        }

        SaveCurrentData();
        RefreshChestSlots();
    }

    private void ResolveCompletedTimers()
    {
        if (saveData == null || saveData.chestSlots == null)
        {
            return;
        }

        long now = GetUtcNowSeconds();
        for (int i = 0; i < saveData.chestSlots.Length; i++)
        {
            ChestSlotSaveData slot = saveData.chestSlots[i];
            if (slot == null
                || !slot.hasChest
                || (ChestSlotState)slot.state != ChestSlotState.Unlocking
                || slot.unlockEndUtc > now)
            {
                continue;
            }

            slot.state = (int)ChestSlotState.ReadyToOpen;
            slot.unlockEndUtc = 0;
        }
    }

    private void UnlockSelectedChestWithGems()
    {
        SoundManager.Instance.PlayButtonClickSound();

        if (!IsValidChestIndex(selectedChestIndex))
        {
            return;
        }

        ChestSlotSaveData slot = saveData.chestSlots[selectedChestIndex];
        ChestDefinition definition = GetChestDefinition((ChestType)slot.chestType);

        if (!PlayerCurrencySystem.TrySpendGems(definition.unlockGemCost))
        {
            Debug.Log("Not enough gems to unlock chest.");
            return;
        }

        slot.state = (int)ChestSlotState.ReadyToOpen;
        slot.unlockEndUtc = 0;
        SaveCurrentData();

        chestPreviewPanel?.Hide();
        RefreshChestSlots();
    }

    private void OpenChest(int chestIndex)
    {
        SoundManager.Instance.PlayButtonClickSound();

        if (!IsValidChestIndex(chestIndex))
        {
            return;
        }

        saveData.chestSlots[chestIndex] = new ChestSlotSaveData();
        SaveCurrentData();
        RefreshChestSlots();
    }

    private bool IsValidChestIndex(int chestIndex)
    {
        return saveData != null
            && saveData.chestSlots != null
            && chestIndex >= 0
            && chestIndex < saveData.chestSlots.Length
            && saveData.chestSlots[chestIndex] != null
            && saveData.chestSlots[chestIndex].hasChest;
    }

    private bool HasActiveTimer()
    {
        if (saveData == null || saveData.chestSlots == null)
        {
            return false;
        }

        for (int i = 0; i < saveData.chestSlots.Length; i++)
        {
            ChestSlotSaveData slot = saveData.chestSlots[i];
            if (slot != null
                && slot.hasChest
                && (ChestSlotState)slot.state == ChestSlotState.Unlocking)
            {
                return true;
            }
        }

        return false;
    }

    private ChestDefinition GetChestDefinition(ChestType chestType)
    {
        return chestConfig != null
            ? chestConfig.GetChest(chestType)
            : ChestDefinition.CreateFallback(chestType);
    }

    private void SaveCurrentData()
    {
        GameSaveSystem.Save(saveData);
    }

    private static void EnsureChestSlots(SaveGameData data)
    {
        if (data.chestSlots == null)
        {
            data.chestSlots = new ChestSlotSaveData[MaxChestSlots];
        }
        else if (data.chestSlots.Length != MaxChestSlots)
        {
            Array.Resize(ref data.chestSlots, MaxChestSlots);
        }

        for (int i = 0; i < data.chestSlots.Length; i++)
        {
            if (data.chestSlots[i] == null)
            {
                data.chestSlots[i] = new ChestSlotSaveData();
            }
        }
    }

    private static int FindEmptyChestSlot(ChestSlotSaveData[] chestSlots)
    {
        for (int i = 0; i < chestSlots.Length; i++)
        {
            if (chestSlots[i] == null || !chestSlots[i].hasChest)
            {
                return i;
            }
        }

        return -1;
    }

    private static ChestSlotSaveData CreateChestSlot(ChestType chestType, ChestDefinition definition)
    {
        ChestReward reward = definition != null ? definition.rewards : null;
        return new ChestSlotSaveData
        {
            hasChest = true,
            chestType = (int)chestType,
            state = (int)ChestSlotState.Locked,
            unlockEndUtc = 0,
            coinsReward = reward != null ? reward.coins : 0,
            gemsReward = reward != null ? reward.gems : 0,
            cardsReward = reward != null ? reward.cards : 0
        };
    }

    private static void MarkChestRewardClaimed(int completedWave)
    {
        SaveGameData data = GameSaveSystem.Load();
        EnsureChestRewardLevel(data, GetCurrentLevelNumber());

        if (completedWave == 4)
        {
            data.wave4ChestClaimed = true;
        }
        else if (completedWave == 8)
        {
            data.wave8ChestClaimed = true;
        }

        GameSaveSystem.Save(data);
    }

    private static void EnsureChestRewardLevel(SaveGameData data, int levelNumber)
    {
        int currentLevel = Mathf.Max(1, levelNumber);
        if (data.chestRewardLevel == currentLevel)
        {
            return;
        }

        data.chestRewardLevel = currentLevel;
        data.wave4ChestClaimed = false;
        data.wave8ChestClaimed = false;
        GameSaveSystem.Save(data);
    }

    private static int GetCurrentLevelNumber()
    {
        return LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : 1;
    }
}
