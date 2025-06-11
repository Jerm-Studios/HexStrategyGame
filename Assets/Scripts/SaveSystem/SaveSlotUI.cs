using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Text slotNumberText;
    public Text saveNameText;
    public Text saveDateText;
    public Text turnNumberText;
    public Button saveButton;
    public Button deleteButton;

    private int slotIndex;
    private SaveFileInfo saveInfo;
    private SaveLoadUI saveLoadUI;

    public void Initialize(int index, SaveFileInfo info, SaveLoadUI ui)
    {
        slotIndex = index;
        saveInfo = info;
        saveLoadUI = ui;

        // Set slot number
        if (slotNumberText != null)
        {
            slotNumberText.text = $"Slot {index + 1}";
        }

        // Set save info if available
        if (info != null)
        {
            if (saveNameText != null)
            {
                saveNameText.text = info.SaveName;
            }

            if (saveDateText != null)
            {
                saveDateText.text = info.SaveDate.ToString("yyyy-MM-dd HH:mm");
            }

            if (turnNumberText != null)
            {
                turnNumberText.text = $"Turn {info.TurnNumber}";
            }

            // Enable delete button
            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(true);
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => OnDeleteClicked());
            }
        }
        else
        {
            // Empty slot
            if (saveNameText != null)
            {
                saveNameText.text = "Empty Slot";
            }

            if (saveDateText != null)
            {
                saveDateText.text = "";
            }

            if (turnNumberText != null)
            {
                turnNumberText.text = "";
            }

            // Disable delete button
            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(false);
            }
        }

        // Set up save button
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(() => OnSaveClicked());
        }
    }

    private void OnSaveClicked()
    {
        if (saveLoadUI != null)
        {
            // Get save name from input field
            string saveName = saveLoadUI.saveNameInput != null ?
                saveLoadUI.saveNameInput.text : $"Save {System.DateTime.Now:yyyy-MM-dd HH:mm}";

            // Save to this slot
            SaveLoadManager.Instance.SaveGameToSlot(slotIndex, saveName);

            // Refresh UI
            saveLoadUI.ToggleSavePanel();
        }
    }

    private void OnDeleteClicked()
    {
        if (saveInfo != null && saveLoadUI != null)
        {
            // Delete save file
            saveLoadUI.DeleteSave(saveInfo.FilePath);
        }
    }
}
