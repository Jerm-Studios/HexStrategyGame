using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

public class VisualManager : MonoBehaviour
{
    [Header("Visual References")]
    public GameObject healthBarPrefab;
    public GameObject statusEffectPrefab;
    public GameObject selectionIndicatorPrefab;
    public GameObject moveRangePrefab;
    public GameObject attackRangePrefab;

    [Header("Visual Settings")]
    public float floatingTextDuration = 1.5f;
    public float floatingTextRiseSpeed = 1.0f;
    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;
    public Color neutralColor = Color.white;

    // Dictionary to track visual elements by unit
    private Dictionary<Unit, GameObject> healthBars = new Dictionary<Unit, GameObject>();
    private Dictionary<Unit, GameObject> selectionIndicators = new Dictionary<Unit, GameObject>();
    private Dictionary<Unit, List<GameObject>> statusEffects = new Dictionary<Unit, List<GameObject>>();

    public void Initialize()
    {
        Debug.Log("VisualManager initialized");
    }

    public void CreateHealthBar(Unit unit)
    {
        if (healthBarPrefab == null || unit == null)
            return;

        // Create health bar
        GameObject healthBarObj = Instantiate(healthBarPrefab, unit.transform);
        healthBarObj.transform.localPosition = new Vector3(0, 0.5f, 0);

        // Get health bar component
        HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.Initialize(unit);
        }

