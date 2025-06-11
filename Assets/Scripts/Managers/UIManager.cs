using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI turnDisplayText;
    public GameObject unitInfoPanel;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public Button endTurnButton;
    public GameObject settingsPanel;

    public void Initialize()
    {
        Debug.Log("UIManager initialized");

        //Set up End Turn Button
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(EndTurnButtonClicked);
            Debug.Log("End Turn Button Initialized");
        }


        UpdateTurnDisplay(GameManager.GameState.PlayerTurn);
    }

    public void UpdateTurnDisplay(GameManager.GameState currentState)
    {
        if (turnDisplayText != null)
        {
            switch (currentState)
            {
                case GameManager.GameState.PlayerTurn:
                    turnDisplayText.text = "Player Turn";
                    if (endTurnButton != null) endTurnButton.interactable = true;
                    break;
                case GameManager.GameState.EnemyTurn:
                    turnDisplayText.text = "Enemy Turn";
                    if (endTurnButton != null) endTurnButton.interactable = false;
                    break;
                case GameManager.GameState.Victory:
                    turnDisplayText.text = "Victory!";
                    if (endTurnButton != null) endTurnButton.interactable = false;
                    break;
                case GameManager.GameState.Defeat:
                    turnDisplayText.text = "Defeat!";
                    if (endTurnButton != null) endTurnButton.interactable = false;
                    break;
                default:
                    turnDisplayText.text = "Game Setup";
                    if (endTurnButton != null) endTurnButton.interactable = false;
                    break;
            }
        }
    }

    public void ShowUnitInfo(Unit unit)
    {
        if (unitInfoPanel != null)
        {
            unitInfoPanel.SetActive(true);

 

            if (nameText != null) nameText.text = unit.UnitName;
            if (healthText != null) healthText.text = $"HP: {unit.CurrentHealth}/{unit.MaxHealth}";
            if (statsText != null) statsText.text = $"ATK: {unit.BaseAttack}  DEF: {unit.Defense}  MOV: {unit.MovementRange} RANGE: {unit.AttackRange}";
        }
    }

    public void HideUnitInfo()
    {
        if (unitInfoPanel != null)
        {
            unitInfoPanel.SetActive(false);
        }
    }

    private void EndTurnButtonClicked()
    {
        Debug.Log("End Turn Button Clicked");
        if(GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn)
        {
            GameManager.Instance.TurnManager.EndPlayerTurn();
        }

        //Change the Turn Text
        UpdateTurnDisplay(GameManager.Instance.CurrentState);
    }
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);

            // Play sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlayUISound(isActive ? "MenuClose" : "MenuOpen");
            }
        }
    }
}
