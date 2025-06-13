using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using MoreMountains.Feedbacks;

public class Unit : MonoBehaviour
{
    [Header("Unit Info")]
    public string UnitName;
    public string UnitDescription;
    public Faction UnitFaction;

    [Header("Stats")]
    public int MaxHealth = 100;
    public int CurrentHealth;
    public int BaseAttack = 10;
    public int Defense = 5;
    public int MovementRange = 3;
    public int AttackRange = 1;

    [Header("Resources")]
    public int MaxEnergy = 100;
    public int CurrentEnergy;

    [Header("References")]
    public SpriteRenderer SpriteRenderer;
    public Animator UnitAnimator;

    [Header("Combat Stats")]
    public int CriticalHitChance = 10; // Percentage chance (0-100)
    public float CriticalHitMultiplier = 1.5f;
    public int DodgeChance = 5; // Percentage chance (0-100)
    public int CounterAttackChance = 30; // Percentage chance (0-100)

    // Combat feedback
    private TextMesh combatTextMesh;
    private float textDisplayDuration = 1.5f;

    // Position tracking
    public Vector3Int HexCoordinates { get; private set; }
    public HexTile CurrentTile { get; private set; }

    // State tracking
    public bool HasMoved { get; private set; }
    public bool HasAttacked { get; private set; }

    // Pathfinding
    private List<HexTile> currentPath = new List<HexTile>();

    //More Mountain Feel!
    [SerializeField] private MMF_Player movementFeedback;
    [SerializeField] private MMF_Player attackFeedback;
    [SerializeField] private MMF_Player damageFeedback;
    [SerializeField] private MMF_Player deathFeedback;

    private void Awake()
    {
        // Initialize health and energy
        CurrentHealth = MaxHealth;
        CurrentEnergy = MaxEnergy;

        SetupCombatText();
    }

    public void Initialize(string name, Faction faction, int health, int attack, int defense, int moveRange, int attackRange)
    {
        UnitName = name;
        UnitFaction = faction;
        MaxHealth = health;
        CurrentHealth = health;
        BaseAttack = attack;
        Defense = defense;
        MovementRange = moveRange;
        AttackRange = attackRange;
    }

    public void SetPosition(HexTile tile)
    {
        // Clear the old tile
        if (CurrentTile != null)
        {
            CurrentTile.ClearUnit();
        }

        // Set the new tile
        CurrentTile = tile;
        if (tile != null)
        {
            HexCoordinates = tile.HexCoordinates;
            tile.SetUnit(this);

            // Update world position
            transform.position = GameManager.Instance.GridManager.HexToWorldPosition(HexCoordinates);
        }
    }

    public void ShowMovementRange()
    {
        if (GameManager.Instance.GridManager != null)
        {
            // This will be expanded with proper pathfinding later
            List<HexTile> tilesInRange = GetTilesInRange(CurrentTile, MovementRange);

            foreach (HexTile tile in tilesInRange)
            {
                if (!tile.IsOccupied())
                {
                    tile.Highlight(true, Color.blue);
                }
            }
        }
    }

    public void HideMovementRange()
    {
        if (GameManager.Instance.GridManager != null)
        {
            List<HexTile> tilesInRange = GetTilesInRange(CurrentTile, MovementRange);

            foreach (HexTile tile in tilesInRange)
            {
                tile.Highlight(false, Color.white);
            }
        }
    }

    public void ShowAttackRange()
    {
        if (GameManager.Instance.GridManager != null)
        {
            List<HexTile> tilesInRange = GetTilesInRange(CurrentTile, AttackRange);

            foreach (HexTile tile in tilesInRange)
            {
                if (tile.IsOccupied() && tile.OccupyingUnit.UnitFaction != UnitFaction)
                {
                    tile.Highlight(true, Color.red);
                }
            }
        }
    }

    public void HideAttackRange()
    {
        if (GameManager.Instance.GridManager != null)
        {
            List<HexTile> tilesInRange = GetTilesInRange(CurrentTile, AttackRange);

            foreach (HexTile tile in tilesInRange)
            {
                tile.Highlight(false, Color.white);
            }
        }
    }

    private List<HexTile> GetTilesInRange(HexTile centerTile, int range)
    {
        // This is a simple implementation that will be expanded with proper pathfinding later
        List<HexTile> tilesInRange = new List<HexTile>();

        // Get all tiles within a certain distance (this is not accurate for movement yet)
        for (int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
            {
                int s = -q - r;
                Vector3Int hexCoord = centerTile.HexCoordinates + new Vector3Int(q, r, s);

                HexTile tile = GameManager.Instance.GridManager.GetTileAt(hexCoord);
                if (tile != null)
                {
                    tilesInRange.Add(tile);
                }
            }
        }

        return tilesInRange;
    }

    public bool CanMoveTo(HexTile targetTile)
    {
        // Check if the unit has already moved this turn
        if (HasMoved)
            return false;

        // Check if the target tile is occupied
        if (targetTile.IsOccupied())
            return false;

        // Check if the target tile is within movement range
        // This will be expanded with proper pathfinding later
        List<HexTile> tilesInRange = GetTilesInRange(CurrentTile, MovementRange);
        return tilesInRange.Contains(targetTile);
    }

