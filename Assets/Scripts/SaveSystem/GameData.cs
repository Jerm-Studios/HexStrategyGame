using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    // Game state
    public int currentTurnNumber;
    public string currentGameState;

    // Grid data
    public int gridWidth;
    public int gridHeight;

    // Unit data
    public List<UnitData> units = new List<UnitData>();

    // Save metadata
    public string saveName;
    public DateTime saveDate;
    public string gameVersion;

    public GameData()
    {
        // Default constructor
        saveDate = DateTime.Now;
        gameVersion = Application.version;
    }
}

[Serializable]
public class UnitData
{
    // Basic info
    public string unitName;
    public string unitDescription;
    public string unitFaction;

    // Position
    public int hexCoordX;
    public int hexCoordY;
    public int hexCoordZ;

    // Stats
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int defense;
    public int movementRange;
    public int attackRange;

    // Resources
    public int maxEnergy;
    public int currentEnergy;

    // State
    public bool hasMoved;
    public bool hasAttacked;

    // Status effects
    public List<StatusEffectData> statusEffects = new List<StatusEffectData>();
}

[Serializable]
public class StatusEffectData
{
    public string effectType;
    public int duration;
}
