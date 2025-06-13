using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI saveNameText;
    public TextMeshProUGUI saveDateText;
    public TextMeshProUGUI turnNumberText;
    public Button loadButton;
    public Button deleteButton;

    private SaveFileInfo saveInfo;
    private SaveLoadUI saveLoadUI;

    public void Initialize(SaveFileInfo info, SaveLoadUI ui)
    {
        saveInfo = info;
        saveLoadUI = ui;

        // Set save info
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

        // Set up load button
        if (loadButton != null)
        {
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => OnLoadClicked());
        }

        // Set up delete button
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => OnDeleteClicked());
        }
    }

    private void OnLoadClicked()
    {
        if (saveInfo != null && saveLoadUI != null)
        {
            // Load game
            saveLoadUI.LoadGame(saveInfo.FilePath);
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
