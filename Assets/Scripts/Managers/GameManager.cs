using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton pattern implementation
    public static GameManager Instance { get; private set; }

    // References to other managers
    [HideInInspector] public GridManager GridManager;
    [HideInInspector] public UnitManager UnitManager;
    [HideInInspector] public TurnManager TurnManager;
    [HideInInspector] public CombatManager CombatManager;
    [HideInInspector] public InputManager InputManager;
    [HideInInspector] public UIManager UIManager;
    [HideInInspector] public AudioManager AudioManager;
    [HideInInspector] public VisualManager VisualManager;
    [HideInInspector] public SaveLoadManager SaveLoadManager;

    // Game state
    public enum GameState
    {
        Setup,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize game state
        CurrentState = GameState.Setup;
    }

    private void Start()
    {
        // Find and store references to all managers
        GridManager = FindFirstObjectByType<GridManager>();
        UnitManager = FindFirstObjectByType<UnitManager>();
        TurnManager = FindFirstObjectByType<TurnManager>();
        CombatManager = FindFirstObjectByType<CombatManager>();
        InputManager = FindFirstObjectByType<InputManager>();
        UIManager = FindFirstObjectByType<UIManager>();
        AudioManager = FindFirstObjectByType<AudioManager>();
        VisualManager = FindFirstObjectByType<VisualManager>();
        SaveLoadManager = FindFirstObjectByType<SaveLoadManager>();

        // Initialize the game
        InitializeGame();
    }

    private void InitializeGame()
    {
        // This will be called when the game starts
        Debug.Log("Game initializing...");

        // Initialize all managers
        if (GridManager != null) GridManager.InitializeGrid();
        if (UnitManager != null) UnitManager.Initialize();
        if (TurnManager != null) TurnManager.Initialize();
        if (CombatManager != null) CombatManager.Initialize();
        if (InputManager != null) InputManager.Initialize();   
        if (UIManager != null) UIManager.Initialize();
        if (AudioManager != null) AudioManager.Initialize();
        if (VisualManager != null) VisualManager.Initialize();
        if (SaveLoadManager != null) SaveLoadManager.Initialize();




        // Change game state to player turn
        ChangeState(GameState.PlayerTurn);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        // Update UI
        if (UIManager != null)
        {
            UIManager.UpdateTurnDisplay(newState);
        }

        // Play appropriate music for the new state
        if (AudioManager != null)
        {
            AudioManager.PlayGameStateMusic(newState);
            AudioManager.PlayTurnSound(newState);
        }

        switch (newState)
        {
            case GameState.PlayerTurn:
                Debug.Log("Player turn started");
                break;
            case GameState.EnemyTurn:
                Debug.Log("Enemy turn started");
                break;
            case GameState.Victory:
                Debug.Log("Victory!");
                break;
            case GameState.Defeat:
                Debug.Log("Defeat!");
                break;
        }
    }
    public void ResetGame()
    {
        // Clear the grid
        if (GridManager != null)
        {
            GridManager.ClearGrid();
        }

        // Clear units
        if (UnitManager != null)
        {
            UnitManager.ClearAllUnits();
        }

        // Reset game state
        CurrentState = GameState.Setup;
    }
}