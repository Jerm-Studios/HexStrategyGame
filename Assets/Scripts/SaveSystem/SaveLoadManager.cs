using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadManager : MonoBehaviour
{
    [Header("Save Settings")]
    public string saveFilePrefix = "save_";
    public string saveFileExtension = ".sav";
    public int maxSaveSlots = 5;
    public bool useEncryption = false;

    // Singleton pattern
    public static SaveLoadManager Instance { get; private set; }

    // Path to save directory
    private string SaveDirectory => Application.persistentDataPath + "/Saves/";

    // List of available save files
    private List<SaveFileInfo> saveFiles = new List<SaveFileInfo>();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create save directory if it doesn't exist
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        // Load save file list
        RefreshSaveFiles();
    }

    public void Initialize()
    {
        Debug.Log("SaveLoadManager initialized");
    }

    public void RefreshSaveFiles()
    {
        saveFiles.Clear();

        // Get all save files
        string[] files = Directory.GetFiles(SaveDirectory, $"{saveFilePrefix}*{saveFileExtension}");

        foreach (string file in files)
        {
            try
            {
                // Try to load save metadata
                GameData saveData = LoadGameDataFromFile(file);
                if (saveData != null)
                {
                    SaveFileInfo info = new SaveFileInfo
                    {
                        FilePath = file,
                        FileName = Path.GetFileNameWithoutExtension(file),
                        SaveName = saveData.saveName,
                        SaveDate = saveData.saveDate,
                        TurnNumber = saveData.currentTurnNumber
                    };

                    saveFiles.Add(info);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load save file {file}: {e.Message}");
            }
        }

        // Sort by date (newest first)
        saveFiles.Sort((a, b) => b.SaveDate.CompareTo(a.SaveDate));
    }

    public List<SaveFileInfo> GetSaveFiles()
    {
        return saveFiles;
    }

    public bool SaveGameToSlot(int slotIndex, string saveName = "")
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"Invalid save slot: {slotIndex}");
            return false;
        }

        string fileName = $"{saveFilePrefix}{slotIndex}{saveFileExtension}";
        string filePath = Path.Combine(SaveDirectory, fileName);

        return SaveGameToFile(filePath, saveName);
    }

    public bool SaveGameToNewSlot(string saveName = "")
    {
        // Find the first available slot
        for (int i = 0; i < maxSaveSlots; i++)
        {
            string fileName = $"{saveFilePrefix}{i}{saveFileExtension}";
            string filePath = Path.Combine(SaveDirectory, fileName);

            if (!File.Exists(filePath))
            {
                return SaveGameToSlot(i, saveName);
            }
        }

        // If all slots are used, use the oldest one
        RefreshSaveFiles();
        if (saveFiles.Count > 0)
        {
            // Get the oldest save
            SaveFileInfo oldestSave = saveFiles[saveFiles.Count - 1];
            for (int i = 0; i < maxSaveSlots; i++)
            {
                if (oldestSave.FileName == $"{saveFilePrefix}{i}")
                {
                    return SaveGameToSlot(i, saveName);
                }
            }
        }

        // If we couldn't find a slot, use slot 0
        return SaveGameToSlot(0, saveName);
    }

    public bool SaveGameToFile(string filePath, string saveName = "")
    {
        try
        {
            // Create game data
            GameData gameData = CreateGameData(saveName);

            // Serialize and save
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Create(filePath);

            if (useEncryption)
            {
                // Simple XOR encryption (for demonstration)
                string json = JsonUtility.ToJson(gameData);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] ^ 42); // XOR with key 42
                }
                file.Write(bytes, 0, bytes.Length);
            }
            else
            {
                formatter.Serialize(file, gameData);
            }

            file.Close();

            Debug.Log($"Game saved to {filePath}");
            RefreshSaveFiles();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            return false;
        }
    }

    public bool LoadGameFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"Invalid save slot: {slotIndex}");
            return false;
        }

        string fileName = $"{saveFilePrefix}{slotIndex}{saveFileExtension}";
        string filePath = Path.Combine(SaveDirectory, fileName);

        return LoadGameFromFile(filePath);
    }

    public bool LoadGameFromFile(string filePath)
    {
        try
        {
            GameData gameData = LoadGameDataFromFile(filePath);
            if (gameData == null)
            {
                return false;
            }

            // Apply the loaded data to the game
            ApplyGameData(gameData);

            Debug.Log($"Game loaded from {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    private GameData LoadGameDataFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found: {filePath}");
            return null;
        }

        try
        {
            GameData gameData;

            if (useEncryption)
            {
                // Simple XOR decryption
                byte[] bytes = File.ReadAllBytes(filePath);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] ^ 42); // XOR with key 42
                }
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                gameData = JsonUtility.FromJson<GameData>(json);
            }
            else
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream file = File.Open(filePath, FileMode.Open);
                gameData = (GameData)formatter.Deserialize(file);
                file.Close();
            }

            return gameData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save data: {e.Message}");
            return null;
        }
    }

    private GameData CreateGameData(string saveName)
    {
        GameData gameData = new GameData();

        // Set save name
        gameData.saveName = string.IsNullOrEmpty(saveName) ?
            $"Save {DateTime.Now:yyyy-MM-dd HH:mm}" : saveName;

        // Get game state from GameManager
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameData.currentGameState = gameManager.CurrentState.ToString();

            // Get turn number from TurnManager
            if (gameManager.TurnManager != null)
            {
                gameData.currentTurnNumber = gameManager.TurnManager.currentTurnNumber;
            }

            // Get grid data from GridManager
            if (gameManager.GridManager != null)
            {
                gameData.gridWidth = gameManager.GridManager.gridWidth;
                gameData.gridHeight = gameManager.GridManager.gridHeight;
            }

            // Get unit data from UnitManager
            if (gameManager.UnitManager != null)
            {
                List<Unit> allUnits = new List<Unit>();
                allUnits.AddRange(gameManager.UnitManager.GetPlayerUnits());
                allUnits.AddRange(gameManager.UnitManager.GetEnemyUnits());

                foreach (Unit unit in allUnits)
                {
                    if (unit == null) continue;

                    UnitData unitData = new UnitData
                    {
                        unitName = unit.UnitName,
                        unitDescription = unit.UnitDescription,
                        unitFaction = unit.UnitFaction.ToString(),

                        hexCoordX = unit.HexCoordinates.x,
                        hexCoordY = unit.HexCoordinates.y,
                        hexCoordZ = unit.HexCoordinates.z,

                        maxHealth = unit.MaxHealth,
                        currentHealth = unit.CurrentHealth,
                        attack = unit.BaseAttack,
                        defense = unit.Defense,
                        movementRange = unit.MovementRange,
                        attackRange = unit.AttackRange,

                        maxEnergy = unit.MaxEnergy,
                        currentEnergy = unit.CurrentEnergy,

                        hasMoved = unit.HasMoved,
                        hasAttacked = unit.HasAttacked

                        // Status effects would be added here if implemented
                    };

                    gameData.units.Add(unitData);
                }
            }
        }

        return gameData;
    }

    private void ApplyGameData(GameData gameData)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        // Reset the game state
        gameManager.ResetGame();

        // Initialize grid with saved dimensions
        if (gameManager.GridManager != null)
        {
            gameManager.GridManager.gridWidth = gameData.gridWidth;
            gameManager.GridManager.gridHeight = gameData.gridHeight;
            gameManager.GridManager.InitializeGrid();
        }

        // Spawn units from saved data
        if (gameManager.UnitManager != null)
        {
            foreach (UnitData unitData in gameData.units)
            {
                // Convert string faction to enum
                Faction faction = (Faction)Enum.Parse(typeof(Faction), unitData.unitFaction);

                // Create hex coordinates
                Vector3Int hexCoord = new Vector3Int(unitData.hexCoordX, unitData.hexCoordY, unitData.hexCoordZ);

                // Get the tile at these coordinates
                HexTile tile = gameManager.GridManager.GetTileAt(hexCoord);
                if (tile == null)
                {
                    Debug.LogWarning($"Could not find tile at coordinates {hexCoord}");
                    continue;
                }

                // Spawn the unit
                Unit unit = gameManager.UnitManager.SpawnUnitAt(
                    faction, unitData.unitName, tile);

                if (unit != null)
                {
                    // Apply unit data
                    unit.UnitDescription = unitData.unitDescription;
                    unit.MaxHealth = unitData.maxHealth;
                    unit.CurrentHealth = unitData.currentHealth;
                    unit.BaseAttack = unitData.attack;
                    unit.Defense = unitData.defense;
                    unit.MovementRange = unitData.movementRange;
                    unit.AttackRange = unitData.attackRange;
                    unit.MaxEnergy = unitData.maxEnergy;
                    unit.CurrentEnergy = unitData.currentEnergy;

                    // Apply unit state
                    if (unitData.hasMoved) unit.MarkAsMoved();
                    if (unitData.hasAttacked) unit.MarkAsAttacked();

                    // Apply status effects if implemented
                }
            }
        }

        // Set turn number
        if (gameManager.TurnManager != null)
        {
            gameManager.TurnManager.currentTurnNumber = gameData.currentTurnNumber;
        }

        // Set game state
        if (Enum.TryParse(gameData.currentGameState, out GameManager.GameState state))
        {
            gameManager.ChangeState(state);
        }
    }

    public bool DeleteSaveFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                RefreshSaveFiles();
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
            return false;
        }
    }

    public bool DeleteSaveSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"Invalid save slot: {slotIndex}");
            return false;
        }

        string fileName = $"{saveFilePrefix}{slotIndex}{saveFileExtension}";
        string filePath = Path.Combine(SaveDirectory, fileName);

        return DeleteSaveFile(filePath);
    }
}

// Class to store save file information
[Serializable]
public class SaveFileInfo
{
    public string FilePath;
    public string FileName;
    public string SaveName;
    public DateTime SaveDate;
    public int TurnNumber;
}
