using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    [Header("Unit Prefabs")]
    public GameObject playerUnitPrefab;
    public GameObject enemyUnitPrefab;

    // List of all units in the game
    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();

    public void Initialize()
    {
        Debug.Log("UnitManager initialized");
        SpawnInitialUnits();
    }

    public void SpawnInitialUnits()
    {
        // Clear existing units
        ClearAllUnits();

        // Get the grid manager
        GridManager gridManager = GameManager.Instance.GridManager;
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        // Spawn player units
        SpawnPlayerUnit("Scout", gridManager.GetTileAt(new Vector3Int(-3, 0, 3)), 100, 10, 5, 3, 1);
        SpawnPlayerUnit("Soldier", gridManager.GetTileAt(new Vector3Int(-2, -1, 3)), 120, 15, 8, 2, 1);
        SpawnPlayerUnit("Sniper", gridManager.GetTileAt(new Vector3Int(-1, -2, 3)), 80, 20, 3, 2, 3);

        // Spawn enemy units
        SpawnEnemyUnit("Enemy Scout", gridManager.GetTileAt(new Vector3Int(3, -3, 0)), 90, 8, 4, 3, 1);
        SpawnEnemyUnit("Enemy Soldier", gridManager.GetTileAt(new Vector3Int(2, -3, 1)), 110, 12, 7, 2, 1);
        SpawnEnemyUnit("Enemy Sniper", gridManager.GetTileAt(new Vector3Int(1, -3, 2)), 70, 18, 2, 2, 3);

        Debug.Log($"Spawned {playerUnits.Count} player units and {enemyUnits.Count} enemy units");
    }

    private Unit SpawnPlayerUnit(string name, HexTile tile, int health, int attack, int defense, int moveRange, int attackRange)
    {
        if (tile == null || tile.IsOccupied())
            return null;

        GameObject unitObj = Instantiate(playerUnitPrefab, transform);
        Unit unit = unitObj.GetComponent<Unit>();

        if (unit != null)
        {
            unit.Initialize(name, Faction.Player, health, attack, defense, moveRange, attackRange);
            unit.SetPosition(tile);
            playerUnits.Add(unit);
        }

        return unit;
    }

    private Unit SpawnEnemyUnit(string name, HexTile tile, int health, int attack, int defense, int moveRange, int attackRange)
    {
        if (tile == null || tile.IsOccupied())
            return null;

        GameObject unitObj = Instantiate(enemyUnitPrefab, transform);
        Unit unit = unitObj.GetComponent<Unit>();

        if (unit != null)
        {
            unit.Initialize(name, Faction.Enemy, health, attack, defense, moveRange, attackRange);
            unit.SetPosition(tile);
            enemyUnits.Add(unit);
        }

        return unit;
    }

    public void ClearAllUnits()
    {
        // Destroy all units
        List<Unit> allUnits = new List<Unit>();
        allUnits.AddRange(playerUnits);
        allUnits.AddRange(enemyUnits);

        foreach (Unit unit in allUnits)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }

        // Clear the lists
        playerUnits.Clear();
        enemyUnits.Clear();
    }

    public List<Unit> GetPlayerUnits()
    {
        return playerUnits;
    }

    public List<Unit> GetEnemyUnits()
    {
        return enemyUnits;
    }

    public void ResetAllUnitTurns(Faction faction)
    {
        if (faction == Faction.Player)
        {
            foreach (Unit unit in playerUnits)
            {
                if (unit != null)
                {
                    unit.ResetTurn();
                }
            }
        }
        else if (faction == Faction.Enemy)
        {
            foreach (Unit unit in enemyUnits)
            {
                if (unit != null)
                {
                    unit.ResetTurn();
                }
            }
        }
    }
   
    public Unit SpawnUnitAt(Faction faction, string unitName, HexTile tile)
    {
        if (tile == null || tile.IsOccupied())
        {
            Debug.LogWarning($"Cannot spawn unit at tile: {tile}");
            return null;
        }

        GameObject unitPrefab = null;

        // Get the appropriate prefab based on faction
        switch (faction)
        {
            case Faction.Player:
                unitPrefab = playerUnitPrefab;
                break;
            case Faction.Enemy:
                unitPrefab = enemyUnitPrefab;
                break;
            default:
                Debug.LogWarning($"Unsupported faction: {faction}");
                return null;
        }

        if (unitPrefab == null)
        {
            Debug.LogError($"Unit prefab not found for faction: {faction}");
            return null;
        }

        // Instantiate the unit
        GameObject unitObj = Instantiate(unitPrefab, tile.transform.position, Quaternion.identity);
        unitObj.transform.parent = transform;

        // Get the Unit component
        Unit unit = unitObj.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError("Unit component not found on prefab");
            Destroy(unitObj);
            return null;
        }

        // Set unit properties
        unit.UnitName = unitName;
        unit.UnitFaction = faction;

        // Place the unit on the tile
        unit.SetPosition(tile);

        // Add to appropriate list
        if (faction == Faction.Player)
        {
            playerUnits.Add(unit);
        }
        else if (faction == Faction.Enemy)
        {
            enemyUnits.Add(unit);
        }

        return unit;
    }
}