        // Store reference
        healthBars[unit] = healthBarObj;
    }

    public void UpdateHealthBar(Unit unit)
    {
        if (unit == null || !healthBars.ContainsKey(unit))
            return;

        GameObject healthBarObj = healthBars[unit];
        HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.UpdateHealth(unit.CurrentHealth, unit.MaxHealth);
        }
    }

    public void ShowSelectionIndicator(Unit unit)
    {
        if (selectionIndicatorPrefab == null || unit == null)
            return;

        // Remove any existing selection indicator
        HideSelectionIndicator(unit);

        // Create selection indicator
        GameObject indicatorObj = Instantiate(selectionIndicatorPrefab, unit.transform);
        indicatorObj.transform.localPosition = new Vector3(0, 0.1f, 0);

        // Set color based on faction
        SpriteRenderer renderer = indicatorObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            switch (unit.UnitFaction)
            {
                case Faction.Player:
                    renderer.color = playerColor;
                    break;
                case Faction.Enemy:
                    renderer.color = enemyColor;
                    break;
                case Faction.Neutral:
                    renderer.color = neutralColor;
                    break;
            }
        }

        // Store reference
        selectionIndicators[unit] = indicatorObj;
    }

    public void HideSelectionIndicator(Unit unit)
    {
        if (unit == null || !selectionIndicators.ContainsKey(unit))
            return;

        GameObject indicatorObj = selectionIndicators[unit];
        Destroy(indicatorObj);
        selectionIndicators.Remove(unit);
    }

    public void AddStatusEffect(Unit unit, StatusEffectType effectType, int duration)
    {
        if (statusEffectPrefab == null || unit == null)
            return;

        // Initialize list if needed
        if (!statusEffects.ContainsKey(unit))
        {
            statusEffects[unit] = new List<GameObject>();
        }

        // Create status effect
        GameObject effectObj = Instantiate(statusEffectPrefab, unit.transform);

        // Position based on how many effects the unit already has
        int effectCount = statusEffects[unit].Count;
        effectObj.transform.localPosition = new Vector3(0.3f * effectCount - 0.3f, 0.7f, 0);

        // Set up status effect
        StatusEffect statusEffect = effectObj.GetComponent<StatusEffect>();
        if (statusEffect != null)
        {
            statusEffect.Initialize(effectType, duration);
        }

        // Store reference
        statusEffects[unit].Add(effectObj);
    }

    public void RemoveStatusEffect(Unit unit, int effectIndex)
    {
        if (unit == null || !statusEffects.ContainsKey(unit) || effectIndex >= statusEffects[unit].Count)
            return;

        GameObject effectObj = statusEffects[unit][effectIndex];
        statusEffects[unit].RemoveAt(effectIndex);
        Destroy(effectObj);

        // Reposition remaining effects
        for (int i = effectIndex; i < statusEffects[unit].Count; i++)
        {
            statusEffects[unit][i].transform.localPosition = new Vector3(0.3f * i - 0.3f, 0.7f, 0);
        }
    }

    public void ShowMoveRange(Unit unit, List<HexTile> tiles)
    {
        if (moveRangePrefab == null || unit == null || tiles == null)
            return;

        foreach (HexTile tile in tiles)
        {
            if (tile.IsOccupied())
                continue;

            GameObject rangeObj = Instantiate(moveRangePrefab, tile.transform);
            rangeObj.transform.localPosition = new Vector3(0, 0.05f, 0);
            rangeObj.name = "MoveRange";
        }
    }

    public void HideMoveRange()
    {
        GameObject[] rangeObjects = GameObject.FindGameObjectsWithTag("MoveRange");
        foreach (GameObject obj in rangeObjects)
        {
            Destroy(obj);
        }
    }

    public void ShowAttackRange(Unit unit, List<HexTile> tiles)
    {
        if (attackRangePrefab == null || unit == null || tiles == null)
            return;

        foreach (HexTile tile in tiles)
        {
            GameObject rangeObj = Instantiate(attackRangePrefab, tile.transform);
            rangeObj.transform.localPosition = new Vector3(0, 0.05f, 0);
            rangeObj.name = "AttackRange";
        }
    }

    public void HideAttackRange()
    {
        GameObject[] rangeObjects = GameObject.FindGameObjectsWithTag("AttackRange");
        foreach (GameObject obj in rangeObjects)
        {
            Destroy(obj);
        }
    }

    public void ShowFloatingText(Vector3 position, string text, Color color)
    {
        GameObject textObj = new GameObject("FloatingText");
        textObj.transform.position = position;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = color;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        StartCoroutine(AnimateFloatingText(textObj));
    }

    private IEnumerator AnimateFloatingText(GameObject textObj)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = textObj.transform.position;

        while (elapsedTime < floatingTextDuration)
        {
            textObj.transform.position = startPosition + new Vector3(0, elapsedTime * floatingTextRiseSpeed, 0);

            // Fade out near the end
            if (elapsedTime > floatingTextDuration * 0.7f)
            {
                TextMesh textMesh = textObj.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    float alpha = 1 - ((elapsedTime - floatingTextDuration * 0.7f) / (floatingTextDuration * 0.3f));
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(textObj);
    }

    public void PlayAttackAnimation(Unit attacker, Unit defender)
    {
        // Shake camera
        CameraManager CameraManager = Camera.main.GetComponent<CameraManager>();
        if (CameraManager != null)
        {
            CameraManager.ShakeCamera(0.5f);
        }

        // Show floating damage text
        int damage = Mathf.Max(1, attacker.BaseAttack - defender.Defense);
        ShowFloatingText(defender.transform.position + Vector3.up * 0.5f, "-" + damage, Color.red);
    }

    public void CleanupUnit(Unit unit)
    {
        // Remove health bar
        if (healthBars.ContainsKey(unit))
        {
            Destroy(healthBars[unit]);
            healthBars.Remove(unit);
        }   

        // Remove selection indicator
        if (selectionIndicators.ContainsKey(unit))
        {
            Destroy(selectionIndicators[unit]);
            selectionIndicators.Remove(unit);
        }

        // Remove status effects
        if (statusEffects.ContainsKey(unit))
        {
            foreach (GameObject effectObj in statusEffects[unit])
            {
                Destroy(effectObj);
            }
            statusEffects.Remove(unit);
        }
    }
}

// Status effect types
public enum StatusEffectType
{
    Poisoned,
    Stunned,
    Strengthened,
    Protected,
    Hasted
}
