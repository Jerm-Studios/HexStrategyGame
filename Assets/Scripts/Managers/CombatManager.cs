using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackAnimationDuration = 0.5f;
    public float damageDisplayDuration = 1.0f;

    [Header("Combat VFX")]
    public GameObject meleeAttackVFXPrefab;
    public GameObject rangedAttackVFXPrefab;

    public void Initialize()
    {
        Debug.Log("CombatManager initialized");
    }

    public IEnumerator PerformAttackSequence(Unit attacker, Unit defender)
    {
        // Highlight the units involved
        attacker.CurrentTile.Highlight(true, Color.blue);
        defender.CurrentTile.Highlight(true, Color.red);

        // Move attacker towards defender slightly
        Vector3 originalPosition = attacker.transform.position;
        Vector3 attackPosition = Vector3.Lerp(attacker.transform.position, defender.transform.position, 0.3f);

        // Animate the attack
        float elapsedTime = 0f;
        while (elapsedTime < attackAnimationDuration / 2)
        {
            attacker.transform.position = Vector3.Lerp(originalPosition, attackPosition, elapsedTime / (attackAnimationDuration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Spawn attack VFX
        if (attacker.AttackRange > 1 && rangedAttackVFXPrefab != null)
        {
            // Ranged attack
            GameObject vfx = Instantiate(rangedAttackVFXPrefab, attacker.transform.position, Quaternion.identity);

            // Initialize the projectile line effect
            ProjectileLineEffect lineEffect = vfx.GetComponent<ProjectileLineEffect>();
            if (lineEffect != null)
            {
                lineEffect.Initialize(attacker.transform.position, defender.transform.position);
            }

            // No need to manually destroy it as the script handles that
        }
        else if (meleeAttackVFXPrefab != null)
        {
            // Melee attack
            GameObject vfx = Instantiate(meleeAttackVFXPrefab, defender.transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        // Move attacker back to original position
        elapsedTime = 0f;
        while (elapsedTime < attackAnimationDuration / 2)
        {
            attacker.transform.position = Vector3.Lerp(attackPosition, originalPosition, elapsedTime / (attackAnimationDuration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure attacker is back at original position
        attacker.transform.position = originalPosition;

        // Wait for damage display
        yield return new WaitForSeconds(damageDisplayDuration);

        // Remove highlights
        attacker.CurrentTile.Highlight(false, Color.white);
        defender.CurrentTile.Highlight(false, Color.white);
    }

    public void CalculateCombatPreview(Unit attacker, Unit defender, out int estimatedDamage, out bool canKill)
    {
        // Calculate base damage
        estimatedDamage = Mathf.Max(1, attacker.BaseAttack - defender.Defense);

        // Check if this would kill the defender
        canKill = defender.CurrentHealth <= estimatedDamage;
    }

    public void ShowCombatPreview(Unit attacker, Unit defender)
    {
        int estimatedDamage;
        bool canKill;
        CalculateCombatPreview(attacker, defender, out estimatedDamage, out canKill);

        Debug.Log($"Combat Preview: {attacker.UnitName} vs {defender.UnitName}");
        Debug.Log($"Estimated damage: {estimatedDamage}");
        Debug.Log($"Can kill: {canKill}");

        // This would be expanded to show a UI element with the preview
    }
}
