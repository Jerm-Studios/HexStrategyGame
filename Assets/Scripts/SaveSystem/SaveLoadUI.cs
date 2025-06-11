using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SaveLoadUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject savePanel;
    public GameObject loadPanel;
    public InputField saveNameInput;
    public Transform saveSlotContainer;
    public Transform loadSlotContainer;
    public GameObject saveSlotPrefab;
    public GameObject loadSlotPrefab;

    private SaveLoadManager saveLoadManager;

    private void Start()
    {
        saveLoadManager = SaveLoadManager.Instance;
        if (saveLoadManager == null)
        {
            Debug.LogError("SaveLoadManager not found!");
            return;
        }

        // Hide panels initially
        if (savePanel != null) savePanel.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(false);

        // Refresh save files
        RefreshSaveSlots();
    }

    public void ToggleSavePanel()
    {
        if (savePanel != null)
        {
            bool isActive = savePanel.activeSelf;
            savePanel.SetActive(!isActive);

            if (!isActive)
            {
                // Clear input field
                if (saveNameInput != null)
                {
                    saveNameInput.text = $"Save {System.DateTime.Now:yyyy-MM-dd HH:mm}";
                }

                // Refresh save slots
                RefreshSaveSlots();
            }

            // Hide load panel
            if (loadPanel != null) loadPanel.SetActive(false);

            // Play sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound(isActive ? "MenuClose" : "MenuOpen");
            }
        }
    }

    public void ToggleLoadPanel()
    {
        if (loadPanel != null)
        {
            bool isActive = loadPanel.activeSelf;
            loadPanel.SetActive(!isActive);

            if (!isActive)
            {
                // Refresh save slots
                RefreshSaveSlots();
            }

            // Hide save panel
            if (savePanel != null) savePanel.SetActive(false);

            // Play sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound(isActive ? "MenuClose" : "MenuOpen");
            }
        }
    }

    public void SaveGame()
    {
        if (saveLoadManager == null || saveNameInput == null)
            return;

        string saveName = saveNameInput.text;
        if (string.IsNullOrEmpty(saveName))
        {
            saveName = $"Save {System.DateTime.Now:yyyy-MM-dd HH:mm}";
        }

        bool success = saveLoadManager.SaveGameToNewSlot(saveName);

        if (success)
        {
            // Play success sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
            }

            // Hide save panel
            if (savePanel != null) savePanel.SetActive(false);

            // Show notification
            Debug.Log("Game saved successfully!");
        }
        else
        {
            // Play error sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
            }

            // Show error
            Debug.LogError("Failed to save game!");
        }
    }

    public void LoadGame(string filePath)
    {
        if (saveLoadManager == null)
            return;

        bool success = saveLoadManager.LoadGameFromFile(filePath);

        if (success)
        {
            // Play success sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
            }

            // Hide load panel
            if (loadPanel != null) loadPanel.SetActive(false);

            // Show notification
            Debug.Log("Game loaded successfully!");
        }
        else
        {
            // Play error sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
            }

            // Show error
            Debug.LogError("Failed to load game!");
        }
    }

    public void DeleteSave(string filePath)
    {
        if (saveLoadManager == null)
            return;

        bool success = saveLoadManager.DeleteSaveFile(filePath);

        if (success)
        {
            // Play success sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
            }

            // Refresh save slots
            RefreshSaveSlots();

            // Show notification
            Debug.Log("Save file deleted!");
        }
        else
        {
            // Play error sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
            }

            // Show error
            Debug.LogError("Failed to delete save file!");
        }
    }

    private void RefreshSaveSlots()
    {
        if (saveLoadManager == null)
            return;

        // Refresh save files
        saveLoadManager.RefreshSaveFiles();
        List<SaveFileInfo> saveFiles = saveLoadManager.GetSaveFiles();

        // Clear save slot container
        if (saveSlotContainer != null)
        {
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // Create save slots
            for (int i = 0; i < saveLoadManager.maxSaveSlots; i++)
            {
                // Find save file for this slot
                SaveFileInfo saveInfo = null;
                foreach (SaveFileInfo info in saveFiles)
                {
                    if (info.FileName == $"{saveLoadManager.saveFilePrefix}{i}")
                    {
                        saveInfo = info;
                        break;
                    }
                }

                // Create save slot
                GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
                SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(i, saveInfo, this);
                }
            }
        }

        // Clear load slot container
        if (loadSlotContainer != null)
        {
            foreach (Transform child in loadSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // Create load slots
            foreach (SaveFileInfo saveInfo in saveFiles)
            {
                GameObject slotObj = Instantiate(loadSlotPrefab, loadSlotContainer);
                LoadSlotUI slotUI = slotObj.GetComponent<LoadSlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(saveInfo, this);
                }
            }
        }
    }
}
