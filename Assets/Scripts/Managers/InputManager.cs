using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    private Camera mainCamera;

    [Header("Selection")]
    private HexTile selectedTile;
    private Unit selectedUnit;

    private void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("InputManager initialized");
    }

    private void Update()
    {
        // Process mouse input
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        // Play click sound
        if (GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlayUISound("ButtonClick");
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            // Check if we hit a hex tile
            HexTile tile = hit.collider.GetComponent<HexTile>();
            if (tile != null)
            {
                HandleTileClick(tile);
            }

            // Check if we hit a unit directly
            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit != null && unit.UnitFaction == Faction.Player)
            {
                HandleUnitClick(unit);
            }
        }
        else
        {
            // Clicked on empty space, deselect
            ClearSelection();
        }
    }

    private void SelectTile(HexTile tile)
    {
        // Clear previous selection
        if (selectedTile != null)
        {
            selectedTile.Highlight(false, Color.white);
        }

        // Set new selection
        selectedTile = tile;
        selectedTile.Highlight(true, Color.yellow);

        Debug.Log($"Selected tile at {tile.HexCoordinates}");

        // Check if there's a unit on this tile
        if (tile.IsOccupied())
        {
            SelectUnit(tile.OccupyingUnit);
        }
        else
        {
            // If we have a unit selected and clicked on an empty tile, we might want to move there
            if (selectedUnit != null && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn)
            {
                // This will be expanded later with pathfinding and movement validation
                Debug.Log($"Attempting to move unit to {tile.HexCoordinates}");
            }
        }
    }

    private void SelectUnit(Unit unit)
    {

        // Play select sound
        if (GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlayUISound("UnitSelect");
        }

        // Clear previous selection
        ClearSelection();

        // Set new selection
        selectedUnit = unit;
        selectedTile = unit.CurrentTile;
        selectedTile.Highlight(true, Color.yellow);

        Debug.Log($"Selected unit: {unit.UnitName}");

        // Show movement range if the unit hasn't moved yet
        if (!unit.HasMoved)
        {
            unit.ShowMovementRange();
        }

        // Show attack range if the unit hasn't attacked yet
        if (!unit.HasAttacked)
        {
            unit.ShowAttackRange();
        }

        // Show unit info in UI
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowUnitInfo(unit);
        }
    }

    public HexTile GetSelectedTile()
    {
        return selectedTile;
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    // Add these methods to your existing InputManager class



    private void HandleTileClick(HexTile tile)
    {
        // If we have a unit selected and clicked on an empty tile, try to move there
        if (selectedUnit != null && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn)
        {
            if (tile.IsOccupied())
            {
                // If the tile has an enemy unit and our selected unit can attack it
                if (tile.OccupyingUnit.UnitFaction == Faction.Enemy && selectedUnit.CanAttack(tile.OccupyingUnit))
                {
                    // Show combat preview first
                    if (GameManager.Instance.CombatManager != null)
                    {
                        GameManager.Instance.CombatManager.ShowCombatPreview(selectedUnit, tile.OccupyingUnit);
                    }

                    // Perform the attack
                    selectedUnit.Attack(tile.OccupyingUnit);
                    selectedUnit.HideAttackRange();

                    // Play attack animation sequence
                    if (GameManager.Instance.CombatManager != null)
                    {
                        StartCoroutine(GameManager.Instance.CombatManager.PerformAttackSequence(selectedUnit, tile.OccupyingUnit));
                    }
                }
                // If the tile has a player unit, select it
                else if (tile.OccupyingUnit.UnitFaction == Faction.Player)
                {
                    SelectUnit(tile.OccupyingUnit);
                }
            }
            else if (selectedUnit.CanMoveTo(tile))
            {
                // Move the selected unit to the tile
                selectedUnit.HideMovementRange();
                selectedUnit.MoveTo(tile);

                // After moving, show attack range if the unit hasn't attacked yet
                if (!selectedUnit.HasAttacked)
                {
                    selectedUnit.ShowAttackRange();
                }
            }
        }
        else
        {
            // Just select the tile
            SelectTile(tile);
        }
    }

    private void HandleUnitClick(Unit unit)
    {
        // Only select player units during player turn
        if (unit.UnitFaction == Faction.Player && GameManager.Instance.CurrentState == GameManager.GameState.PlayerTurn)
        {
            SelectUnit(unit);
        }
    }

   

    private void ClearSelection()
    {
        // Hide ranges
        if (selectedUnit != null)
        {
            selectedUnit.HideMovementRange();
            selectedUnit.HideAttackRange();
        }

        // Clear tile highlight
        if (selectedTile != null)
        {
            selectedTile.Highlight(false, Color.white);
            selectedTile = null;
        }

        // Clear unit selection
        selectedUnit = null;

        // Hide unit info in UI
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.HideUnitInfo();
        }
    }

    public void Initialize()
    {
        Debug.Log("InputManager Initialized");
    }
}
