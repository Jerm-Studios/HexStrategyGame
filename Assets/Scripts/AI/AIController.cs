using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    public float decisionDelay = 0.5f;
    public float moveDelay = 0.3f;
    public float attackDelay = 0.5f;

    [Header("Difficulty Settings")]
    [Range(0, 1)] public float aggressiveness = 0.6f;
    [Range(0, 1)] public float tacticalIntelligence = 0.7f;

    private GridManager gridManager;
    private UnitManager unitManager;

    public void Initialize()
    {
        gridManager = GameManager.Instance.GridManager;
        unitManager = GameManager.Instance.UnitManager;
        Debug.Log("AIController initialized");
    }

    public IEnumerator ProcessEnemyTurn()
    {
        Debug.Log("AI processing enemy turn");

        // Get all enemy units
        List<Unit> enemyUnits = unitManager.GetEnemyUnits();
        List<Unit> playerUnits = unitManager.GetPlayerUnits();

        // Remove null entries (destroyed units)
        enemyUnits.RemoveAll(unit => unit == null);
        playerUnits.RemoveAll(unit => unit == null);

        if (playerUnits.Count == 0)
        {
            Debug.Log("No player units found, ending AI turn");
            yield break;
        }

        // Sort enemy units by priority (e.g., ranged units first)
        enemyUnits.Sort((a, b) => {
            // Prioritize units that can attack
            bool aCanAttack = CanUnitAttackAnyTarget(a, playerUnits);
            bool bCanAttack = CanUnitAttackAnyTarget(b, playerUnits);

            if (aCanAttack && !bCanAttack) return -1;
            if (!aCanAttack && bCanAttack) return 1;

            // Then prioritize by attack range
            return b.AttackRange.CompareTo(a.AttackRange);
        });

        // Process each enemy unit
        foreach (Unit enemyUnit in enemyUnits)
        {
            yield return new WaitForSeconds(decisionDelay);

            // Skip if unit is null or has already acted
            if (enemyUnit == null || (enemyUnit.HasMoved && enemyUnit.HasAttacked))
                continue;

            // Highlight the active unit
            enemyUnit.CurrentTile.Highlight(true, Color.red);

            // Evaluate tactical options
            AIDecision bestDecision = EvaluateBestDecision(enemyUnit, playerUnits);

            // Execute the decision
            yield return StartCoroutine(ExecuteAIDecision(enemyUnit, bestDecision));

            // Remove highlight
            if (enemyUnit != null && enemyUnit.CurrentTile != null)
            {
                enemyUnit.CurrentTile.Highlight(false, Color.white);
            }

            yield return new WaitForSeconds(decisionDelay);
        }

        Debug.Log("AI finished processing enemy turn");
    }

    private bool CanUnitAttackAnyTarget(Unit attacker, List<Unit> potentialTargets)
    {
        foreach (Unit target in potentialTargets)
        {
            if (attacker.CanAttack(target))
                return true;
        }
        return false;
    }

    private AIDecision EvaluateBestDecision(Unit enemyUnit, List<Unit> playerUnits)
    {
        AIDecision decision = new AIDecision();

        // Check if we can attack any player unit from current position
        Unit bestTargetFromCurrentPos = FindBestAttackTarget(enemyUnit, playerUnits);
        if (bestTargetFromCurrentPos != null)
        {
            // We can attack without moving
            decision.type = AIDecisionType.Attack;
            decision.targetUnit = bestTargetFromCurrentPos;
            return decision;
        }

        // We need to move - evaluate possible moves
        List<HexTile> movementOptions = GetMovementOptions(enemyUnit);
        if (movementOptions.Count == 0)
        {
            // No valid moves
            decision.type = AIDecisionType.Wait;
            return decision;
        }

        // For each possible move, check if we can attack after moving
        HexTile bestMoveTile = null;
        Unit bestTargetAfterMove = null;
        float bestScore = float.MinValue;

        foreach (HexTile moveTile in movementOptions)
        {
            // Simulate moving to this tile
            Vector3Int originalCoords = enemyUnit.HexCoordinates;
            HexTile originalTile = enemyUnit.CurrentTile;

            // Temporarily update unit position for calculation
            enemyUnit.CurrentTile.ClearUnit();
            moveTile.SetUnit(enemyUnit);

            // Store the simulated coordinates for calculation (don't modify HexCoordinates directly)
            Vector3Int simulatedCoordinates = moveTile.HexCoordinates;

            // Check if we can attack from this new position
            foreach (Unit playerUnit in playerUnits)
            {
                if (playerUnit == null) continue;

                // Calculate distance to player unit
                float distance = HexDistance(simulatedCoordinates, playerUnit.HexCoordinates);

                // Calculate score based on various factors
                float score = EvaluateAttackScore(enemyUnit, playerUnit, distance);

                // If we can attack from this position and score is better
                if (distance <= enemyUnit.AttackRange && score > bestScore)
                {
                    bestScore = score;
                    bestMoveTile = moveTile;
                    bestTargetAfterMove = playerUnit;
                }
                else if (bestMoveTile == null)
                {
                    // If we can't attack, move towards weakest player unit
                    float moveScore = -distance + (1 - (playerUnit.CurrentHealth / (float)playerUnit.MaxHealth)) * 10;
                    if (moveScore > bestScore)
                    {
                        bestScore = moveScore;
                        bestMoveTile = moveTile;
                        bestTargetAfterMove = null; // No attack after move
                    }
                }
            }

            // Restore original position
            moveTile.ClearUnit();
            originalTile.SetUnit(enemyUnit);
        }

        // Decide what to do based on evaluation
        if (bestMoveTile != null)
        {
            decision.type = bestTargetAfterMove != null ? AIDecisionType.MoveAndAttack : AIDecisionType.Move;
            decision.moveTile = bestMoveTile;
            decision.targetUnit = bestTargetAfterMove;
        }
        else
        {
            decision.type = AIDecisionType.Wait;
        }

        return decision;
    }


    private float EvaluateAttackScore(Unit attacker, Unit defender, float distance)
    {
        // Base score is damage we can do
        int estimatedDamage = Mathf.Max(1, attacker.BaseAttack - defender.Defense);
        float score = estimatedDamage;

        // Bonus for potentially killing the target
        if (defender.CurrentHealth <= estimatedDamage)
        {
            score += 100;
        }

        // Bonus for attacking low health targets
        score += (1 - defender.CurrentHealth / (float)defender.MaxHealth) * 20;

        // Penalty for being too close to multiple enemies (if we're not a tank)
        if (attacker.Defense < 7)
        {
            int nearbyEnemies = CountNearbyPlayerUnits(attacker.HexCoordinates, 2);
            score -= nearbyEnemies * 5;
        }

        // Adjust based on AI aggressiveness
        score *= (0.5f + aggressiveness);

        return score;
    }

    private int CountNearbyPlayerUnits(Vector3Int position, int range)
    {
        int count = 0;
        List<Unit> playerUnits = unitManager.GetPlayerUnits();

        foreach (Unit playerUnit in playerUnits)
        {
            if (playerUnit != null)
            {
                float distance = HexDistance(position, playerUnit.HexCoordinates);
                if (distance <= range)
                {
                    count++;
                }
            }
        }

        return count;
    }


    private List<HexTile> GetMovementOptions(Unit unit)
    {
        List<HexTile> options = new List<HexTile>();

        // Get all tiles within movement range
        for (int q = -unit.MovementRange; q <= unit.MovementRange; q++)
        {
            for (int r = Mathf.Max(-unit.MovementRange, -q - unit.MovementRange);
                 r <= Mathf.Min(unit.MovementRange, -q + unit.MovementRange); r++)
            {
                int s = -q - r;
                Vector3Int hexCoord = unit.HexCoordinates + new Vector3Int(q, r, s);

                HexTile tile = gridManager.GetTileAt(hexCoord);
                if (tile != null && !tile.IsOccupied() && unit.CanMoveTo(tile))
                {
                    options.Add(tile);
                }
            }
        }

        return options;
    }

    private Unit FindBestAttackTarget(Unit attacker, List<Unit> potentialTargets)
    {
        Unit bestTarget = null;
        float bestScore = float.MinValue;

        foreach (Unit target in potentialTargets)
        {
            if (target == null) continue;

            if (attacker.CanAttack(target))
            {
                float distance = HexDistance(attacker.HexCoordinates, target.HexCoordinates);
                float score = EvaluateAttackScore(attacker, target, distance);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }
        }

        return bestTarget;
    }

    private float HexDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2f;
    }

    private IEnumerator ExecuteAIDecision(Unit unit, AIDecision decision)
    {
        switch (decision.type)
        {
            case AIDecisionType.Attack:
                Debug.Log($"{unit.UnitName} attacking {decision.targetUnit.UnitName}");
                unit.Attack(decision.targetUnit);

                // Play attack animation
                if (GameManager.Instance.CombatManager != null)
                {
                    yield return StartCoroutine(GameManager.Instance.CombatManager.PerformAttackSequence(unit, decision.targetUnit));
                }
                else
                {
                    yield return new WaitForSeconds(attackDelay);
                }
                break;

            case AIDecisionType.Move:
                Debug.Log($"{unit.UnitName} moving to {decision.moveTile.HexCoordinates}");
                unit.MoveTo(decision.moveTile);
                yield return new WaitForSeconds(moveDelay);
                break;

            case AIDecisionType.MoveAndAttack:
                Debug.Log($"{unit.UnitName} moving to {decision.moveTile.HexCoordinates} and attacking {decision.targetUnit.UnitName}");
                unit.MoveTo(decision.moveTile);
                yield return new WaitForSeconds(moveDelay);

                // After moving, attack
                if (!unit.HasAttacked && unit.CanAttack(decision.targetUnit))
                {
                    unit.Attack(decision.targetUnit);

                    // Play attack animation
                    if (GameManager.Instance.CombatManager != null)
                    {
                        yield return StartCoroutine(GameManager.Instance.CombatManager.PerformAttackSequence(unit, decision.targetUnit));
                    }
                    else
                    {
                        yield return new WaitForSeconds(attackDelay);
                    }
                }
                break;

            case AIDecisionType.Wait:
                Debug.Log($"{unit.UnitName} waiting");
                yield return new WaitForSeconds(moveDelay);
                break;
        }
    }
}

// AI decision structure
public class AIDecision
{
    public AIDecisionType type;
    public HexTile moveTile;
    public Unit targetUnit;
}

// AI decision types
public enum AIDecisionType
{
    Attack,
    Move,
    MoveAndAttack,
    Wait
}
