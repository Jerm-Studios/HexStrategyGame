using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HealthBar : MonoBehaviour
{
    public Image healthFill;
    public TextMeshProUGUI healthText;

    private Unit targetUnit;

    public void Initialize(Unit unit)
    {
        targetUnit = unit;
        UpdateHealth(unit.CurrentHealth, unit.MaxHealth);
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)current / max;
        }

        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }

    private void LateUpdate()
    {
        // Always face the camera
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }

        // Update health if target unit exists
        if (targetUnit != null)
        {
            UpdateHealth(targetUnit.CurrentHealth, targetUnit.MaxHealth);
        }
    }
}