    public void MoveTo(HexTile targetTile)
    {
        if (CanMoveTo(targetTile))
        {
            // Play move sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlaySFX("UnitMove");
            }

            // Move the unit
            SetPosition(targetTile);

            // Mark as moved
            HasMoved = true;

            Debug.Log($"{UnitName} moved to {targetTile.HexCoordinates}");
        }
    }

    public bool CanAttack(Unit targetUnit)
    {
        // Check if the unit has already attacked this turn
        if (HasAttacked)
            return false;

        // Check if the target is an enemy
        if (targetUnit.UnitFaction == UnitFaction)
            return false;

        // Check if the target is within attack range
        // This will be expanded with proper line of sight checks later
        List<HexTile> tilesInRange = GetTilesInRange(CurrentTile, AttackRange);
        return tilesInRange.Contains(targetUnit.CurrentTile);
    }

    public void Attack(Unit targetUnit)
    {
        if (CanAttack(targetUnit))
        {
            // Play attack sound
            if (GameManager.Instance.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlaySFX("UnitAttack");
            }

            //Play attack feedbacks (Feel!)
            if (attackFeedback != null)
            {
                attackFeedback.PlayFeedbacks();
            }


            // Check if target dodges
            if (Random.Range(0, 100) < targetUnit.DodgeChance)
            {
                // Attack missed
                Debug.Log($"{UnitName}'s attack was dodged by {targetUnit.UnitName}!");
                ShowCombatText("Miss!", Color.white);
                targetUnit.ShowCombatText("Dodge!", Color.cyan);
            }
            else
            {
                // Calculate if critical hit
                bool isCritical = Random.Range(0, 100) < CriticalHitChance;

                // Calculate damage
                int baseDamage = Mathf.Max(1, BaseAttack - targetUnit.Defense);
                int damage = isCritical ? Mathf.RoundToInt(baseDamage * CriticalHitMultiplier) : baseDamage;

                // Apply damage
                targetUnit.TakeDamage(damage);

                // Show combat text
                if (isCritical)
                {
                    ShowCombatText("Critical!", Color.yellow);
                    targetUnit.ShowCombatText($"-{damage}", Color.red);
                }
                else
                {
                    ShowCombatText("Hit!", Color.white);
                    targetUnit.ShowCombatText($"-{damage}", Color.red);
                }

                // Check for counter attack
                if (!targetUnit.HasAttacked &&
                    targetUnit.CurrentHealth > 0 &&
                    targetUnit.CanAttack(this) &&
                    Random.Range(0, 100) < targetUnit.CounterAttackChance)
                {
                    Debug.Log($"{targetUnit.UnitName} counter-attacks!");
                    StartCoroutine(DelayedCounterAttack(targetUnit));
                }
            }

            // Mark as attacked
            HasAttacked = true;
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;

        //Play Feedback
        if (damageFeedback != null)
        {
            damageFeedback.PlayFeedbacks();
        }

        // Update health bar
        if (GameManager.Instance.VisualManager != null)
        {
            GameManager.Instance.VisualManager.UpdateHealthBar(this);
        }

        // Check if the unit is defeated
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }

        Debug.Log($"{UnitName} took {damage} damage, {CurrentHealth}/{MaxHealth} HP remaining");
    }

    public void Die()
    {

        // Play death sound
        if (GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySFX("UnitDie");
        }

        // Play death feedback
        if (deathFeedback != null)
        {
            deathFeedback.PlayFeedbacks();
        }



        // Clean up visual elements
        if (GameManager.Instance.VisualManager != null)
        {
            GameManager.Instance.VisualManager.CleanupUnit(this);
        }

        // Clear the tile
        if (CurrentTile != null)
        {
            CurrentTile.ClearUnit();
        }

        // Play death animation
        // This will be expanded later

        Debug.Log($"{UnitName} has been defeated");

        GameManager.Instance.TurnManager.CheckEndGameState();
        // Destroy the game object after a delay
        Destroy(gameObject, 1f);
    }

    public void ResetTurn()
    {
        HasMoved = false;
        HasAttacked = false;
    }

    public void ShowUnitInfo()
    {
        // Create a simple text display above the unit
        // This will be enhanced with the Text Animator asset later
        Debug.Log($"{UnitName}: HP {CurrentHealth}/{MaxHealth}, ATK {BaseAttack}, DEF {Defense}");

        // Show unit info in UI
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowUnitInfo(this);
        }
    }

    private void SetupCombatText()
    {
        GameObject textObj = new GameObject("CombatText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 0.5f, 0);

        combatTextMesh = textObj.AddComponent<TextMesh>();
        combatTextMesh.characterSize = 0.1f;
        combatTextMesh.anchor = TextAnchor.MiddleCenter;
        combatTextMesh.alignment = TextAlignment.Center;
        combatTextMesh.fontSize = 48;

        // Hide initially
        combatTextMesh.text = "";
    }

    private IEnumerator DelayedCounterAttack(Unit counterAttacker)
    {
        // Wait a moment before counter-attacking
        yield return new WaitForSeconds(0.5f);

        // Calculate counter attack damage (usually less than normal attack)
        int counterDamage = Mathf.Max(1, counterAttacker.BaseAttack / 2 - Defense);

        // Apply damage
        TakeDamage(counterDamage);

        // Show combat text
        counterAttacker.ShowCombatText("Counter!", Color.yellow);
        ShowCombatText($"-{counterDamage}", Color.red);
    }

    // Add this method to show combat text
    public void ShowCombatText(string text, Color color)
    {
        if (combatTextMesh == null)
        {
            SetupCombatText();
        }

        combatTextMesh.text = text;
        combatTextMesh.color = color;

        // Hide the text after a delay
        StartCoroutine(HideCombatTextAfterDelay());
    }

    private IEnumerator HideCombatTextAfterDelay()
    {
        yield return new WaitForSeconds(textDisplayDuration);
        if (combatTextMesh != null)
        {
            combatTextMesh.text = "";
        }
    }
    public void MarkAsMoved()
    {
        HasMoved = true;
    }

    public void MarkAsAttacked()
    {
        HasAttacked = true;
    }

}

// Enum for unit factions
public enum Faction
{
    Player,
    Enemy,
    Neutral
}
