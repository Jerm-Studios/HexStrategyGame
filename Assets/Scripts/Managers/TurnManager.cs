using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [Header("Turn Settings")]
    public float turnTransitionDelay = 0.5f;
    public int currentTurnNumber = 1;

    private bool isProcessingTurn = false;
    private AIController aiController;

    public void Initialize()
    {
        Debug.Log("TurnManager initialized");

        // Find or add AIController
        aiController = GetComponent<AIController>();
        if (aiController == null)
        {
            aiController = gameObject.AddComponent<AIController>();
        }
        aiController.Initialize();
    }

    public void StartFirstTurn()
    {
        currentTurnNumber = 1;
        StartPlayerTurn();
        Debug.Log("Start First Turn Initialized");
    }

    public void StartPlayerTurn()
    {
        if (isProcessingTurn)
            return;

        isProcessingTurn = true;

        Debug.Log($"Starting Player Turn {currentTurnNumber}");

        // Reset all player units
        GameManager.Instance.UnitManager.ResetAllUnitTurns(Faction.Player);

        // Update game state
        GameManager.Instance.ChangeState(GameManager.GameState.PlayerTurn);

        // Update UI
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.UpdateTurnDisplay(GameManager.GameState.PlayerTurn);
        }

        isProcessingTurn = false;
    }

    public void EndPlayerTurn()
    {
        if (isProcessingTurn)
            return;

        isProcessingTurn = true;

        Debug.Log("Ending Player Turn");

        // Start enemy turn after a short delay
        StartCoroutine(StartEnemyTurnAfterDelay());
    }

    public void CheckEndGameState()
    {
        CheckGameEndConditions();
    }

    private IEnumerator StartEnemyTurnAfterDelay()
    {
        yield return new WaitForSeconds(turnTransitionDelay);

        Debug.Log($"Starting Enemy Turn {currentTurnNumber}");

        // Reset all enemy units
        GameManager.Instance.UnitManager.ResetAllUnitTurns(Faction.Enemy);

        // Update game state
        GameManager.Instance.ChangeState(GameManager.GameState.EnemyTurn);

        // Update UI
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.UpdateTurnDisplay(GameManager.GameState.EnemyTurn);
        }

        // Process enemy turn
        yield return StartCoroutine(ProcessEnemyTurn());

        EndEnemyTurn();
        
    }

    private IEnumerator ProcessEnemyTurn()
    {
        // Use the AI controller to process the enemy turn
        if (aiController != null)
        {
            yield return StartCoroutine(aiController.ProcessEnemyTurn());
        }
        else
        {
            Debug.LogError("AIController not found!");
            yield return null;
        }

        // End enemy turn
        EndEnemyTurn();
    }

    private void EndEnemyTurn()
    {
        Debug.Log("Ending Enemy Turn");
        //Finish Processing turn
        isProcessingTurn = false;

        // Increment turn number
        currentTurnNumber++;

        // Check victory/defeat conditions
        CheckGameEndConditions();

        // Start player turn after a short delay
        StartCoroutine(StartPlayerTurnAfterDelay());
    }

    private IEnumerator StartPlayerTurnAfterDelay()
    {
        yield return new WaitForSeconds(turnTransitionDelay);
        StartPlayerTurn();
    }

    private void CheckGameEndConditions()
    {
        // Get unit counts
        List<Unit> playerUnits = GameManager.Instance.UnitManager.GetPlayerUnits();
        List<Unit> enemyUnits = GameManager.Instance.UnitManager.GetEnemyUnits();

        // Remove null entries (destroyed units)
        playerUnits.RemoveAll(unit => unit == null);
        enemyUnits.RemoveAll(unit => unit == null);

        // Check victory condition
        if (enemyUnits.Count == 0)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Victory);
        }
        // Check defeat condition
        else if (playerUnits.Count == 0)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Defeat);
        }
    }
}
